using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Radar
{
    public class BoundingBox : MonoBehaviour
    {
        protected RectTransform rectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            }
        }
        [field: SerializeField] public Image Icon { get; protected set; }
        [field: SerializeField] public Image SelectionIcon { get; protected set; }
        [field: SerializeField] public Image BBox { get; protected set; }
        [field: SerializeField] public TextMeshProUGUI DistanceText { get; protected set; }
    }
}