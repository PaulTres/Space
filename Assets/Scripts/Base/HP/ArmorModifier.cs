using UnityEngine;
using Utilities;

namespace HP
{
    [System.Serializable]
    public class ArmorModifier
    {
        [field: SerializeField] public Directions Direction { get; protected set; }
        [Tooltip("Incoming damage is multiplied by this modifier.")]
        [field: SerializeField] public float Modifier { get; protected set; } = 1;
    }
}