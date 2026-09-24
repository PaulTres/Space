using Effects;
using UnityEngine;
/// <summary>
/// This class is used to store parameters about a ship/unit on the object.
/// </summary>
public class ShipParameters : MonoBehaviour
{
    public float MaxVelocityMagnitude = 30;
    public float VelocitySoftCap = 1000000;
    [Tooltip("How much morale damage will allies suffer when this unit is destroyed.")]
    public int MoraleWeight = 10;
    public float TickInterval = 0.2f;
    [Tooltip("Plays when the ship is spawned in.")]
    public PoolableEffect EffectOnSpawn;
    public float MinimumSignature = 10;
    public float MaximumSignature = 50;
    public float SignatureDecreaseRate = 5f;
    [Tooltip("The time it takes before the signature starts decreasing after it has been increased. This is used to prevent the signature from decreasing too quickly after it has been increased.")]
    public float SignatureDecreaseCooldown = 5f;
    public float ScanRange = 30;
    [Tooltip("On spawn, the unit will check for obstructions in a sphere of this radius.")]
    public float SpawnCheckRadius = 5;
    public float EffectScaleFactor = 1;
    public float EscortValue = 10;
    [Header("Ship")]
    public float ForwardThrust = 15;
    public float StrafeThrust = 15;
    public float RotationSpeed = 50;

    protected void OnValidate()
    {
        VelocitySoftCap = Mathf.Min(VelocitySoftCap, MaxVelocityMagnitude * .9f);
    }
}
