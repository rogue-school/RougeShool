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
        private readonly CombatSlotManager slotManager;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IPlayerHandManager playerHandManager;

        public CombatResultState(
            TurnManager turnManager,
            CombatSlotManager slotManager,
            ICoroutineRunner coroutineRunner,
            IPlayerHandManager playerHandManager)
        {
            this.turnManager = turnManager;
            this.slotManager = slotManager;
            this.coroutineRunner = coroutineRunner;
            this.playerHandManager = playerHandManager;
        }

        public void EnterState()
        {
            Debug.Log("<color=cyan>[STATE] CombatResultState 진입</color>");
            coroutineRunner.RunCoroutine(ExecuteResultPhase());
        }

        private IEnumerator ExecuteResultPhase()
        {
            Debug.Log("[CombatResultState] 결과 처리 중...");
            
            // 1. 플레이어 카드 복귀
            ReturnPlayerCardsToHand();
            yield return new WaitForEndOfFrame();

            // 2. 사망 판정 및 연출 타이밍 제어
            // Note: Death checking is now handled by the simplified architecture
            yield return new WaitForSeconds(0.5f);

            // 3. 다음 턴으로 전환
            Debug.Log("<color=cyan>[STATE] CombatResultState → 다음 턴으로 전환</color>");
            turnManager.NextTurn();
        }

        private void ReturnPlayerCardsToHand()
        {
            // Note: Card return logic is now handled by the simplified architecture
            Debug.Log("[CombatResultState] 플레이어 카드 복귀 처리");
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=cyan>[STATE] CombatResultState 종료</color>");
        }
    }
}
