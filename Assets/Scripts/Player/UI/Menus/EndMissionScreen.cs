using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Player
{
    public class EndMissionScreen : MenuPDA
    {
        [SerializeField] protected TextMeshProUGUI playerKills;

        protected override void OnEnable()
        {
            base.OnEnable();
            PopulateText(GameManager.UnitsDestroyed);
        }

        public void PopulateText(List<ObjectDestroyed> unitsDestroyed)
        {
            playerKills.text = "Kills:";
            var killCounts = unitsDestroyed.Where(a => a.KillerName.
                Contains("Player")).GroupBy(a => a.VictimName).
                Select(a => new KeyValuePair<string, int>(a.Key, a.Count()));
            foreach (var pair in killCounts)
            {
                playerKills.text += '\n' + pair.Key + " x " + pair.Value;
            }
            playerKills.SetAllDirty();
        }

        public void ReturnToLobby()
        {
            GameManager.LoadMissionSelection();
        }
    }
}