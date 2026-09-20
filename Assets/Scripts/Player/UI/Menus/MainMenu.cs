using UnityEngine;
namespace Player
{
    public class MainMenu : MenuPDA
    {
        [SerializeField] protected PlayerData playerTutorialShip;

        public void StartTutorial()
        {
            UnitData.PlayerChoice = playerTutorialShip;
            GameManager.LoadTutorial();
        }

        public override void Pop()
        {
            if (windows.Count == 1) initialWindows[1].Open();
            else base.Pop();
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
