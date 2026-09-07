using UnityEngine;
namespace Player
{
    public class PrimitiveTextPopup : TextPopup
    {
        [SerializeField] protected string text;
        public override string Text => text;
    }
}
