using Effects;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace Level
{
    public class Wormhole : MonoBehaviour
    {
        protected Transform tr;
        public Transform Transform
        {
            get
            {
                if (tr == null) tr = transform;
                return tr;
            }
        }
        [SerializeField] protected UnityEvent onClosed;
        [Tooltip("The amount of HP the wormhole has. When it reaches 0, the wormhole will close. The wormhole loses one HP per second when in a safe zone. Multiple safezones do not stack.")]
        [SerializeField] protected float HP = 120;
        [SerializeField] protected PoolableEffect effectOnClose;

        protected void OnEnable()
        {
            GlobalUpdater.TryGetInstance(true).RegisterFixedUpdate(PerformFixedUpdate);
        }

        protected void OnDisable()
        {
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterFixedUpdate(PerformFixedUpdate);
        }

        protected void PerformFixedUpdate(float deltaTime)
        {
            if (SafeZone.InSafeZone(Transform.position))
            {
                //take damage
                HP -= deltaTime;
                if (HP <= 0)
                {
                    onClosed?.Invoke();
                    effectOnClose.SpawnEffect(Transform.position, 1);
                    gameObject.SetActive(false);
                }
            }
        }
    }
}