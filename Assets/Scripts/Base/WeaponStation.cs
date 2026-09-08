using Effects;
using EventBus;
using HP;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(HPComponent))]
public class WeaponStation : ImprovedMonoBehaviour
{
    public override Vector3 LinearVelocity => OwningUnit.LinearVelocity;
    public override Vector3 AngularVelocity => OwningUnit.AngularVelocity;
    public override ImprovedMonoBehaviour Target => null;
    protected Unit unit;
    public Unit OwningUnit
    {
        get
        {
            if (unit == null) unit = Transform.root.GetComponent<Unit>();
            return unit;
        }
    }
    [Tooltip("This object will be enabled when this WeaponStation is killed.")]
    [SerializeField] protected GameObject remains;
    [SerializeField] protected PoolableEffect effectOnDestruction;
    [Tooltip("When the object is destroyed the owning unit will receive this much damage.")]
    [SerializeField] protected float damageOnDestroy;
    public override float CurrentSignature => OwningUnit.CurrentSignature;

    protected void OnEnable()
    {
        Team = OwningUnit.Team;
        UnitManager.Register(this);
        EventBus<ObjectDestroyed>.AddActions(this.GetTransformInstanceID(), Die);
    }

    public override void SetObjectLayer()
    {
        gameObject.layer = GlobalConfig.GetShipLayer(OwningUnit.Team);
    }

    protected void OnDisable()
    {
        EventBus<ObjectDestroyed>.RemoveActions(this.GetTransformInstanceID(), Die);
        UnitManager.Unregister(this);
    }

    public void Die(ObjectDestroyed @event)
    {
        if (IsDying) return;
        IsDying = true;
        //Debug.Log($"{Transform} is dead.");
        UnitManager.Unregister(this);
        if (remains != null) remains.SetActive(true);
        effectOnDestruction.SpawnEffect(Position, EffectScaleFactor);
        var damageInfo = new DamageInfo
        {
            Position = Position,
            Source = @event.Killer,
            IgnoreArmor = true,
            Amount = damageOnDestroy
        };
        Transform.root.TakeDamage(damageInfo);
        gameObject.SetActive(false);
    }
}
