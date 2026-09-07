using AudioSystem;
using Level;
using UnityEngine;
namespace Player
{
    public class TextPopup : MonoBehaviour
    {
        [SerializeField] protected Vector3 anchoredPosition;
        [SerializeField] protected float lifeTime;
        [SerializeField] protected string text;
        [SerializeField] protected Color color;
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
            UISystem.ShowText(new(anchoredPosition, lifeTime, text, color, sound));
        }
    }
}
