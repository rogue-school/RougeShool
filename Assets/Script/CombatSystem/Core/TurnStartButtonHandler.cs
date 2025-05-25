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

            Debug.Log("[TurnStartButtonHandler] Inject() 호출됨");
            Debug.Log($" → conditionChecker: {(conditionChecker != null ? "OK" : "NULL")}");
            Debug.Log($" → turnStateController: {(turnStateController != null ? "OK" : "NULL")}");
            Debug.Log($" → stateFactory: {(stateFactory != null ? "OK" : "NULL")}");
        }

        private void Awake()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
                Debug.Log("[TurnStartButtonHandler] startButton 클릭 리스너 등록됨");
            }
            else
            {
                Debug.LogWarning("[TurnStartButtonHandler] startButton이 할당되지 않았습니다!");
            }
        }

        private void OnDestroy()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        private void Update()
        {
            if (startButton != null && conditionChecker != null)
            {
                bool canStart = conditionChecker.CanStartTurn();
                startButton.interactable = canStart;

            }
            else
            {
                if (startButton == null)
                    Debug.LogWarning("[TurnStartButtonHandler] startButton이 null입니다.");
                if (conditionChecker == null)
                    Debug.LogWarning("[TurnStartButtonHandler] conditionChecker가 null입니다.");
            }
        }

        private void OnStartButtonClicked()
        {
            if (conditionChecker != null && conditionChecker.CanStartTurn())
            {
                Debug.Log("[TurnStartButtonHandler] 전투 시작 버튼 클릭됨");

                var nextState = stateFactory?.CreateFirstAttackState();
                if (nextState != null)
                {
                    Debug.Log($"[TurnStartButtonHandler] 상태 전이 요청됨 → {nextState.GetType().Name}");
                    turnStateController?.RequestStateChange(nextState);
                }
                else
                {
                    Debug.LogError("[TurnStartButtonHandler] CreateFirstAttackState() 반환값이 null입니다.");
                }
            }
            else
            {
                Debug.LogWarning("[TurnStartButtonHandler] 버튼 클릭 시 조건 불충족 또는 conditionChecker가 null입니다.");
            }
        }
    }
}
