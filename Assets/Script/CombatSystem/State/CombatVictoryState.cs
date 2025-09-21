using UnityEngine;
using System.Collections;
using Game.CombatSystem.Manager;
using Game.UtilitySystem;
using Game.CoreSystem.Utility;
using Game.CombatSystem;

namespace Game.CombatSystem.State
{
    public class CombatVictoryState
    {
        private readonly TurnManager turnManager;
        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합
        private readonly ICoroutineRunner coroutineRunner;

        public CombatVictoryState(
            TurnManager turnManager,
            ICoroutineRunner coroutineRunner)
        {
            this.turnManager = turnManager;
            this.coroutineRunner = coroutineRunner;
        }

        public void EnterState()
        {
            GameLogger.LogInfo("CombatVictoryState 진입", GameLogger.LogCategory.Combat);
            CombatEvents.RaiseVictory();
            coroutineRunner.RunCoroutine(HandleVictory());
        }

        private IEnumerator HandleVictory()
        {
            GameLogger.LogInfo("승리 처리 중...", GameLogger.LogCategory.Combat);
            yield return new WaitForSeconds(1.0f);

            // 승리 UI 표시 (향후 구현)
            GameLogger.LogInfo("스테이지 완료 - 승리!", GameLogger.LogCategory.Combat);
            
            // 다음 스테이지로 진행하거나 메인 화면으로 복귀 (향후 구현)
            GameLogger.LogInfo("승리 처리 완료", GameLogger.LogCategory.Combat);
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            GameLogger.LogInfo("CombatVictoryState 종료", GameLogger.LogCategory.Combat);
        }
    }
}
