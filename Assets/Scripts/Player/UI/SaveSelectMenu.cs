using TMPro;
using UnityEngine;
namespace Player
{
    public class SaveSelectMenu : MonoBehaviour
    {
        [SerializeField] protected TMP_Dropdown dropdown;

        protected void PopulateDropdown()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(GameSave.GetSaveFileNames());
            dropdown.SetValueWithoutNotify(0);
        }

        protected void OnEnable()
        {
            PopulateDropdown();
        }

        public void NewGame()
        {
            GameManager.NewGame();
        }

        public void Load()
        {
            if (dropdown.options.Count > 0)
            {
                GameManager.LoadSave(GameSave.Saves[dropdown.value]);
            }
        }
        public void DeleteFile()
        {
            GameSave.DeleteSave(GameSave.ConvertToFileName(dropdown.options[dropdown.value].text));
        }
    }
}