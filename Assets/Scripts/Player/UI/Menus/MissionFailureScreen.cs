using UnityEngine;
namespace Player
{
    public class MissionFailureScreen : MonoBehaviour
    {
        protected void OnEnable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        }

        public void RestartMission()
        {
            GameManager.LoadLoadingScreen();
        }

        public void ReplanMission()
        {
            GameManager.LoadMissionSelection();
        }

        public void GoToMainMenu()
        {
            GameManager.LoadMainMenu();
        }
    }
}
