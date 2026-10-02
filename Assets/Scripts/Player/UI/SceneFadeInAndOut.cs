using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace Player
{
    public class SceneFadeInAndOut : MonoBehaviour
    {
        [SerializeField] protected Image blackImage;
        [SerializeField] protected float fadeDuration = 1;
        protected Coroutine coroutine;

        protected void OnEnable()
        {
            blackImage.enabled = true;
            blackImage.CrossFadeAlpha(1, 0, true);
            blackImage.CrossFadeAlpha(0, fadeDuration, true);
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
            blackImage.CrossFadeAlpha(1, fadeDuration, true);
            yield return new WaitForSeconds(fadeDuration);
            callback?.Invoke();
        }
    }
}
