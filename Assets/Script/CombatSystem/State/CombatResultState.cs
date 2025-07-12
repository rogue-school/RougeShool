using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.Utility;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem;
using Game.IManager;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.State
{
    public class CombatResultState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly ISlotRegistry slotRegistry;
        private readonly IPlayerHandManager playerHandManager;

        public CombatResultState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICoroutineRunner coroutineRunner,
            ISlotRegistry slotRegistry,
            IPlayerHandManager playerHandManager)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.coroutineRunner = coroutineRunner;
            this.slotRegistry = slotRegistry;
            this.playerHandManager = playerHandManager;
        }

        public void EnterState()
        {
            flowCoordinator.DisablePlayerInput();
            coroutineRunner.RunCoroutine(ExecuteResultPhase());
        }

        private IEnumerator ExecuteResultPhase()
        {
            // 1. 턴 종료 시 가드 상태 해제
            ClearGuardStates();
            yield return new WaitForEndOfFrame();

            // 2. 플레이어 카드 복귀
            ReturnPlayerCardsToHand();
            yield return new WaitForEndOfFrame();

            // 3. 전투 시각 효과 및 UI 정리
            yield return flowCoordinator.PerformResultPhase();
            yield return new WaitForEndOfFrame();

            // 4. 사망 판정
            if (flowCoordinator.IsEnemyDead())
            {
                // 적 캐릭터 사망 시 스킬카드 소멸 애니메이션 실행
                // flowCoordinator에서 이미 구현된 메서드 사용
                yield return new WaitForSeconds(0.5f); // 소멸 애니메이션을 위한 대기 시간
                
                // CombatEvents.RaiseCombatEnded(true); // 승리 (일시적으로 주석 처리)

                flowCoordinator.RemoveEnemyCharacter();
                yield return flowCoordinator.ClearEnemyHandSafely();
                yield return new WaitForSeconds(0.2f);

                if (!flowCoordinator.CheckHasNextEnemy())
                {
                    yield return new WaitForEndOfFrame();

                    turnManager.RequestStateChange(turnManager.GetStateFactory().CreateVictoryState());
                    yield break;
                }
            }
            else if (flowCoordinator.IsPlayerDead())
            {
                // CombatEvents.RaiseCombatEnded(false); // 패배 (일시적으로 주석 처리)
                yield return new WaitForSeconds(0.1f);

                turnManager.RequestStateChange(turnManager.GetStateFactory().CreateGameOverState());
                yield break;
            }

            // 5. 전투 지속 → 다음 준비 상태로 전이
            yield return new WaitForSeconds(0.1f);
            turnManager.RequestStateChange(turnManager.GetStateFactory().CreatePrepareState());
        }

        /// <summary>
        /// 턴 종료 시 모든 캐릭터의 가드 상태를 해제합니다.
        /// </summary>
        private void ClearGuardStates()
        {
            // 레거시 방식의 가드 상태 해제 (서비스가 없는 경우 사용)
            ClearGuardStatesLegacy();
        }

        /// <summary>
        /// 레거시 방식의 가드 상태 해제 (서비스가 없는 경우 사용)
        /// </summary>
        private void ClearGuardStatesLegacy()
        {
            // 플레이어 가드 상태 해제
            var playerManager = flowCoordinator.GetType().GetField("playerManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(flowCoordinator) as IPlayerManager;
            var player = playerManager?.GetPlayer();
            if (player != null && player.IsGuarded())
            {
                player.SetGuarded(false);
                Debug.Log($"[CombatResultState] 플레이어 가드 상태 해제: {player.GetCharacterName()}");
            }

            // 적 가드 상태 해제
            var enemy = flowCoordinator.GetEnemy();
            if (enemy != null && enemy.IsGuarded())
            {
                enemy.SetGuarded(false);
                Debug.Log($"[CombatResultState] 적 가드 상태 해제: {enemy.GetCharacterName()}");
            }
        }

        private void ReturnPlayerCardsToHand()
        {
            foreach (CombatSlotPosition pos in System.Enum.GetValues(typeof(CombatSlotPosition)))
            {
                var card = flowCoordinator.GetCardInSlot(pos);
                if (card != null && card.IsFromPlayer())
                {
                    playerHandManager.RemoveCard(card);
                    playerHandManager.RestoreCardToHand(card);

                    var slot = slotRegistry.GetCombatSlot(pos);
                    slot?.ClearAll();
                }
            }
        }

        public void ExecuteState() { }



        public void ExitState()
        {
        }
    }
}
