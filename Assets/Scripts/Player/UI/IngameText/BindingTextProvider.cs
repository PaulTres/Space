using UnityEngine;
using UnityEngine.InputSystem;
namespace Player
{
    [CreateAssetMenu(fileName = "BindingText", menuName = "ScriptableObjects/BindingTextProvider")]
    public class BindingTextProvider : TextProvider
    {
        [SerializeField] protected InputActionReference inputAction;
        [SerializeField] protected string actionName;
        public override string Text => inputAction.action.GetBindingDisplayString() + " to " + (actionName.Length > 0 ? actionName : inputAction.action.name.ToLower()) + ".";
    }
}
