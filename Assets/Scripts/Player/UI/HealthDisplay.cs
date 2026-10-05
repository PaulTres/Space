using UnityEngine;
using UnityEngine.UI;
namespace Player
{
    public class HealthDisplay : MonoBehaviour
    {
        [field:SerializeField] public Image HealthImage { get; protected set; }
    }
}
