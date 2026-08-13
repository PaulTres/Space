using UnityEngine;

namespace Weapons
{
    public interface IThreat
    {
        event System.Action<IThreat> OnThreatOver;

        /// <summary>
        /// The direction a ship should dodge. Used so AI don't facetank obvious dangers.
        /// </summary>
        /// <param name="threatenedObject"></param>
        /// <returns></returns>
        Vector3 DodgeDirection(ImprovedMonoBehaviour threatenedObject);
    }
}