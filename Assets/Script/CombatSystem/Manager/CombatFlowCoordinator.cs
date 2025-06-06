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
using Game.SkillCardSystem.Executor;
using Game.CombatSystem.Context;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;

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
        [Inject] private ICardExecutor cardExecutor;

        private ICombatTurnManager turnManager;
        private ICombatStateFactory stateFactory;
        private TurnStartButtonHandler startButtonHandler;
        private bool playerInputEnabled = false;

        [Inject]
        public void Construct(TurnStartButtonHandler startButtonHandler)
        {
            this.startButtonHandler = startButtonHandler;
        }

        public void InjectTurnStateDependencies(ICombatTurnManager turnManager, ICombatStateFactory stateFactory)
        {
            this.turnManager = turnManager;
            this.stateFactory = stateFactory;
        }

        public void StartCombatFlow()
        {
            DisableStartButton();
        }

        public void RequestCombatPreparation(Action<bool> onComplete)
        {
            StartCoroutine(PerformCombatPreparation(onComplete));
        }
        public void RegisterCardToCombatSlot(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            var slot = slotRegistry.GetCombatSlot(pos);
            if (slot is ICombatCardSlot combatSlot)
            {
                ui.transform.SetParent(combatSlot.GetTransform());
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localScale = Vector3.one;
                combatSlot.SetCard(card);
                combatSlot.SetCardUI(ui);
            }
        }

        public ITurnCardRegistry GetTurnCardRegistry()
        {
            return turnCardRegistry;
        }


        public IEnumerator PerformCombatPreparation() => PerformCombatPreparation(_ => { });

        public IEnumerator PerformCombatPreparation(Action<bool> onComplete)
        {
            var enemy = enemyManager.GetEnemy();
            if (enemy == null)
            {
                Debug.LogWarning("[CombatFlowCoordinator] 적이 존재하지 않습니다.");
                onComplete?.Invoke(false);
                yield break;
            }

            yield return new WaitForSeconds(0.5f);

            bool enemyFirst = UnityEngine.Random.value < 0.5f;
            var slotToRegister = enemyFirst ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND;

            var (card, cardUI) = enemyHandManager.PopFirstAvailableCard();
            if (card != null)
            {
                turnCardRegistry.RegisterCard(slotToRegister, card, cardUI, SlotOwner.ENEMY);
                RegisterCardToCombatSlotUI(slotToRegister, card, cardUI);
                yield return new WaitForSeconds(0.6f);
            }
            else
            {
                onComplete?.Invoke(false);
                yield break;
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
            }
        }
        public void RemoveEnemyCharacter()
        {
            var enemy = enemyManager.GetEnemy();
            if (enemy == null)
            {
                Debug.LogWarning("[CombatFlowCoordinator] 적이 이미 제거된 상태입니다.");
                return;
            }

            if (enemy is EnemyCharacter concreteEnemy)
            {
                Destroy(concreteEnemy.gameObject);
                enemyManager.ClearEnemy();
                Debug.Log("[CombatFlowCoordinator] 적 캐릭터 제거 완료");
            }
            else
            {
                Debug.LogWarning("[CombatFlowCoordinator] EnemyCharacter가 아닙니다.");
            }
        }

        public void ClearEnemyHand()
        {
            Debug.Log("[CombatFlowCoordinator] 적 핸드 제거");
            enemyHandManager.ClearHand(); // ← 수정
        }

        public bool HasEnemy()
        {
            return enemyManager.GetEnemy() != null;
        }

        public void SpawnNextEnemy()
        {
            Debug.Log("[CombatFlowCoordinator] 다음 적 스폰 시도");
            stageManager.SpawnNextEnemy();
        }


        public void RequestFirstAttack(Action onComplete = null)
        {
            StartCoroutine(PerformFirstAttackInternal(onComplete));
        }

        public IEnumerator PerformFirstAttack() => PerformFirstAttackInternal(null);

        private IEnumerator PerformFirstAttackInternal(Action onComplete = null)
        {
            Debug.Log("[CombatFlowCoordinator] 첫 번째 공격 시작");

            var firstCard = turnCardRegistry.GetCardInSlot(CombatSlotPosition.FIRST);
            if (firstCard != null)
            {
                ExecuteCard(firstCard);
                Debug.Log($"[CombatFlowCoordinator] 선공 카드 실행 완료: {firstCard.GetCardName()}");
            }

            yield return new WaitForSeconds(1f);
            Debug.Log("[CombatFlowCoordinator] 첫 번째 공격 종료");
            onComplete?.Invoke();
        }

        public IEnumerator PerformSecondAttack()
        {
            Debug.Log("[CombatFlowCoordinator] 두 번째 공격 시작");

            var secondCard = turnCardRegistry.GetCardInSlot(CombatSlotPosition.SECOND);
            if (secondCard != null)
            {
                ExecuteCard(secondCard);
                Debug.Log($"[CombatFlowCoordinator] 후공 카드 실행 완료: {secondCard.GetCardName()}");
            }

            yield return new WaitForSeconds(1f);
            Debug.Log("[CombatFlowCoordinator] 두 번째 공격 종료");
        }

        private void ExecuteCard(ISkillCard card)
        {
            ICharacter source = card.IsFromPlayer() ? playerManager.GetPlayer() : enemyManager.GetEnemy();
            ICharacter target = card.IsFromPlayer() ? enemyManager.GetEnemy() : playerManager.GetPlayer();

            Debug.Log($"[ExecuteCard] 카드: {card.GetCardName()}, 소유자: {(card.IsFromPlayer() ? "플레이어" : "적")}");
            Debug.Log($"[ExecuteCard] 대상: {target?.GetCharacterName() ?? "null"}, IsDead: {target?.IsDead()}, 현재 쿨타임: {card.GetCurrentCoolTime()}, 최대 쿨타임: {card.GetMaxCoolTime()}");

            var context = new DefaultCardExecutionContext(card, source, target);
            cardExecutor.Execute(card, context, turnManager);

            // UI만 제거하고 카드 정보는 유지
            var slot = slotRegistry.GetCombatSlot(card.GetCombatSlot().Value);
            if (slot != null)
            {
                slot.ClearCardUI(); // ✔ UI만 제거
            }
        }

        public IEnumerator PerformResultPhase()
        {
            yield return new WaitForSeconds(1f);

            // 개선된 처리
            ClearAllSlotUIs();         // 카드 UI만 제거
            ClearEnemyCombatSlots();   // 적 카드 정보까지 제거

            Debug.Log("[CombatFlowCoordinator] 전투 결과 처리 완료 (플레이어 카드 유지)");
        }

        public void ClearAllSlotUIs()
        {
            foreach (CombatSlotPosition pos in Enum.GetValues(typeof(CombatSlotPosition)))
            {
                var slot = slotRegistry.GetCombatSlot(pos);
                slot?.ClearCardUI(); // 카드 UI만 제거
            }
            Debug.Log("[CombatFlowCoordinator] 모든 슬롯의 카드 UI 제거 완료");
        }

        public void ClearEnemyCombatSlots()
        {
            foreach (CombatSlotPosition pos in Enum.GetValues(typeof(CombatSlotPosition)))
            {
                var card = turnCardRegistry.GetCardInSlot(pos);
                if (card != null && !card.IsFromPlayer())
                {
                    var slot = slotRegistry.GetCombatSlot(pos);
                    slot?.ClearAll(); // 적 카드 정보 및 UI 제거
                }
            }
            Debug.Log("[CombatFlowCoordinator] 적 카드만 제거 완료");
        }

        // 전체 초기화가 필요한 경우 사용 (예: 강제 리셋 등)
        private void ClearAllCombatSlots()
        {
            foreach (CombatSlotPosition pos in Enum.GetValues(typeof(CombatSlotPosition)))
            {
                var slot = slotRegistry.GetCombatSlot(pos);
                slot?.ClearAll(); // 카드 + UI 전부 제거
            }

            Debug.Log("[CombatFlowCoordinator] 모든 전투 슬롯 완전 클리어 완료");
        }

        public IEnumerator PerformVictoryPhase()
        {
            yield return new WaitForSeconds(1f);
        }

        public IEnumerator PerformGameOverPhase()
        {
            yield return new WaitForSeconds(1f);
        }

        public void EnablePlayerInput() => playerInputEnabled = true;
        public void DisablePlayerInput() => playerInputEnabled = false;
        public bool IsPlayerInputEnabled() => playerInputEnabled;

        public bool IsPlayerDead() => playerManager.GetPlayer() == null;
        public bool IsEnemyDead()
        {
            var enemy = enemyManager.GetEnemy();
            return enemy == null || (enemy is EnemyCharacter e && e.IsMarkedDead);
        }

        public bool CheckHasNextEnemy() => stageManager.HasNextEnemy();

        public void CleanupAfterVictory() => enemyHandManager.ClearHand();

        private Action onStartButtonPressed;

        public void ShowPlayerCardSelectionUI() { }
        public void HidePlayerCardSelectionUI() { }

        public void EnableStartButton()
        {
            Debug.Log("[CombatFlowCoordinator] 전투 시작 버튼 활성화");
            startButtonHandler?.SetInteractable(true);
        }

        public void DisableStartButton()
        {
            Debug.Log("[CombatFlowCoordinator] 전투 시작 버튼 비활성화");
            startButtonHandler?.SetInteractable(false);
        }

        public ISkillCard GetCardInSlot(CombatSlotPosition pos)
        {
            return turnCardRegistry.GetCardInSlot(pos);
        }

        public void RegisterStartButton(Action callback) => onStartButtonPressed = callback;
        public void UnregisterStartButton() => onStartButtonPressed = null;
        public void OnStartButtonClickedExternally() => onStartButtonPressed?.Invoke();
    }
}
