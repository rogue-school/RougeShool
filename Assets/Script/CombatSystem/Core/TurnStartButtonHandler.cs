using UnityEngine;
using UnityEngine.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Core
{
    public class TurnStartButtonHandler : MonoBehaviour
    {
        [SerializeField] private Button turnStartButton;

        private ICombatTurnManager turnManager;
        private ITurnStartConditionChecker turnConditionChecker;

        public void Inject(ICombatTurnManager manager, ITurnStartConditionChecker checker)
        {
            turnManager = manager;
            turnConditionChecker = checker;
        }

        private void Start()
        {
            if (turnStartButton == null)
                turnStartButton = GetComponent<Button>();

            if (turnStartButton != null)
                turnStartButton.onClick.AddListener(OnTurnStartClicked);

            SetInteractable(false);
        }

        private void Update()
        {
            if (turnStartButton == null || turnConditionChecker == null)
                return;

            SetInteractable(turnConditionChecker.CanStartTurn());
        }

        private void SetInteractable(bool value)
        {
            if (turnStartButton.interactable != value)
                turnStartButton.interactable = value;
        }

        private void OnTurnStartClicked()
        {
            SetInteractable(false);
            turnManager?.ExecuteCombat();
        }
    }
}
