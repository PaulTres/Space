using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Radar
{
    public class RadarSystem : MonoBehaviour
    {
        [SerializeField] protected Image radarPingPrefab;
        protected Stack<GameObject> pingPool { get; } = new();
        public RectTransform radarCenter;
        public float radarDisplayRadius = 2;
        [SerializeField] protected float radarRange = 10f;
        protected readonly Dictionary<IObject, GameObject> radarPings = new();

        protected void OnDisable()
        {
            radarPings.Clear();
        }

        protected void OnDestroy()
        {
            pingPool.Clear();
        }

        public void RegisterTarget(IObject target)
        {

        }

        public void UnregisterTarget(IObject target)
        {

        }

        public void ProcessTarget(IObject target)
        {
            var distance = Vector3.Distance(transform.position, target.Position);
            if (distance > radarRange) return;
            var ping = radarPings[target];
            ping.transform.GetChild(0).localScale = Vector3.one * Mathf.Clamp01(1 - (distance / radarRange));

            ping.transform.localPosition = Vector3.Lerp(Vector3.zero, ((target.Position - transform.position) / distance) * radarDisplayRadius, Mathf.Clamp01(distance / radarRange));

            var line = ping.GetComponent<LineRenderer>();
            var basePosition = new Vector3(ping.transform.localPosition.x, 0, ping.transform.localPosition.z);
            line.SetPosition(0, ping.transform.TransformVector(basePosition));
            line.SetPosition(1, ping.transform.position);
        }
    }
}