using EventBus;
using UnityEngine;
using UnityEngine.UI;
namespace Player
{
    public class HUD : UISystem
    {
        [SerializeField] protected Image healthBar;
        
        protected override void Awake()
        {
            base.Awake();
            healthBar.fillAmount = 1;
            EventBus<ObjectDamaged>.AddActions(transform.root.GetInstanceID(), 
                UpdateHealthBar);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventBus<ObjectDamaged>.RemoveActions(transform.root.GetInstanceID(), 
                UpdateHealthBar);
        }

        protected void UpdateHealthBar(ObjectDamaged @event)
        {
            healthBar.fillAmount =Mathf.Max(0,@event.CurrentHPPercentage);
        }
    }
}
