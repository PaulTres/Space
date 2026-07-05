using System.Collections.Generic;
using Timers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Radar
{
    public class ObjectDisplay : MonoBehaviour
    {
        [SerializeField] protected Image boundingBoxPrefab;
        protected Stack<GameObject> boundingBoxPool { get; } = new();
        protected readonly struct ObjectUIData
        {
            public readonly RectTransform RectTransform;
            public Image BoundingBox { get; }
            public Image Icon { get; }
            public readonly IObject Object;
            public readonly TextMeshProUGUI DistanceText;
            public readonly GameObject GameObject { get => RectTransform.gameObject; }
            public ObjectUIData(Image boundingBox, Image icon, IObject @object, TextMeshProUGUI text)
            {
                RectTransform = boundingBox.rectTransform;
                BoundingBox = boundingBox;
                Icon = icon;
                Object = @object;
                DistanceText = text;
            }
        }
        protected readonly Dictionary<IObject, GameObject> hudImages = new();
        [SerializeField] protected RectTransform boundingBoxParent;
        [SerializeField] protected float minTextScale, maxTextScale;
        /// <summary>
        /// This timer determines how often the bounding boxes are reordered.
        /// </summary>
        protected readonly CountdownTimer reorderTimer = new(GlobalConfig.UIUpdateCooldown);

        protected void OnDisable()
        {
            hudImages.Clear();
        }

        protected void OnDestroy()
        {
            boundingBoxPool.Clear();
            reorderTimer.Dispose();
        }

        public void RegisterTarget(IObject target)
        {

        }

        public void UnregisterTarget(IObject target)
        {

        }

        public void ProcessTarget(IObject target)
        {

        }
    }
}