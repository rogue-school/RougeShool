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
using Game.SkillCardSystem.Effect;
using Game.Utility;
using Game.CombatSystem.State;
using AnimationSystem.Animator;
using System.Threading.Tasks;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 전투의 전체 흐름을 조율하는 핵심 클래스.
    /// 전투 준비, 공격, 슬롯 등록, 승패 처리 등을 담당한다.
    /// </summary>
    public class CombatFlowCoordinator : MonoBehaviour, ICombatFlowCoordinator
    {
        #region 의존성 주입

        [Inject] private IStageManager stageManager;
        [Inject] private IPlayerManager playerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private IPlayerHandManager playerHandManager;
        [Inject] private IEnemyHandManager enemyHandManager;
        [Inject] private ITurnCardRegistry turnCardRegistry;
        [Inject] private ICombatPreparationService preparationService;
        [Inject] private ISlotRegistry slotRegistry;
        [Inject] private ICardExecutor cardExecutor;
        [Inject] private ICoroutineRunner coroutineRunner;
        [Inject] private DeathUIManager deathUIManager;

        #endregion

        #region 내부 상태

        private ICombatTurnManager turnManager;
        private ICombatStateFactory stateFactory;
        private TurnStartButtonHandler startButtonHandler;
        private bool playerInputEnabled = false;
        public bool IsEnemyFirst { get; set; }
        private Action onStartButtonPressed;

        #endregion

        #region 초기화 및 구성

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

        #endregion

        #region 인터페이스 구현

        /// <summary>
        /// 전투 준비 루틴을 실행합니다. (인터페이스 구현)
        /// </summary>
        public void RequestCombatPreparation(Action<bool> onComplete)
        {
            StartCoroutine(PerformCombatPreparation(onComplete));
        }

        public void RegisterCardToTurnRegistry(CombatSlotPosition pos, ISkillCard card, ISkillCardUI ui)
        {
            if (ui is not SkillCardUI concreteUI)
            {
                Debug.LogError("[CombatFlowCoordinator] RegisterCardToTurnRegistry 실패: ui가 SkillCardUI 아님");
                return;
            }

            turnCardRegistry.RegisterCard(pos, card, concreteUI, SlotOwner.ENEMY);
        }

        public IEnumerator RegisterCardToCombatSlotCoroutine(CombatSlotPosition pos, ISkillCard card, ISkillCardUI ui)
        {
            yield return RegisterCardToCombatSlotCoroutine(pos, card, ui as SkillCardUI); // 다운캐스팅 유의
        }

        public IEnumerator PerformCombatPreparation() => PerformCombatPreparation(_ => { });

        public IEnumerator PerformCombatPreparation(Action<bool> onComplete)
        {
            if (!slotRegistry.IsInitialized)
            {
                Debug.LogError("[CombatFlowCoordinator] 슬롯 레지스트리가 아직 초기화되지 않았습니다.");
                onComplete?.Invoke(false);
                yield break;
            }

            var enemy = enemyManager.GetEnemy();
            if (enemy == null)
            {
                Debug.LogWarning("[CombatFlowCoordinator] 적이 존재하지 않습니다.");
                onComplete?.Invoke(false);
                yield break;
            }

            yield return new WaitForSeconds(0.5f);

            IsEnemyFirst = UnityEngine.Random.value < 0.5f;
            var slotToRegister = IsEnemyFirst ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND;

            yield return enemyHandManager.StepwiseFillSlotsFromBack(0.3f);

            yield return new WaitUntil(() =>
            {
                var (card, ui) = enemyHandManager.PeekCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
                return card != null && ui != null;
            });

            var (cardToRegister, uiToRegister) = enemyHandManager.PopCardFromSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (cardToRegister != null && uiToRegister != null)
            {
                // UI 애니메이션 완료까지 대기
                bool animationComplete = false;
                UnityMainThreadDispatcher.Enqueue(async () =>
                {
                    await RegisterCardToCombatSlotAsync(slotToRegister, cardToRegister, uiToRegister);
                    // 애니메이션이 끝난 후에만 슬롯 등록
                    turnCardRegistry.RegisterCard(slotToRegister, cardToRegister, uiToRegister, SlotOwner.ENEMY);
                    animationComplete = true;
                });

                yield return new WaitUntil(() => animationComplete);
            }
            else
            {
                Debug.LogWarning("[CombatFlowCoordinator] 적 카드 등록 실패");
                onComplete?.Invoke(false);
                yield break;
            }

            yield return enemyHandManager.StepwiseFillSlotsFromBack(0.3f);
            Debug.Log("[CombatFlowCoordinator] 전투 준비 완료");
            onComplete?.Invoke(true);
        }

        #endregion

        #region 카드 및 슬롯 등록

        /// <summary>
        /// 카드와 UI를 지정된 전투 슬롯에 등록합니다.
        /// </summary>
        public void RegisterCardToCombatSlot(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            var slot = slotRegistry.GetCombatSlot(pos);
            if (slot is not ICombatCardSlot combatSlot)
            {
                Debug.LogError($"[CombatFlowCoordinator] 전투 슬롯 {pos}이 null이거나 ICombatCardSlot이 아님.");
                return;
            }

            ui.transform.SetParent(combatSlot.GetTransform());
            ui.transform.localPosition = Vector3.zero;
            ui.transform.localScale = Vector3.one;

            combatSlot.SetCard(card);
            combatSlot.SetCardUI(ui);
        }
        /// <summary>
        /// 카드와 UI를 지정된 전투 슬롯에 애니메이션을 포함하여 등록합니다.
        /// </summary>
        /// <summary>
        /// 카드와 UI를 지정된 전투 슬롯에 애니메이션을 포함하여 등록합니다.
        /// </summary>
        public async Task RegisterCardToCombatSlotAsync(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            var slot = slotRegistry.GetCombatSlot(pos);
            if (slot is not ICombatCardSlot combatSlot)
            {
                Debug.LogError($"[CombatFlowCoordinator] 전투 슬롯 {pos}이 null이거나 ICombatCardSlot이 아님.");
                return; // 모든 경로가 Task 종료되도록 명시
            }

            var slotRectTransform = combatSlot.GetTransform() as RectTransform;
            if (slotRectTransform == null)
            {
                Debug.LogError($"[CombatFlowCoordinator] 슬롯 {pos}의 Transform이 RectTransform이 아닙니다.");
                return;
            }

            if (ui is MonoBehaviour uiMb)
            {
                var shiftAnimator = uiMb.GetComponent<SkillCardShiftAnimator>();
                var spawnAnimator = uiMb.GetComponent<SkillCardSpawnAnimator>();

                var rect = uiMb.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Debug.Log($"[Before SetParent] parent={rect.parent?.name}, anchored={rect.anchoredPosition}, world={rect.position}");
                    Vector3 worldPos = rect.position;
                    uiMb.transform.SetParent(slotRectTransform.parent, true);
                    Debug.Log($"[After SetParent] parent={rect.parent?.name}, anchored={rect.anchoredPosition}, world={rect.position}");
                    // 월드 위치 강제 세팅
                    rect.position = worldPos;
                    Debug.Log($"[After position fix] parent={rect.parent?.name}, anchored={rect.anchoredPosition}, world={rect.position}");
                }

                // 부모를 임시로 월드 유지용으로 설정
                // uiMb.transform.SetParent(slotRectTransform.parent, true);

                // 이동 애니메이션
                if (shiftAnimator != null)
                    await shiftAnimator.PlayMoveAnimationAsync(slotRectTransform); // 그림자도 카드와 함께 이동

                // 슬롯에 부착
                uiMb.transform.SetParent(slotRectTransform, false);
                uiMb.transform.localPosition = Vector3.zero;
                uiMb.transform.localScale = Vector3.one;

                // 등장 애니메이션
                if (spawnAnimator != null)
                    await spawnAnimator.PlaySpawnAnimationAsync();
            }
            else
            {
                // 애니메이션 없는 기본 처리
                ui.transform.SetParent(slotRectTransform);
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localScale = Vector3.one;
            }

            // 슬롯에 카드 등록
            combatSlot.SetCard(card);
            combatSlot.SetCardUI(ui);

            // 모든 경로가 Task를 완료해야 하므로 명시적으로 종료
            await Task.CompletedTask;
        }
        public IEnumerator RegisterCardToCombatSlotCoroutine(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            var slot = slotRegistry.GetCombatSlot(pos);
            if (slot is not ICombatCardSlot combatSlot)
            {
                Debug.LogError($"[CombatFlowCoordinator] 전투 슬롯 {pos}이 null이거나 ICombatCardSlot이 아님.");
                yield break;
            }

            var slotRect = combatSlot.GetTransform() as RectTransform;
            if (slotRect == null)
            {
                Debug.LogError($"[CombatFlowCoordinator] 슬롯 {pos}의 Transform이 RectTransform이 아님");
                yield break;
            }

            if (ui is MonoBehaviour uiMb)
            {
                var shiftAnimator = uiMb.GetComponent<SkillCardShiftAnimator>();
                var spawnAnimator = uiMb.GetComponent<SkillCardSpawnAnimator>();

                var rect = uiMb.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Debug.Log($"[Before SetParent] parent={rect.parent?.name}, anchored={rect.anchoredPosition}, world={rect.position}");
                    Vector3 worldPos = rect.position;
                    uiMb.transform.SetParent(slotRect.parent, true);
                    Debug.Log($"[After SetParent] parent={rect.parent?.name}, anchored={rect.anchoredPosition}, world={rect.position}");
                    rect.position = worldPos;
                    Debug.Log($"[After position fix] parent={rect.parent?.name}, anchored={rect.anchoredPosition}, world={rect.position}");
                }

                if (shiftAnimator != null)
                    yield return shiftAnimator.PlayMoveAnimationCoroutine(slotRect); // 그림자도 카드와 함께 이동

                uiMb.transform.SetParent(slotRect, false);
                uiMb.transform.localPosition = Vector3.zero;
                uiMb.transform.localScale = Vector3.one;

                if (spawnAnimator != null)
                    yield return spawnAnimator.PlaySpawnAnimationCoroutine();
            }
            else
            {
                ui.transform.SetParent(slotRect);
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localScale = Vector3.one;
            }

            combatSlot.SetCard(card);
            combatSlot.SetCardUI(ui);
        }

        /// <summary>
        /// 내부 전용: 슬롯 UI만 따로 다시 등록할 때 사용
        /// </summary>
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

        #endregion

        #region 공격 처리

        public void RequestFirstAttack(Action onComplete = null)
        {
            StartCoroutine(PerformFirstAttackInternal(onComplete));
        }

        public IEnumerator PerformFirstAttack() => PerformFirstAttackInternal(null);

        private IEnumerator PerformFirstAttackInternal(Action onComplete = null)
        {
            var firstCard = turnCardRegistry.GetCardInSlot(CombatSlotPosition.FIRST);
            if (firstCard != null)
            {
                ExecuteCard(firstCard);
                Debug.Log($"[CombatFlowCoordinator] 선공 카드 실행 완료: {firstCard.GetCardName()}");
            }

            yield return new WaitForSeconds(1f);
            onComplete?.Invoke();
        }

        public IEnumerator PerformSecondAttack()
        {
            var secondCard = turnCardRegistry.GetCardInSlot(CombatSlotPosition.SECOND);
            var firstCard = turnCardRegistry.GetCardInSlot(CombatSlotPosition.FIRST);

            if (firstCard != null && firstCard.IsFromPlayer())
            {
                foreach (var effect in firstCard.CreateEffects())
                {
                    if (effect is GuardEffectSO)
                    {
                        Debug.Log("<color=orange>[CombatFlowCoordinator] 가드 스킬 → 후공 무효화</color>");
                        if (secondCard != null && !secondCard.IsFromPlayer())
                        {
                            slotRegistry.GetCombatSlot(CombatSlotPosition.SECOND)?.ClearAll();
                            yield break;
                        }
                    }
                }
            }

            if (secondCard != null)
            {
                ExecuteCard(secondCard);
                Debug.Log($"[CombatFlowCoordinator] 후공 카드 실행 완료: {secondCard.GetCardName()}");
            }

            yield return new WaitForSeconds(1f);
        }

        /// <summary>
        /// 카드 실행 및 사망 여부 검사
        /// </summary>
        private void ExecuteCard(ISkillCard card)
        {
            ICharacter source = card.IsFromPlayer() ? playerManager.GetPlayer() : enemyManager.GetEnemy();
            ICharacter target = card.IsFromPlayer() ? enemyManager.GetEnemy() : playerManager.GetPlayer();

            var context = new DefaultCardExecutionContext(card, source, target);
            cardExecutor.Execute(card, context, turnManager);

            card.SetCurrentCoolTime(card.GetMaxCoolTime());
            slotRegistry.GetCombatSlot(card.GetCombatSlot().Value)?.ClearCardUI();

            if (IsPlayerDead())
            {
                var gameOverState = new CombatGameOverState(
                    turnManager, this,
                    slotRegistry as ICombatSlotRegistry,
                    coroutineRunner, deathUIManager, playerManager
                );
                turnManager.RequestStateChange(gameOverState);
            }
        }

        #endregion

        #region 결과 처리 단계

        public IEnumerator PerformResultPhase()
        {
            yield return new WaitForSeconds(1f);
            ClearAllSlotUIs();
            ClearEnemyCombatSlots();
        }

        public IEnumerator PerformVictoryPhase()
        {
            yield return new WaitForSeconds(1f);
        }

        public IEnumerator PerformGameOverPhase()
        {
            yield return new WaitForSeconds(1f);
        }

        #endregion

        #region 슬롯 및 적 정리

        public void ClearAllSlotUIs()
        {
            foreach (CombatSlotPosition pos in Enum.GetValues(typeof(CombatSlotPosition)))
                slotRegistry.GetCombatSlot(pos)?.ClearCardUI();
        }

        public void ClearEnemyCombatSlots()
        {
            foreach (CombatSlotPosition pos in Enum.GetValues(typeof(CombatSlotPosition)))
            {
                var card = turnCardRegistry.GetCardInSlot(pos);
                if (card != null && !card.IsFromPlayer())
                    slotRegistry.GetCombatSlot(pos)?.ClearAll();
            }
        }

        private void ClearAllCombatSlots()
        {
            foreach (CombatSlotPosition pos in Enum.GetValues(typeof(CombatSlotPosition)))
                slotRegistry.GetCombatSlot(pos)?.ClearAll();
        }

        public void RemoveEnemyCharacter()
        {
            var enemy = enemyManager.GetEnemy();
            if (enemy is EnemyCharacter concreteEnemy)
            {
                Destroy(concreteEnemy.gameObject);
                enemyManager.ClearEnemy();
            }
        }

        public void ClearEnemyHand() => enemyHandManager.ClearHand();
        public void CleanupAfterVictory() => enemyHandManager.ClearHand();

        #endregion

        #region 턴 유틸리티

        public ITurnCardRegistry GetTurnCardRegistry() => turnCardRegistry;
        public ISkillCard GetCardInSlot(CombatSlotPosition pos) => turnCardRegistry.GetCardInSlot(pos);
        public IEnemyCharacter GetEnemy() => enemyManager.GetEnemy();

        #endregion

        #region 상태 및 플래그

        public bool HasEnemy() => enemyManager.GetEnemy() != null;
        public bool CheckHasNextEnemy() => stageManager.HasNextEnemy();

        public bool IsPlayerDead()
        {
            var player = playerManager.GetPlayer();
            return player == null || player.IsDead();
        }

        public bool IsEnemyDead()
        {
            var enemy = enemyManager.GetEnemy();
            return enemy is EnemyCharacter e && e.IsMarkedDead && e.GetCurrentHP() <= 0;
        }

        public void EnablePlayerInput() => playerInputEnabled = true;
        public void DisablePlayerInput() => playerInputEnabled = false;
        public bool IsPlayerInputEnabled() => playerInputEnabled;

        #endregion

        #region UI 및 버튼

        public void EnableStartButton() => startButtonHandler?.SetInteractable(true);
        public void DisableStartButton() => startButtonHandler?.SetInteractable(false);
        public void RegisterStartButton(Action callback) => onStartButtonPressed = callback;
        public void UnregisterStartButton() => onStartButtonPressed = null;
        public void OnStartButtonClickedExternally() => onStartButtonPressed?.Invoke();

        public void ShowPlayerCardSelectionUI() { }
        public void HidePlayerCardSelectionUI() { }

        #endregion

        #region 적 소환

        public void SpawnNextEnemy() => stageManager.SpawnNextEnemy();

        #endregion
    }
}
