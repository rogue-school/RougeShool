using System.Collections;
using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Utility;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.AnimationSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Interface;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 시작 시퀀스를 관리하는 매니저입니다.
    /// 플레이어/적 캐릭터 스폰, 카드 배치, 턴 시스템 초기화를 순차적으로 처리합니다.
    /// </summary>
    public class CombatStartupManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("전투 시작 설정")]
        [Tooltip("캐릭터 스폰 애니메이션 지속시간")]
        [SerializeField] private float characterSpawnDuration = 1.5f;
        
        [Tooltip("카드 배치 애니메이션 지속시간")]
        [SerializeField] private float cardPlacementDuration = 0.8f;
        
        [Tooltip("전투 시작 전 대기시간")]
        [SerializeField] private float combatStartDelay = 1.0f;

        #endregion

        #region Private Fields

        private IPlayerManager playerManager;
        private IEnemySpawnerManager enemySpawnerManager;
        private IEnemyManager enemyManager;
        private IPlayerHandManager playerHandManager;
        private ITurnCardRegistry turnCardRegistry;
        private ICardPlacementService placementService;
        private ICombatSlotRegistry slotRegistry;
        private ICombatTurnManager turnManager;
        private ICombatFlowCoordinator flowCoordinator;
        private IAnimationFacade animationFacade;
        private ICoroutineRunner coroutineRunner;

        #endregion

        #region DI

        /// <summary>
        /// Zenject 의존성 주입.
        /// </summary>
        [Inject]
        public void Construct(
            IPlayerManager playerManager,
            IEnemySpawnerManager enemySpawnerManager,
            IEnemyManager enemyManager,
            IPlayerHandManager playerHandManager,
            ITurnCardRegistry turnCardRegistry,
            ICardPlacementService placementService,
            ICombatSlotRegistry slotRegistry,
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            IAnimationFacade animationFacade,
            ICoroutineRunner coroutineRunner)
        {
            this.playerManager = playerManager;
            this.enemySpawnerManager = enemySpawnerManager;
            this.enemyManager = enemyManager;
            this.playerHandManager = playerHandManager;
            this.turnCardRegistry = turnCardRegistry;
            this.placementService = placementService;
            this.slotRegistry = slotRegistry;
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.animationFacade = animationFacade;
            this.coroutineRunner = coroutineRunner;
        }

        #endregion

        #region 전투 시작 시퀀스

        /// <summary>
        /// 전투 시작 시퀀스를 실행합니다.
        /// </summary>
        /// <param name="onComplete">전투 시작 완료 콜백</param>
        public void StartCombatSequence(System.Action<bool> onComplete)
        {
            coroutineRunner.RunCoroutine(ExecuteCombatStartupSequence(onComplete));
        }

        /// <summary>
        /// 전투 시작 시퀀스를 순차적으로 실행합니다.
        /// </summary>
        private IEnumerator ExecuteCombatStartupSequence(System.Action<bool> onComplete)
        {
            GameLogger.LogInfo("전투 시작 시퀀스 시작", GameLogger.LogCategory.Combat);

            bool success = true;
            
            // 1. 플레이어 캐릭터 스폰
            yield return StartCoroutine(SpawnPlayerCharacter());
            
            // 2. 적 캐릭터 스폰
            yield return StartCoroutine(SpawnEnemyCharacter());
            
            // 3. 적 스킬카드 대기 슬롯 배치
            yield return StartCoroutine(PlaceEnemyCardsInWaitSlots());
            
            // 4. 플레이어 핸드 카드 생성
            yield return StartCoroutine(GeneratePlayerHandCards());
            
            // 5. 턴 시스템 초기화
            yield return StartCoroutine(InitializeTurnSystem());
            
            // 6. 전투 시작 대기
            yield return new WaitForSeconds(combatStartDelay);
            
            GameLogger.LogInfo("전투 시작 시퀀스 완료", GameLogger.LogCategory.Combat);
            onComplete?.Invoke(success);
        }

        #endregion

        #region 개별 시퀀스 단계

        /// <summary>
        /// 플레이어 캐릭터를 스폰합니다.
        /// </summary>
        private IEnumerator SpawnPlayerCharacter()
        {
            GameLogger.LogInfo("플레이어 캐릭터 스폰 시작", GameLogger.LogCategory.Combat);
            
            playerManager.CreateAndRegisterPlayer();
            
            // 플레이어 캐릭터 등장 애니메이션
            var player = playerManager.GetPlayer();
            if (player != null && player is MonoBehaviour playerMono)
            {
                bool animDone = false;
                animationFacade.PlayCharacterAnimation(
                    player.CharacterData.name,
                    "spawn",
                    playerMono.gameObject,
                    () => animDone = true,
                    false // isEnemy = false
                );
                
                yield return new WaitUntil(() => animDone);
                yield return new WaitForSeconds(characterSpawnDuration);
            }
            
            GameLogger.LogInfo("플레이어 캐릭터 스폰 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 적 캐릭터를 스폰합니다.
        /// </summary>
        private IEnumerator SpawnEnemyCharacter()
        {
            GameLogger.LogInfo("적 캐릭터 스폰 시작", GameLogger.LogCategory.Combat);
            
            // 적 데이터 가져오기 (StageManager에서 설정된 적)
            var enemyData = GetCurrentEnemyData();
            if (enemyData == null)
            {
                GameLogger.LogError("적 데이터를 찾을 수 없습니다", GameLogger.LogCategory.Error);
                yield break;
            }
            
            // 적 스폰 및 애니메이션
            bool spawnDone = false;
            EnemySpawnResult result = null;
            
            StartCoroutine(enemySpawnerManager.SpawnEnemyWithAnimation(
                enemyData,
                (spawnResult) => {
                    result = spawnResult;
                    spawnDone = true;
                }
            ));
            
            yield return new WaitUntil(() => spawnDone);
            
            if (result?.IsNewlySpawned == true)
            {
                GameLogger.LogInfo($"적 캐릭터 스폰 완료: {enemyData.DisplayName}", GameLogger.LogCategory.Combat);
                yield return new WaitForSeconds(characterSpawnDuration);
            }
            else
            {
                GameLogger.LogError("적 캐릭터 스폰 실패", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 적 스킬카드를 대기 슬롯에 배치합니다.
        /// </summary>
        private IEnumerator PlaceEnemyCardsInWaitSlots()
        {
            GameLogger.LogInfo("적 스킬카드 대기 슬롯 배치 시작", GameLogger.LogCategory.Combat);
            
            var enemy = enemyManager.GetCurrentEnemy();
            if (enemy?.CharacterData is not EnemyCharacterData enemyData)
            {
                GameLogger.LogWarning("적 캐릭터 데이터를 찾을 수 없어 스킬카드 배치를 건너뜁니다", GameLogger.LogCategory.Combat);
                yield break;
            }
            
            if (enemyData.EnemyDeck == null)
            {
                GameLogger.LogWarning("적 덱이 없어 스킬카드 배치를 건너뜁니다", GameLogger.LogCategory.Combat);
                yield break;
            }
            
            var enemyCards = enemyData.EnemyDeck.GetAllCards();
            var waitSlots = new[] { 
                CombatSlotPosition.WAIT_SLOT_4, 
                CombatSlotPosition.WAIT_SLOT_3, 
                CombatSlotPosition.WAIT_SLOT_2, 
                CombatSlotPosition.WAIT_SLOT_1 
            };
            
            var factory = new SkillCardFactory();
            
            for (int i = 0; i < Mathf.Min(enemyCards.Count, waitSlots.Length); i++)
            {
                var cardEntry = enemyCards[i];
                var cardDefinition = cardEntry.definition;
                var slotPosition = waitSlots[i];
                
                if (cardDefinition == null)
                {
                    GameLogger.LogWarning($"적 카드 정의가 null입니다. 건너뜁니다.", GameLogger.LogCategory.Combat);
                    continue;
                }
                
                // 적 카드 생성
                var skillCard = factory.CreateFromDefinition(
                    cardDefinition,
                    Owner.Enemy,
                    enemyData.name
                );
                
                // 카드 UI 생성 및 배치
                var cardUI = CreateEnemyCardUI(skillCard);
                if (cardUI != null)
                {
                // 카드 배치
                var slot = slotRegistry.GetCombatSlot(slotPosition);
                if (slot != null)
                {
                    placementService.PlaceCardInSlot(skillCard, cardUI, slot);
                }
                else
                {
                    GameLogger.LogWarning($"슬롯 {slotPosition}을 찾을 수 없습니다.", GameLogger.LogCategory.Combat);
                }
                    
                    // 턴 카드 레지스트리에 등록
                    turnCardRegistry.RegisterCard(
                        slotPosition,
                        skillCard,
                        cardUI,
                        SlotOwner.ENEMY
                    );
                    
                    yield return new WaitForSeconds(cardPlacementDuration);
                }
            }
            
            GameLogger.LogInfo("적 스킬카드 대기 슬롯 배치 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 플레이어 핸드 카드를 생성합니다.
        /// </summary>
        private IEnumerator GeneratePlayerHandCards()
        {
            GameLogger.LogInfo("플레이어 핸드 카드 생성 시작", GameLogger.LogCategory.Combat);
            
            playerHandManager.GenerateInitialHand();
            
            // 핸드 카드 생성 애니메이션 대기
            yield return new WaitForSeconds(0.5f);
            
            GameLogger.LogInfo("플레이어 핸드 카드 생성 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 턴 시스템을 초기화합니다.
        /// </summary>
        private IEnumerator InitializeTurnSystem()
        {
            GameLogger.LogInfo("턴 시스템 초기화 시작", GameLogger.LogCategory.Combat);
            
            // 첫 턴 결정 (플레이어 또는 적)
            bool isPlayerFirst = UnityEngine.Random.value < 0.5f;
            flowCoordinator.IsEnemyFirst = !isPlayerFirst;
            
            // 턴 매니저 초기화
            turnManager.Initialize();
            
            GameLogger.LogInfo($"턴 시스템 초기화 완료 - 첫 턴: {(isPlayerFirst ? "플레이어" : "적")}", GameLogger.LogCategory.Combat);
            
            yield return null;
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 현재 적 데이터를 가져옵니다.
        /// </summary>
        private EnemyCharacterData GetCurrentEnemyData()
        {
            // StageManager에서 현재 스테이지의 적 데이터를 가져와야 함
            // 임시로 기본 적 데이터 반환
            return Resources.Load<EnemyCharacterData>("EnemyData/DefaultEnemy");
        }

        /// <summary>
        /// 적 카드 UI를 생성합니다.
        /// </summary>
        private SkillCardUI CreateEnemyCardUI(ISkillCard skillCard)
        {
            // 적 카드 UI 생성 로직
            // 실제 구현에서는 SkillCardUIFactory를 사용해야 함
            return null; // 임시
        }

        #endregion
    }
}
