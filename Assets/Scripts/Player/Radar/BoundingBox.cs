using TMPro;
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
        [field: SerializeField] public TextMeshProUGUI DistanceText { get; protected set; }
        [field: SerializeField] public Image BBox { get; protected set; }
        [field: SerializeField] public Material RegularLineMaterial { get; protected set; }
        [field: SerializeField] public Material DottedLineMaterial { get; protected set; }
        [field: SerializeField] public LineRenderer LineRenderer { get; protected set; }
        public float TimeSinceLastFlicker { get; set; }

        public void Show()
        {
            //BBox.color = new Color(1, 1, 1, 0);
            BBox.gameObject.SetActive(true);
            //SelectionIcon.enabled = true;
            // DistanceText.enabled = true;
        }

        public void Flicker()
        {
            var toggle = BBox.enabled;
            //Debug.Log(bbox.BBox.enabled);
            BBox.enabled = !toggle;
            //Debug.Log(bbox.BBox.enabled);
            DistanceText.enabled = !toggle;
            TimeSinceLastFlicker = Time.time;
        }

        public void Hide()
        {
            //BBox.enabled = false;
            //SelectionIcon.enabled = false;
            //DistanceText.enabled = false;
            BBox.gameObject.SetActive(false);
        }
    }
}