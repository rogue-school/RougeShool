using UnityEngine;
using System.Collections;
using Game.CombatSystem.Manager;
using Game.UtilitySystem;
using Game.CoreSystem.Utility;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.State
{
    public class CombatResultState
    {
        private readonly TurnManager turnManager;
        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IPlayerHandManager playerHandManager;

        public CombatResultState(
            TurnManager turnManager,
            ICoroutineRunner coroutineRunner,
            IPlayerHandManager playerHandManager)
        {
            this.turnManager = turnManager;
            this.coroutineRunner = coroutineRunner;
            this.playerHandManager = playerHandManager;
        }

        public void EnterState()
        {
            GameLogger.LogInfo("[STATE] CombatResultState 진입", GameLogger.LogCategory.Combat);
            coroutineRunner.RunCoroutine(ExecuteResultPhase());
        }

        private IEnumerator ExecuteResultPhase()
        {
            GameLogger.LogInfo("[CombatResultState] 결과 처리 중...", GameLogger.LogCategory.Combat);
            
            // 1. 플레이어 카드 복귀
            ReturnPlayerCardsToHand();
            yield return new WaitForEndOfFrame();

            // 2. 사망 판정 및 연출 타이밍 제어
            // Note: Death checking is now handled by the simplified architecture
            yield return new WaitForSeconds(0.5f);

            // 3. 다음 턴으로 전환
            GameLogger.LogInfo("[STATE] CombatResultState → 다음 턴으로 전환", GameLogger.LogCategory.Combat);
            turnManager.NextTurn();
        }

        private void ReturnPlayerCardsToHand()
        {
            // Note: Card return logic is now handled by the simplified architecture
            GameLogger.LogInfo("[CombatResultState] 플레이어 카드 복귀 처리", GameLogger.LogCategory.Combat);
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            GameLogger.LogInfo("[STATE] CombatResultState 종료", GameLogger.LogCategory.Combat);
        }
    }
}
