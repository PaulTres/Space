using EventBus;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace HP
{
    public class DamageReceiver : MonoBehaviour
    {
        [SerializeField] protected List<HPComponent> hpComponents = new();
        [SerializeField] protected List<ArmorModifier> armorModifiers = new();
        [Tooltip("This transform is used to determine the direction of incoming damage. If not set, the transform of this component will be used.")]
        [SerializeField] protected Transform body;

        protected void Awake()
        {
            if (!EventBus<DamageInfo>.AddActions(transform.GetInstanceID(), ReceiveDamage))
            {
                Debug.LogWarning($"{transform} unable to add DamageInfo actions to event bus binding.");
            }
            if (body == null) body = transform;
            Debug.Assert(!hpComponents.IsEmpty(), $"{this} has no HPComponents!");
        }

        protected void OnDestroy()
        {
            EventBus<DamageInfo>.RemoveBinding(transform.GetInstanceID());
        }

        protected void OnValidate()
        {
            if (hpComponents.IsEmpty())
            {
                hpComponents ??= new();
                if (TryGetComponent<HPComponent>(out var hp))
                {
                    hpComponents.Add(hp);
                }
                else if (!TryGetComponent<Collider>(out _)) Debug.LogError($"{this} has no collider or HPComponent!");
                if (transform.root.TryGetComponent<HPComponent>(out var hproot)) hpComponents.Add(hproot);
            }
            if (body == null) body = transform;
        }

        public void ReceiveDamage(DamageInfo damageInfo)
        {
            damageInfo.Amount = CalculateDamage(damageInfo);
            for (int i = 0; i < hpComponents.Count; i++)
            {
                hpComponents[i].TakeDamage(damageInfo);
            }
        }

        public float CalculateDamage(DamageInfo damageInfo)
        {
            if (damageInfo.IgnoreArmor || armorModifiers.IsEmpty()) return damageInfo.Amount;
            var direction = body.GetDirection(damageInfo.Position);
            for (int i = 0; i < armorModifiers.Count; i++)
            {
                if (armorModifiers[i].Direction == direction)
                {
                    return damageInfo.Amount * armorModifiers[i].Modifier;
                }
            }
            return damageInfo.Amount;
        }
    }
}