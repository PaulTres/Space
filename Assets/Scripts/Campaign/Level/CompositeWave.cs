using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Level
{
    public class CompositeWave : MonoBehaviour
    {
        [SerializeField] protected UnityEvent onAllWavesDefeated;
        protected int waveCount;

        protected void Awake()
        {
            var waves = GetComponentsInChildren<Wave>().ToList();
            waveCount = waves.Count;
            foreach (var wave in waves)
            {
                wave.OnAllUnitsDestroyed.AddListener(OnWaveDefeated);
            }
        }

        public void OnWaveDefeated()
        {
            waveCount--;
            if (waveCount <= 0) onAllWavesDefeated?.Invoke();
        }
    }
}