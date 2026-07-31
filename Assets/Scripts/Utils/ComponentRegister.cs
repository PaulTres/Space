using System.Collections.Generic;
using UnityEngine;
namespace Utilities
{
    public interface IRegisterableComponent { }
    public static class ComponentRegister<T> where T : class, IRegisterableComponent
    {
        static readonly Dictionary<Transform, T> storage = new();

        public static void Clear() => storage.Clear();

        public static T Get(Transform transform)
        {
            if (transform == null) throw new System.ArgumentNullException();
            return storage.TryGetValue(transform, out T instance) ? instance : null;
        }

        public static bool Register(Transform transform, T component)
        {
            if (transform == null || component == null) throw new System.ArgumentNullException();
            return storage.TryAdd(transform, component);
        }

        public static void Unregister(Transform transform)
        {
            if (transform == null)
            {
                Debug.LogError("Attempted to unregister null transform!");
            }
            storage.Remove(transform);
        }
    }
}