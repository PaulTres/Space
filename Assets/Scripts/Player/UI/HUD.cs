using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Weapons;
namespace Player
{
    public class WeaponDisplay : MonoBehaviour
    {
        public Image WeaponSprite;
        public Text WeaponButtonText;
    }
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
        [SerializeField] protected Color weaponReadyColor = Color.green;
        [SerializeField] protected Color weaponNotReadyColor = Color.red;
        [SerializeField] protected List<MissileLauncher> missileLaunchers = new();
        protected Dictionary<WeaponBase, WeaponDisplay> weaponDisplays = new();
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            player = GetComponentInParent<PlayerShip>();
            healthBar.fillAmount = 1;
            GlobalUpdater.TryGetInstance(true).RegisterLateUpdate(PerformLateUpdate);
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

        /// <summary>
        /// Update a weapon's readiness, stance, heat status etc.
        /// </summary>
        /// <param name="weapon"></param>
        public void UpdateWeaponDisplay(Turret turret)
        {

        }

        /// <summary>
        /// Update a weapon's readiness, stance, heat status etc.
        /// </summary>
        /// <param name="weapon"></param>
        public void UpdateWeaponDisplay(WeaponBase weapon)
        {
            var display = weaponDisplays[weapon];
            display.WeaponSprite.color = weapon.CanShoot() ? weaponReadyColor : weaponNotReadyColor;
        }
        #endregion
    }
}
