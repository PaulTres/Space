using AudioSystem;
using Effects;
using HP;
using HybridBT2;
using System.Collections.Generic;
using UnityEngine;
namespace Utilities
{
    public enum Directions
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Backward
    }
    public static class Extensions
    {
        #region Unit
        public static float GetMoraleWeight(this Unit unit)
        {
            if (unit == null) return 0;
            return unit.UnitType switch
            {
                Unit.Type.Skiff => 5,
                Unit.Type.Frigate => 10,
                Unit.Type.Cruiser => 20,
                _ => 0
            };
        }

        /// <summary>
        /// Returns true if the distance between a unit and its target is less 
        /// than the unit's scan range + the target's signature.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool InOptimalFiringRange(this Unit unit, ImprovedMonoBehaviour target)
        {
            if (unit == null) return false;
            return Vector3.Distance(unit.Position, target.Position) <=
                unit.ScanRange + target.CurrentSignature;
        }

        /// <summary>
        /// Returns true if the distance between a unit and a position is less 
        /// than the unit's scan range + the provided signature.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="targetPosition"></param>
        /// <param name="targetSignature"></param>
        /// <returns></returns>
        public static bool InOptimalFiringRange(this Unit unit, Vector3 targetPosition, float targetSignature)
        {
            if (unit == null) return false;
            return Vector3.Distance(targetPosition, unit.Position) <=
                unit.ScanRange + targetSignature;
        }
        #endregion

        public static void Play(this SoundData soundData, Vector3 position, bool createManager = true)
        {
            if (soundData != null)
            {
                var m = SoundManager.TryGetInstance(createManager);
                if (m != null) m.Play(soundData, position);
            }
        }

        public static void SpawnEffect(this PoolableEffect effect, Vector3 position, float scaleFactor)
        {
            SpawnEffect(effect, position, Quaternion.identity, scaleFactor);
        }

        public static void SpawnEffect(this PoolableEffect effect, Vector3 position, Quaternion rotation, float scaleFactor)
        {
            PoolableEffect.Spawn(effect, position, rotation, scaleFactor * Vector3.one);
        }

        public static void SpawnEffect(this PoolableEffect effect, Transform parent, float scaleFactor)
        {
            PoolableEffect.Spawn(effect, parent, scaleFactor * Vector3.one);
        }

        #region ImprovedMonoBehaviour      
        public static bool Active(this ImprovedMonoBehaviour @object, bool allowDying = false)
        {
            if (@object == null) return false;
            if (@object is Unit unityObj) return unityObj != null &&
                    @object.Transform.gameObject.activeSelf && (allowDying || !unityObj.IsDying);
            if (@object is Object obj) return obj != null && @object.Transform.gameObject.activeSelf;
            return @object != null && @object.Transform.gameObject.activeSelf;
        }

        public static int GetInstanceID(this ImprovedMonoBehaviour @object)
        {
            return @object.Transform.GetInstanceID();
        }

        public static bool IsEqual(this ImprovedMonoBehaviour @object, ImprovedMonoBehaviour other)
        {
            if (@object == null && other == null) return true;
            if (@object == null || other == null) return false;
            return @object.GetInstanceID() == other.GetInstanceID();
        }

        /// <summary>
        /// Returns a position with oscillation based on the distance between the object 
        /// and the scanner. The further away the object is, the more oscillation is applied. This is used to simulate radar inaccuracies.
        /// </summary>
        /// <param name="object">The object to get the position for.</param>
        /// <param name="scanner">The scanner unit.</param>
        /// <returns>The position with oscillation applied.</returns>
        public static Vector3 GetPositionWithOscilation(this ImprovedMonoBehaviour @object, Unit scanner)
        {
            var dist = Mathf.Max((scanner.Transform.position -
                @object.Transform.position).magnitude -
                @object.CurrentSignature - scanner.ScanRange, 0);
            return @object.Transform.position + Mathf.Min(GlobalConfig.
                MaxOscillationMagnitude, dist * GlobalConfig.PositionOscillationCoefficient) *
                Random.insideUnitSphere;
        }
        #endregion

        /// <summary>
        /// Returns true if the collection is null or has no elements in it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool IsEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        /// <summary>
        /// If an angle is > 180, will subtract 360. If it is < 180, will add 360.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float NormalizeAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            if (angle < -180) angle += 360f;
            return angle;
        }

        /// <summary>
        /// Returns a vector that is perpendicular to the input vector. If the input vector 
        /// is zero, returns zero. If normalize is true, returns a normalized vector.
        /// </summary>
        /// <param name="inputVector">The input vector.</param>
        /// <param name="normalize">Whether to normalize the resulting vector.</param>
        /// <returns>A vector that is perpendicular to the input vector.</returns>
        public static Vector3 Perpendicular(this Vector3 inputVector, bool normalize = true)
        {
            if (inputVector == Vector3.zero) return Vector3.zero;
            Vector3 v;
            if (inputVector.z != 0)
            {
                v = new Vector3(1, 1, -(inputVector.x + inputVector.y) / inputVector.z);
            }
            else if (inputVector.x != 0)
            {
                v = new Vector3(-(inputVector.y + inputVector.z) / inputVector.x, 1, 1);
            }
            else
            {
                v = new Vector3(1, -(inputVector.x + inputVector.z) / inputVector.y, 1);
            }
            return normalize ? v.normalized : v;
        }

        #region Transform
        /// <summary>
        /// Get the direction from the transform to the target position in terms of the transform's local axes.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Directions GetDirection(this Transform transform, Vector3 targetPosition)
        {
            if (transform == null) throw new System.ArgumentNullException();
            var direction = (targetPosition - transform.position).normalized;
            var forward = transform.forward;
            var right = transform.right;
            var up = transform.up;
            float forwardDot = Vector3.Dot(forward, direction);
            float rightDot = Vector3.Dot(right, direction);
            float upDot = Vector3.Dot(up, direction);
            if (forwardDot > 0.707f) return Directions.Forward;
            if (forwardDot < -0.707f) return Directions.Backward;
            if (rightDot > 0.707f) return Directions.Right;
            if (rightDot < -0.707f) return Directions.Left;
            if (upDot > 0.707f) return Directions.Up;
            if (upDot < -0.707f) return Directions.Down;
            return Directions.Forward; // Default case
        }

        public static Blackboard GetBlackboard(this Transform transform)
        {
            if (transform == null) throw new System.ArgumentNullException();
            var comp = ComponentRegister<BanditTree>.Get(transform);
            if (comp != null)
            {
                return comp.Context;
            }
            return null;
        }

        public static float GetHP(this Transform transform, bool percentage = false)
        {
            if (transform == null) throw new System.ArgumentNullException();
            var comp = ComponentRegister<Unit>.Get(transform);
            if (comp == null) return 0;

            return percentage ? comp.CurrentHPPercentage : comp.CurrentHP;
        }

        public static int GetTeam(this Transform transform, bool throwOnNullTransform = true, int defaultValue = -1)
        {
            if (transform == null)
            {
                if (!throwOnNullTransform) return defaultValue;
                throw new System.ArgumentNullException();
            }
            var ship = ComponentRegister<Unit>.Get(transform);
            return ship != null ? ship.Team : defaultValue;
        }

        public static T GetComponentViaRegister<T>(this Transform transform) where T : Component, IRegisterableComponent
        {
            if (transform == null) throw new System.ArgumentNullException();
            return ComponentRegister<T>.Get(transform);
        }

        public static T GetOrAddRegisterableComponent<T>(this Transform transform) where T : Component, IRegisterableComponent
        {
            if (transform == null) throw new System.ArgumentNullException();
            T component = ComponentRegister<T>.Get(transform);
            if (component == null)
            {
                component = transform.GetComponent<T>();
                if (component == null)
                {
                    component = transform.gameObject.AddComponent<T>();
                }
                ComponentRegister<T>.Register(transform, component);
            }
            return component;
        }

        public static T GetOrAddComponent<T>(this Transform transform) where T : Component
        {
            if (transform == null) throw new System.ArgumentNullException();
            if (!transform.TryGetComponent<T>(out var component))
            {
                component = transform.gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Applies a DamageInfo to the HPComponent of the given Transform. Returns true if 
        /// damage was applied, false otherwise.
        /// </summary>
        /// <param name="transform">The Transform to apply damage to.</param>
        /// <param name="damageInfo">The DamageInfo to apply.</param>
        /// <returns>True if damage was applied, false otherwise.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static bool TakeDamage(this Transform transform, DamageInfo damageInfo)
        {
            if (transform == null) throw new System.ArgumentNullException();
            if (transform.parent != null)
            {
                Debug.LogWarning($"{transform} has parent!");
            }
            return HPComponent.TakeDamage(transform, damageInfo);
        }
        #endregion
    }
}