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
            GameManager.LoadMission();
        }

        public void ReplanMission()
        {
            GameManager.LoadLobby();
        }

        public void GoToMainMenu()
        {
            GameManager.LoadMainMenu();
        }
    }
}
