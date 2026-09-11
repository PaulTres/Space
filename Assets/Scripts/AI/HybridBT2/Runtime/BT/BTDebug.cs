using TMPro;
using UnityEngine;
namespace HybridBT2
{
    public class BTDebug : MonoBehaviour
    {
        public BT Tree;
        public TextMeshProUGUI text;

        public void OnValidate()
        {
            if (text == null) text = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (Tree != null && text != null)
            {
                text.text = Tree.Root.GetInfo(0);
            }
        }
    }
}
