using UnityEngine;
using System.Collections;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;

namespace Game.CombatSystem.Manager
{
    public class CombatFlowCoordinator : MonoBehaviour
    {
        private ISlotInitializer slotInitializer;
        private IPlayerCharacterInitializer playerInitializer;
        private IEnemyInitializer enemyInitializer;
        private IPlayerHandManager playerHandManager;
        private IEnemyHandManager enemyHandManager;
        private IEnemyManager enemyManager;
        private IEnemySpawnerManager spawnerManager;
        private ICombatTurnManager turnManager;
        private IStageManager stageManager;
        private ICombatStateFactory stateFactory;
        private ISlotRegistry slotRegistry;
        private IPlayerManager playerManager;

        public void Inject(
            ISlotInitializer slotInitializer,
            IPlayerCharacterInitializer playerInitializer,
            IEnemyInitializer enemyInitializer,
            IPlayerHandManager playerHandManager,
            IEnemyHandManager enemyHandManager,
            IEnemyManager enemyManager,
            IEnemySpawnerManager spawnerManager,
            ICombatTurnManager turnManager,
            IStageManager stageManager,
            ICombatStateFactory stateFactory,
            ISlotRegistry slotRegistry,
            IPlayerManager playerManager)
        {
            this.slotInitializer = slotInitializer;
            this.playerInitializer = playerInitializer;
            this.enemyInitializer = enemyInitializer;
            this.playerHandManager = playerHandManager;
            this.enemyHandManager = enemyHandManager;
            this.enemyManager = enemyManager;
            this.spawnerManager = spawnerManager;
            this.turnManager = turnManager;
            this.stageManager = stageManager;
            this.stateFactory = stateFactory;
            this.slotRegistry = slotRegistry;
            this.playerManager = playerManager;
        }

        public void StartCombatFlow()
        {
            StartCoroutine(RunCombatStartupFlow());
        }

        private IEnumerator RunCombatStartupFlow()
        {
            Debug.Log("[CombatFlowCoordinator] === 전투 초기화 루틴 시작 ===");

            yield return InitializeSlots();
            yield return InitializePlayer();
            yield return InitializeEnemy();
            yield return GenerateHands();

            yield return new WaitForEndOfFrame(); // 상태 전이 한 프레임 지연
            yield return StartCombat();

            Debug.Log("[CombatFlowCoordinator] === 전투 초기화 루틴 완료 ===");
        }

        private IEnumerator InitializeSlots()
        {
            Debug.Log("[CombatFlowCoordinator] 슬롯 초기화");

            slotInitializer?.AutoBindAllSlots();

            var playerSlot = slotRegistry?.GetCharacterSlot(SlotOwner.PLAYER);
            var enemySlot = slotRegistry?.GetCharacterSlot(SlotOwner.ENEMY);

            Debug.Log($"[CombatFlowCoordinator] PlayerSlot: {playerSlot?.GetTransform()?.name}");
            Debug.Log($"[CombatFlowCoordinator] EnemySlot: {enemySlot?.GetTransform()?.name}");

            yield return null;
        }

        private IEnumerator InitializePlayer()
        {
            Debug.Log("[CombatFlowCoordinator] 플레이어 초기화");

            playerInitializer?.Setup();
            yield return null;

            var player = playerManager?.GetPlayer();
            playerHandManager?.Inject(player, slotRegistry, turnManager);
            playerHandManager?.Initialize();

            yield return null;
        }

        private IEnumerator InitializeEnemy()
        {
            Debug.Log("[CombatFlowCoordinator] 적 초기화");

            var enemyData = stageManager?.GetCurrentStage()?.enemies?[0];
            if (enemyData == null)
            {
                Debug.LogError("[CombatFlowCoordinator] 적 데이터 없음");
                yield break;
            }

            enemyInitializer?.SetupWithData(enemyData);
            yield return null;

            var enemy = enemyInitializer?.GetSpawnedEnemy();
            if (enemy == null)
            {
                Debug.LogError("[CombatFlowCoordinator] 적 생성 실패");
                yield break;
            }

            enemyManager?.RegisterEnemy(enemy);
            enemyHandManager?.Initialize(enemy);

            yield return null;
        }

        private IEnumerator GenerateHands()
        {
            Debug.Log("[CombatFlowCoordinator] 핸드 생성");

            playerHandManager?.GenerateInitialHand();
            enemyHandManager?.GenerateInitialHand();

            yield return null;
        }

        private IEnumerator StartCombat()
        {
            Debug.Log("[CombatFlowCoordinator] 상태 전이 시작");

            // 실질적인 준비 로직 수행
            yield return PerformCombatPreparation();

            var state = stateFactory?.CreatePrepareState();
            if (state == null)
            {
                Debug.LogError("[CombatFlowCoordinator] 상태 생성 실패");
                yield break;
            }

            turnManager?.RequestStateChange(state);
            yield return null;
        }

        private IEnumerator PerformCombatPreparation()
        {
            Debug.Log("[CombatFlowCoordinator] 전투 준비 로직 실행");

            spawnerManager?.SpawnInitialEnemy(); // 적 스폰
            yield return null;

            enemyHandManager?.GenerateInitialHand(); // 적 핸드 생성
            yield return null;

            // 카드 배치 시도 (슬롯1에 카드가 있을 경우)
            var card = enemyHandManager?.GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (card != null)
            {
                var slot = Random.value < 0.5f ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND;
                var combatSlot = slotRegistry?.GetCombatSlot(slot);
                combatSlot?.SetCard(card);
                turnManager?.ReserveEnemySlot(slot);
                Debug.Log($"[CombatFlowCoordinator] 적 카드 '{card.GetCardName()}' 전투 슬롯 {slot}에 배치 완료");
            }
            else
            {
                Debug.LogWarning("[CombatFlowCoordinator] 적 슬롯 1에 카드가 없어 전투 슬롯 배치 스킵");
            }

            yield return null;
        }
    }
}
