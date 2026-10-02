using System.Collections;
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

        public static bool StopSoundtrack()
        {
            var i = TryGetInstance();
            if (i != null)
            {
                i.audioSourceProperty.Stop();
                return true;
            }
            return false;
        }

        public static bool FadeSoundtrack(float duration)
        {
            var i = TryGetInstance();
            if (i != null && !i.fading)
            {
                i.coroutine = i.StartCoroutine(i.FadeOutMusic(duration));
                return true;
            }
            return false;
        }

        protected Coroutine coroutine;
        protected bool fading = false;

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            audioSourceProperty.volume = 1f;
        }

        protected IEnumerator FadeOutMusic(float duration)
        {
            if (duration <= 0)
            {
                audioSourceProperty.volume = 0;
                yield break;
            }
            fading = true;
            float startVolume = audioSourceProperty.volume;
            while (audioSourceProperty.volume > 0)
            {
                audioSourceProperty.volume -= ((startVolume * Time.deltaTime / duration) + Mathf.Epsilon);
                yield return null;
            }
            fading = false;
        }
    }
}