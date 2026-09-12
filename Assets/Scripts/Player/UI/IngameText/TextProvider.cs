using UnityEngine;
namespace Player
{
    public abstract class TextProvider : ScriptableObject
    {
        public abstract string Text { get; }
    }
}
