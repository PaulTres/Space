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
        [field: SerializeField] public TextMeshProUGUI PriorityText { get; protected set; }
        [field: SerializeField] public LineRenderer LineRenderer { get; protected set; }
        public float TimeSinceLastFlicker { get; set; }
        public bool Hidden => !BBox.gameObject.activeSelf;
        [SerializeField] protected Gradient lineRendererColors;

        /// <summary>
        /// Activates the main bounding box object - the child of this object.
        /// </summary>
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
            //Debugging.Log(bbox.BBox.enabled);
            BBox.enabled = !toggle;
            //Debugging.Log(bbox.BBox.enabled);
            DistanceText.enabled = !toggle;
            TimeSinceLastFlicker = Time.time;
        }

        /// <summary>
        /// Deactivates the main bounding box object - the child of this object.
        /// </summary>
        public void Hide()
        {
            //BBox.enabled = false;
            //SelectionIcon.enabled = false;
            //DistanceText.enabled = false;
            BBox.gameObject.SetActive(false);
        }

        public void SetLineRendererTransparency(float value)
        {
            //Debug.Log(value);
            LineRenderer.startColor = LineRenderer.endColor = lineRendererColors.Evaluate(value);
        }
    }
}