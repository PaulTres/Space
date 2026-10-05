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
        [SerializeField] protected Image healthBar;
        [SerializeField] protected TextMeshProUGUI healthText;
        [SerializeField] protected Image speedBar;
        [SerializeField] protected TextMeshProUGUI speedText;
        [SerializeField] protected Image maxSignatureDisplay, signatureDisplay;
        [SerializeField] protected TextMeshProUGUI signatureText;
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
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            canvas = transform.root.GetComponentInChildren<Canvas>();
            player = GetComponentInParent<PlayerShip>();
            healthBar.fillAmount = 1;
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
            UpdateSignatureDisplay();
            UpdateWeaponDisplays();
        }

        public void UpdateDampenerText(bool value)
        {
            dampenerText.text = value ? "DAMPENERS ON" : "DAMPENERS OFF";
        }

        public void UpdateHealthBar()
        {
            healthBar.fillAmount = Mathf.Max(0, player.CurrentHPPercentage);
            healthText.text = $"HP: {(int)player.CurrentHP}/{(int)player.MaxHP}";
        }

        public void UpdateSpeedBar()
        {
            speedBar.fillAmount = Mathf.Max(0, player.LinearVelocity.magnitude / player.MaxVelocityMagnitude);
            speedText.text = $"SPD: {(int)(10 * player.LinearVelocity.magnitude)} m/s";
        }

        public void UpdateSignatureDisplay()
        {
            var percentage = player.CurrentSignature / (player.MaximumSignature + Mathf.Epsilon);
            signatureText.text = $"{(int)player.CurrentSignature}/{(int)player.MaximumSignature}";
            signatureDisplay.rectTransform.localScale = percentage * maxSignatureDisplay.rectTransform.localScale;
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
