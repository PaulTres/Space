using Level;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Player
{
    public class EndMissionScreen : MenuPDA
    {
        [SerializeField] protected TextMeshProUGUI playerKills, afterActionReport;

        protected override void OnEnable()
        {
            base.OnEnable();
            PopulateText(GameManager.UnitsDestroyed);
        }

        public void PopulateText(List<ObjectDestroyed> unitsDestroyed)
        {
            afterActionReport.text = Mission.CurrentMission.AfterActionReport;
            playerKills.text = "Kills:";
            var killCounts = unitsDestroyed.Where(a => a.KillerName.
                Contains("Player")).GroupBy(a => a.VictimName).
                Select(a => new KeyValuePair<string, int>(a.Key, a.Count()));
            foreach (var pair in killCounts)
            {
                playerKills.text += '\n' + pair.Value + " x " + pair.Key;
            }
            playerKills.SetAllDirty();
        }

        public void ReturnToLobby()
        {
            GameManager.LoadMissionSelection();
        }
    }
}