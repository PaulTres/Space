using System.Collections;
using UnityEngine;
namespace Player
{
    public class MissionFailureScreen : MonoBehaviour
    {
        [SerializeField] protected float delayBeforeShowing = 1f;
        protected GameObject canvas;
        public GameObject Canvas
        {
            get
            {
                if (canvas == null)
                {
                    var o = GetComponentInChildren<Canvas>(true);
                    if (o != null) canvas = o.gameObject;
                    else Debug.LogError("No canvas found in children of " + name);
                }
                return canvas;
            }
        }

        protected void OnValidate()
        {
            if (delayBeforeShowing < 0) delayBeforeShowing = 0;
            //trigger the error if no canvas is found in children of the game object
            var c = Canvas;
        }

        protected void OnEnable()
        {
            if (PlayerShip.PlayerDead) ShowCanvas();
            else StartCoroutine(Delay());
        }

        protected void OnDisable()
        {
            Time.timeScale = 1;
        }

        protected IEnumerator Delay()
        {
            yield return new WaitForSeconds(delayBeforeShowing);
            ShowCanvas();
        }

        protected void ShowCanvas()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
            Canvas.SetActive(true);
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
