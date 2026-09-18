using UnityEngine;
using Utilities;
namespace Player
{
    [RequireComponent(typeof(AudioListener))]
    [RequireComponent(typeof(AudioSource))]
    public class Soundtrack : Singleton<Soundtrack>
    {
        protected AudioSource audioSource;
        protected AudioSource audioSourceProperty
        {
            get
            {
                if (audioSource == null) audioSource = GetComponent<AudioSource>();
                audioSource.playOnAwake = true;
                audioSource.loop = true;
                return audioSource;
            }
        }

        public static bool SetSoundtrack(AudioClip audioClip)
        {
            var i = TryGetInstance();
            if (i != null)
            {
                i.audioSourceProperty.clip = audioClip;
                i.audioSourceProperty.Play();
                return true;
            }
            return false;
        }
    }
}