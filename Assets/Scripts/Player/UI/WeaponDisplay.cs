using System.Collections.Generic;
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
        public RectTransform ReloadIconsParent;
        public TextMeshProUGUI AmmoText;
        protected RectTransform rectTransform;
        [SerializeField] protected Color emptyColor = Color.white, reloadedColor = Color.orange;
        [SerializeField] protected List<Image> reloadIcons;

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            }
        }

        public void Initialize(TurretGroup turretGroup, Image reloadIconPrefab)
        {
            reloadIcons = new();
            HeatStatusImage.enabled = turretGroup.Heatsink != null;
            WeaponIcon.sprite = turretGroup.Icon;
            AmmoText.enabled = false;
            SelectionIcon.enabled = false;
            for (int i = 0; i < turretGroup.Turrets.Count; i++)
            {
                reloadIcons.Add(Instantiate(reloadIconPrefab, ReloadIconsParent));
            }
        }

        public void Initialize(MissileGroup missileGroup, Image reloadIconPrefab)
        {
            reloadIcons = new();
            HeatStatusImage.enabled = false;
            WeaponIcon.sprite = missileGroup.Icon;
            TurretStanceIcon.enabled = false;
            AmmoText.enabled = missileGroup.Magazine != null;
            SelectionIcon.enabled = false;
            for (int i = 0; i < missileGroup.Launchers.Count; i++)
            {
                reloadIcons.Add(Instantiate(reloadIconPrefab, ReloadIconsParent));
            }
            //Debug.Log("Missile display has " + reloadIcons.Count + " icons");
        }

        public void UpdateDisplay(TurretGroup turretGroup)
        {
            for (int i = 0; i < turretGroup.Turrets.Count; i++)
            {
                UpdateReloadIcon(reloadIcons[i], 1 - turretGroup.Turrets[i].ReloadStatus);
            }
            if (turretGroup.Heatsink != null)
            {
                HeatStatusImage.fillAmount = turretGroup.Heatsink.OverheatPercentage;
                if (turretGroup.Heatsink.Overheated)
                {
                    HeatStatusImage.color = Color.red;
                }
                else
                {
                    HeatStatusImage.color = Color.Lerp(Color.white, Color.red,
                        turretGroup.Heatsink.OverheatPercentage);
                }
            }
        }

        public void UpdateDisplay(MissileGroup missileGroup)
        {
            for (int i = 0; i < missileGroup.Launchers.Count; i++)
            {
                UpdateReloadIcon(reloadIcons[i], 1 - missileGroup.Launchers[i].ReloadStatus);
            }
            if (missileGroup.Magazine != null)
            {
                AmmoText.text = missileGroup.Magazine.AmmoCount.ToString();
            }
        }

        public void UpdateReloadIcon(Image image, float value)
        {
            image.fillAmount = value;
            image.color = Color.Lerp(emptyColor, reloadedColor, value);
        }
    }
}
