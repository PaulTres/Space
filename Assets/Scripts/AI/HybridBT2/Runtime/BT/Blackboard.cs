using System.Collections.Generic;
using UnityEngine;
namespace HybridBT2
{
    public class Blackboard : IResettable
    {
        public enum Keys
        {
            Fleeing,
            LastMoraleDamageTime,
            DefaultMorale,
            Morale,
            Target,
            TargetPosition,
            PDTurrets,
            AllTurrets
        }
        public readonly Transform Transform;
        public Vector3 Position => Navigation.Position;
        public readonly Navigation Navigation;
        /// <summary>
        /// Time since last tree tick.
        /// </summary>
        public float DeltaTime { get; protected set; }
        protected Dictionary<Keys, object> data = new();
        protected float lastTime;

        public Blackboard(Transform transform)
        {
            Transform = transform;
            Navigation = transform.GetComponent<Navigation>();
            DeltaTime = 0;
            lastTime = Time.time;
        }

        public void Clear() => data.Clear();

        public void PerformReset()
        {
            lastTime = Time.time;
            DeltaTime = 0;
        }

        public void Tick()
        {
            DeltaTime = Time.time - lastTime;
            lastTime = Time.time;
        }

        public void Tick(float deltaTime) => DeltaTime = deltaTime;

        public void SetData<R>(Keys key, R value) => data[key] = value;

        public R GetData<R>(Keys key)
        {
            if (data.TryGetValue(key, out var value))
            {
                return (R)value;
            }
            //Debug.LogError("Cannot find data for key " + key.ToString());
            return default;
        }
    }
}