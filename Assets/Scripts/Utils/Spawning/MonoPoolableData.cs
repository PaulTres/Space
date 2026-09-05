using UnityEngine;

namespace Spawning
{

    public abstract class MonoPoolableData<K, T> : MonoSpawnableData<T> where T : MonoBehaviour
    {
        public K Key;

        /// <summary>
        /// Returns an instance of the poolable object at the position and rotation of the point.
        /// </summary>
        /// <param name="point">The transform whose position and rotation will be used.</param>
        /// <returns>The initialized instance of the poolable object.</returns>
        public T GetInstance(Transform point) => GetInstance(point.position, point.rotation);

        public T GetInstance(Vector3 position, Quaternion rotation)
        {
            var instance = GetInstance();
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        protected abstract T GetInstance();
    }
}