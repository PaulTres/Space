using TMPro;
using UnityEngine;
namespace Player
{
    public class WorldText : MonoBehaviour
    {
        protected TextMeshProUGUI text;
        public TextMeshProUGUI Text
        {
            get
            {
                if (text == null) text = GetComponentInChildren<TextMeshProUGUI>(true);
                return text;
            }
        }
        [SerializeField] protected TextProvider textProvider;

        protected void OnEnable()
        {
            Text.text = textProvider.Text;
        }
    }
}
