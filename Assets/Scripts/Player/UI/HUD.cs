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
        public class WeaponGroup
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

        #region Fields
        [SerializeField] protected Image healthBar;
        [SerializeField] protected TextMeshProUGUI healthText;
        [SerializeField] protected Image speedBar;
        [SerializeField] protected TextMeshProUGUI speedText;
        [SerializeField] protected Image maxSignatureDisplay, signatureDisplay;
        [SerializeField] protected TextMeshProUGUI signatureText;
        protected PlayerShip player;
        [SerializeField] protected List<Sprite> turretStanceIcons = new();
        [SerializeField] protected Color weaponReadyColor = Color.green;
        [SerializeField] protected Color weaponNotReadyColor = Color.red;
        protected Canvas canvas;
        [SerializeField] protected List<WeaponGroup> weaponGroups = new();
        [SerializeField] protected WeaponDisplay weaponDisplayPrefab;
        protected List<WeaponDisplay> weaponDisplays = new();
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            canvas = transform.root.GetComponentInChildren<Canvas>();
            player = GetComponentInParent<PlayerShip>();
            healthBar.fillAmount = 1;
            GlobalUpdater.TryGetInstance(true).RegisterLateUpdate(PerformLateUpdate);
            var totalSpace = 50 * weaponGroups.Count + (weaponGroups.Count - 1) * 10;
            var startX = -totalSpace / 2;
            foreach (var weapon in weaponGroups)
            {
                var display = Instantiate(weaponDisplayPrefab, canvas.transform);
                display.HeatStatusImage.enabled = weapon.Heatsink != null;
                display.ReloadStatusImage.fillAmount = 0;
                display.WeaponIcon.sprite = weapon.Icon;
                display.TurretStanceIcon.sprite = turretStanceIcons[weapon.Turrets[0].CurrentStanceIndex];
                display.RectTransform.anchoredPosition = new Vector3(startX, 40, 0);
                startX += 115;
                weaponDisplays.Add(display);
            }
        }

        protected void OnDisable()
        {
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
            for (int i = 0; i < weaponDisplays.Count; i++)
            {
                if (weaponGroups[i].ShowReload)
                {
                    weaponDisplays[i].ReloadStatusImage.fillAmount = weaponGroups[i].ReloadStatus;
                }
                if (weaponDisplays[i].HeatStatusImage.enabled)
                {
                    weaponDisplays[i].HeatStatusImage.fillAmount = weaponGroups[i].Heatsink.OverheatPercentage;
                    weaponDisplays[i].HeatStatusImage.color = Color.Lerp(Color.blue, Color.red, weaponGroups[i].Heatsink.OverheatPercentage);
                }
            }
        }

        public void CycleStance(int groupIndex)
        {
            foreach (var turret in weaponGroups[groupIndex].Turrets)
            {
                turret.CycleStance();
            }
            weaponDisplays[groupIndex].TurretStanceIcon.sprite = turretStanceIcons[weaponGroups[groupIndex].Turrets[0].CurrentStanceIndex];
        }
        #endregion
    }
}
