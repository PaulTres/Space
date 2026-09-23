using Effects;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(ParticleSystem))]
public class DamageEffect : MonoBehaviour
{
    protected ParticleSystem particles;
    protected Transform tr;
    [field: SerializeField] public float DamageThreshold { get; protected set; } = .3f;
    [SerializeField] protected PoolableEffect effect;
    [SerializeField] protected float effectScaleFactor = 1f;

    protected void OnEnable()
    {
        //Debugging.Log("DamageEffect enabled");
        if (particles == null) particles = GetComponent<ParticleSystem>();
        if (tr == null) tr = transform;
        particles.Play();
        effect.SpawnEffect(tr.position, tr.rotation, effectScaleFactor);
    }
}
