using System;
using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Core
{
    public class CombatFlowCoordinator : MonoBehaviour, ICombatFlowCoordinator, ICharacterDeathListener
    {
        private ISlotRegistry slotRegistry;
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

        // 기본 게임 컴포넌트 의존성 주입
        public void Inject(
            ISlotRegistry slotRegistry,
            IEnemyHandManager enemyHandManager,
            IPlayerHandManager playerHandManager,
            IEnemySpawnerManager enemySpawner,
            IPlayerManager playerManager,
            IStageManager stageManager,
            IEnemyManager enemyManager,
            ICombatTurnManager turnManager,
            ICombatStateFactory stateFactory)
        {
            this.slotRegistry = slotRegistry;
            this.enemyHandManager = enemyHandManager;
            this.playerHandManager = playerHandManager;
            this.enemySpawner = enemySpawner;
            this.playerManager = playerManager;
            this.stageManager = stageManager;
            this.enemyManager = enemyManager;
            this.turnManager = turnManager;
            this.stateFactory = stateFactory;
        }

        // UI 프리팹 주입
        public void InjectUI(SkillCardUI skillCardPrefab)
        {
            this.skillCardPrefab = skillCardPrefab;
        }

        // 턴 상태 매니저/팩토리 주입
        public void InjectTurnStateDependencies(ICombatTurnManager turnManager, ICombatStateFactory stateFactory)
        {
            this.turnManager = turnManager;
            this.stateFactory = stateFactory;
        }

        // 외부 서비스 주입: 실행기, 입력 컨트롤러, 준비 서비스, 컨텍스트 제공자
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

            this.executor?.InjectExecutionDependencies(this.contextProvider, this.cardExecutor);
        }

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

        public IEnumerator PerformResultPhase() => CoroutineStub("PerformResultPhase");
        public IEnumerator PerformVictoryPhase() => CoroutineStub("PerformVictoryPhase");
        public IEnumerator PerformGameOverPhase() => CoroutineStub("PerformGameOverPhase");

        private IEnumerator CoroutineStub(string phaseName)
        {
            Debug.Log($"[CombatFlowCoordinator] {phaseName} 호출");
            yield return null;
        }

        public bool IsPlayerDead() => playerManager?.GetPlayer()?.IsDead() ?? true;
        public bool IsEnemyDead() => enemyManager?.GetEnemy()?.IsDead() ?? true;
        public bool CheckHasNextEnemy() => stageManager?.HasNextEnemy() ?? false;

        public void StartCombatFlow()
        {
            Debug.Log("[CombatFlowCoordinator] StartCombatFlow 호출됨");
            turnManager?.Initialize();
        }

        public void OnCharacterDied(ICharacter character)
        {
            if (character is IEnemyCharacter)
            {
                Debug.Log("[CombatFlowCoordinator] 적 사망 감지 → 참조 정리 및 슬롯 초기화");
                enemyManager?.ClearEnemy();

                var slot = slotRegistry?.GetCharacterSlot(SlotOwner.ENEMY);
                slot?.SetCharacter(null);
            }
        }
    }
}
