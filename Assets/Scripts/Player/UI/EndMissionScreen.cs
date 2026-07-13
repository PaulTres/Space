using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Player
{
    public class EndMissionScreen : MenuPDA
    {
        [SerializeField]
        protected TextMeshProUGUI playerKills, teamCasualties,
            enemyCasualties, outcomeText;

        protected override void OnEnable()
        {
            base.OnEnable();
            PopulateText(GameManager.UnitsDestroyed);
        }

        public void PopulateText(List<ObjectDestroyed> unitsDestroyed)
        {
            var score = 0;
            unitsDestroyed.Sort((a, b) =>
            {
                return a.KillerName.CompareTo(b.KillerName);
            });

            for (int i = 0; i < unitsDestroyed.Count; i++)
            {
                if (unitsDestroyed[i].VictimTeam == GlobalConfig.PlayerTeam)
                {
                    teamCasualties.text += unitsDestroyed[i] + "\n";
                }
                else if (unitsDestroyed[i].VictimTeam == GlobalConfig.EnemyTeam)
                {
                    if (unitsDestroyed[i].KillerName.Contains("Player"))
                    {
                        playerKills.text += unitsDestroyed[i] + "\n";
                    }
                    else enemyCasualties.text += unitsDestroyed[i] + "\n";
                }
            }

            outcomeText.text = score > 0 ? "Victory" : score < 0 ? "Defeat" : "Stalemate";
        }

        public void ReturnToLobby()
        {
            GameManager.LoadLobby();
        }
    }
}