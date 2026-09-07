using AudioSystem;
using UnityEngine;
using Utilities;

namespace Level
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] protected SoundData sound;
        [SerializeField] protected float delay = 0;
        [Tooltip("If false, the sound will be played at this object's position.")]
        [SerializeField] protected bool playAtPlayerPosition = true;

        public void PlaySound()
        {
            if(playAtPlayerPosition&&GameManager.Player!=null)
            {
                sound.Play(GameManager.Player.Position);
            }
            else sound.Play(transform.position);
        }

        public void PlayWithDelay()
        {
            MissionManager.TryGetInstance().ExecuteAction(PlaySound, delay);
        }
    }
}