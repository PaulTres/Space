using System.Collections.Generic;
using UnityEngine;
namespace Level
{
    [CreateAssetMenu(menuName = "Missions/Campaign", fileName = "Campaign")]
    public class Campaign : ScriptableObject
    {
        public List<Mission> Missions = new();
    }
}