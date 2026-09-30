using System.Collections;
using UnityEngine;
namespace Player
{
    public class OSTTrigger : MonoBehaviour
    {
        [SerializeField] protected AudioClip audioClip;
        protected Coroutine coroutine;
        readonly WaitForSeconds wait = new(.5f);

        protected void OnDisable()
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }

        public void SetOST()
        {
            if (audioClip != null) coroutine = StartCoroutine(SetOSTCoroutine());
        }

        protected IEnumerator SetOSTCoroutine()
        {
            while (!Soundtrack.SetSoundtrack(audioClip))
            {
                yield return wait;
            }
        }
    }
}