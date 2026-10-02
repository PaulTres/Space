using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Player
{
    public class ControlsSettingsMenu : MonoBehaviour
    {
        #region Sensitivity
        [SerializeField] protected Slider xSensSlider, ySensSlider, zSensSlider;
        [SerializeField] protected TextMeshProUGUI xSensValueText, ySensValueText, zSensValueText;

        protected void DisplaySensitivities()
        {
            xSensSlider.SetValueWithoutNotify(GlobalConfig.XSensitivity);
            ySensSlider.SetValueWithoutNotify(GlobalConfig.YSensitivity);
            zSensSlider.SetValueWithoutNotify(GlobalConfig.ZSensitivity);
            xSensValueText.text = GlobalConfig.XSensitivity.ToString();
            ySensValueText.text = GlobalConfig.YSensitivity.ToString();
            zSensValueText.text = GlobalConfig.ZSensitivity.ToString();
        }

        public void OnXSensChanged(float value)
        {
            GlobalConfig.XSensitivity = (int)value;
            xSensValueText.text = GlobalConfig.XSensitivity.ToString();
            GlobalConfig.SaveSensitivitySettings();
        }

        public void OnYSensChanged(float value)
        {
            GlobalConfig.YSensitivity = (int)value;
            ySensValueText.text = GlobalConfig.YSensitivity.ToString();
            GlobalConfig.SaveSensitivitySettings();
        }

        public void OnZSensChanged(float value)
        {
            GlobalConfig.ZSensitivity = (int)value;
            zSensValueText.text = GlobalConfig.ZSensitivity.ToString();
            GlobalConfig.SaveSensitivitySettings();
        }
        #endregion

        #region Invert Y
        [SerializeField] protected Toggle invertY;

        public void SetInvertY(bool value)
        {
            GlobalConfig.InvertY = value;
            GlobalConfig.SaveInvertYSetting();
        }
        #endregion

        #region Keybinds
        [SerializeField] protected RebindRow rebindRowPrefab;
        [SerializeField] protected RectTransform rowParent;
        [SerializeField] protected List<RebindRow> rebindRows = new();

        protected void GenerateMenu()
        {
            rebindRows = new();

            //go over Combat and BasicControls
            foreach (var action in GameManager.InputActions)
            {
                if (action.name == "MouseLook") continue;
                if (action.actionMap.name == "Weapons" || action.actionMap.name == "BasicControls")
                {
                    //Debug.Log("Processing action: " + action.name + '\n' + action.bindings.Count);
                    if (action.bindings.Count > 1)
                    {
                        //this is a composite binding
                        //Debug.Log("Action is composite: " + action.name);
                        for (int i = 1; i < action.bindings.Count; i++)
                        {
                            if (action.bindings[i].isPartOfComposite)
                            {
                                var row = Instantiate(rebindRowPrefab, rowParent);
                                row.BindingIndex = i;
                                row.Action = action;
                                row.Redraw();
                                rebindRows.Add(row);
                            }
                        }
                    }
                    else
                    {
                        //this is a simple action
                        var row = Instantiate(rebindRowPrefab, rowParent);
                        row.BindingIndex = 0;
                        row.Action = action;
                        row.Redraw();
                        rebindRows.Add(row);
                    }
                }
            }
        }
        #endregion

        public static void LoadSettings()
        {
            GlobalConfig.LoadInvertYSetting();
            GlobalConfig.LoadSensitivitySettings();
        }

        protected void Awake()
        {
            LoadSettings();
            invertY.onValueChanged.AddListener(SetInvertY);
            xSensSlider.onValueChanged.AddListener(OnXSensChanged);
            ySensSlider.onValueChanged.AddListener(OnYSensChanged);
            zSensSlider.onValueChanged.AddListener(OnZSensChanged);
            for (int i = 0; i < rowParent.childCount; i++)
            {
                Destroy(rowParent.GetChild(i).gameObject);
            }
            GenerateMenu();
        }

        protected void OnEnable()
        {
            invertY.SetIsOnWithoutNotify(GlobalConfig.InvertY);
            DisplaySensitivities();
        }

        public void OnResetBindings()
        {
            GameManager.InputActions.RemoveAllBindingOverrides();
            foreach (var row in rebindRows)
            {
                row.Redraw();
            }
        }
    }
}