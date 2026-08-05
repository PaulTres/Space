using UnityEngine;
namespace Player
{
    public class LoadingScreen : MonoBehaviour
    {
        async void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            //await Task.Delay(1000); // Wait for 1 second before loading the mission
            GameManager.LoadMission();
        }
    }
}