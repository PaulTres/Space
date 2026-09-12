using AudioSystem;
using Level;
using UnityEngine;
namespace Player
{
    public abstract class TextPopup : MonoBehaviour
    {
        [SerializeField] protected Vector3 anchoredPosition = Vector3.zero;
        [SerializeField] protected float lifeTime = 5;
        public abstract string Text { get; }
        [SerializeField] protected Color color = new(255, 255, 255, 255);
        [SerializeField] protected SoundData sound;
        [SerializeField] protected bool fireOnEnable = true;
        [SerializeField] protected float delay = 0;

        protected void OnEnable()
        {
            if (fireOnEnable)
            {
                MissionManager.TryGetInstance().ExecuteAction(SpawnText, delay);
            }
        }

        public void SpawnText()
        {
            UISystem.ShowText(new(anchoredPosition, lifeTime, Text, color, sound));
        }
    }
}
