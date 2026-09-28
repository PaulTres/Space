using UnityEngine;
using UnityEngine.Events;

namespace Level
{
    public abstract class Trigger : MonoBehaviour
    {
        public UnityEvent Delegate;
    }
}