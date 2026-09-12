using UnityEngine;
namespace Player
{
    [CreateAssetMenu(fileName = "RawText", menuName = "ScriptableObjects/RawTextProvider")]
    public class RawTextProvider : TextProvider
    {
        [SerializeField] protected string text;
        public override string Text => text;
    }
}
