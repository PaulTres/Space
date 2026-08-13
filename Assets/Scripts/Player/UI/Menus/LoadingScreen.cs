using UnityEngine;
namespace Player
{
    public class LoadingScreen : MonoBehaviour
    {
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            //await Task.delay(1000); // Wait for 1 second before loading the mission
            GameManager.LoadMission();
        }
    }
}