using System.Collections;
using UnityEngine;
using System.Linq;
using Game.CombatSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Manager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.UI;
using Game.SkillCardSystem.Deck;
using Game.SkillCardSystem.Manager;
using Game.SkillCardSystem.Service;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;
using Game.CoreSystem.Utility;
using Game.StageSystem.Interface;
using Game.StageSystem.Manager;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 싱글게임용 전투 시작 시퀀스 관리자
    /// 새로운 통합 CombatManager로 대체되었습니다.
    /// 이 클래스는 호환성을 위해 유지되지만 실제로는 CombatManager를 사용합니다.
    /// </summary>
    [System.Obsolete("CombatStartupManager는 더 이상 사용되지 않습니다. CombatManager를 사용하세요.")]
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
        
        [Header("UI 프리팹")]
        [Tooltip("스킬카드 UI 프리팹")]
        [SerializeField] private SkillCardUI cardUIPrefab;

        [Header("의존성 매니저")]
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private EnemySpawnerManager enemySpawnerManager;
        [SerializeField] private EnemyManager enemyManager;
        [SerializeField] private PlayerHandManager playerHandManager;
        [SerializeField] private CoroutineRunner coroutineRunner;
        [SerializeField] private StageManager stageManager;

        #endregion

        #region 내부 상태
        // hasStartedCombat 변수 제거 (사용되지 않음)
        #endregion

        #region 초기화

        private void Start()
        {
            GameLogger.LogInfo("CombatStartupManager 시작 - CombatManager로 전환", GameLogger.LogCategory.Combat);
            
            // 새로운 CombatManager 사용
            var combatManager = FindFirstObjectByType<CombatManager>();
            if (combatManager == null)
            {
                var combatManagerObj = new GameObject("CombatManager");
                combatManager = combatManagerObj.AddComponent<CombatManager>();
            }
            
            // CombatManager를 통해 전투 시작
            combatManager.StartCombat();
        }
        
        private IEnumerator DelayedCombatStart()
        {
            // 다른 시스템들이 초기화될 시간을 기다림
            yield return new WaitForSeconds(2.0f);
            
            GameLogger.LogInfo("자동 전투 시작 시퀀스 실행", GameLogger.LogCategory.Combat);
            StartCombatSequence(success => 
            {
                if (success)
                {
                    GameLogger.LogInfo("자동 전투 시작 완료", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogError("자동 전투 시작 실패", GameLogger.LogCategory.Error);
                }
            });
        }

        private void AutoInjectDependencies()
        {
            if (playerManager == null)
                playerManager = FindFirstObjectByType<PlayerManager>();
            if (enemySpawnerManager == null)
                enemySpawnerManager = FindFirstObjectByType<EnemySpawnerManager>();
            if (enemyManager == null)
                enemyManager = FindFirstObjectByType<EnemyManager>();
            if (playerHandManager == null)
                playerHandManager = FindFirstObjectByType<PlayerHandManager>();
            // Note: turnCardRegistry, placementService, turnManager are now handled by singleton managers
            if (coroutineRunner == null)
                coroutineRunner = FindFirstObjectByType<CoroutineRunner>();
            if (stageManager == null)
                stageManager = FindFirstObjectByType<StageManager>();
            // Note: slotRegistry is now handled by CombatSlotManager singleton
        }

        #endregion

        #region 전투 시작 시퀀스

        /// <summary>
        /// 전투 시작 시퀀스를 실행합니다.
        /// </summary>
        /// <param name="onComplete">전투 시작 완료 콜백</param>
        public void StartCombatSequence(System.Action<bool> onComplete)
        {
            StartCoroutine(ExecuteCombatStartupSequence(onComplete));
        }

        /// <summary>
        /// 전투 시작 시퀀스를 순차적으로 실행합니다.
        /// </summary>
        private IEnumerator ExecuteCombatStartupSequence(System.Action<bool> onComplete)
        {
            GameLogger.LogInfo("전투 시작 시퀀스 시작", GameLogger.LogCategory.Combat);

            bool success = true;
            
            // 1. 슬롯 초기화
            yield return StartCoroutine(InitializeSlots());
            
            // 2. 플레이어 캐릭터 스폰
            yield return StartCoroutine(SpawnPlayerCharacter());
            
            // 3. 적 캐릭터 스폰
            yield return StartCoroutine(SpawnEnemyCharacter());
            
            // 4. 전투 공간에 적 카드 배치
            yield return StartCoroutine(PlaceEnemyCardsInWaitSlots());
            
            // 5. 플레이어 핸드 카드 생성
            yield return StartCoroutine(GeneratePlayerHandCards());
            
            // 6. 전투 슬롯에 플레이어 카드 예약 (플레이어가 먼저 시작할 수 있도록)
            yield return StartCoroutine(ReservePlayerCardInBattleSlot());
            
            // 7. 플레이어 입력 턴 활성화
            yield return StartCoroutine(EnablePlayerInputTurn());
            
            // 8. 전투 준비 완료
            yield return StartCoroutine(CompleteCombatPreparation());

            GameLogger.LogInfo($"전투 시작 시퀀스 완료 (성공: {success})", GameLogger.LogCategory.Combat);
            onComplete?.Invoke(success);
        }

        #endregion

        #region 슬롯 초기화

        /// <summary>
        /// 슬롯을 초기화합니다.
        /// </summary>
        private IEnumerator InitializeSlots()
        {
            GameLogger.LogInfo("슬롯 초기화 시작", GameLogger.LogCategory.Combat);
            
            // 새로운 슬롯 시스템 초기화
            CombatSlotManager.Instance.InitializeSlots();
            TurnManager.Instance.ResetTurn();

            // 슬롯 상태 로그 출력
            CombatSlotManager.Instance.LogSlotStates();

                GameLogger.LogInfo("슬롯 초기화 완료", GameLogger.LogCategory.Combat);
            yield return null;
        }

        #endregion

        #region 캐릭터 스폰

        /// <summary>
        /// 플레이어 캐릭터를 스폰합니다.
        /// </summary>
        private IEnumerator SpawnPlayerCharacter()
        {
            GameLogger.LogInfo("플레이어 캐릭터 스폰 시작", GameLogger.LogCategory.Combat);
            
            if (playerManager != null)
            {
            playerManager.CreateAndRegisterPlayer();
                yield return new WaitForSeconds(characterSpawnDuration);
            }
            else
            {
                GameLogger.LogWarning("PlayerManager를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
            }
            
            GameLogger.LogInfo("플레이어 캐릭터 스폰 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 적 캐릭터를 스폰합니다.
        /// </summary>
        private IEnumerator SpawnEnemyCharacter()
        {
            GameLogger.LogInfo("적 캐릭터 스폰 시작", GameLogger.LogCategory.Combat);
            
            if (enemySpawnerManager != null && enemyManager != null)
            {
                GameLogger.LogInfo("EnemySpawnerManager와 EnemyManager 발견", GameLogger.LogCategory.Combat);
                
                var enemyData = GetCurrentEnemyData();
                if (enemyData != null)
                {
                    GameLogger.LogInfo($"적 데이터 발견: {enemyData.name}", GameLogger.LogCategory.Combat);
                    
                    bool spawnCompleted = false;
                    EnemySpawnResult spawnResult = null;
                    float timeout = 10f; // 10초 타임아웃
                    float elapsed = 0f;
                    
                    // 스폰 시작
                    enemySpawnerManager.SpawnEnemyWithAnimation(enemyData, (result) => {
                        spawnResult = result;
                        spawnCompleted = true;
                        GameLogger.LogInfo($"적 캐릭터 스폰 콜백 호출: {result}", GameLogger.LogCategory.Combat);
                    });
                    
                    // 스폰 완료까지 대기 (타임아웃 포함)
                    while (!spawnCompleted && elapsed < timeout)
                    {
                        elapsed += Time.deltaTime;
                yield return null;
            }

                    if (spawnCompleted)
                    {
                        GameLogger.LogInfo("적 캐릭터 스폰 완료 확인", GameLogger.LogCategory.Combat);
                        yield return new WaitForSeconds(0.5f); // 추가 대기시간으로 등록 완료 보장
                        
                        // 스폰 결과 확인
                        if (spawnResult != null && spawnResult.IsNewlySpawned)
                        {
                            var spawnedEnemy = enemyManager.GetEnemy();
                            if (spawnedEnemy != null)
                            {
                                GameLogger.LogInfo($"적 캐릭터 스폰 확인: {spawnedEnemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                            }
                            else
                            {
                                GameLogger.LogWarning("적 캐릭터 스폰 후 EnemyManager에서 확인 실패", GameLogger.LogCategory.Combat);
                            }
                        }
                        else
                        {
                            GameLogger.LogWarning($"적 캐릭터 스폰 실패: {spawnResult}", GameLogger.LogCategory.Combat);
                        }
                    }
                    else
                    {
                        GameLogger.LogError($"적 캐릭터 스폰 타임아웃 ({timeout}초)", GameLogger.LogCategory.Error);
                        
                        // 타임아웃 시 대안 방법으로 적 캐릭터 생성 시도
                        GameLogger.LogInfo("대안 방법으로 적 캐릭터 생성 시도", GameLogger.LogCategory.Combat);
                        yield return StartCoroutine(CreateEnemyCharacterDirectly(enemyData));
                    }
                }
                else
                {
                    GameLogger.LogWarning("적 데이터를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                }
            }
            else
            {
                GameLogger.LogWarning($"EnemySpawnerManager: {enemySpawnerManager != null}, EnemyManager: {enemyManager != null}", GameLogger.LogCategory.Combat);
            }

            GameLogger.LogInfo("적 캐릭터 스폰 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 대안 방법으로 적 캐릭터를 직접 생성합니다.
        /// </summary>
        /// <param name="enemyData">적 데이터</param>
        private IEnumerator CreateEnemyCharacterDirectly(EnemyCharacterData enemyData)
        {
            GameLogger.LogInfo("대안 방법으로 적 캐릭터 직접 생성 시작", GameLogger.LogCategory.Combat);
            
            try
            {
                // EnemyManager를 통해 직접 적 캐릭터 생성
                if (enemyManager != null)
                {
                    // 간단한 적 캐릭터 생성 (애니메이션 없이)
                    var enemyPrefab = Resources.Load<GameObject>("Prefabs/EnemyCharacter");
                    if (enemyPrefab != null)
                    {
                        var enemyInstance = Instantiate(enemyPrefab);
                        var enemyCharacter = enemyInstance.GetComponent<IEnemyCharacter>();
                        
                        if (enemyCharacter != null)
                        {
                            // EnemyCharacter로 캐스팅하여 Initialize 메서드 호출
                            var enemyCharacterImpl = enemyCharacter as Game.CharacterSystem.Core.EnemyCharacter;
                            if (enemyCharacterImpl != null)
                            {
                                enemyCharacterImpl.Initialize(enemyData);
                                enemyManager.RegisterEnemy(enemyCharacter);
                                
                                GameLogger.LogInfo($"대안 방법으로 적 캐릭터 생성 완료: {enemyCharacter.GetCharacterName()}", GameLogger.LogCategory.Combat);
                            }
                            else
                            {
                                GameLogger.LogError("EnemyCharacter 구현체로 캐스팅할 수 없습니다", GameLogger.LogCategory.Error);
                            }
                        }
                        else
                        {
                            GameLogger.LogError("적 캐릭터 컴포넌트를 찾을 수 없습니다", GameLogger.LogCategory.Error);
                        }
                    }
                    else
                    {
                        GameLogger.LogError("적 캐릭터 프리팹을 찾을 수 없습니다", GameLogger.LogCategory.Error);
                    }
            }
            else
            {
                    GameLogger.LogError("EnemyManager가 null입니다", GameLogger.LogCategory.Error);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"대안 적 캐릭터 생성 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
            
            yield return new WaitForSeconds(1f); // 생성 완료 대기
            GameLogger.LogInfo("대안 방법으로 적 캐릭터 직접 생성 완료", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 카드 배치

        /// <summary>
        /// 대기 슬롯에 적 카드를 배치합니다.
        /// </summary>
        private IEnumerator PlaceEnemyCardsInWaitSlots()
        {
            GameLogger.LogInfo("전투 공간에 적 카드 배치 시작", GameLogger.LogCategory.Combat);

            var enemyData = GetCurrentEnemyData();
            EnemySkillDeck enemyDeck = null;
            
            // 적 덱 확인 및 기본 덱 사용
            if (enemyData?.EnemyDeck != null)
            {
                enemyDeck = enemyData.EnemyDeck;
                GameLogger.LogInfo($"적 데이터에서 덱 발견: {enemyDeck.name}", GameLogger.LogCategory.Combat);
            }
            else
            {
                enemyDeck = CreateDefaultEnemyDeck();
                GameLogger.LogInfo($"기본 적 덱 사용: {enemyDeck.name}", GameLogger.LogCategory.Combat);
            }

            if (enemyDeck == null)
            {
                GameLogger.LogWarning("적 덱을 생성할 수 없습니다", GameLogger.LogCategory.Combat);
                yield break;
            }
            
            var enemySlots = new[] { 
                CombatSlotPosition.WAIT_SLOT_1, 
                CombatSlotPosition.WAIT_SLOT_3 
            };

            foreach (var position in enemySlots)
            {
                var enemyCard = SelectEnemyCardByProbability(enemyDeck);
                if (enemyCard != null)
                {
                    var success = CombatSlotManager.Instance.TryPlaceCard(position, enemyCard);
                    if (success)
                    {
                        // UI 생성
                        CreateEnemyCardUI(enemyCard, position);
                        GameLogger.LogInfo($"적 카드 배치 성공: {enemyCard.GetCardName()} → {position}", GameLogger.LogCategory.Combat);
                    }
                    else
                    {
                        GameLogger.LogWarning($"적 카드 배치 실패: {enemyCard.GetCardName()} → {position}", GameLogger.LogCategory.Combat);
                    }
                }
                else
                {
                    GameLogger.LogWarning($"적 카드 선택 실패: {position}", GameLogger.LogCategory.Combat);
                }
                    
                    yield return new WaitForSeconds(cardPlacementDuration);
            }
            
            GameLogger.LogInfo("전투 공간에 적 카드 배치 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 플레이어 핸드 카드를 생성합니다.
        /// </summary>
        private IEnumerator GeneratePlayerHandCards()
        {
            GameLogger.LogInfo("플레이어 핸드 카드 생성 시작", GameLogger.LogCategory.Combat);
            
            if (playerHandManager != null)
            {
                GameLogger.LogInfo("PlayerHandManager 발견, 핸드 생성 시도", GameLogger.LogCategory.Combat);
                playerHandManager.GenerateInitialHand();
                yield return new WaitForSeconds(cardPlacementDuration);
                
                // 핸드 생성 결과 확인
                var handCards = playerHandManager.GetAllHandCards();
                GameLogger.LogInfo($"플레이어 핸드 카드 생성 완료: {handCards?.Count() ?? 0}장", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogWarning("PlayerHandManager를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                
                // PlayerHandManager가 없어도 기본 카드라도 생성해보자
                GameLogger.LogInfo("기본 플레이어 카드 생성 시도", GameLogger.LogCategory.Combat);
                yield return new WaitForSeconds(cardPlacementDuration);
            }
            
            GameLogger.LogInfo("플레이어 핸드 카드 생성 완료", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 플레이어 카드 예약 및 입력 턴 활성화

        /// <summary>
        /// 전투 슬롯에 플레이어 카드를 예약합니다 (플레이어가 먼저 시작할 수 있도록).
        /// </summary>
        private IEnumerator ReservePlayerCardInBattleSlot()
        {
            GameLogger.LogInfo("전투 슬롯에 플레이어 카드 예약 시작", GameLogger.LogCategory.Combat);

            // 플레이어 핸드에서 첫 번째 카드를 가져와서 전투 슬롯에 예약
            if (playerHandManager != null)
            {
                var handCards = playerHandManager.GetAllHandCards();
                if (handCards != null && handCards.Any())
                {
                    var firstCard = handCards.First().card;
                    if (firstCard != null)
                    {
                        // 전투 슬롯에 플레이어 카드 예약 (실제로는 드래그/드롭으로 배치해야 함)
                        GameLogger.LogInfo($"플레이어 카드 예약: {firstCard.GetCardName()} → BATTLE_SLOT (드래그/드롭 대기)", GameLogger.LogCategory.Combat);
                        
                        // 플레이어가 드래그/드롭할 수 있도록 UI 활성화
                        EnablePlayerCardDragDrop();
                    }
                    else
                    {
                        GameLogger.LogWarning("플레이어 핸드의 첫 번째 카드가 null입니다", GameLogger.LogCategory.Combat);
                    }
                }
                else
                {
                    GameLogger.LogWarning("플레이어 핸드에 카드가 없습니다", GameLogger.LogCategory.Combat);
                }
            }
            else
            {
                GameLogger.LogWarning("PlayerHandManager를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
            }

            yield return new WaitForSeconds(cardPlacementDuration);
            GameLogger.LogInfo("전투 슬롯에 플레이어 카드 예약 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 플레이어 입력 턴을 활성화합니다.
        /// </summary>
        private IEnumerator EnablePlayerInputTurn()
        {
            GameLogger.LogInfo("플레이어 입력 턴 활성화 시작", GameLogger.LogCategory.Combat);

            // 턴 매니저에서 플레이어 턴 활성화
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.SetTurn(TurnManager.TurnType.Player);
                GameLogger.LogInfo("플레이어 턴으로 설정 완료", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogWarning("TurnManager.Instance를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
            }

            // 드래그/드롭 시스템 활성화
            EnablePlayerCardDragDrop();

            yield return new WaitForSeconds(0.5f);
            GameLogger.LogInfo("플레이어 입력 턴 활성화 완료 - 게임 시작!", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 플레이어 카드 드래그/드롭을 활성화합니다.
        /// </summary>
        private void EnablePlayerCardDragDrop()
        {
            // 드래그 핸들러들을 활성화
            var dragHandlers = FindObjectsByType<Game.CombatSystem.DragDrop.CardDragHandler>(FindObjectsSortMode.None);
            foreach (var handler in dragHandlers)
            {
                if (handler != null)
                {
                    handler.enabled = true;
                }
            }

            // 드롭 핸들러들을 활성화
            var dropHandlers = FindObjectsByType<Game.CombatSystem.DragDrop.CardDropToSlotHandler>(FindObjectsSortMode.None);
            foreach (var handler in dropHandlers)
            {
                if (handler != null)
                {
                    handler.enabled = true;
                }
            }

            GameLogger.LogInfo($"드래그/드롭 시스템 활성화: 드래그 핸들러 {dragHandlers.Length}개, 드롭 핸들러 {dropHandlers.Length}개", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 전투 준비 완료

        /// <summary>
        /// 전투 준비를 완료합니다.
        /// </summary>
        private IEnumerator CompleteCombatPreparation()
        {
            GameLogger.LogInfo("전투 준비 완료 시작", GameLogger.LogCategory.Combat);

            // 최종 슬롯 상태 확인
            CombatSlotManager.Instance?.LogSlotStates();
            TurnManager.Instance?.LogTurnInfo();

            yield return new WaitForSeconds(combatStartDelay);

            GameLogger.LogInfo("전투 준비 완료 - 게임 시작 가능", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 현재 적 데이터를 반환합니다.
        /// </summary>
        /// <returns>적 캐릭터 데이터</returns>
        private EnemyCharacterData GetCurrentEnemyData()
        {
            if (stageManager != null)
            {
                var enemyData = stageManager.PeekNextEnemyData();
                if (enemyData != null)
                    return enemyData;
            }

            // 기본 적 데이터 생성
            return CreateDefaultEnemyData();
        }

        /// <summary>
        /// 기본 적 데이터를 생성합니다.
        /// </summary>
        /// <returns>기본 적 데이터</returns>
        private EnemyCharacterData CreateDefaultEnemyData()
        {
            var defaultData = ScriptableObject.CreateInstance<EnemyCharacterData>();
            defaultData.name = "기본 적";
            
            // 기본 적 덱 생성 및 설정
            var defaultDeck = CreateDefaultEnemyDeck();
            // Note: EnemyDeck이 read-only라면 다른 방법으로 설정해야 함
            // 일단 로그로 확인해보자
            
            GameLogger.LogInfo($"기본 적 데이터 생성 완료: {defaultData.name}", GameLogger.LogCategory.Combat);
            GameLogger.LogInfo($"기본 적 덱 생성 완료: {defaultDeck.name}, 카드 수: {defaultDeck.GetAllCards().Count}", GameLogger.LogCategory.Combat);
            
            return defaultData;
        }

        /// <summary>
        /// 기본 적 덱을 생성합니다.
        /// </summary>
        /// <returns>기본 적 덱</returns>
        private EnemySkillDeck CreateDefaultEnemyDeck()
        {
            var defaultDeck = ScriptableObject.CreateInstance<EnemySkillDeck>();
            defaultDeck.name = "기본 적 덱";
            
            // 기본 카드 추가 (test001)
            var testCard = Resources.Load<SkillCardDefinition>("SkillCards/test001");
            if (testCard != null)
            {
                var cardEntry = new EnemySkillDeck.CardEntry
                {
                    definition = testCard,
                    probability = 1.0f
                };
                defaultDeck.GetAllCards().Add(cardEntry);
            }

            GameLogger.LogInfo("기본 적 덱 생성 완료", GameLogger.LogCategory.Combat);
            return defaultDeck;
        }

        /// <summary>
        /// 확률에 따라 적 카드를 선택합니다.
        /// </summary>
        /// <param name="enemyDeck">적 덱</param>
        /// <returns>선택된 적 카드</returns>
        private ISkillCard SelectEnemyCardByProbability(EnemySkillDeck enemyDeck)
        {
            var validCards = enemyDeck.GetValidCards();
            if (validCards.Count == 0)
            {
                GameLogger.LogWarning("유효한 적 카드가 없습니다", GameLogger.LogCategory.Combat);
            return null;
            }

            var random = Random.Range(0f, 1f);
            float cumulativeProbability = 0f;

            foreach (var cardEntry in validCards)
            {
                cumulativeProbability += cardEntry.probability;
                if (random <= cumulativeProbability)
                {
                    var factory = new SkillCardFactory();
                    var card = factory.CreateFromDefinition(
                        cardEntry.definition, 
                        Owner.Enemy, 
                        "Enemy");
                    
                    GameLogger.LogInfo($"적 카드 선택: {cardEntry.definition.displayName} (확률: {cardEntry.probability:P0})", GameLogger.LogCategory.Combat);
                    return card;
                }
            }

            // 확률 계산 실패 시 첫 번째 카드 반환
            var firstCard = validCards[0];
            var fallbackFactory = new SkillCardFactory();
            var fallbackCard = fallbackFactory.CreateFromDefinition(
                firstCard.definition, 
                Owner.Enemy, 
                "Enemy");
            
            GameLogger.LogWarning($"확률 계산 실패, 첫 번째 카드 사용: {firstCard.definition.displayName}", GameLogger.LogCategory.Combat);
            return fallbackCard;
        }

        /// <summary>
        /// 적 카드 UI를 생성합니다.
        /// </summary>
        /// <param name="card">카드</param>
        /// <param name="position">슬롯 위치</param>
        /// <returns>생성된 카드 UI</returns>
        private SkillCardUI CreateEnemyCardUI(ISkillCard card, CombatSlotPosition position)
        {
            if (cardUIPrefab == null)
            {
                GameLogger.LogWarning("카드 UI 프리팹이 설정되지 않았습니다", GameLogger.LogCategory.Combat);
                return null;
            }

            var slot = CombatSlotManager.Instance?.GetSlot(position);
            if (slot == null)
            {
                GameLogger.LogWarning($"슬롯을 찾을 수 없습니다: {position}", GameLogger.LogCategory.Combat);
                return null;
            }

            // 다양한 슬롯 GameObject 명명 규칙 시도
            var slotGameObject = FindSlotGameObject(position);
            SkillCardUI cardUI = null;
            
            if (slotGameObject != null)
            {
                cardUI = Instantiate(cardUIPrefab, slotGameObject.transform);
                cardUI.SetCard(card);
                GameLogger.LogInfo($"적 카드 UI 생성 완료: {card.GetCardName()} → {position}", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogWarning($"슬롯 GameObject를 찾을 수 없습니다: {position}", GameLogger.LogCategory.Combat);
            }
            
            return cardUI;
        }

        /// <summary>
        /// 슬롯 위치에 해당하는 GameObject를 찾습니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>슬롯 GameObject</returns>
        private GameObject FindSlotGameObject(CombatSlotPosition position)
        {
            // 다양한 명명 규칙 시도
            var possibleNames = new[]
            {
                $"Slot_{position}",
                $"Slot{position}",
                $"CombatSlot_{position}",
                $"CombatSlot{position}",
                $"BattleSlot_{position}",
                $"BattleSlot{position}",
                position.ToString(),
                $"Slot_{(int)position}",
                $"Slot{(int)position}"
            };

            foreach (var name in possibleNames)
            {
                var gameObject = GameObject.Find(name);
                if (gameObject != null)
                {
                    GameLogger.LogInfo($"슬롯 GameObject 발견: {name}", GameLogger.LogCategory.Combat);
                    return gameObject;
                }
            }

            // 모든 슬롯 GameObject 찾기 시도 (태그가 정의되지 않았을 수 있으므로 try-catch 사용)
            try
            {
                var allSlots = GameObject.FindGameObjectsWithTag("CombatSlot");
                if (allSlots.Length > 0)
                {
                    GameLogger.LogInfo($"태그로 슬롯 GameObject {allSlots.Length}개 발견", GameLogger.LogCategory.Combat);
                    // 첫 번째 슬롯 반환 (임시)
                    return allSlots[0];
                }
            }
            catch (UnityException ex)
            {
                GameLogger.LogWarning($"CombatSlot 태그가 정의되지 않았습니다: {ex.Message}", GameLogger.LogCategory.Combat);
            }

            return null;
        }

        #endregion

        #region 디버그

        /// <summary>
        /// 전투 시작 시퀀스를 수동으로 실행합니다.
        /// </summary>
        [ContextMenu("전투 시작 시퀀스 실행")]
        public void StartCombatSequenceManual()
        {
            StartCombatSequence(success => 
            {
                if (success)
                {
                    GameLogger.LogInfo("전투 시작 시퀀스 수동 실행 완료", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogError("전투 시작 시퀀스 수동 실행 실패", GameLogger.LogCategory.Error);
                }
            });
        }

        #endregion
    }
}