using AudioSystem;
using EventBus;
using Radar;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;
using Weapons;
namespace Player
{
    public class PlayerWeapons : MonoBehaviour, PlayerControls.IWeaponsActions
    {
        [SerializeField] protected Transform weaponParent;
        [field: SerializeField] public List<MissileGroup> LauncherGroups { get; protected set; } = new();
        [field: SerializeField] public List<TurretGroup> TurretGroups { get; protected set; } = new();
        protected int currentWeaponIndex;
        public event System.Action<(int oldValue, int newValue)> OnWeaponsCycled = delegate { };
        public event System.Action<int> OnTurretGroupStanceChange = delegate { };
        [SerializeField] protected SoundData errorSound;

        protected void OnValidate()
        {
            if (weaponParent != null && LauncherGroups.IsEmpty() && TurretGroups.IsEmpty())
            {
                for (int i = 0; i < weaponParent.childCount; i++)
                {
                    //this object holds a number of weapons
                    var child = weaponParent.GetChild(i);
                    var turrets = child.GetComponentsInChildren<Turret>();
                    if (turrets.IsEmpty())
                    {
                        //this is a launcher group
                        var launchers = child.GetComponentsInChildren<MissileLauncher>();
                        LauncherGroups.Add(new(launchers.ToList()));
                    }
                    else
                    {
                        TurretGroups.Add(new(turrets.ToList(), child.GetComponent<Heatsink>()));
                    }
                }
            }
        }

        protected void OnEnable()
        {
            EventBus<PauseScreen.PauseEvent>.AddActions(OnPauseEvent);
            currentWeaponIndex = 0;
            GameManager.InputActions.Weapons.SetCallbacks(this);
        }

        protected void OnDisable()
        {
            EventBus<PauseScreen.PauseEvent>.RemoveActions(OnPauseEvent);
            GameManager.InputActions.Weapons.RemoveCallbacks(this);
        }

        public void OnPauseEvent(PauseScreen.PauseEvent pauseEvent)
        {
            if (pauseEvent.Paused) GameManager.InputActions.Weapons.RemoveCallbacks(this);
            else GameManager.InputActions.Weapons.SetCallbacks(this);
        }

        public void OnCycle(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                var newValue = (currentWeaponIndex + 1) % (LauncherGroups.Count + TurretGroups.Count);
                OnWeaponsCycled?.Invoke((currentWeaponIndex, newValue));
                GetWeaponGroup(currentWeaponIndex)?.OnDeselected?.Invoke();
                GetWeaponGroup(newValue)?.OnSelected?.Invoke();
                currentWeaponIndex = newValue;
            }
        }

        public WeaponGroup GetWeaponGroup(int index)
        {
            if (index < LauncherGroups.Count) return LauncherGroups[index];
            else if (index - LauncherGroups.Count < TurretGroups.Count) return TurretGroups[index - LauncherGroups.Count];
            else return null;
        }

        public void OnExecuteAction(InputAction.CallbackContext context)
        {
            if (currentWeaponIndex < LauncherGroups.Count)
            {
                if (context.canceled)
                {
                    //Debug.Log("Firing all");
                    FireAllMissiles(currentWeaponIndex);
                }
                else if (context.performed)
                {
                    //Debug.Log("Tap firing");
                    TapFireMissiles(currentWeaponIndex);
                }
            }
            else if (context.started)
            {
                //this is a turret group
                CycleTurretStance(currentWeaponIndex - LauncherGroups.Count);
            }
        }

        public void OnFindTarget(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                var t = PlayerTargetSystem.TryGetInstance();
                if (t != null)
                {
                    t.SelectTarget();
                }
            }
        }

        /// <summary>
        /// Only fires one launcher at a time at most.
        /// </summary>
        /// <param name="missileGroup"></param>
        public void TapFireMissiles(int missileGroup)
        {
            var t = PlayerTargetSystem.TryGetInstance();
            if (t == null || !t.CurrentTarget.Active()) return;
            foreach (var launcher in LauncherGroups[missileGroup].Launchers)
            {
                if (launcher.Shoot(t.CurrentTarget)) return;
            }
            //Debug.Log("Error");
            errorSound.Play(transform.position);
        }

        /// <summary>
        /// Fires all launcher in selected group.
        /// </summary>
        /// <param name="missileGroup"></param>
        public void FireAllMissiles(int missileGroup)
        {
            var t = PlayerTargetSystem.TryGetInstance();
            if (t == null || !t.CurrentTarget.Active()) return;
            bool playedErrorSound = false;
            foreach (var launcher in LauncherGroups[missileGroup].Launchers)
            {
                if (!launcher.Shoot(t.CurrentTarget) && !playedErrorSound)
                {
                    //Debug.Log("Error1");
                    errorSound.Play(transform.position);
                    playedErrorSound = true;
                }
            }
        }

        /// <summary>
        /// Cycles the stance of all turrets in the selected group.
        /// </summary>
        /// <param name="groupIndex"></param>
        public void CycleTurretStance(int groupIndex)
        {
            foreach (var turret in TurretGroups[groupIndex].Turrets)
            {
                turret.CycleStance();
            }
            //Debug.Log(TurretGroups[groupIndex].Turrets[0].CurrentStanceIndex);
            OnTurretGroupStanceChange?.Invoke(groupIndex);
        }
    }
}
