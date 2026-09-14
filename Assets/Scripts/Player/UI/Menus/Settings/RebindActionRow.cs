using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Player
{
    /// <summary>
    /// Represents a single rebindable row in the UI: one action + one binding index
    /// (a binding index may be a simple binding or one part of a composite, e.g. "Up" in WASD).
    /// </summary>
    public class RebindActionRow : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] protected TMP_Text actionNameText;
        [SerializeField] protected TMP_Text bindingDisplayText;
        [SerializeField] protected Button rebindButton;
        [SerializeField] protected Button resetButton;
        [Tooltip("Optional prompt.")]
        [SerializeField] protected GameObject inputPrompt;

        protected InputAction action;
        protected int bindingIndex;
        protected InputActionRebindingExtensions.RebindingOperation rebindOperation;

        //fires when a rebind completes successfully, so the parent menu
        //can re-checkevery row for conflicts (two actions bound to the
        //same control)
        public event Action<RebindActionRow> OnRebound;
        public InputAction Action => action;
        public int BindingIndex => bindingIndex;

        protected void OnEnable()
        {
            if (inputPrompt != null) inputPrompt.SetActive(false);
        }

        public void Initialize(InputAction action, int bindingIndex, string displayName)
        {
            this.action = action;
            this.bindingIndex = bindingIndex;

            gameObject.name = displayName;
            actionNameText.text = displayName;
            rebindButton.onClick.AddListener(StartRebind);
            resetButton.onClick.AddListener(ResetToDefault);

            if (inputPrompt != null)
                inputPrompt.SetActive(false);

            UpdateBindingDisplay();
        }

        public void UpdateBindingDisplay()
        {
            string displayString = action.GetBindingDisplayString(bindingIndex,
                InputBinding.DisplayStringOptions.DontIncludeInteractions);
            bindingDisplayText.text = displayString;
        }

        protected void StartRebind()
        {
            // Disable the action map while rebinding so the action itself doesn't
            // fire from whatever key you press to confirm/cancel.
            action.actionMap.Disable();

            rebindButton.interactable = false;
            if (inputPrompt != null)
                inputPrompt.SetActive(true);

            var binding = action.bindings[bindingIndex];

            // If this binding is part of a composite, we still rebind just this part.
            rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")           // avoid accidental mouse-move capture
                .WithControlsExcluding("<Pointer>/delta")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)               // debounce noisy inputs (e.g. gamepad drift)
                .OnComplete(_ => OnRebindComplete(success: true))
                .OnCancel(_ => OnRebindComplete(success: false))
                .Start();
        }

        protected void OnRebindComplete(bool success)
        {
            rebindOperation.Dispose();
            rebindOperation = null;

            action.actionMap.Enable();
            rebindButton.interactable = true;
            if (inputPrompt != null)
                inputPrompt.SetActive(false);

            UpdateBindingDisplay();

            if (success)
                OnRebound?.Invoke(this);
        }

        protected void ResetToDefault()
        {
            action.RemoveBindingOverride(bindingIndex);
            UpdateBindingDisplay();
            OnRebound?.Invoke(this);
        }

        /// <summary>
        /// Visually flags this row as conflicting with another binding (e.g. red text).
        /// Call this after a conflict check; pass false to clear it.
        /// </summary>
        public void SetConflictHighlight(bool hasConflict)
        {
            bindingDisplayText.color = hasConflict ? Color.red : Color.white;
        }

        protected void OnDestroy()
        {
            rebindOperation?.Dispose();
        }
    }
}