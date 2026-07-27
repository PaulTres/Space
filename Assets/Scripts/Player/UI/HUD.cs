using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
namespace Player
{
    public class HUD : UISystem
    {
        [SerializeField] protected Image healthBar;
        [SerializeField] protected TextMeshProUGUI healthText;
        [SerializeField] protected Image speedBar;
        [SerializeField] protected TextMeshProUGUI speedText;
        [SerializeField] protected Image maxSignatureDisplay, signatureDisplay;
        [SerializeField] protected TextMeshProUGUI signatureText;
        protected PlayerShip player;

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

        protected void PerformLateUpdate(float deltaTime)
        {
            healthBar.fillAmount = Mathf.Max(0, player.CurrentHPPercentage);
            healthText.text = $"{(int)player.CurrentHP}/{(int)player.MaxHP}";

            speedBar.fillAmount = Mathf.Max(0, player.LinearVelocity.magnitude / player.MaxVelocityMagnitude);
            speedText.text = $"{(int)player.LinearVelocity.magnitude}/{(int)player.MaxVelocityMagnitude}";

            #region Signature display
            var percentage = player.Signature / (player.MaxSignature + Mathf.Epsilon);
            signatureText.text = $"{(int)player.Signature}/{(int)player.MaxSignature}";
            signatureDisplay.rectTransform.localScale = percentage *
                maxSignatureDisplay.rectTransform.localScale;
            #endregion
        }
    }
}
