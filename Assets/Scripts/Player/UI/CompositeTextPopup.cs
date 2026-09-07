using UnityEngine;
namespace Player
{
    public class CompositeTextPopup : TextPopup
    {
        [SerializeField] protected TextPopup[] textPopups;
        public override string Text
        {
            get
            {
                string text = "";
                foreach (var popup in textPopups)
                {
                    text += popup.Text + " ";
                }
                return text.Trim();
            }
        }
    }
}
