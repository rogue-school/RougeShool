using System;
using System.Collections;
using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Core
{
    public class CombatFlowCoordinator : MonoBehaviour, ICombatFlowCoordinator
    {
        [Inject] private IStageManager stageManager;
        [Inject] private IPlayerManager playerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private IPlayerHandManager playerHandManager;
        [Inject] private IEnemyHandManager enemyHandManager;
        [Inject] private ITurnCardRegistry turnCardRegistry;
        [Inject] private ICombatPreparationService preparationService;
        [Inject] private ISlotRegistry slotRegistry;

        private ICombatTurnManager turnManager;
        private ICombatStateFactory stateFactory;
        private bool playerInputEnabled = false;

        public void InjectTurnStateDependencies(ICombatTurnManager turnManager, ICombatStateFactory stateFactory)
        {
            this.turnManager = turnManager;
            this.stateFactory = stateFactory;
        }

        public void StartCombatFlow()
        {
            var initialState = stateFactory.CreatePrepareState();
            turnManager.ChangeState(initialState);
        }

        public void RequestCombatPreparation(Action<bool> onComplete)
        {
            StartCoroutine(PerformCombatPreparation(onComplete));
        }

        public IEnumerator PerformCombatPreparation()
        {
            yield return PerformCombatPreparation(_ => { });
        }

        public IEnumerator PerformCombatPreparation(Action<bool> onComplete)
        {
            Debug.Log("[CombatFlowCoordinator] 이전 적 핸드 정리");
            enemyHandManager.ClearHand();

            var enemy = enemyManager.GetEnemy();
            if (enemy == null)
            {
                Debug.LogWarning("[CombatFlowCoordinator] 적이 존재하지 않습니다. 초기화 실패");
                onComplete?.Invoke(false);
                yield break;
            }

            Debug.Log($"[CombatFlowCoordinator] 적 사용 가능: {enemy.GetName()}");
            yield return new WaitForSeconds(0.5f);

            // 적 선공 여부 결정
            bool enemyFirst = UnityEngine.Random.value < 0.5f;
            var slotToRegister = enemyFirst ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND;

            // 카드 추출 및 등록
            var (card, cardUI) = enemyHandManager.PopCardFromSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
            if (card != null)
            {
                Debug.Log($"[CombatFlowCoordinator] 적 카드 등록: {card.GetCardName()} → 슬롯: {slotToRegister}");

                turnCardRegistry.RegisterCard(slotToRegister, card, cardUI);
                RegisterCardToCombatSlotUI(slotToRegister, card, cardUI);
                enemyHandManager.RegisterCardToSlot(SkillCardSlotPosition.ENEMY_SLOT_3, card, cardUI);
            }

            enemyHandManager.FillEmptySlots();
            yield return new WaitForSeconds(0.5f);

            Debug.Log("[CombatFlowCoordinator] 전투 준비 완료");
            onComplete?.Invoke(true);
        }

        private void RegisterCardToCombatSlotUI(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            var slot = slotRegistry.GetCombatSlot(pos);
            if (slot is ICombatCardSlot combatSlot)
            {
                ui.transform.SetParent(combatSlot.GetTransform());
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localScale = Vector3.one;
                combatSlot.SetCard(card);
                combatSlot.SetCardUI(ui);

                Debug.Log($"[CombatFlowCoordinator] CombatSlot에 카드 UI 적용 완료: {card.GetCardName()} → {pos}");
            }
            else
            {
                Debug.LogError($"[CombatFlowCoordinator] 전투 슬롯 {pos}을 찾을 수 없거나 ICombatExecutionSlot 아님");
            }
        }

        public void RequestFirstAttack(Action onComplete = null)
        {
            StartCoroutine(PerformFirstAttack(onComplete));
        }

        public IEnumerator PerformFirstAttack() => PerformFirstAttack(null);

        public IEnumerator PerformFirstAttack(Action onComplete)
        {
            Debug.Log("[CombatFlowCoordinator] 첫 번째 공격 시작");
            yield return new WaitForSeconds(1f);
            Debug.Log("[CombatFlowCoordinator] 첫 번째 공격 종료");
            onComplete?.Invoke();
        }

        public IEnumerator PerformSecondAttack()
        {
            Debug.Log("[CombatFlowCoordinator] 두 번째 공격 시작");
            yield return new WaitForSeconds(1f);
            Debug.Log("[CombatFlowCoordinator] 두 번째 공격 종료");
        }

        public IEnumerator PerformResultPhase()
        {
            Debug.Log("[CombatFlowCoordinator] 결과 처리 시작");
            yield return new WaitForSeconds(1f);
            Debug.Log("[CombatFlowCoordinator] 결과 처리 완료");
        }

        public IEnumerator PerformVictoryPhase()
        {
            Debug.Log("[CombatFlowCoordinator] 승리 처리 시작");
            yield return new WaitForSeconds(1f);
            Debug.Log("[CombatFlowCoordinator] 승리 처리 완료");
        }

        public IEnumerator PerformGameOverPhase()
        {
            Debug.Log("[CombatFlowCoordinator] 게임 오버 처리 시작");
            yield return new WaitForSeconds(1f);
            Debug.Log("[CombatFlowCoordinator] 게임 오버 처리 완료");
        }

        public void EnablePlayerInput() => playerInputEnabled = true;
        public void DisablePlayerInput() => playerInputEnabled = false;
        public bool IsPlayerInputEnabled() => playerInputEnabled;

        public bool IsPlayerDead() => playerManager.GetPlayer() == null;
        public bool IsEnemyDead() => enemyManager.GetEnemy() == null;
        public bool CheckHasNextEnemy() => stageManager.HasNextEnemy();

        public void CleanupAfterVictory()
        {
            enemyHandManager.ClearHand();
            Debug.Log("[CombatFlowCoordinator] 승리 후 적 핸드 정리 완료");
        }
    }
}
