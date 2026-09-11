using Effects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class UnitDeathEffects : MonoBehaviour
{
    [Tooltip("Will play this effect several times during the death sequence.")]
    [SerializeField] protected PoolableEffect effectDuringDeath;
    [Tooltip("Will play this effect at the end of the death sequence.")]
    [SerializeField] protected PoolableEffect finalEffect;
    [SerializeField] protected Transform explosionOriginsParent;
    protected List<Transform> explosionOrigins = new();
    [SerializeField] protected float interval = .5f;
    [SerializeField] protected float count = 3;
    [SerializeField] protected float effectScaleFactor = 1, finalEffectScaleFactor = 1;
    protected Coroutine coroutine;
    protected WaitForSeconds wait;

    protected void OnValidate()
    {
        if (explosionOriginsParent == null) return;
        if (explosionOrigins.IsEmpty())
        {
            explosionOrigins = new();
            for (int i = 0; i < explosionOriginsParent.childCount; i++)
            {
                explosionOrigins.Add(explosionOriginsParent.GetChild(i));
            }
        }
    }

    protected void Awake()
    {
        wait = new(interval);
        OnValidate();
        explosionOrigins.Shuffle();
    }

    protected void OnDisable()
    {
        if (coroutine != null) StopCoroutine(coroutine);
    }

    public void PlayDeathEffects()
    {
        coroutine = StartCoroutine(Death());
    }

    protected IEnumerator Death()
    {
        int j = 0;
        for (int i = 0; i < count; i++)
        {
            effectDuringDeath.SpawnEffect(explosionOrigins[j].position, effectScaleFactor);
            j = (j + 1) % explosionOrigins.Count;
            yield return wait;
        }
        //if (transform.name.Contains("Chungus")) Debug.Log("Spawning final death effect: " + finalEffect);
        finalEffect.SpawnEffect(transform.position, finalEffectScaleFactor);
        transform.root.gameObject.SetActive(false);
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        OnValidate();
        foreach (var origin in explosionOrigins)
        {
            Gizmos.DrawWireSphere(origin.position, 0.1f * effectScaleFactor);
        }
    }
}
