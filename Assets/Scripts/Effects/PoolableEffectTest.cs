using Effects;
using UnityEngine;
using Utilities;

public class PoolableEffectTest : MonoBehaviour
{
    public float ScaleFactor = 1;
    public bool Play;
    public PoolableEffect PoolableEffect;

    private void Update()
    {
        if(Play)
        {
            Play = false;
            PoolableEffect.SpawnEffect(transform.position, ScaleFactor);
        }
    }
}
