using HP;
using Radar;
using System.Collections.Generic;
using UnityEngine;
namespace Utilities
{
    public static class Extensions
    {
        #region IObject
        public static void RegisterForRadar(this IObject @object)
        {
            if (@object == null) throw new System.ArgumentNullException();

            var r = RadarSystem.TryGetInstance();
            if (r != null) r.RegisterTarget(@object);
        }

        public static void UnregisterForRadar(this IObject @object)
        {
            if (@object == null) throw new System.ArgumentNullException();

            var r = RadarSystem.TryGetInstance();
            if (r != null) r.UnregisterTarget(@object);
        }

        public static int GetInstanceID(this IObject @object)
        {
            return @object.Transform.GetInstanceID();
        }

        public static bool IsEqual(this IObject @object, IObject other)
        {
            if (@object == null && other == null) return true;
            if (@object == null || other == null) return false;
            return @object.GetInstanceID() == other.GetInstanceID();
        }

        public static Vector3 GetPositionWithOscilation(this IObject @object, Unit scanner)
        {
            var dist = Mathf.Max((scanner.Transform.position -
                @object.Transform.position).sqrMagnitude -
                @object.Signature - scanner.ScanRange, 0);
            return @object.Transform.position + dist * Random.insideUnitSphere;
        }
        #endregion

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

        public static Vector3 GetLinearVelocity(this Transform transform)
        {
            if (transform == null) throw new System.ArgumentNullException();
            IObject velocityProvider = ComponentRegister<IObject>.Get(transform);
            return velocityProvider != null ? velocityProvider.LinearVelocity : Vector3.zero;
        }

        public static Vector3 GetAngularVelocity(this Transform transform)
        {
            if (transform == null) throw new System.ArgumentNullException();
            IObject velocityProvider = ComponentRegister<IObject>.Get(transform);
            return velocityProvider != null ? velocityProvider.AngularVelocity : Vector3.zero;
        }

        public static (Vector3 linear, Vector3 angular) GetVelocities(this Transform transform)
        {
            if (transform == null) throw new System.ArgumentNullException();
            IObject velocityProvider = ComponentRegister<IObject>.Get(transform);
            return velocityProvider != null ? (velocityProvider.LinearVelocity, velocityProvider.AngularVelocity) : (Vector3.zero, Vector3.zero);
        }

        public static bool TakeDamage(this Transform transform, DamageInfo damageInfo)
        {
            if (transform == null) throw new System.ArgumentNullException();
            return HPComponent.TakeDamage(transform, damageInfo);
        }
        #endregion
    }
}