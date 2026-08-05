using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DamageEffect : MonoBehaviour
{
    protected ParticleSystem particles;
    [field: SerializeField] public float DamageThreshold { get; protected set; } = .3f;

    protected void OnEnable()
    {
        Debug.Log("DamageEffect enabled");
        if (particles == null) particles = GetComponent<ParticleSystem>();
        particles.Play();
    }
}
