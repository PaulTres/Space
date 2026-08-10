using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Player
{
    public class WeaponDisplay : MonoBehaviour
    {
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
