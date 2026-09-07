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
            var totalSpace = 50 * count + (count - 1) * 10;
            var startX = -totalSpace / 2;
            foreach (var weapon in PlayerWeaponsProperty.LauncherGroups)
            {
                var display = Instantiate(weaponDisplayPrefab, canvas.transform);
                display.HeatStatusImage.enabled = false;
                display.ReloadStatusImage.enabled = true;
                display.ReloadStatusImage.fillAmount = 0;
                display.WeaponIcon.sprite = weapon.Icon;
                display.TurretStanceIcon.enabled = false;
                display.AmmoText.enabled = weapon.Magazine != null;
                display.RectTransform.anchoredPosition = new Vector3(startX, 40, 0);
                display.SelectionIcon.enabled = false;
                startX += 115;
                launcherDisplays.Add(display);
            }
            foreach (var weapon in PlayerWeaponsProperty.TurretGroups)
            {
                var display = Instantiate(weaponDisplayPrefab, canvas.transform);
                display.HeatStatusImage.enabled = weapon.Heatsink != null;
                display.ReloadStatusImage.enabled = true;
                display.ReloadStatusImage.fillAmount = 0;
                display.WeaponIcon.sprite = weapon.Icon;
                display.AmmoText.enabled = false;
                display.TurretStanceIcon.sprite = turretStanceIcons[weapon.Turrets[0].CurrentStanceIndex];
                display.RectTransform.anchoredPosition = new Vector3(startX, 40, 0);
                display.SelectionIcon.enabled = false;
                startX += 115;
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
            healthText.text = $"{(int)player.CurrentHP}/{(int)player.MaxHP}";
        }

        public void UpdateSpeedBar()
        {
            speedBar.fillAmount = Mathf.Max(0, player.LinearVelocity.magnitude / player.MaxVelocityMagnitude);
            speedText.text = $"{(int)player.LinearVelocity.magnitude}/{(int)player.MaxVelocityMagnitude}";
        }

        public void UpdateSignatureDisplay()
        {
            var percentage = player.CurrentSignature / (player.MaxSignature + Mathf.Epsilon);
            signatureText.text = $"{(int)player.CurrentSignature}/{(int)player.MaxSignature}";
            signatureDisplay.rectTransform.localScale = percentage * maxSignatureDisplay.rectTransform.localScale;
        }

        public void UpdateWeaponDisplays()
        {
            for (int i = 0; i < PlayerWeaponsProperty.TurretGroups.Count; i++)
            {
                turretDisplays[i].ReloadStatusImage.fillAmount = PlayerWeaponsProperty.TurretGroups[i].ReloadStatus;
                if (turretDisplays[i].HeatStatusImage.enabled)
                {
                    turretDisplays[i].HeatStatusImage.fillAmount = PlayerWeaponsProperty.TurretGroups[i].Heatsink.OverheatPercentage;
                    if (PlayerWeaponsProperty.TurretGroups[i].Heatsink.Overheated)
                    {
                        turretDisplays[i].HeatStatusImage.color = Color.red;
                    }
                    else
                    {
                        turretDisplays[i].HeatStatusImage.color = Color.Lerp(
                            Color.white, Color.red, PlayerWeaponsProperty.TurretGroups[i].Heatsink.
                            OverheatPercentage);
                    }
                }
            }
            for (int i = 0; i < launcherDisplays.Count; i++)
            {
                if (launcherDisplays[i].AmmoText.enabled)
                {
                    launcherDisplays[i].AmmoText.text = PlayerWeaponsProperty.LauncherGroups[i].Magazine.AmmoCount.ToString();
                }
                launcherDisplays[i].ReloadStatusImage.fillAmount = PlayerWeaponsProperty.LauncherGroups[i].ReloadStatus;
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
