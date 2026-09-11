using HybridBT2;
using Radar;
using UnityEngine;
using Utilities;

public abstract class ImprovedMonoBehaviour : MonoBehaviour, IRegisterableComponent
{
    /// <summary>
    /// This is used for quick removal from the unit manager lists. 
    /// DO NOT FUCK WITH THIS!
    /// </summary>
    public int UnitManagerIndex { get; set; } = -1;
    [Tooltip("Team 1 is player team, team 2 is enemy team.")]
    [field: SerializeField] public int Team { get; set; } = 1;
    [field: SerializeField] public float MaxVelocityMagnitude { get; protected set; } = 20f;
    public abstract ImprovedMonoBehaviour Target { get; }
    public virtual Vector3 Forward => Transform.forward;
    public virtual Vector3 Right => Transform.right;
    public virtual Vector3 Up => Transform.up;
    public virtual Vector3 LinearVelocity => Vector3.zero;
    public virtual Vector3 AngularVelocity => Vector3.zero;
    Transform tr;
    public Transform Transform
    {
        get
        {
            if (tr == null) tr = transform;
            return tr;
        }
    }
    public virtual bool IsFleeing
    {
        get
        {
            var b = Transform.GetBlackboard();
            if (b == null) return false;
            return b.GetData<bool>(Blackboard.Keys.Fleeing);
        }
    }
    public Vector3 Position
    {
        get => Transform.position;
        set => Transform.position = value;
    }
    public abstract float CurrentSignature { get; }
    [field: SerializeField] public bool IsDying { get; protected set; } = false;
    [Tooltip("The scale of effects played for this unit will be multiplied by this number.")]
    [field: SerializeField] public float EffectScaleFactor { get; protected set; } = 1;
    [field: SerializeField] public float EscortValue { get; protected set; } = 0;
    public void RegisterForRadar()
    {
        var r = PlayerTargetSystem.TryGetInstance();
        if (r != null)
        {
            r.RegisterTarget(this);
        }
    }

    public void UnregisterForRadar()
    {
        var r = PlayerTargetSystem.TryGetInstance();
        if (r != null)
        {
            r.UnregisterTarget(this);
        }
    }

    public abstract void SetObjectLayer();
}
