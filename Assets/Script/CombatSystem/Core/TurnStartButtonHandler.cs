using UnityEngine;
using UnityEngine.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 전투 시작 버튼을 관리하는 컴포넌트입니다.
    /// 조건에 따라 버튼의 활성화를 제어하고, 클릭 시 턴 상태를 전이합니다.
    /// </summary>
    public class TurnStartButtonHandler : MonoBehaviour
    {
        [Header("전투 시작 버튼")]
        [SerializeField] private Button startButton;

        private ITurnStartConditionChecker conditionChecker;
        private ICombatTurnManager turnManager;
        private ICombatStateFactory stateFactory;
        private ITurnCardRegistry cardRegistry;

        private bool isInjected = false;

        /// <summary>
        /// Zenject 의존성 주입 메서드
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

        #region 유니티 생명주기 메서드

        private void Awake()
        {
            if (startButton == null)
            {
                Debug.LogWarning("[TurnStartButtonHandler] startButton이 할당되지 않았습니다!");
                return;
            }

            startButton.onClick.AddListener(OnStartButtonClicked);
            startButton.interactable = false; // 초기에는 비활성화
        }

        private void OnDestroy()
        {
            if (cardRegistry != null)
                cardRegistry.OnCardStateChanged -= EvaluateButtonInteractable;

            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        #endregion

        #region 내부 로직

        /// <summary>
        /// 카드 상태가 변경되었을 때 버튼 활성화 여부를 평가합니다.
        /// </summary>
        private void EvaluateButtonInteractable()
        {
            if (!isInjected || conditionChecker == null || startButton == null)
                return;

            bool canStart = conditionChecker.CanStartTurn();
            startButton.interactable = canStart;
        }

        /// <summary>
        /// 버튼 클릭 시 상태 전이를 요청합니다.
        /// </summary>
        private void OnStartButtonClicked()
        {
            if (!isInjected || conditionChecker == null || !conditionChecker.CanStartTurn())
            {
                Debug.LogWarning("[TurnStartButtonHandler] 버튼 클릭 조건 불충족");
                return;
            }

            var nextState = stateFactory?.CreateAttackState();
            if (nextState != null)
            {
                turnManager?.RequestStateChange(nextState);
                Debug.Log("<color=cyan>[TurnStartButtonHandler] 상태 전이 요청</color>");
            }
            else
            {
                Debug.LogError("[TurnStartButtonHandler] nextState 생성 실패");
            }
        }

        #endregion

        #region 외부 API

        /// <summary>
        /// 외부에서 수동으로 버튼 활성화 조건을 평가합니다.
        /// </summary>
        public void ForceEvaluate()
        {
            EvaluateButtonInteractable();
        }

        /// <summary>
        /// 외부에서 버튼의 활성화 상태를 강제로 설정합니다.
        /// </summary>
        public void SetInteractable(bool isEnabled)
        {
            if (startButton != null)
            {
                startButton.interactable = isEnabled;
            }
        }

        #endregion
    }
}
