using TMPro;
using UnityEngine;
namespace HybridBT2
{
    public class BTDebug : MonoBehaviour
    {
        public BT Tree;
        public TextMeshProUGUI text;

        private void Update()
        {
            if (Tree != null && text != null)
            {
                text.text = Tree.Root.GetInfo(0);
            }
        }
    }
}
