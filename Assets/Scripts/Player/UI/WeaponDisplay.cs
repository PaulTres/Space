using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Player
{
    public class WeaponDisplay : MonoBehaviour
    {
        [Tooltip("This will be enabled when the weapon is selected.")]
        public Image SelectionIcon;
        public Image WeaponIcon;
        public Image TurretStanceIcon;
        public Image HeatStatusImage;
        public Image ReloadStatusImage;
        public TextMeshProUGUI AmmoText;
        protected RectTransform rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            }
        }
    }
}
