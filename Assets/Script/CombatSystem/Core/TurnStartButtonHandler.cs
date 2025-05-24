using UnityEngine;
using UnityEngine.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Core
{
    public class TurnStartButtonHandler : MonoBehaviour
    {
        [SerializeField] private Button startButton;

        private ITurnStartConditionChecker conditionChecker;
        private ITurnStateController turnStateController;
        private ICombatStateFactory stateFactory;

        public void Inject(
            ITurnStartConditionChecker conditionChecker,
            ITurnStateController turnStateController,
            ICombatStateFactory stateFactory)
        {
            this.conditionChecker = conditionChecker;
            this.turnStateController = turnStateController;
            this.stateFactory = stateFactory;
        }

        private void Awake()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartButtonClicked);
        }

        private void OnDestroy()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        private void Update()
        {
            if (startButton != null && conditionChecker != null)
                startButton.interactable = conditionChecker.CanStartTurn();
        }

        private void OnStartButtonClicked()
        {
            if (conditionChecker != null && conditionChecker.CanStartTurn())
            {
                Debug.Log("[TurnStartButtonHandler] 전투 시작 버튼 클릭됨");

                var nextState = stateFactory.CreateFirstAttackState();
                turnStateController.RequestStateChange(nextState);
            }
        }
    }
}
