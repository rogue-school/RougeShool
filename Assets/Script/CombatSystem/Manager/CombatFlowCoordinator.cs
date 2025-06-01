using System;
using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Zenject;

namespace Game.CombatSystem.Core
{
    public class CombatFlowCoordinator : MonoBehaviour, ICombatFlowCoordinator, ICharacterDeathListener
    {
        private ICharacterSlotRegistry characterSlotRegistry;
        private IEnemyHandManager enemyHandManager;
        private IPlayerHandManager playerHandManager;
        private IEnemySpawnerManager enemySpawner;
        private IPlayerManager playerManager;
        private IStageManager stageManager;
        private IEnemyManager enemyManager;
        private ICombatTurnManager turnManager;
        private ICombatStateFactory stateFactory;

        private ICombatPreparationService preparationService;
        private IPlayerInputController inputController;
        private ICombatExecutor executor;
        private ICardExecutor cardExecutor;
        private ICardExecutionContextProvider contextProvider;

        private SkillCardUI skillCardPrefab;
        private bool playerInputEnabled = false;

        [Inject]
        public void Construct(
            ICharacterSlotRegistry characterSlotRegistry,
            IEnemyHandManager enemyHandManager,
            IPlayerHandManager playerHandManager,
            IEnemySpawnerManager enemySpawner,
            IPlayerManager playerManager,
            IStageManager stageManager,
            IEnemyManager enemyManager,
            ICombatTurnManager turnManager,
            ICombatStateFactory stateFactory,
            ICombatPreparationService preparationService,
            IPlayerInputController inputController,
            ICombatExecutor executor,
            ICardExecutionContextProvider contextProvider,
            ICardExecutor cardExecutor)
        {
            this.characterSlotRegistry = characterSlotRegistry;
            this.enemyHandManager = enemyHandManager;
            this.playerHandManager = playerHandManager;
            this.enemySpawner = enemySpawner;
            this.playerManager = playerManager;
            this.stageManager = stageManager;
            this.enemyManager = enemyManager;
            this.turnManager = turnManager;
            this.stateFactory = stateFactory;

            this.preparationService = preparationService;
            this.inputController = inputController;
            this.executor = executor;
            this.contextProvider = contextProvider;
            this.cardExecutor = cardExecutor;

            executor?.InjectExecutionDependencies(contextProvider, cardExecutor);
        }

        // === UI 프리팹 주입 ===
        public void InjectUI(SkillCardUI skillCardPrefab)
        {
            this.skillCardPrefab = skillCardPrefab;
        }

        // === 턴 상태 의존성 주입 ===
        public void InjectTurnStateDependencies(
            ICombatTurnManager turnManager,
            ICombatStateFactory stateFactory)
        {
            this.turnManager = turnManager;
            this.stateFactory = stateFactory;
        }

        // === 외부 서비스 주입 ===
        public void ConstructFlowDependencies(
            ICombatPreparationService preparationService,
            IPlayerInputController inputController,
            ICombatExecutor executor,
            ICardExecutionContextProvider contextProvider,
            ICardExecutor cardExecutor)
        {
            this.preparationService = preparationService;
            this.inputController = inputController;
            this.executor = executor;
            this.contextProvider = contextProvider;
            this.cardExecutor = cardExecutor;

            executor?.InjectExecutionDependencies(contextProvider, cardExecutor);
        }

        // === 전투 준비 ===
        public IEnumerator PerformCombatPreparation()
        {
            bool success = false;
            yield return PerformCombatPreparation(result => success = result);
        }

        public IEnumerator PerformCombatPreparation(Action<bool> onComplete)
        {
            if (preparationService == null)
            {
                Debug.LogError("[CombatFlowCoordinator] preparationService가 null입니다.");
                onComplete?.Invoke(false);
                yield break;
            }

            var prevEnemy = enemyManager?.GetEnemy();
            if (prevEnemy != null)
            {
                Debug.Log("[CombatFlowCoordinator] 이전 적 핸드 정리");
                enemyHandManager?.ClearHand();
                enemyManager?.ClearEnemy();
            }

            yield return preparationService.PrepareCombat(success =>
            {
                if (!success)
                {
                    Debug.LogError("[CombatFlowCoordinator] 전투 준비 실패");
                    onComplete?.Invoke(false);
                    return;
                }

                Debug.Log("[CombatFlowCoordinator] 전투 준비 완료");
                onComplete?.Invoke(true);
            });
        }

        // === 입력 제어 ===
        public void EnablePlayerInput()
        {
            playerInputEnabled = true;
            Debug.Log("[CombatFlowCoordinator] EnablePlayerInput 호출");

            inputController?.EnablePlayerInput();
            playerHandManager?.EnableInput(true);
        }

        public void DisablePlayerInput()
        {
            playerInputEnabled = false;
            Debug.Log("[CombatFlowCoordinator] DisablePlayerInput 호출");

            inputController?.DisablePlayerInput();
            playerHandManager?.EnableInput(false);
        }

        public bool IsPlayerInputEnabled() => playerInputEnabled;

        // === 전투 실행 ===
        public IEnumerator PerformFirstAttack()
        {
            Debug.Log("[CombatFlowCoordinator] PerformFirstAttack 호출");
            if (executor != null)
                yield return executor.PerformAttack(CombatSlotPosition.FIRST);
            else
                Debug.LogError("[CombatFlowCoordinator] executor가 null입니다.");
        }

        public IEnumerator PerformSecondAttack()
        {
            Debug.Log("[CombatFlowCoordinator] PerformSecondAttack 호출");
            if (executor != null)
                yield return executor.PerformAttack(CombatSlotPosition.SECOND);
            else
                Debug.LogError("[CombatFlowCoordinator] executor가 null입니다.");
        }

        // === 후속 상태 처리 ===
        public IEnumerator PerformResultPhase() => CoroutineStub("PerformResultPhase");
        public IEnumerator PerformVictoryPhase() => CoroutineStub("PerformVictoryPhase");
        public IEnumerator PerformGameOverPhase() => CoroutineStub("PerformGameOverPhase");

        private IEnumerator CoroutineStub(string phaseName)
        {
            Debug.Log($"[CombatFlowCoordinator] {phaseName} 호출");
            yield return null;
        }

        // === 전투 상태 확인 ===
        public bool IsPlayerDead() => playerManager?.GetPlayer()?.IsDead() ?? true;
        public bool IsEnemyDead() => enemyManager?.GetEnemy()?.IsDead() ?? true;
        public bool CheckHasNextEnemy() => stageManager?.HasNextEnemy() ?? false;

        public void StartCombatFlow()
        {
            Debug.Log("[CombatFlowCoordinator] StartCombatFlow 호출됨");
            turnManager?.Initialize();
        }

        // === 캐릭터 사망 처리 ===
        public void OnCharacterDied(ICharacter character)
        {
            if (character is IEnemyCharacter)
            {
                Debug.Log("[CombatFlowCoordinator] 적 사망 감지 → 참조 정리 및 슬롯 초기화");
                enemyManager?.ClearEnemy();

                var slot = characterSlotRegistry?.GetCharacterSlot(SlotOwner.ENEMY);
                slot?.SetCharacter(null);
            }
        }
        public void RequestCombatPreparation(Action<bool> onComplete)
        {
            if (onComplete == null)
            {
                Debug.LogError("[CombatFlowCoordinator] onComplete 콜백이 null입니다.");
                return;
            }

            StartCoroutine(PerformCombatPreparation(onComplete));
        }
        public void RequestFirstAttack(Action onComplete = null)
        {
            StartCoroutine(PerformFirstAttackCoroutine(onComplete));
        }

        private IEnumerator PerformFirstAttackCoroutine(Action onComplete)
        {
            Debug.Log("[CombatFlowCoordinator] RequestFirstAttack 실행 시작");

            if (executor != null)
                yield return executor.PerformAttack(CombatSlotPosition.FIRST);
            else
                Debug.LogError("[CombatFlowCoordinator] executor가 null입니다.");

            Debug.Log("[CombatFlowCoordinator] RequestFirstAttack 완료");
            onComplete?.Invoke();
        }
        public void CleanupAfterVictory()
        {
            Debug.Log("[CombatFlowCoordinator] CleanupAfterVictory 호출됨");

            enemyHandManager?.ClearHand();
            playerHandManager?.EnableInput(false);
            // 필요하다면 UI 정리, 리셋 등 추가
        }
    }
}
