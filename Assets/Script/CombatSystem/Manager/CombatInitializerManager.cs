using UnityEngine;
using System.Collections;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 씬에서 모든 초기화 과정을 수행하는 전담 매니저입니다. (DI 기반)
    /// </summary>
    public class CombatInitializerManager : MonoBehaviour
    {
        [Header("자동 실행 여부 (비활성화됨)")]

        // 의존성 주입 대상
        private ISlotInitializer slotInitializer;
        private IPlayerCharacterInitializer playerInitializer;
        private IEnemyInitializer enemyInitializer;
        private IPlayerHandManager playerHandManager;
        private IEnemyHandManager enemyHandManager;
        private IEnemyManager enemyManager;
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
            this.turnManager = turnManager;
            this.stageManager = stageManager;
            this.stateFactory = stateFactory;
            this.slotRegistry = slotRegistry;
            this.playerManager = playerManager;

            //Debug.Log("[CombatInitializerManager] 의존성 주입 완료");
        }

        // Start() 메서드는 제거되었습니다.

        public void InitializeCombat()
        {
            StartCoroutine(InitializeRoutine());
        }

        private IEnumerator InitializeRoutine()
        {
            Debug.Log("[CombatInitializerManager] === 전투 초기화 루틴 시작 ===");

            yield return InitializeSlots();
            yield return InitializePlayer();
            yield return InitializeEnemy();
            yield return InitializeHands();
            yield return StartCombat();

            Debug.Log("[CombatInitializerManager] === 전투 초기화 루틴 완료 ===");
        }

        private IEnumerator InitializeSlots()
        {
            Debug.Log("[CombatInitializerManager] 슬롯 초기화 단계 시작");

            if (slotInitializer == null)
            {
                Debug.LogError("[CombatInitializerManager] SlotInitializer가 주입되지 않았습니다.");
                yield break;
            }

            slotInitializer.AutoBindAllSlots();

            var registry = SlotRegistry.Instance;
            if (registry == null)
            {
                Debug.LogError("[CombatInitializerManager] SlotRegistry 인스턴스가 null입니다.");
                yield break;
            }

            var playerSlot = registry.GetCharacterSlot(SlotOwner.PLAYER);
            var enemySlot = registry.GetCharacterSlot(SlotOwner.ENEMY);

            if (playerSlot == null)
            {
                Debug.LogWarning("[CombatInitializerManager] Player용 캐릭터 슬롯이 등록되지 않았습니다.");
            }
            else
            {
                Debug.Log($"[CombatInitializerManager] Player 슬롯 확인 완료 → {playerSlot.GetTransform()?.name}");
            }

            if (enemySlot == null)
            {
                Debug.LogWarning("[CombatInitializerManager] Enemy용 캐릭터 슬롯이 등록되지 않았습니다.");
            }
            else
            {
                Debug.Log($"[CombatInitializerManager] Enemy 슬롯 확인 완료 → {enemySlot.GetTransform()?.name}");
            }

            yield return null;
        }

        private IEnumerator InitializePlayer()
        {
            Debug.Log("[CombatInitializerManager] 플레이어 초기화 단계 시작");

            playerInitializer?.Setup();
            yield return null;

            var player = playerManager?.GetPlayer();
            if (player == null)
            {
                Debug.LogError("[CombatInitializerManager] 플레이어 캐릭터가 생성되지 않았습니다.");
                yield break;
            }

            if (playerHandManager != null && slotRegistry != null && turnManager != null)
            {
                Debug.Log("[CombatInitializerManager] PlayerHandManager에 의존성 주입 및 초기화 진행");
                playerHandManager.Inject(player, slotRegistry, turnManager);
                playerHandManager.Initialize();
            }
            else
            {
                Debug.LogWarning("[CombatInitializerManager] PlayerHandManager 초기화 실패 - slotRegistry 또는 turnManager가 null입니다.");
            }

            yield return null;
        }

        private IEnumerator InitializeEnemy()
        {
            Debug.Log("[CombatInitializerManager] 적 초기화 단계 시작");

            var stage = stageManager?.GetCurrentStage();
            if (stage == null || stage.enemies == null || stage.enemies.Count == 0)
            {
                Debug.LogError("[CombatInitializerManager] 현재 스테이지에 적 데이터가 없습니다.");
                yield break;
            }

            var enemyData = stage.enemies[0];
            Debug.Log($"[CombatInitializerManager] 적 데이터 선택됨: {enemyData.displayName}");

            enemyInitializer?.SetupWithData(enemyData);
            yield return null;

            var enemy = enemyInitializer?.GetSpawnedEnemy();
            if (enemy == null)
            {
                Debug.LogError("[CombatInitializerManager] 적 캐릭터 인스턴스가 생성되지 않았습니다.");
                yield break;
            }

            enemyManager?.RegisterEnemy(enemy);
            enemyHandManager?.Initialize(enemy);
        }

        private IEnumerator InitializeHands()
        {
            Debug.Log("[CombatInitializerManager] 초기 핸드 생성 단계");

            playerHandManager?.GenerateInitialHand();
            enemyHandManager?.GenerateInitialHand();

            Debug.Log("[CombatInitializerManager] 핸드 생성 완료");
            yield return null;
        }

        private IEnumerator StartCombat()
        {
            Debug.Log("[CombatInitializerManager] 전투 시작 상태 전이");

            if (turnManager == null || stateFactory == null)
            {
                Debug.LogError("[CombatInitializerManager] 전투 시작 실패 - TurnManager 또는 StateFactory 누락");
                yield break;
            }

            var state = stateFactory.CreatePrepareState();
            Debug.Log("[CombatInitializerManager] PrepareState 생성 완료 → 상태 전이 요청");
            turnManager.RequestStateChange(state);

            yield return null;
        }
    }
}
