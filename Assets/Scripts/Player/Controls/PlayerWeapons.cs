using EventBus;
using Radar;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;
namespace Player
{
    public class PlayerWeapons : MonoBehaviour, PlayerControls.IWeaponsActions
    {
        [field: SerializeField] public List<MissileGroup> LauncherGroups { get; protected set; } = new();
        [field: SerializeField] public List<TurretGroup> TurretGroups { get; protected set; } = new();
        protected int currentWeaponIndex;
        public event System.Action<(int oldValue, int newValue)> OnWeaponsCycled = delegate { };
        public event System.Action<int> OnTurretGroupStanceChange = delegate { };

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
                currentWeaponIndex = newValue;
            }
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
                else if (context.started)
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
                if (launcher.Shoot(t.CurrentTarget)) break;
            }
        }

        /// <summary>
        /// Fires all launcher in selected group.
        /// </summary>
        /// <param name="missileGroup"></param>
        public void FireAllMissiles(int missileGroup)
        {
            var t = PlayerTargetSystem.TryGetInstance();
            if (t == null || !t.CurrentTarget.Active()) return;
            foreach (var launcher in LauncherGroups[missileGroup].Launchers)
            {
                launcher.Shoot(t.CurrentTarget);
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
