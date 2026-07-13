using System.Collections.Generic;
using System.Linq;
using Timers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities;

namespace Radar
{
    public class PlayerTargetSystem : Singleton<PlayerTargetSystem>
    {
        #region Fields
        protected Camera cam;
        [SerializeField] protected BoundingBox boundingBoxPrefab;
        protected Stack<BoundingBox> boundingBoxPool { get; } = new();

        protected readonly Dictionary<IObject, BoundingBox> boundingBoxes = new();
        [SerializeField] protected RectTransform boundingBoxParent;
        [SerializeField] protected float minTextScale, maxTextScale;
        [SerializeField] protected Image secondView;
        [SerializeField] protected Camera secondViewCamera;
        /// <summary>
        /// This timer determines how often the bounding boxes are reordered.
        /// </summary>
        protected readonly CountdownTimer reorderTimer = new(GlobalConfig.UIUpdateCooldown);
        protected EventSystem eventSystem;
        protected Unit currentTarget;
        public Unit CurrentTarget
        {
            get
            {
                if (currentTarget == null || !currentTarget.gameObject.activeSelf) return null;
                return currentTarget;
            }
            set
            {
                if (currentTarget != value)
                {
                    if (value != null && !value.gameObject.activeSelf) value = null;
                    currentTarget = value;
                    if (secondView != null) secondView.gameObject.SetActive(value != null);
                    if (secondViewCamera != null) secondViewCamera.gameObject.SetActive(value != null);
                }
            }
        }
        protected PointerEventData pointerEventData;
        #endregion

        #region Setup
        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            if (eventSystem != null) eventSystem = EventSystem.current;
            cam = GetComponentInChildren<Camera>();
            pointerEventData = new PointerEventData(eventSystem)
            {
                position = new Vector2(Screen.width / 2, Screen.height / 2)
            };
            if (secondViewCamera != null && secondView != null)
            {
                secondViewCamera.targetTexture = (RenderTexture)secondView.mainTexture;
            }
        }

        protected virtual void OnEnable()
        {
            CurrentTarget = null;

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
            ClearTarget();
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
        #endregion

        #region Functionality
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
                if (GameManager.Player != null && target.Target.
                    IsEqual(GameManager.Player))
                {
                    bbox.DistanceText.text = $"<color=red>{dist:0.0}";
                }
                else bbox.DistanceText.text = $"<color=blue>{dist:0.0}";
                #endregion
            }
            UpdateSecondView();
        }

        protected void UpdateSecondView()
        {
            if (!CurrentTarget.Active()) return;
            if (secondViewCamera == null || secondView == null) return;
            var direction = (transform.root.position - CurrentTarget.Position).normalized;
            if (direction.magnitude < CurrentTarget.EffectScaleFactor)
            {
                secondViewCamera.gameObject.SetActive(false);
                secondView.gameObject.SetActive(false);
            }
            secondViewCamera.gameObject.SetActive(true);
            secondView.gameObject.SetActive(true);
            //Debug.DrawLine(Vector3.zero, transform.root.position, Color.green);
            //Debug.DrawRay(CurrentTarget.Position, direction * Mathf.Max(2, CurrentTarget.EffectScaleFactor));
            secondViewCamera.transform.position = CurrentTarget.Position + direction * Mathf.Max(2, CurrentTarget.EffectScaleFactor);
            //Debug.Log(Vector3.Distance(secondViewCamera.transform.position, CurrentTarget.Position));
            secondViewCamera.transform.LookAt(CurrentTarget.Position);
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
            bbox.SelectionIcon.gameObject.SetActive(false);
            bbox.BBox.color = color;
            bbox.DistanceText.color = color;
            bbox.gameObject.SetActive(true);
            boundingBoxes.Add(target, bbox);
        }

        public void UnregisterTarget(IObject target)
        {
            if (boundingBoxes.TryGetValue(target, out var bbox))
            {
                if (bbox.SelectionIcon.gameObject.activeSelf)
                {
                    //this was the current target
                    ClearTarget();
                }
                if (CurrentTarget != null && !CurrentTarget.gameObject.activeSelf)
                {
                    ClearTarget();
                }
                bbox.gameObject.SetActive(false);
                boundingBoxes.Remove(target);
                boundingBoxPool.Push(bbox);
            }
        }

        #region Target selection
        public void SelectTarget()
        {
            if (GameManager.Player == null) return;
            var targets = GameManager.Player.GetTargets();
            Unit bestCandidate = null;
            float bestScore = float.MinValue;
            foreach (var target in targets)
            {
                //compute a score for all of these
                if (target == null || !target.Transform.gameObject.activeSelf) continue;
                var score = EvaluateTarget(target);
                if (score > bestScore && score > 0)
                {
                    bestCandidate = target;
                    bestScore = score;
                }
            }
            TargetSelected(bestCandidate);
        }

        /// <summary>
        /// Fires when a new target has been selected.
        /// </summary>
        /// <param name="unit"></param>
        protected void TargetSelected(Unit unit)
        {
            if (GameManager.IsPaused) return;
            if (unit == CurrentTarget) return;
            if (CurrentTarget != null && boundingBoxes.TryGetValue(CurrentTarget, out var previousData))
            {
                previousData.SelectionIcon.gameObject.SetActive(false);
            }
            CurrentTarget = unit;
            if (CurrentTarget == null) return;
            if (!boundingBoxes.TryGetValue(CurrentTarget, out var data))
            {
                Debug.LogError($"Current target {unit} has no bounding box!");
                ClearTarget();
                return;
            }
            data.SelectionIcon.gameObject.SetActive(true);
        }

        /// <summary>
        /// Used to evaluate a target for selection. Takes into account distance and angle.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public float EvaluateTarget(Unit target)
        {
            if (GameManager.Player == null || !GameManager.Player.gameObject.activeSelf ||
                target == null | !target.gameObject.activeSelf) return float.MinValue;

            var direction = target.Position - GameManager.Player.Position;
            var distance = direction.magnitude;

            var angleScore = Vector3.Dot(direction.normalized, cam.transform.forward);
            if (angleScore <= 0) return 0;
            var distanceScore = (GlobalConfig.PlayerTargetMaxDistance - distance) /
                GlobalConfig.PlayerTargetMaxDistance;
            return angleScore * GlobalConfig.PlayerTargetAngleWeight + distanceScore *
                GlobalConfig.PlayerTargetDistanceWeight;
        }

        /// <summary>
        /// Clears current target and disables its selection icon.
        /// </summary>
        public void ClearTarget()
        {
            if (CurrentTarget == null) return;
            if (boundingBoxes.TryGetValue(CurrentTarget, out var data))
            {
                data.SelectionIcon.gameObject.SetActive(false);
            }
            CurrentTarget = null;
        }
        #endregion
        #endregion
    }
}