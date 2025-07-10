using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Runtime;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 플레이어의 입력을 처리하는 전투 상태입니다.
    /// 쿨타임 감소, 카드 선택 UI 활성화, 시작 버튼 등록 등을 수행합니다.
    /// </summary>
    public class CombatPlayerInputState : ICombatTurnState
    {
        #region 필드

        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ITurnCardRegistry cardRegistry;
        private readonly ICombatTurnManager turnManager;
        private readonly SkillCardCooldownSystem cooldownSystem;

        private bool hasStarted = false;

        #endregion

        #region 생성자

        /// <summary>
        /// CombatPlayerInputState의 생성자입니다.
        /// </summary>
        public CombatPlayerInputState(
            ICombatFlowCoordinator flowCoordinator,
            ITurnCardRegistry cardRegistry,
            ICombatTurnManager turnManager,
            SkillCardCooldownSystem cooldownSystem)
        {
            this.flowCoordinator = flowCoordinator;
            this.cardRegistry = cardRegistry;
            this.turnManager = turnManager;
            this.cooldownSystem = cooldownSystem;
        }

        #endregion

        #region 상태 인터페이스 구현

        /// <summary>
        /// 상태 진입 시 호출됩니다. 쿨타임을 감소시키고 UI 및 입력을 활성화합니다.
        /// </summary>
        public void EnterState()
        {
            Debug.Log("[CombatPlayerInputState] EnterState");
            hasStarted = false;

            cooldownSystem.ReduceAllCooldowns();

            flowCoordinator.EnablePlayerInput();
            flowCoordinator.ShowPlayerCardSelectionUI();
            flowCoordinator.DisableStartButton();

            flowCoordinator.RegisterStartButton(OnStartButtonPressed);
        }

        /// <summary>
        /// 상태 반복 실행 중 호출됩니다. 현재는 구현되어 있지 않음.
        /// </summary>
        public void ExecuteState()
        {
            // 입력 대기 상태에서 반복 동작이 필요한 경우 확장 가능
        }

        /// <summary>
        /// 상태 종료 시 호출됩니다. UI 및 입력을 정리합니다.
        /// </summary>
        public void ExitState()
        {
            Debug.Log("[CombatPlayerInputState] ExitState");
            flowCoordinator.DisablePlayerInput();
            flowCoordinator.HidePlayerCardSelectionUI();
            flowCoordinator.UnregisterStartButton();
            flowCoordinator.DisableStartButton();
        }

        #endregion

        #region 내부 로직

        /// <summary>
        /// 시작 버튼이 눌렸을 때 호출되는 콜백입니다.
        /// 상태 전이를 요청합니다.
        /// </summary>
        private void OnStartButtonPressed()
        {
            if (hasStarted)
            {
                Debug.LogWarning("[CombatPlayerInputState] 이미 시작 버튼이 눌렸습니다.");
                return;
            }

            hasStarted = true;
            Debug.Log("[CombatPlayerInputState] 상태 전이: FirstAttackState");

            flowCoordinator.DisableStartButton();
            flowCoordinator.DisablePlayerInput();
            flowCoordinator.HidePlayerCardSelectionUI();
            flowCoordinator.UnregisterStartButton();

            var next = turnManager.GetStateFactory().CreateFirstAttackState();
            turnManager.RequestStateChange(next);
        }

        #endregion
    }
}
