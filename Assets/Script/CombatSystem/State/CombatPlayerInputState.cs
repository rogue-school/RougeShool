using UnityEngine;
using Game.CombatSystem.Manager;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 플레이어의 입력을 처리하는 전투 상태입니다.
    /// 카드 선택 UI 활성화, 시작 버튼 등록 등을 수행합니다.
    /// </summary>
    public class CombatPlayerInputState
    {
        #region 필드

        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합
        private readonly TurnManager turnManager;

        private bool hasStarted = false;

        #endregion

        #region 생성자

        /// <summary>
        /// CombatPlayerInputState의 생성자입니다.
        /// </summary>
        public CombatPlayerInputState(
            TurnManager turnManager)
        {
            this.turnManager = turnManager;
        }

        #endregion

        #region 상태 인터페이스 구현

        /// <summary>
        /// 상태 진입 시 호출됩니다. UI 및 입력을 활성화합니다.
        /// </summary>
        public void EnterState()
        {
            GameLogger.LogInfo("[STATE] CombatPlayerInputState 진입", GameLogger.LogCategory.Combat);
            hasStarted = false;

            // Note: UI management is now handled by the simplified architecture
            GameLogger.LogInfo("[CombatPlayerInputState] 플레이어 입력 대기 상태", GameLogger.LogCategory.Combat);
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
            GameLogger.LogInfo("[STATE] CombatPlayerInputState 종료", GameLogger.LogCategory.Combat);
            // Note: UI management is now handled by the simplified architecture
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
                GameLogger.LogWarning("[CombatPlayerInputState] 이미 시작 버튼이 눌렸습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            hasStarted = true;
            GameLogger.LogInfo("[STATE] CombatPlayerInputState → CombatAttackState 전이", GameLogger.LogCategory.Combat);

            // Note: State transitions are now handled by the simplified TurnManager
            turnManager.NextTurn();
        }

        #endregion
    }
}
