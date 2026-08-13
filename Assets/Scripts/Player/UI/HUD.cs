using Radar;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Weapons;
namespace Player
{
    public class HUD : UISystem
    {
        [System.Serializable]
        public class TurretGroup
        {
            public Sprite Icon;
            public List<Turret> Turrets = new();
            public Heatsink Heatsink;
            [Tooltip("If true, will display when the weapon is not ready to fire.")]
            public bool ShowReload = true;
            /// <summary>
            /// 0 - fully reloaded<br/>
            /// 1 - just fired<br/>
            /// </summary>
            public float ReloadStatus
            {
                get
                {
                    float s = 0;
                    for (int i = 0; i < Turrets.Count; i++) s += Turrets[i].ReloadStatus;
                    return s / Turrets.Count;
                }
            }
        }

        [System.Serializable]
        public class MissileGroup
        {
            public Sprite Icon;
            public List<MissileLauncher> Launchers = new();
            public Magazine Magazine;
            /// <summary>
            /// 0 - fully reloaded<br/>
            /// 1 - just fired<br/>
            /// </summary>
            public float ReloadStatus
            {
                get
                {
                    float s = 0;
                    for (int i = 0; i < Launchers.Count; i++) s += Launchers[i].ReloadStatus;
                    return s / Launchers.Count;
                }
            }
        }

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
        [SerializeField] protected List<TurretGroup> turretGroups = new();
        [SerializeField] protected List<MissileGroup> launcherGroups = new();
        [SerializeField] protected WeaponDisplay weaponDisplayPrefab;
        protected List<WeaponDisplay> turretDisplays = new(), launcherDisplays = new();
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            canvas = transform.root.GetComponentInChildren<Canvas>();
            player = GetComponentInParent<PlayerShip>();
            healthBar.fillAmount = 1;
            GlobalUpdater.TryGetInstance(true).RegisterLateUpdate(PerformLateUpdate);
            var count = turretGroups.Count + launcherGroups.Count;
            var totalSpace = 50 * count + (count - 1) * 10;
            var startX = -totalSpace / 2;
            foreach (var weapon in launcherGroups)
            {
                var display = Instantiate(weaponDisplayPrefab, canvas.transform);
                display.HeatStatusImage.enabled = false;
                display.ReloadStatusImage.enabled = true;
                display.ReloadStatusImage.fillAmount = 0;
                display.WeaponIcon.sprite = weapon.Icon;
                display.TurretStanceIcon.enabled = false;
                display.AmmoText.enabled = weapon.Magazine != null;
                display.RectTransform.anchoredPosition = new Vector3(startX, 40, 0);
                startX += 115;
                launcherDisplays.Add(display);
            }
            foreach (var weapon in turretGroups)
            {
                var display = Instantiate(weaponDisplayPrefab, canvas.transform);
                display.HeatStatusImage.enabled = weapon.Heatsink != null;
                display.ReloadStatusImage.enabled = weapon.ShowReload;
                display.ReloadStatusImage.fillAmount = 0;
                display.WeaponIcon.sprite = weapon.Icon;
                display.AmmoText.enabled = false;
                display.TurretStanceIcon.sprite = turretStanceIcons[weapon.Turrets[0].CurrentStanceIndex];
                display.RectTransform.anchoredPosition = new Vector3(startX, 40, 0);
                startX += 115;
                turretDisplays.Add(display);
            }
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
            for (int i = 0; i < turretGroups.Count; i++)
            {
                if (turretGroups[i].ShowReload)
                {
                    turretDisplays[i].ReloadStatusImage.fillAmount = turretGroups[i].ReloadStatus;
                    //turretDisplays[i].WeaponIcon.color = Color.Lerp(Color.green, Color.red, turretGroups[i].ReloadStatus);
                }
                if (turretDisplays[i].HeatStatusImage.enabled)
                {
                    turretDisplays[i].HeatStatusImage.fillAmount = turretGroups[i].Heatsink.OverheatPercentage;
                    turretDisplays[i].HeatStatusImage.color = Color.Lerp(Color.white, Color.red, turretGroups[i].Heatsink.OverheatPercentage);
                }
            }
            for (int i = 0; i < launcherDisplays.Count; i++)
            {
                if (launcherDisplays[i].AmmoText.enabled)
                {
                    launcherDisplays[i].AmmoText.text = launcherGroups[i].Magazine.AmmoCount.ToString();
                }
                launcherDisplays[i].ReloadStatusImage.fillAmount = launcherGroups[i].ReloadStatus;
            }
        }

        public void ExecuteAction(int index)
        {
            if (index < 0) return;
            if (index < launcherDisplays.Count)
            {
                FireMissiles(index);
            }
            else CycleTurretStance(index - turretDisplays.Count);

        }

        public void FireMissiles(int missileGroup)
        {
            var t = PlayerTargetSystem.TryGetInstance();
            if (t == null || !t.CurrentTarget.Active()) return;
            foreach (var launcher in launcherGroups[missileGroup].Launchers)
            {
                launcher.Shoot(t.CurrentTarget);
            }
        }

        public void CycleTurretStance(int groupIndex)
        {
            foreach (var turret in turretGroups[groupIndex].Turrets)
            {
                turret.CycleStance();
            }
            turretDisplays[groupIndex].TurretStanceIcon.sprite = turretStanceIcons[turretGroups[groupIndex].Turrets[0].CurrentStanceIndex];
        }
        #endregion
    }
}
