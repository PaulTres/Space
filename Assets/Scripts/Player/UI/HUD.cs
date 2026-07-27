using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
namespace Player
{
    public class HUD : UISystem
    {
        [SerializeField] protected Image healthBar;
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

            var percentage = player.Signature / player.MaxSignature;
            signatureText.text = $"{(int)player.Signature}/{(int)player.MaxSignature}";
            signatureDisplay.rectTransform.localScale = percentage *
                maxSignatureDisplay.rectTransform.localScale;
        }
    }
}
