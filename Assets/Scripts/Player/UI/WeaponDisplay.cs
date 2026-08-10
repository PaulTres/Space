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
