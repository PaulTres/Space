using Player;
using System.Collections.Generic;
using System.Linq;
using Timers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities;

namespace Radar
{
    public class PlayerTargetSystem : Singleton<PlayerTargetSystem>
    {
        #region Fields
        [SerializeField] protected Camera cam;
        protected EventSystem eventSystem;
        [Header("Object tracking")]
        [SerializeField] protected BoundingBox boundingBoxPrefab;
        [SerializeField] protected Sprite unitIcon;
        [SerializeField] protected Sprite missileIcon;
        [SerializeField] protected RectTransform boundingBoxParent;
        [SerializeField] protected float minScale, maxScale;
        [Header("Materials")]
        [SerializeField] protected Material enemyMaterial;
        [SerializeField] protected Material enemyTransluscentMaterial;
        [SerializeField] protected Material allyMaterial;
        [SerializeField] protected Material allyTransluscentMaterial;
        [SerializeField] protected Material neutralMaterial;
        [SerializeField] protected Material neutralTransluscentMaterial;
        protected Stack<BoundingBox> boundingBoxPool { get; } = new();
        protected readonly Dictionary<IObject, BoundingBox> boundingBoxes = new();
        protected readonly CountdownTimer reorderTimer = new(GlobalConfig.UIUpdateCooldown);
        [Header("Target selection/info")]
        [SerializeField] protected Image secondView;
        [SerializeField] protected Camera secondViewCamera;
        [SerializeField] protected TextMeshProUGUI targetInfoText;
        /// <summary>
        /// This timer determines how often the bounding boxes are reordered.
        /// </summary>
        protected Unit currentTarget;
        public Unit CurrentTarget
        {
            get
            {
                if (currentTarget == null || !currentTarget.gameObject.activeSelf)
                    return null;
                return currentTarget;
            }
            set
            {
                if (currentTarget != value)
                {
                    if (value != null && !value.gameObject.activeSelf) value = null;
                    currentTarget = value;
                    if (secondView != null) secondView.gameObject.
                            SetActive(value != null);
                    if (secondViewCamera != null) secondViewCamera.gameObject.
                            SetActive(value != null);
                }
            }
        }
        protected PointerEventData pointerEventData;
        protected PlayerShip owner;
        #endregion

        #region Setup
        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            if (eventSystem != null) eventSystem = EventSystem.current;
            if (cam == null) cam = GetComponentInChildren<Camera>();
            owner = GetComponentInParent<PlayerShip>();
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
                    minScale, maxScale);
                bbox.RectTransform.localScale = Vector3.one * size;
                #endregion

                var distance = Vector3.Distance(owner.Position, target.Position);
                if (distance <= owner.ScanRange + target.Signature)
                {
                    bbox.BBox.material = bbox.RegularMaterial;
                }
                else bbox.BBox.material = bbox.TransluscentMaterial;
            }
            UpdateTargetInfo();
        }

        protected void UpdateTargetInfo()
        {
            if (!CurrentTarget.Active()) return;
            #region Target view
            if (secondViewCamera != null && secondView != null)
            {
                var direction = (transform.root.position -
                    CurrentTarget.Position).normalized;
                if (direction.magnitude < CurrentTarget.EffectScaleFactor)
                {
                    secondViewCamera.gameObject.SetActive(false);
                    secondView.gameObject.SetActive(false);
                }
                secondViewCamera.gameObject.SetActive(true);
                secondView.gameObject.SetActive(true);
                secondViewCamera.transform.position = CurrentTarget.Position +
                    direction * Mathf.Max(2, CurrentTarget.EffectScaleFactor);
                secondViewCamera.transform.LookAt(CurrentTarget.Position);
            }
            #endregion

            #region Text
            if (targetInfoText != null)
            {
                string enemyTarget = CurrentTarget.Target == null ? "None" :
                    CurrentTarget == GameManager.Player ? "You" :
                    CurrentTarget.Target.Transform.name.Replace("(Clone)", "");
                targetInfoText.text = $"{CurrentTarget.name}: " +
                    $"{Vector3.Distance(transform.root.position, CurrentTarget.Position)}" +
                    $"\nTarget: {enemyTarget}";
            }
            #endregion
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
            if (!target.Active || target.Transform == transform.root || boundingBoxes.ContainsKey(target)) return;
            var bbox = GetBoundingBox();
            bbox.SelectionIcon.gameObject.SetActive(false);
            if (target is Unit) bbox.BBox.sprite = unitIcon;
            else bbox.BBox.sprite = missileIcon;
            var team = GlobalConfig.GetTeam(target.Transform.gameObject.layer);
            if (team == GlobalConfig.PlayerTeam)
            {
                bbox.RegularMaterial = allyMaterial;
                bbox.TransluscentMaterial = allyTransluscentMaterial;
            }
            else if (team == GlobalConfig.EnemyTeam)
            {
                bbox.RegularMaterial = enemyMaterial;
                bbox.TransluscentMaterial = enemyTransluscentMaterial;
            }
            else
            {
                bbox.RegularMaterial = neutralMaterial;
                bbox.TransluscentMaterial = neutralTransluscentMaterial;
            }
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
                if (target == null || !target.Transform.gameObject.activeSelf || target.IsEqual(CurrentTarget)) continue;
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