using Player;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Weapons;

namespace Radar
{
    public class PlayerTargetSystem : Singleton<PlayerTargetSystem>
    {
        #region Fields
        [SerializeField] protected Camera cam;
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
        protected readonly Dictionary<ImprovedMonoBehaviour, BoundingBox> boundingBoxes = new();
        [Header("Target selection/info")]
        [SerializeField] protected Image secondView;
        [SerializeField] protected Camera secondViewCamera;
        [SerializeField] protected TextMeshProUGUI targetInfoText;
        protected ImprovedMonoBehaviour currentTarget;
        public ImprovedMonoBehaviour CurrentTarget
        {
            get
            {
                return currentTarget.Active() ? currentTarget : null;
            }
            set
            {
                if (currentTarget != value)
                {
                    if (value != null && !value.gameObject.activeSelf) value = null;
                    currentTarget = value;
                    if (secondView != null) secondView.gameObject.SetActive(value != null);
                    if (secondViewCamera != null) secondViewCamera.gameObject.SetActive(value != null);
                    if (targetInfoText != null) targetInfoText.gameObject.SetActive(value != null);
                }
            }
        }
        protected PlayerShip owner;
        #endregion

        #region Setup
        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            if (cam == null) cam = GetComponentInChildren<Camera>();
            owner = GetComponentInParent<PlayerShip>();
            //if (secondViewCamera != null && secondView != null)
            //{
            //    secondViewCamera.targetTexture = (RenderTexture)secondView.mainTexture;
            //}
        }

        protected virtual void OnEnable()
        {
            CurrentTarget = null;
            GlobalUpdater.TryGetInstance(true).RegisterLateUpdate(PerformLateUpdate);
            foreach (var unit in UnitManager.GetAllShips())
            {
                RegisterTarget(unit);
            }
        }

        protected void OnDisable()
        {
            ClearTarget();
            boundingBoxes.Clear();
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterLateUpdate(PerformLateUpdate);
        }
        #endregion

        #region Functionality
        protected void PerformLateUpdate(float deltaTime)
        {
            foreach (var (target, bbox) in boundingBoxes)
            {
                ProcessTarget(target, bbox);
            }
            UpdateCurrentTargetInfo();
        }

        #region Bounding boxes
        /// <summary>
        /// Handles bounding box and other UI aspects of objects.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="bbox"></param>
        protected void ProcessTarget(ImprovedMonoBehaviour target, BoundingBox bbox)
        {
            if (!target.Active() || bbox == null) return;

            var unit = target as Unit;
            var directionToTarget = target.Position - owner.Position;
            var distance = directionToTarget.magnitude;
            Vector3 viewportPos = cam.WorldToViewportPoint(target.Position);
            if (viewportPos.z <= 0 || Vector3.Dot(cam.transform.forward, directionToTarget) <= 0)
            {
                bbox.Hide();
                if (CurrentTarget == target) ClearTarget();
            }
            else
            {
                bbox.Show();

                #region Bounding box position
                Vector2 screenPos = new(
                    viewportPos.x * Screen.width,
                    viewportPos.y * Screen.height
                );

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    boundingBoxParent,
                    screenPos,
                    null,//CameraTransform,
                    out Vector2 localPos
                );

                //Debug.Log($"{worldPos}-:-{viewportPos}-:-{localPos}");
                bbox.RectTransform.localPosition = localPos;
                #endregion

                #region Scaling
                //scale based on distance
                float distanceToCamera = Vector3.Distance(cam.transform.position, target.Position);
                var size = Mathf.Clamp(GlobalConfig.IconSizeCoefficient / (distanceToCamera + 0.00001f),
                    minScale, maxScale);
                bbox.RectTransform.localScale = Vector3.one * size;
                #endregion

                #region Distance text
                if (distance < 10) bbox.DistanceText.text = distance.ToString("#.##");
                else bbox.DistanceText.text = ((int)distance).ToString();
                if (distance <= owner.ScanRange + target.CurrentSignature)
                {
                    bbox.BBox.material = bbox.DistanceText.material = bbox.RegularMaterial;
                }
                else bbox.BBox.material = bbox.DistanceText.material = bbox.TransluscentMaterial;
                #endregion

                #region Fleeing
                if (unit != null)
                {
                    if (unit.IsFleeing)
                    {
                        //flicker the bounding box and distance text
                        if (Time.time - bbox.TimeSinceLastFlicker > GlobalConfig.FlickerInterval)
                        {
                            bbox.BBox.enabled = !bbox.BBox.enabled;
                            bbox.DistanceText.enabled = !bbox.DistanceText.enabled;
                            bbox.TimeSinceLastFlicker = Time.time;
                        }
                        bbox.LineRenderer.enabled = false;
                    }
                    else
                    {
                        bbox.BBox.enabled = true;
                        bbox.DistanceText.enabled = true;
                    }
                }
                #endregion
            }

            if (unit != null && unit.Target == (ImprovedMonoBehaviour)GameManager.Player &&
                GameManager.Player != null)
            {
                bbox.LineRenderer.enabled = true;
                bbox.LineRenderer.SetPosition(0, unit.Position);
                bbox.LineRenderer.SetPosition(1, GameManager.Player.Position);
                if (distance > unit.ScanRange + GameManager.Player.CurrentSignature)
                {
                    bbox.LineRenderer.material = bbox.DottedLineMaterial;
                    bbox.LineRenderer.material.mainTextureScale =
                        new Vector2(distance / 2, 1f);
                }
                else bbox.LineRenderer.material = bbox.RegularLineMaterial;
            }
            else bbox.LineRenderer.enabled = false;
        }

        protected BoundingBox GetBoundingBox()
        {
            if (boundingBoxPool.Count == 0)
            {
                return Instantiate(boundingBoxPrefab, boundingBoxParent);
            }
            return boundingBoxPool.Pop();
        }

        public void RegisterTarget(ImprovedMonoBehaviour target)
        {
            if (!target.Active() || target.Transform == transform.root ||
                boundingBoxes.ContainsKey(target)) return;

            var bbox = GetBoundingBox();
            bbox.SelectionIcon.gameObject.SetActive(false);
            if (target is Missile) bbox.BBox.sprite = missileIcon;
            else bbox.BBox.sprite = unitIcon;
            bbox.TimeSinceLastFlicker = Time.time;

            if (target.Team == GlobalConfig.PlayerTeam)
            {
                bbox.RegularMaterial = allyMaterial;
                bbox.TransluscentMaterial = allyTransluscentMaterial;
            }
            else if (target.Team == GlobalConfig.EnemyTeam)
            {
                bbox.RegularMaterial = enemyMaterial;
                bbox.TransluscentMaterial = enemyTransluscentMaterial;
            }
            else
            {
                bbox.RegularMaterial = neutralMaterial;
                bbox.TransluscentMaterial = neutralTransluscentMaterial;
            }

            bbox.BBox.SetAllDirty();
            bbox.gameObject.SetActive(true);
            boundingBoxes.Add(target, bbox);
        }

        public void UnregisterTarget(ImprovedMonoBehaviour target)
        {
            if (boundingBoxes.TryGetValue(target, out var bbox))
            {
                if (CurrentTarget == target)
                {
                    ClearTarget();
                }
                bbox.gameObject.SetActive(false);
                boundingBoxes.Remove(target);
                boundingBoxPool.Push(bbox);
            }
        }
        #endregion

        #region Current target
        protected void UpdateCurrentTargetInfo()
        {
            if (!CurrentTarget.Active())
            {
                SelectTarget();
                return;
            }

            #region Target view
            if (secondViewCamera != null && secondView != null)
            {
                var direction = (transform.root.position -
                    CurrentTarget.Position).normalized;
                if (direction.magnitude < CurrentTarget.EffectScaleFactor * 2)
                {
                    secondViewCamera.gameObject.SetActive(false);
                    secondView.gameObject.SetActive(false);
                }
                else
                {
                    secondViewCamera.gameObject.SetActive(true);
                    secondView.gameObject.SetActive(true);
                    secondViewCamera.transform.position = CurrentTarget.Position +
                        direction * Mathf.Max(2, CurrentTarget.EffectScaleFactor * 2);
                    secondViewCamera.transform.LookAt(CurrentTarget.Position);
                }
            }
            #endregion

            #region Text
            if (targetInfoText != null)
            {
                string enemyTarget = CurrentTarget.Target == null ? "None" :
                    CurrentTarget == GameManager.Player ? "You" :
                    CurrentTarget.Target.Transform.name.Replace("(Clone)", "");
                targetInfoText.text = $"{CurrentTarget.name.Replace("(Clone)", "")}:\nTarget: {enemyTarget}";
            }
            #endregion
        }

        public void SelectTarget()
        {
            if (GameManager.Player == null || Time.timeScale == 0) return;
            var targets = GameManager.Player.GetTargets();
            ImprovedMonoBehaviour bestCandidate = null;
            float bestScore = 0;
            foreach (var target in targets)
            {
                //compute a score for all of these
                if (!target.Active() || target.IsEqual(CurrentTarget)) continue;
                var score = EvaluateTarget(target);
                if (score > bestScore)
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
        protected void TargetSelected(ImprovedMonoBehaviour unit)
        {
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
        public float EvaluateTarget(ImprovedMonoBehaviour target)
        {
            if (!GameManager.Player.Active() || !target.Active()) return float.MinValue;

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