using UnityEngine;
using UnityEngine.UI;

namespace Radar
{
    public class BoundingBox : MonoBehaviour
    {
        public Material RegularMaterial { get; set; }
        public Material TransluscentMaterial { get; set; }
        protected RectTransform rectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            }
        }
        [field: SerializeField] public Image SelectionIcon { get; protected set; }
        [field: SerializeField] public Image BBox { get; protected set; }
    }
}