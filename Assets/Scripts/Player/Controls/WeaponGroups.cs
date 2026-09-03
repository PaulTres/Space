using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Weapons;
namespace Player
{
    public abstract class WeaponGroup
    {
        public Sprite Icon;
        public abstract float ReloadStatus { get; }
    }

    [System.Serializable]
    public class TurretGroup : WeaponGroup
    {
        [SerializeField] protected Transform turretParent;
        protected List<Turret> turrets = new();
        public List<Turret> Turrets
        {
            get
            {
                if (turrets.IsEmpty())
                {
                    turretParent.GetComponentsInChildren(true, turrets);
                }
                return turrets;
            }
        }
        public Heatsink Heatsink;
        /// <summary>
        /// 0 - fully reloaded<br/>
        /// 1 - just fired<br/>
        /// </summary>
        public override float ReloadStatus
        {
            get
            {
                float s = 0;
                for (int i = 0; i < Turrets.Count; i++) s += Turrets[i].ReloadStatus;
                return s / Turrets.Count;
            }
        }
    }

    [System.Serializable]
    public class MissileGroup : WeaponGroup
    {
        [SerializeField] protected Transform missileLauncherParent;
        protected List<MissileLauncher> launchers = new();
        public List<MissileLauncher> Launchers
        {
            get
            {
                if (launchers.IsEmpty())
                {
                    missileLauncherParent.GetComponentsInChildren(true, launchers);
                }
                return launchers;
            }
        }
        public Magazine Magazine
        {
            get
            {
                return Launchers.IsEmpty() ? null : Launchers[0].Magazine;
            }
        }
        /// <summary>
        /// 0 - fully reloaded<br/>
        /// 1 - just fired<br/>
        /// </summary>
        public override float ReloadStatus
        {
            get
            {
                float s = 0;
                for (int i = 0; i < Launchers.Count; i++) s += Launchers[i].ReloadStatus;
                return s / Launchers.Count;
            }
        }
    }
}
