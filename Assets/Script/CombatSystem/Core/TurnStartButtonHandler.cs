using UnityEngine;
using UnityEngine.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 전투 시작 버튼의 상태를 조건에 따라 관리하고, 클릭 시 상태 전이를 요청합니다.
    /// </summary>
    public class TurnStartButtonHandler : MonoBehaviour
    {
        [SerializeField] private Button startButton;

        private ITurnStartConditionChecker conditionChecker;
        private ICombatTurnManager turnManager;
        private ICombatStateFactory stateFactory;
        private ITurnCardRegistry cardRegistry;

        private bool isInjected = false;

        /// <summary>
        /// Zenject를 통해 의존성 주입됩니다.
        /// </summary>
        public void Inject(
            ITurnStartConditionChecker conditionChecker,
            ICombatTurnManager turnManager,
            ICombatStateFactory stateFactory,
            ITurnCardRegistry cardRegistry)
        {
            this.conditionChecker = conditionChecker;
            this.turnManager = turnManager;
            this.stateFactory = stateFactory;
            this.cardRegistry = cardRegistry;

            if (this.cardRegistry != null)
                this.cardRegistry.OnCardStateChanged += EvaluateButtonInteractable;

            isInjected = true;
        }

        private void Awake()
        {
            if (startButton == null)
            {
                Debug.LogWarning("[TurnStartButtonHandler] startButton이 할당되지 않았습니다!");
                return;
            }

            startButton.onClick.AddListener(OnStartButtonClicked);
            startButton.interactable = false;
        }

        private void OnDestroy()
        {
            if (cardRegistry != null)
                cardRegistry.OnCardStateChanged -= EvaluateButtonInteractable;

            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        /// <summary>
        /// 카드 상태 변화에 따라 버튼 활성화 여부를 평가합니다.
        /// </summary>
        private void EvaluateButtonInteractable()
        {
            if (!isInjected || conditionChecker == null || startButton == null)
                return;

            bool canStart = conditionChecker.CanStartTurn();
            startButton.interactable = canStart;
            Debug.Log($"[TurnStartButtonHandler] 버튼 {(canStart ? "활성화됨" : "비활성화됨")}");
        }

        /// <summary>
        /// 외부에서 수동으로 조건 평가를 트리거할 수 있도록 합니다.
        /// </summary>
        public void ForceEvaluate()
        {
            EvaluateButtonInteractable();
        }

        /// <summary>
        /// 버튼 클릭 시 호출됩니다. 조건을 만족하면 첫 공격 상태로 전이합니다.
        /// </summary>
        private void OnStartButtonClicked()
        {
            if (!isInjected || conditionChecker == null || !conditionChecker.CanStartTurn())
            {
                Debug.LogWarning("[TurnStartButtonHandler] 버튼 클릭 조건 불충족");
                return;
            }

            var nextState = stateFactory?.CreateFirstAttackState();
            if (nextState != null)
            {
                turnManager?.RequestStateChange(nextState);
                Debug.Log("[TurnStartButtonHandler] 상태 전이 요청됨");
            }
            else
            {
                Debug.LogError("[TurnStartButtonHandler] nextState 생성 실패");
            }
        }

        /// <summary>
        /// 외부에서 직접 버튼의 인터랙션 상태를 설정할 수 있습니다.
        /// </summary>
        public void SetInteractable(bool isEnabled)
        {
            if (startButton != null)
            {
                startButton.interactable = isEnabled;
                Debug.Log($"[TurnStartButtonHandler] 버튼 {(isEnabled ? "강제 활성화됨" : "강제 비활성화됨")}");
            }
        }
    }
}
