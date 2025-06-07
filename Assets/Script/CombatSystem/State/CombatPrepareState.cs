using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.Utility;
using Game.CombatSystem.Context;

namespace Game.CombatSystem.State
{
    public class CombatPrepareState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly IPlayerHandManager playerHandManager;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly ISlotRegistry slotRegistry;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly TurnContext turnContext;

        public CombatPrepareState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            IPlayerHandManager playerHandManager,
            IEnemyHandManager enemyHandManager,
            ISlotRegistry slotRegistry,
            ICoroutineRunner coroutineRunner,
            TurnContext turnContext)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.playerHandManager = playerHandManager;
            this.enemyHandManager = enemyHandManager;
            this.slotRegistry = slotRegistry;
            this.coroutineRunner = coroutineRunner;
            this.turnContext = turnContext;
        }

        public void EnterState()
        {
            Debug.Log("<color=cyan>[CombatPrepareState] 상태 진입</color>");
            turnContext.Reset();
            coroutineRunner.RunCoroutine(PrepareRoutine());
        }

        private IEnumerator PrepareRoutine()
        {
            yield return new WaitUntil(() => slotRegistry.IsInitialized);

            ReturnPlayerCardToHand();
            yield return new WaitForEndOfFrame();

            // 적 없으면 생성
            if (!flowCoordinator.HasEnemy())
            {
                flowCoordinator.SpawnNextEnemy();
                yield return new WaitForSeconds(0.5f);
            }

            // 적 핸드 초기화
            var enemy = flowCoordinator.GetEnemy();
            if (enemy != null && !enemyHandManager.HasInitializedEnemy(enemy))
            {
                enemyHandManager.Initialize(enemy);
                yield return new WaitForEndOfFrame();
            }

            // 전투 슬롯 UI 정리
            flowCoordinator.ClearEnemyCombatSlots();
            yield return new WaitForEndOfFrame();

            // 핸드 슬롯 이동 + 새 카드 생성
            yield return enemyHandManager.StepwiseFillSlotsFromBack(0.3f);

            // 적 카드 준비 대기
            yield return WaitForEnemyCardReady();

            // SLOT_1에서 전투 슬롯 등록
            RegisterEnemyCardToSlot();

            // 비워진 핸드 슬롯을 다시 채움
            yield return enemyHandManager.StepwiseFillSlotsFromBack(0.3f);

            // 다음 상태로
            var next = turnManager.GetStateFactory().CreatePlayerInputState();
            turnManager.RequestStateChange(next);
        }


        private IEnumerator WaitForEnemyCardReady()
        {
            Debug.Log("[CombatPrepareState] 적 카드 등록 준비 대기 중...");
            while (true)
            {
                var (card, ui) = enemyHandManager.PeekCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
                if (card != null && ui != null)
                    break;

                yield return null;
            }
            Debug.Log("[CombatPrepareState] 적 카드 등록 준비 완료");
        }

        private void RegisterEnemyCardToSlot()
        {
            var (card, ui) = enemyHandManager.PopCardFromSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (card == null || ui == null)
            {
                Debug.LogWarning("[CombatPrepareState] 카드 또는 UI가 null입니다.");
                return;
            }

            var slotPos = flowCoordinator.IsEnemyFirst ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND;

            flowCoordinator.RegisterCardToCombatSlot(slotPos, card, ui);
            flowCoordinator.GetTurnCardRegistry().RegisterCard(slotPos, card, ui, SlotOwner.ENEMY);

            Debug.Log($"[CombatPrepareState] 적 카드 등록 완료 → {card.GetCardName()} in {slotPos}");
        }

        private void ReturnPlayerCardToHand()
        {
            foreach (CombatSlotPosition pos in System.Enum.GetValues(typeof(CombatSlotPosition)))
            {
                var card = flowCoordinator.GetCardInSlot(pos);
                if (card != null && card.IsFromPlayer())
                {
                    playerHandManager.RemoveCard(card);
                    playerHandManager.RestoreCardToHand(card);
                    slotRegistry.GetCombatSlot(pos)?.ClearAll();
                    Debug.Log($"[CombatPrepareState] 카드 복귀 완료: {card.GetCardName()}");
                }
            }
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=grey>[CombatPrepareState] 상태 종료</color>");
        }
    }
}
