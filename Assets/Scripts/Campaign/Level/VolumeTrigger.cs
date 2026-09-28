using UnityEngine;
using Utilities;

namespace Level
{
    [RequireComponent(typeof(Collider))]
    public class VolumeTrigger : Trigger
    {
        [SerializeField] protected bool playerOnly = true;

        protected void OnValidate()
        {
            var collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (!playerOnly||other.transform.root.IsPlayer()) Delegate?.Invoke();
        }
    }
}