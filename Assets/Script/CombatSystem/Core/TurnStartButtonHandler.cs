using UnityEngine;
using UnityEngine.UI;
using Game.CoreSystem.Utility;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Service;

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

        private DefaultTurnStartConditionChecker conditionChecker;
        private TurnManager turnManager;

        private bool isInjected = false;

        /// <summary>
        /// Zenject 의존성 주입 메서드
        /// </summary>
        public void Inject(
            DefaultTurnStartConditionChecker conditionChecker,
            TurnManager turnManager)
        {
            this.conditionChecker = conditionChecker;
            this.turnManager = turnManager;

            isInjected = true;
        }

        #region 유니티 생명주기 메서드

        private void Awake()
        {
            if (startButton == null)
            {
                GameLogger.LogWarning("[TurnStartButtonHandler] startButton이 할당되지 않았습니다!", GameLogger.LogCategory.Combat);
                return;
            }

            startButton.onClick.AddListener(OnStartButtonClicked);
            startButton.interactable = false; // 초기에는 비활성화
        }

        private void OnDestroy()
        {
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

            // TODO: conditionChecker가 object 타입이므로 적절한 캐스팅 필요
            // bool canStart = conditionChecker.CanStartTurn();
            bool canStart = conditionChecker?.CanStartTurn() ?? false;
            startButton.interactable = canStart;
        }

        /// <summary>
        /// 버튼 클릭 시 턴을 진행합니다.
        /// </summary>
        private void OnStartButtonClicked()
        {
            // TODO: conditionChecker가 object 타입이므로 적절한 캐스팅 필요
            // if (!isInjected || conditionChecker == null || !conditionChecker.CanStartTurn())
            if (!isInjected || conditionChecker == null || !(conditionChecker?.CanStartTurn() ?? false))
            {
                GameLogger.LogWarning("[TurnStartButtonHandler] 버튼 클릭 조건 불충족", GameLogger.LogCategory.Combat);
                return;
            }

            // 레거시: 상태 패턴으로 전환되어 이 버튼은 사용되지 않음
            // turnManager?.NextTurn(); // 제거됨
            GameLogger.LogWarning("[TurnStartButtonHandler] 레거시 버튼 - 상태 패턴에서 자동으로 턴 진행됨", GameLogger.LogCategory.Combat);
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
