using System.Collections.Generic;
using UnityEngine;
using Weapons;
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
            AllTurrets,
            MinEscortValue,
            EscortCandidate
        }

        public Vector3 Position => Navigation.Position;
        public readonly Navigation Navigation;
        /// <summary>
        /// Time since last tree tick.
        /// </summary>
        public float DeltaTime { get; protected set; }
        /// <summary>
        /// This should only be used for tests. Use Tick() instead, which will automatically calculate the delta time.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void SetDeltaTime(float deltaTime) => DeltaTime = deltaTime;
        protected Dictionary<Keys, object> data = new();
        protected float lastTime;
        public HashSet<IThreat> Threats { get; protected set; } = new();

        public Blackboard(Transform transform)
        {
            if (transform != null) Navigation = transform.GetComponent<Navigation>();
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

        public R GetData<R>(Keys key, bool debug = false)
        {
            if (data.TryGetValue(key, out var value))
            {
                if (debug) Debug.Log(value);
                return (R)value;
            }
            //Debug.LogError("Cannot find data for key " + key.ToString());
            return default;
        }
    }
}