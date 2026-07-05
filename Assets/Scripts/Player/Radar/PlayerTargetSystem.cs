using System.Collections.Generic;
using UnityEngine;
using Utilities;
namespace Radar
{
    [RequireComponent(typeof(RadarSystem))]
    [RequireComponent(typeof(ObjectDisplay))]
    public class PlayerTargetSystem : Singleton<PlayerTargetSystem>
    {
        [SerializeField] protected RadarSystem radarSystem;
        [SerializeField] protected ObjectDisplay objectDisplay;
        protected readonly HashSet<IObject> registeredTargets = new();

        #region Setup
        protected void OnEnable()
        {
            GlobalUpdater.TryGetInstance(true).RegisterLateUpdate(PerformLateUpdate);
        }

        protected void OnDisable()
        {
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterLateUpdate(PerformLateUpdate);
            registeredTargets.Clear();
        }
        #endregion

        #region Functionality
        protected virtual void PerformLateUpdate(float dt)
        {
            foreach (var target in registeredTargets)
            {
                radarSystem.ProcessTarget(target);
                objectDisplay.ProcessTarget(target);
            }
        }

        public virtual void RegisterTarget(IObject target)
        {
            registeredTargets.Add(target);
            radarSystem.RegisterTarget(target);
            objectDisplay.RegisterTarget(target);
        }



        public virtual void UnregisterTarget(IObject target)
        {
            registeredTargets.Remove(target);
            radarSystem.UnregisterTarget(target);
            objectDisplay.UnregisterTarget(target);
        }
        #endregion
    }
}