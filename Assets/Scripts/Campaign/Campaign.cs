using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Missions/Campaign", fileName = "Campaign")]
public class Campaign : ScriptableObject
{
    public List<Mission> Missions = new();
}
