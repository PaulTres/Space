using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace Player
{
    public class SceneFadeInAndOut : MonoBehaviour
    {
        [SerializeField] protected Image blackImage;
        [SerializeField] protected float fadeInDuration = 1, fadeOutDuration = 1;
        public bool IsFading => blackImage.canvasRenderer.GetAlpha() > 0.001f;
        protected Coroutine coroutine;

        protected void OnEnable()
        {
            blackImage.enabled = true;
            blackImage.CrossFadeAlpha(1, 0, true);
            blackImage.CrossFadeAlpha(0, fadeInDuration, true);
        }

        protected void OnDisable()
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }

        public void FadeOut(Action callback)
        {
            coroutine = StartCoroutine(FadeOutCoroutine(callback));
        }

        protected IEnumerator FadeOutCoroutine(Action callback)
        {
            blackImage.CrossFadeAlpha(1, fadeOutDuration, true);
            yield return new WaitForSecondsRealtime(fadeOutDuration);
            callback?.Invoke();
        }
    }
}
