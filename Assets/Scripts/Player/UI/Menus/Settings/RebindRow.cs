using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Player
{
    public class RebindRow : MonoBehaviour
    {
        protected ControlsSettingsMenu parentMenu;
        public ControlsSettingsMenu ParentMenu
        {
            get
            {
                if (parentMenu == null) parentMenu = GetComponentInParent<ControlsSettingsMenu>();
                return parentMenu;
            }
        }
        public InputAction Action { get; set; }
        [SerializeField] protected TextMeshProUGUI actionNameText, keybindText;
        [SerializeField] protected Button rebindButton;
        [field: SerializeField] public int BindingIndex { get; set; } = 0;

        public void Redraw()
        {
            if (Action == null || Action.bindings.Count == 0) return;
            if (BindingIndex == 0) actionNameText.text = Action.name;
            else
            {
                //Debug.Log("Binding index: " + BindingIndex + " for action: " + Action.name);
                actionNameText.text = Action.name + "/" + Action.bindings[BindingIndex].name;
            }
            BindingIndex = Mathf.Clamp(BindingIndex, 0, Action.bindings.Count - 1);
            keybindText.text = Action.bindings[BindingIndex].ToDisplayString();
        }

        public void OnEnable()
        {
            rebindButton.interactable = true;
            rebindButton.onClick.AddListener(PerformRebind);
            Redraw();
        }

        protected void OnDisable()
        {
            rebindButton.onClick.RemoveListener(PerformRebind);
        }

        public void PerformRebind()
        {
            if (Action == null)
            {
                Debug.LogError("Action is null for RebindRow: " + gameObject.name);
                return;
            }
            keybindText.text = "Press a key...";
            Action.Disable();
            rebindButton.interactable = false;
            var rebindingOperation = Action.PerformInteractiveRebinding()
                    .WithControlsExcluding("Mouse/delta")
                    .WithControlsExcluding("Mouse/position")
                    .WithCancelingThrough("<Keyboard>/escape")
                    .WithTargetBinding(BindingIndex)
                    .Start()
                    .OnComplete((x) =>
                    {
                        //update keybind text
                        Redraw();
                        Action.Enable();
                        x.Dispose();
                        rebindButton.interactable = true;
                    })
                    .OnCancel((x) =>
                    {
                        x.Dispose();
                        Action.Enable();
                        rebindButton.interactable = true;
                    });

            if (Action.bindings[BindingIndex].isPartOfComposite)
            {
                rebindingOperation.WithExpectedControlType("Button");
            }

            //OnValidate();
        }
    }
}