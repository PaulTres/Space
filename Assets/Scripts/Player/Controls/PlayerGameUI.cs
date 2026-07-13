using HP;
using UnityEngine;
using UnityEngine.UI;
namespace Player
{
    public class PlayerGameUI : MonoBehaviour
    {
        [SerializeField] protected UIInputReader inputReader;
        [SerializeField] protected GameObject pauseMenu;
        [SerializeField] protected Image hullImage;
        protected HPComponent hpComponent;

        protected void Awake()
        {
            inputReader.EnablePlayerActions();
            hpComponent = GetComponentInParent<HPComponent>();
            hpComponent.OnDamageTaken += UpdateHullImage;
        }

        protected void OnEnable()
        {
            inputReader.OnEscapeEvent += OnEscapeEvent;
            hullImage.fillAmount = 1;
        }

        protected void OnDisable()
        {
            inputReader.DisablePlayerActions();
        }

        protected void UpdateHullImage()
        {
            hullImage.fillAmount = hpComponent.CurrentHP / hpComponent.MaxHP;
        }

        protected void OnEscapeEvent()
        {
            pauseMenu.SetActive(true);
        }
    }
}
