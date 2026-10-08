using UnityEngine;

namespace Spawning
{
    public abstract class MonoSpawnableData<T> : ScriptableObject where T : MonoBehaviour
    {
        public T Prefab;

        public virtual T CreateInstance()
        {
            var t = Instantiate(Prefab);
            t.transform.name = t.transform.name.Replace("(Clone)", "");
            return t;
        }
    }
}