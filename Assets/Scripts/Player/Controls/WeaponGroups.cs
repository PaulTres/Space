using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities;
using Weapons;
namespace Player
{
    public abstract class WeaponGroup
    {
        public Sprite Icon;
        public abstract float ReloadStatus { get; }
        public UnityEvent OnSelected;
        public UnityEvent OnDeselected;
    }

    [System.Serializable]
    public class TurretGroup : WeaponGroup
    {
        [field: SerializeField] public List<Turret> Turrets { get; protected set; }
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

        public TurretGroup(List<Turret> turrets, Heatsink heatsink)
        {
            Turrets = turrets;
            Heatsink = heatsink;
        }
    }

    [System.Serializable]
    public class MissileGroup : WeaponGroup
    {
        [field: SerializeField] public List<MissileLauncher> Launchers { get; protected set; }
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

        public MissileGroup(List<MissileLauncher> launchers)
        {
            Launchers = launchers;
        }
    }
}
