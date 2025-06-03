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
            var initialState = stateFactory.CreatePrepareState();
            turnManager.ChangeState(initialState);
            DisableStartButton();
        }

        public void RequestCombatPreparation(Action<bool> onComplete)
        {
            StartCoroutine(PerformCombatPreparation(onComplete));
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
            else
            {
                Debug.LogWarning("[CombatFlowCoordinator] 선공 카드가 없습니다.");
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
            else
            {
                Debug.LogWarning("[CombatFlowCoordinator] 후공 카드가 없습니다.");
            }

            yield return new WaitForSeconds(1f);
            Debug.Log("[CombatFlowCoordinator] 두 번째 공격 종료");
        }

        private void ExecuteCard(ISkillCard card)
        {
            var source = card.GetOwner(new DefaultCardExecutionContext(card, null, null));
            var target = card.GetTarget(new DefaultCardExecutionContext(card, source, null));
            var context = new DefaultCardExecutionContext(card, source, target);

            cardExecutor.Execute(card, context, turnManager);
        }

        public IEnumerator PerformResultPhase() { yield return new WaitForSeconds(1f); }
        public IEnumerator PerformVictoryPhase() { yield return new WaitForSeconds(1f); }
        public IEnumerator PerformGameOverPhase() { yield return new WaitForSeconds(1f); }

        public void EnablePlayerInput() => playerInputEnabled = true;
        public void DisablePlayerInput() => playerInputEnabled = false;
        public bool IsPlayerInputEnabled() => playerInputEnabled;

        public bool IsPlayerDead() => playerManager.GetPlayer() == null;
        public bool IsEnemyDead() => enemyManager.GetEnemy() == null;
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

        public void RegisterStartButton(Action callback) => onStartButtonPressed = callback;
        public void UnregisterStartButton() => onStartButtonPressed = null;
        public void OnStartButtonClickedExternally() => onStartButtonPressed?.Invoke();
    }
}
