using UnityEngine;
namespace Player
{
    public class PauseScreen : MenuPDA
    {
        [SerializeField] protected bool stopTime = true;
        protected override void OnEnable()
        {
            base.OnEnable();
            if (stopTime) Time.timeScale = 0;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Time.timeScale = 1;
        }

        public override void Pop()
        {
            if (windows.Count == 1) gameObject.SetActive(false);
            else base.Pop();
        }

        public void ReturnToMenu() => GameManager.LoadMainMenu();
    }
}