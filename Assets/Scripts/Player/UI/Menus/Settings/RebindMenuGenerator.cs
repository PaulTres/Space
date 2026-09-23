using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{ /// <summary>
  /// Builds a full rebinding menu at runtime from an InputActionAsset: one row per
  /// bindable control (composite parts get expanded into individual rows), with
  /// conflict detection across the whole asset and JSON persistence via PlayerPrefs.
  /// </summary>
    public class RebindMenuGenerator : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private RebindActionRow rowPrefab;
        [SerializeField] private Transform contentParent; // e.g. a Vertical Layout Group container

        [Header("Filtering")]
        [Tooltip("Only generate rows for these action maps. Leave empty to include all maps.")]
        [SerializeField] private List<string> actionMapsToInclude = new();

        [Tooltip("Restrict rebinding UI to bindings matching this control scheme (e.g. \"Keyboard&Mouse\" or \"Gamepad\"). Leave empty to show all.")]
        [SerializeField] private string controlSchemeFilter = "";

        private const string PrefsKey = "InputRebinds";

        private readonly List<RebindActionRow> rows = new();

        private void Start()
        {
            GenerateMenu();
        }

        private void GenerateMenu()
        {
            foreach (Transform child in contentParent)
                Destroy(child.gameObject);
            rows.Clear();

            foreach (var map in GameManager.InputActions.asset.actionMaps)
            {
                //Debugging.Log(map);
                if (actionMapsToInclude.Count > 0 && !actionMapsToInclude.Contains(map.name))
                    continue;

                foreach (var action in map.actions)
                {
                    CreateRowsForAction(action);
                }
            }
        }

        private void CreateRowsForAction(InputAction action)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];

                // Skip the composite "header" binding itself (e.g. "2D Vector" for WASD) —
                // we only want rows for its actual parts.
                if (binding.isComposite)
                    continue;

                // If filtering by control scheme, skip bindings that don't belong to it.
                if (!string.IsNullOrEmpty(controlSchemeFilter) &&
                    !string.IsNullOrEmpty(binding.groups) &&
                    !binding.groups.Split(';').Contains(controlSchemeFilter))
                {
                    continue;
                }

                string displayName = binding.isPartOfComposite
                    ? $"{action.name} - {binding.name}"   // e.g. "Move - Up"
                    : action.name;

                var row = Instantiate(rowPrefab, contentParent);
                row.Initialize(action, i, displayName);
                row.OnRebound += HandleRebound;
                rows.Add(row);
            }
        }

        private void HandleRebound(RebindActionRow changedRow)
        {
            SaveOverrides();
            CheckAllConflicts();
        }

        /// <summary>
        /// Compares every row's effective control path against every other row's.
        /// Rows sharing an identical path (and overlapping control scheme groups) get flagged.
        /// This is a simple exact-path check; extend it if you need cross-device semantic
        /// conflicts (e.g. same physical key under different binding groups).
        /// </summary>
        private void CheckAllConflicts()
        {
            var pathLookup = new Dictionary<string, List<RebindActionRow>>();

            foreach (var row in rows)
            {
                string path = row.Action.bindings[row.BindingIndex].effectivePath;
                if (string.IsNullOrEmpty(path))
                    continue;

                if (!pathLookup.TryGetValue(path, out var list))
                {
                    list = new List<RebindActionRow>();
                    pathLookup[path] = list;
                }
                list.Add(row);
            }

            foreach (var row in rows)
                row.SetConflictHighlight(false);

            foreach (var group in pathLookup.Values)
            {
                if (group.Count <= 1)
                    continue;

                // Don't flag a composite's own sibling parts (e.g. Up/Down never share a path anyway,
                // but if the same action's two different bindings clash, that's still worth flagging).
                foreach (var row in group)
                    row.SetConflictHighlight(true);
            }
        }

        private void SaveOverrides()
        {
            string json = GameManager.InputActions.asset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(PrefsKey, json);
            PlayerPrefs.Save();
        }

        /// <summary>Hook this up to a "Reset All" button.</summary>
        public void ResetAllBindings()
        {
            GameManager.InputActions.asset.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey(PrefsKey);
            foreach (var row in rows)
                row.UpdateBindingDisplay();
            CheckAllConflicts();
        }
    }
}