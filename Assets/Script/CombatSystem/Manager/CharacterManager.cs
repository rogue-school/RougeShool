using UnityEngine;
using System.Collections;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 통합 캐릭터 매니저 - 플레이어와 적 캐릭터를 동일한 방식으로 관리
    /// 기존 PlayerManager와 EnemyManager를 통합하여 일관성 있는 캐릭터 관리 제공
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        #region 싱글톤 패턴
        
        public static CharacterManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        #endregion
        
        #region 캐릭터 관리
        
        // 플레이어 캐릭터
        private IPlayerCharacter player;
        
        // 적 캐릭터
        private IEnemyCharacter enemy;
        
        // 기존 매니저들 (호환성 유지)
        private PlayerManager playerManager;
        private EnemyManager enemyManager;
        private EnemySpawnerManager enemySpawnerManager;
        
        #endregion
        
        #region 초기화
        
        /// <summary>
        /// 캐릭터 매니저 초기화
        /// </summary>
        private void Initialize()
        {
            GameLogger.LogInfo("CharacterManager 초기화 시작", GameLogger.LogCategory.Combat);
            
            // 기존 매니저들 초기화
            InitializePlayerManager();
            InitializeEnemyManager();
            InitializeEnemySpawnerManager();
            
            GameLogger.LogInfo("CharacterManager 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 플레이어 매니저 초기화
        /// </summary>
        private void InitializePlayerManager()
        {
            playerManager = FindFirstObjectByType<PlayerManager>();
            if (playerManager == null)
            {
                var playerManagerObj = new GameObject("PlayerManager");
                playerManager = playerManagerObj.AddComponent<PlayerManager>();
            }
            GameLogger.LogInfo("플레이어 매니저 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 적 매니저 초기화
        /// </summary>
        private void InitializeEnemyManager()
        {
            enemyManager = FindFirstObjectByType<EnemyManager>();
            if (enemyManager == null)
            {
                var enemyManagerObj = new GameObject("EnemyManager");
                enemyManager = enemyManagerObj.AddComponent<EnemyManager>();
            }
            GameLogger.LogInfo("적 매니저 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 적 스폰 매니저 초기화
        /// </summary>
        private void InitializeEnemySpawnerManager()
        {
            enemySpawnerManager = FindFirstObjectByType<EnemySpawnerManager>();
            if (enemySpawnerManager == null)
            {
                var enemySpawnerManagerObj = new GameObject("EnemySpawnerManager");
                enemySpawnerManager = enemySpawnerManagerObj.AddComponent<EnemySpawnerManager>();
            }
            GameLogger.LogInfo("적 스폰 매니저 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        #endregion
        
        #region 플레이어 관리
        
        /// <summary>
        /// 플레이어 캐릭터 스폰
        /// </summary>
        public void SpawnPlayer()
        {
            GameLogger.LogInfo("플레이어 캐릭터 스폰 시작", GameLogger.LogCategory.Combat);
            
            if (playerManager != null)
            {
                // 플레이어 매니저를 통해 플레이어 가져오기
                player = playerManager.GetPlayer();
                
                if (player != null)
                {
                    GameLogger.LogInfo($"플레이어 캐릭터 스폰 완료: {player.GetCharacterName()}", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogWarning("플레이어 캐릭터 스폰 실패", GameLogger.LogCategory.Combat);
                }
            }
            else
            {
                GameLogger.LogError("PlayerManager가 null입니다", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// 플레이어 캐릭터 조회
        /// </summary>
        public IPlayerCharacter GetPlayer() => player;
        
        /// <summary>
        /// 플레이어 캐릭터 등록
        /// </summary>
        public void RegisterPlayer(IPlayerCharacter playerCharacter)
        {
            player = playerCharacter;
            GameLogger.LogInfo($"플레이어 캐릭터 등록: {playerCharacter.GetCharacterName()}", GameLogger.LogCategory.Combat);
        }
        
        #endregion
        
        #region 적 관리
        
        /// <summary>
        /// 적 캐릭터 스폰
        /// </summary>
        public IEnumerator SpawnEnemy()
        {
            GameLogger.LogInfo("적 캐릭터 스폰 시작", GameLogger.LogCategory.Combat);
            
            if (enemySpawnerManager != null && enemyManager != null)
            {
                // 기본 적 데이터 생성
                var enemyData = CreateDefaultEnemyData();
                
                bool spawnCompleted = false;
                EnemySpawnResult spawnResult = null;
                float timeout = 10f;
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
                    yield return new WaitForSeconds(0.5f);
                    
                    // 스폰 결과 확인
                    if (spawnResult != null && spawnResult.IsNewlySpawned)
                    {
                        enemy = enemyManager.GetEnemy();
                        if (enemy != null)
                        {
                            GameLogger.LogInfo($"적 캐릭터 스폰 확인: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
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
                GameLogger.LogWarning($"EnemySpawnerManager: {enemySpawnerManager != null}, EnemyManager: {enemyManager != null}", GameLogger.LogCategory.Combat);
            }
            
            GameLogger.LogInfo("적 캐릭터 스폰 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 대안 방법으로 적 캐릭터를 직접 생성
        /// </summary>
        private IEnumerator CreateEnemyCharacterDirectly(EnemyCharacterData enemyData)
        {
            GameLogger.LogInfo("대안 방법으로 적 캐릭터 직접 생성 시작", GameLogger.LogCategory.Combat);
            
            try
            {
                if (enemyManager != null)
                {
                    var enemyPrefab = Resources.Load<GameObject>("Prefabs/EnemyCharacter");
                    if (enemyPrefab != null)
                    {
                        var enemyInstance = Instantiate(enemyPrefab);
                        var enemyCharacter = enemyInstance.GetComponent<IEnemyCharacter>();
                        
                        if (enemyCharacter != null)
                        {
                            var enemyCharacterImpl = enemyCharacter as EnemyCharacter;
                            if (enemyCharacterImpl != null)
                            {
                                enemyCharacterImpl.Initialize(enemyData);
                                enemyManager.RegisterEnemy(enemyCharacter);
                                enemy = enemyCharacter;
                                
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
            
            yield return new WaitForSeconds(1f);
            GameLogger.LogInfo("대안 방법으로 적 캐릭터 직접 생성 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 기본 적 데이터 생성
        /// </summary>
        private EnemyCharacterData CreateDefaultEnemyData()
        {
            var enemyData = ScriptableObject.CreateInstance<EnemyCharacterData>();
            
            // 기본 적 데이터 설정
            enemyData.name = "DefaultEnemy";
            
            GameLogger.LogInfo("기본 적 데이터 생성 완료", GameLogger.LogCategory.Combat);
            return enemyData;
        }
        
        /// <summary>
        /// 적 캐릭터 조회
        /// </summary>
        public IEnemyCharacter GetEnemy() => enemy;
        
        /// <summary>
        /// 적 캐릭터 등록
        /// </summary>
        public void RegisterEnemy(IEnemyCharacter enemyCharacter)
        {
            enemy = enemyCharacter;
            GameLogger.LogInfo($"적 캐릭터 등록: {enemyCharacter.GetCharacterName()}", GameLogger.LogCategory.Combat);
        }
        
        #endregion
        
        #region 공통 캐릭터 관리
        
        /// <summary>
        /// 현재 턴의 캐릭터 조회
        /// </summary>
        public ICharacter GetCurrentCharacter()
        {
            var turnManager = TurnManager.Instance;
            if (turnManager != null)
            {
                var currentTurn = turnManager.GetCurrentTurn();
                return currentTurn == TurnManager.TurnType.Player ? player : enemy;
            }
            return null;
        }
        
        /// <summary>
        /// 플레이어 턴인지 확인
        /// </summary>
        public bool IsPlayerTurn()
        {
            var turnManager = TurnManager.Instance;
            return turnManager != null && turnManager.GetCurrentTurn() == TurnManager.TurnType.Player;
        }
        
        /// <summary>
        /// 적 턴인지 확인
        /// </summary>
        public bool IsEnemyTurn()
        {
            var turnManager = TurnManager.Instance;
            return turnManager != null && turnManager.GetCurrentTurn() == TurnManager.TurnType.Enemy;
        }
        
        /// <summary>
        /// 캐릭터 초기화
        /// </summary>
        public void ClearCharacters()
        {
            player = null;
            enemy = null;
            GameLogger.LogInfo("캐릭터 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        #endregion
    }
}
