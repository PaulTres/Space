using System.Collections.Generic;
using System.Linq;
using Timers;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;
namespace Radar
{
    public class PlayerTargetSystem : Singleton<PlayerTargetSystem>
    {
        protected Camera cam;
        [SerializeField] protected BoundingBox boundingBoxPrefab;
        protected Stack<BoundingBox> boundingBoxPool { get; } = new();

        protected readonly Dictionary<IObject, BoundingBox> boundingBoxes = new();
        [SerializeField] protected RectTransform boundingBoxParent;
        [SerializeField] protected float minTextScale, maxTextScale;
        /// <summary>
        /// This timer determines how often the bounding boxes are reordered.
        /// </summary>
        protected readonly CountdownTimer reorderTimer = new(GlobalConfig.UIUpdateCooldown);
        protected EventSystem eventSystem;

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            cam = GetComponentInChildren<Camera>();
        }

        protected virtual void OnEnable()
        {
            if (eventSystem != null) eventSystem = EventSystem.current;
            reorderTimer.OnTimerStop += () =>
            {
                ReorderBoundingBoxes();
                reorderTimer.Start();
            };
            reorderTimer.Start();
            GlobalUpdater.TryGetInstance(true).RegisterLateUpdate(PerformLateUpdate);
        }

        protected void OnDisable()
        {
            boundingBoxes.Clear();
            reorderTimer.OnTimerStop = null;
            reorderTimer.Stop();
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterLateUpdate(PerformLateUpdate);
        }

        protected override void ClearSingleton()
        {
            base.ClearSingleton();
            reorderTimer.Dispose();
        }

        protected void PerformLateUpdate(float deltaTime)
        {
            foreach (var (target, bbox) in boundingBoxes)
            {
                if (target == null || !target.Active) continue;

                Vector3 worldPos = target.Position;

                Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);
                if (viewportPos.z <= 0)
                {
                    bbox.gameObject.SetActive(false);
                    return;
                }
                bbox.gameObject.SetActive(true);

                Vector2 screenPos = new(
                    viewportPos.x * Screen.width,
                    viewportPos.y * Screen.height
                );

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    boundingBoxParent,
                    screenPos,
                    null,//cam,
                    out Vector2 localPos
                );

                //Debug.Log($"{worldPos}-:-{viewportPos}-:-{localPos}");
                bbox.RectTransform.localPosition = localPos;

                #region Scaling
                //scale based on distance
                float dist = Vector3.Distance(cam.transform.position, worldPos);
                var size = Mathf.Clamp(GlobalConfig.IconSizeCoefficient / (dist + 0.00001f),
                    minTextScale, maxTextScale);
                bbox.RectTransform.localScale = Vector3.one * size;
                #endregion

                #region Text
                dist = Vector3.Distance(cam.transform.root.position, worldPos);
                if (GameManager.Player != null && target.Target.GetInstanceID() ==
                    GameManager.Player.GetInstanceID())
                {
                    bbox.DistanceText.text = $"<color=red>{dist:0.0}";
                }
                else bbox.DistanceText.text = $"<color=blue>{dist:0.0}";
                #endregion
            }
        }

        protected void ReorderBoundingBoxes()
        {
            var list = boundingBoxes.ToList();
            list.Sort((a, b) =>
            {
                float distA = Vector3.Distance(cam.transform.position, a.Key.Position);
                float distB = Vector3.Distance(cam.transform.position, b.Key.Position);
                return distB.CompareTo(distA);
            });
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Value.RectTransform.SetSiblingIndex(i);
            }
        }

        protected BoundingBox GetBoundingBox()
        {
            if (boundingBoxPool.Count == 0)
            {
                return Instantiate(boundingBoxPrefab, boundingBoxParent);
            }
            return boundingBoxPool.Pop();
        }

        public void RegisterTarget(IObject target)
        {
            //Debug.Log(target.Transform.name);
            if (boundingBoxes.ContainsKey(target)) return;
            var bbox = GetBoundingBox();
            //Debug.Log(bbox.gameObject.name);
            bbox.Icon.sprite = GlobalConfig.GetIcon(target.ObjectType);
            var (color, _) = GlobalConfig.GetTeamColor(target.Transform.gameObject.layer);
            bbox.Icon.color = color;
            bbox.BBox.color = color;
            bbox.DistanceText.color = color;
            bbox.gameObject.SetActive(true);
            boundingBoxes.Add(target, bbox);
        }

        public void UnregisterTarget(IObject target)
        {
            if (boundingBoxes.TryGetValue(target, out var bbox))
            {
                boundingBoxes.Remove(target);
                bbox.gameObject.SetActive(false);
                boundingBoxPool.Push(bbox);
            }
        }
    }
}