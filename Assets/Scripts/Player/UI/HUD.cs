using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
namespace Player
{
    public class HUD : UISystem
    {
        #region Fields
        [SerializeField] protected RectTransform healthParent;
        [SerializeField] protected HealthDisplay healthDisplayPrefab;
        protected List<HealthDisplay> healthDisplays = new();
        [SerializeField] protected Image speedBar;
        [SerializeField] protected TextMeshProUGUI speedText;
        protected PlayerShip player;
        [SerializeField] protected List<Sprite> turretStanceIcons = new();
        protected Canvas canvas;
        [SerializeField] protected WeaponDisplay weaponDisplayPrefab;
        protected List<WeaponDisplay> turretDisplays = new(), launcherDisplays = new();
        protected PlayerWeapons playerWeapons;
        public PlayerWeapons PlayerWeaponsProperty
        {
            get
            {
                if (playerWeapons == null) playerWeapons = transform.root.GetComponentInChildren<PlayerWeapons>();
                return playerWeapons;
            }
        }
        [SerializeField] protected TextMeshProUGUI dampenerText;
        [Tooltip("All weapon displays will be children of this object.")]
        [SerializeField] protected RectTransform weaponDisplayParent;
        [SerializeField] protected Image reloadIconPrefab;
        [SerializeField] protected TextMeshProUGUI friendlyHP;
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            canvas = transform.root.GetComponentInChildren<Canvas>();
            player = GetComponentInParent<PlayerShip>();
            GlobalUpdater.TryGetInstance(true).RegisterLateUpdate(PerformLateUpdate);
            player.OnDampenersChanged += UpdateDampenerText;
            UpdateDampenerText(player.Dampeners);

            #region Weapons
            var count = PlayerWeaponsProperty.TurretGroups.Count +
                PlayerWeaponsProperty.LauncherGroups.Count;
            foreach (var weapon in PlayerWeaponsProperty.LauncherGroups)
            {
                var display = Instantiate(weaponDisplayPrefab, weaponDisplayParent);
                display.Initialize(weapon, reloadIconPrefab);
                launcherDisplays.Add(display);
            }
            foreach (var weapon in PlayerWeaponsProperty.TurretGroups)
            {
                var display = Instantiate(weaponDisplayPrefab, weaponDisplayParent);
                display.Initialize(weapon, reloadIconPrefab);
                display.TurretStanceIcon.sprite = turretStanceIcons[weapon.Turrets[0].CurrentStanceIndex];
                turretDisplays.Add(display);
            }
            PlayerWeaponsProperty.OnTurretGroupStanceChange += (i) =>
            {
                turretDisplays[i].TurretStanceIcon.sprite = turretStanceIcons[
                    PlayerWeaponsProperty.TurretGroups[i].Turrets[0].CurrentStanceIndex];
            };
            PlayerWeaponsProperty.OnWeaponsCycled += OnWeaponsCycled;
            OnWeaponsCycled((-1, 0));
            #endregion
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterLateUpdate(PerformLateUpdate);
        }
        #endregion

        #region Functionality
        /// <summary>
        /// Update health, speed, signature.
        /// </summary>
        /// <param name="deltaTime"></param>
        protected void PerformLateUpdate(float deltaTime)
        {
            UpdateHealthBar();
            UpdateSpeedBar();
            UpdateWeaponDisplays();
            UpdateFriendlyHP();
        }

        public void UpdateFriendlyHP()
        {
            string s = "";
            foreach (var ship in UnitManager.GetShips(GlobalConfig.PlayerTeam))
            {
                if (ship.CriticalObjective)
                {
                    s += $"{ship.Transform.name}: {ship.CurrentHPPercentage * 100:0}%\n";
                }
            }
            friendlyHP.text = s;
        }

        public void UpdateDampenerText(bool value)
        {
            dampenerText.text = value ? "DAMPENERS ON" : "DAMPENERS OFF";
        }

        public void UpdateHealthBar()
        {
            if (player.HPComponent.HPBlocks.Count <= 0) return;
            if (player.HPComponent.HPBlocks.Count != healthParent.childCount)
            {
                //generate the health blocks
                healthDisplays.Clear();
                for (int i = 0; i < healthParent.childCount; i++)
                {
                    Destroy(healthParent.GetChild(i).gameObject);
                }
                for (int i = 0; i < player.HPComponent.HPBlocks.Count; i++)
                {
                    healthDisplays.Add(Instantiate(healthDisplayPrefab, healthParent));
                }
            }
            for (int i = 0; i < healthDisplays.Count; i++)
            {
                var val = Mathf.Max(0, player.HPComponent.HPBlocks[i].CurrentHPPercentage);
                healthDisplays[i].HealthImage.fillAmount = val;
                //healthDisplays[i].HealthImage.color = Color.Lerp(Color.red, Color.green, val);
            }
        }

        public void UpdateSpeedBar()
        {
            speedBar.fillAmount = Mathf.Max(0, player.LinearVelocity.magnitude / player.MaxVelocityMagnitude);
            speedText.text = $"SPD: {(int)(10 * player.LinearVelocity.magnitude)} m/s";
        }

        public void UpdateWeaponDisplays()
        {
            for (int i = 0; i < PlayerWeaponsProperty.TurretGroups.Count; i++)
            {
                turretDisplays[i].UpdateDisplay(PlayerWeaponsProperty.TurretGroups[i]);
            }
            for (int i = 0; i < launcherDisplays.Count; i++)
            {
                launcherDisplays[i].UpdateDisplay(PlayerWeaponsProperty.LauncherGroups[i]);
            }
        }

        public void OnWeaponsCycled((int oldValue, int newValue) pair)
        {
            if (pair.oldValue != -1)
            {
                if (pair.oldValue < launcherDisplays.Count)
                {
                    //this is a launcher group
                    launcherDisplays[pair.oldValue].SelectionIcon.enabled = false;
                }
                else
                {
                    //this is a turret group
                    turretDisplays[pair.oldValue - launcherDisplays.Count].SelectionIcon.enabled = false;
                }
            }

            if (pair.newValue < launcherDisplays.Count)
            {
                //this is a launcher group
                launcherDisplays[pair.newValue].SelectionIcon.enabled = true;
            }
            else
            {
                //this is a turret group
                turretDisplays[pair.newValue - launcherDisplays.Count].SelectionIcon.enabled = true;
            }
        }
        #endregion
    }
}
