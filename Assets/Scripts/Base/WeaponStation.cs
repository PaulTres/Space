using Effects;
using EventBus;
using HP;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(HPComponent))]
public class WeaponStation : ImprovedMonoBehaviour
{
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
    public override float CurrentSignature => OwningUnit.CurrentSignature;

    protected void OnEnable()
    {
        Team = OwningUnit.Team;
        UnitManager.Register(this);
        EventBus<ObjectDestroyed>.AddActions(this.GetTransformInstanceID(), null, Die);
    }

    public override void SetObjectLayer()
    {
        gameObject.layer = GlobalConfig.GetShipLayer(OwningUnit.Team);
    }

    protected void OnDisable()
    {
        EventBus<ObjectDestroyed>.RemoveActions(this.GetTransformInstanceID(), null, Die);
        UnitManager.Unregister(this);
    }

    public void Die()
    {
        if (IsDying) return;
        IsDying = true;
        //Debug.Log($"{Transform} is dead.");
        UnitManager.Unregister(this);
        if (remains != null) remains.SetActive(true);
        effectOnDestruction.SpawnEffect(Position, EffectScaleFactor);
        gameObject.SetActive(false);
    }
}
