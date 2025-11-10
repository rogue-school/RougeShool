using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.CoreSystem.Utility;
using Game.CombatSystem;
using Game.CombatSystem.Manager;
using Game.CharacterSystem.Manager;
using Game.StageSystem.Manager;

namespace Game.CoreSystem.Statistics
{
    /// <summary>
    /// 게임 세션 통계 수집기
    /// 플레이 세션 전체의 통계를 수집하고 관리합니다.
    /// </summary>
    public class GameSessionStatistics : MonoBehaviour
    {
        /// <summary>
        /// 현재 세션 통계 데이터
        /// </summary>
        private SessionStatisticsData _currentSession;

        /// <summary>
        /// 게임 시작 시간
        /// </summary>
        private float _gameStartTime;

        /// <summary>
        /// 게임 종료 시간
        /// </summary>
        private float _gameEndTime;

        /// <summary>
        /// 현재 전투 통계 데이터
        /// </summary>
        private CombatStatisticsData _currentCombatStats;

        /// <summary>
        /// 전투 시작 시간
        /// </summary>
        private float _combatStartTime;

        /// <summary>
        /// 현재 전투 승리 여부 플래그
        /// </summary>
        private bool _isCurrentCombatVictory;

        /// <summary>
        /// 의존성 주입
        /// </summary>
        [Inject(Optional = true)] private ICombatStatsProvider _combatStatsProvider;
        [Inject(Optional = true)] private PlayerManager _playerManager;
        [Inject(Optional = true)] private StageManager _stageManager;
        [Inject(Optional = true)] private Game.ItemSystem.Interface.IItemService _itemService;

        /// <summary>
        /// CombatStatsAggregator 캐시 (성능 최적화)
        /// </summary>
        private CombatStatsAggregator _cachedCombatStatsAggregator;

        /// <summary>
        /// 세션이 시작되었는지 여부
        /// </summary>
        public bool IsSessionActive { get; private set; }

        /// <summary>
        /// 세션이 저장되었는지 여부
        /// </summary>
        public bool IsSaved { get; private set; }

        private void Awake()
        {
            // 씬 전환 후에도 GameObject 유지 (CoreScene과 StageScene 간 통신)
            // 참고: CoreContainer의 DontDestroyOnLoadContainer가 이미 자식 오브젝트를 유지하지만,
            //       안전을 위해 중복 적용 (DontDestroyOnLoad는 중복 호출해도 안전함)
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
                GameLogger.LogInfo("[GameSessionStatistics] Awake 완료 - DontDestroyOnLoad 설정 (루트 오브젝트)", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogInfo("[GameSessionStatistics] Awake 완료 - CoreContainer의 자식으로 유지됨", GameLogger.LogCategory.UI);
            }
        }

        private void Start()
        {
            // Start()에서 구독하여 모든 컴포넌트 초기화 후 이벤트 구독
            SubscribeToEvents();
            GameLogger.LogInfo("[GameSessionStatistics] Start 완료 - 이벤트 구독 시작", GameLogger.LogCategory.UI);
        }

        private void OnEnable()
        {
            // OnEnable()에서도 구독 (씬 전환 후 재활성화 시)
            SubscribeToEvents();
            GameLogger.LogInfo("[GameSessionStatistics] OnEnable 완료 - 이벤트 구독 시작", GameLogger.LogCategory.UI);
        }

        private void OnDisable()
        {
            // OnDisable()에서 구독 해제 (씬 전환 시)
            UnsubscribeFromEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            // 기존 구독 해제 (중복 방지)
            UnsubscribeFromEvents();
            
            CombatEvents.OnCombatStarted += HandleCombatStarted;
            CombatEvents.OnVictory += HandleVictory;
            CombatEvents.OnDefeat += HandleDefeat;
            CombatEvents.OnGameOver += HandleGameOver;
            
            // 아이템 서비스 이벤트 구독
            if (_itemService != null)
            {
                // 세션 레벨 즉시 집계: 획득/제거 모두 반영
                _itemService.OnActiveItemAdded += HandleActiveItemAdded;
                _itemService.OnActiveItemRemoved += HandleActiveItemRemoved;
                GameLogger.LogInfo("[GameSessionStatistics] ItemService 이벤트 구독 완료", GameLogger.LogCategory.UI);
            }
            
            GameLogger.LogInfo("[GameSessionStatistics] 이벤트 구독 완료", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            CombatEvents.OnCombatStarted -= HandleCombatStarted;
            CombatEvents.OnVictory -= HandleVictory;
            CombatEvents.OnDefeat -= HandleDefeat;
            CombatEvents.OnGameOver -= HandleGameOver;
            
            if (_itemService != null)
            {
                _itemService.OnActiveItemAdded -= HandleActiveItemAdded;
                _itemService.OnActiveItemRemoved -= HandleActiveItemRemoved;
            }
        }

        /// <summary>
        /// 게임 세션 시작 (새 세션)
        /// </summary>
        public void StartSession(string characterName)
        {
            StartSession(characterName, null);
        }

        /// <summary>
        /// 게임 세션 시작 (이어하기 지원)
        /// </summary>
        /// <param name="characterName">캐릭터 이름</param>
        /// <param name="existingSessionId">기존 세션 ID (이어하기 시 사용, null이면 새 세션)</param>
        public void StartSession(string characterName, string existingSessionId)
        {
            if (IsSessionActive)
            {
                GameLogger.LogWarning("[GameSessionStatistics] 세션이 이미 시작되었습니다", GameLogger.LogCategory.Error);
                return;
            }

            // 이어하기 시 기존 세션 ID 사용, 새 게임 시 새 세션 ID 생성
            string sessionId = existingSessionId ?? GenerateSessionId();
            
            if (!string.IsNullOrEmpty(existingSessionId))
            {
                GameLogger.LogInfo($"[GameSessionStatistics] 이어하기 세션 ID: {existingSessionId}, 세션 재개", GameLogger.LogCategory.Save);
                // 기존 세션 데이터는 StatisticsManager.SaveSessionStatistics에서 자동으로 업데이트됨
                // 여기서는 세션 ID만 사용하여 재개
            }

            // 새 세션 생성 (이어하기 시에도 기존 데이터는 StatisticsManager에서 자동으로 업데이트됨)
            _currentSession = new SessionStatisticsData
            {
                sessionId = sessionId,
                gameStartTime = string.IsNullOrEmpty(existingSessionId) ? DateTime.UtcNow.ToString("o") : DateTime.UtcNow.ToString("o"), // 이어하기 시에도 새 시작 시간 (누적 시간은 별도 계산)
                selectedCharacterName = characterName ?? "Unknown",
                finalStageNumber = 0,
                finalEnemyIndex = 0,
                totalVictoryCount = 0,
                totalDefeatCount = 0,
                totalResourceGained = 0,
                totalResourceSpent = 0,
                unacquiredActiveItemCount = 0,
                finalTurns = 0,
                combatStatistics = new List<CombatStatisticsData>()
            };

            // Dictionary 초기화
            _currentSession.skillCardSpawnCountByCardId = new Dictionary<string, int>();
            _currentSession.skillCardUseCountByCardId = new Dictionary<string, int>();
            _currentSession.skillUseCountByName = new Dictionary<string, int>();
            _currentSession.activeItemSpawnCountByItemId = new Dictionary<string, int>();
            _currentSession.activeItemUseCountByName = new Dictionary<string, int>();
            _currentSession.activeItemDiscardCountByItemId = new Dictionary<string, int>();
            _currentSession.passiveItemAcquiredCountByItemId = new Dictionary<string, int>();

            // 자원 통계 초기화
            _currentSession.totalResourceGained = 0;
            _currentSession.totalResourceSpent = 0;

            _gameStartTime = Time.time;
            IsSessionActive = true;
            IsSaved = false;

            if (!string.IsNullOrEmpty(existingSessionId))
            {
                GameLogger.LogInfo($"[GameSessionStatistics] 세션 재개: {_currentSession.sessionId}, 캐릭터: {_currentSession.selectedCharacterName}", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogInfo($"[GameSessionStatistics] 새 세션 시작: {_currentSession.sessionId}, 캐릭터: {_currentSession.selectedCharacterName}", GameLogger.LogCategory.UI);
            }

            // CombatStatsAggregator 미리 찾기 시도
            EnsureCombatStatsAggregator();
        }

        /// <summary>
        /// 게임 세션 종료 (완전 종료)
        /// </summary>
        public void EndSession()
        {
            EndSession(true);
        }

        /// <summary>
        /// 게임 세션 종료
        /// </summary>
        /// <param name="finalEnd">완전 종료 여부 (true: 게임 종료, false: 중간 저장)</param>
        public void EndSession(bool finalEnd)
        {
            if (!IsSessionActive)
            {
                GameLogger.LogWarning("[GameSessionStatistics] 활성화된 세션이 없습니다", GameLogger.LogCategory.Error);
                return;
            }

            _gameEndTime = Time.time;
            
            // 중간 저장 시에는 gameEndTime을 업데이트하지 않음 (이어하기 가능성)
            if (finalEnd)
            {
                _currentSession.gameEndTime = DateTime.UtcNow.ToString("o");
            }
            
            // 플레이 시간 누적 계산 (이어하기 고려)
            float currentPlayTime = _gameEndTime - _gameStartTime;
            if (_currentSession.totalPlayTimeSeconds > 0)
            {
                // 기존 세션 재개인 경우 누적
                _currentSession.totalPlayTimeSeconds += currentPlayTime;
            }
            else
            {
                _currentSession.totalPlayTimeSeconds = currentPlayTime;
            }

            // 최종 스테이지 정보 업데이트
            if (_stageManager != null)
            {
                _currentSession.finalStageNumber = _stageManager.GetCurrentStageNumber();
                _currentSession.finalEnemyIndex = _stageManager.GetCurrentEnemyIndex();
            }

            // 최종 전투의 턴수 업데이트 (마지막 전투의 턴수)
            if (_currentSession.combatStatistics != null && _currentSession.combatStatistics.Count > 0)
            {
                var lastCombat = _currentSession.combatStatistics[_currentSession.combatStatistics.Count - 1];
                _currentSession.finalTurns = lastCombat.totalTurns;
            }
            else
            {
                _currentSession.finalTurns = 0;
            }

            // 세션 요약 계산
            CalculateSessionSummary();

            IsSessionActive = false;

            GameLogger.LogInfo($"[GameSessionStatistics] 세션 종료: {_currentSession.sessionId}", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 세션이 저장되었음을 표시
        /// </summary>
        public void MarkAsSaved()
        {
            IsSaved = true;
            GameLogger.LogInfo($"[GameSessionStatistics] 세션 저장 완료 표시: {_currentSession?.sessionId ?? "Unknown"}", GameLogger.LogCategory.Save);
        }

        /// <summary>
        /// 세션 재개 (중간 저장 후 재개)
        /// </summary>
        public void ResumeSession()
        {
            if (_currentSession == null)
            {
                GameLogger.LogWarning("[GameSessionStatistics] 재개할 세션이 없습니다", GameLogger.LogCategory.Error);
                return;
            }

            _gameStartTime = Time.time; // 재개 시간 업데이트
            IsSessionActive = true;
            IsSaved = false;
            GameLogger.LogInfo($"[GameSessionStatistics] 세션 재개: {_currentSession.sessionId}", GameLogger.LogCategory.Save);
        }

        /// <summary>
        /// 현재 세션 통계 데이터 가져오기
        /// </summary>
        public SessionStatisticsData GetCurrentSessionData()
        {
            if (_currentSession == null)
            {
                GameLogger.LogWarning("[GameSessionStatistics] 세션이 시작되지 않았습니다", GameLogger.LogCategory.Error);
                return null;
            }

            // 최신 데이터로 업데이트
            if (IsSessionActive)
            {
                // 활성 세션인 경우 현재 플레이 시간 계산
                float currentPlayTime = Time.time - _gameStartTime;
                if (_currentSession.totalPlayTimeSeconds > 0)
                {
                    // 이어하기 세션인 경우 누적 시간 계산
                    _currentSession.totalPlayTimeSeconds = _currentSession.totalPlayTimeSeconds + currentPlayTime;
                }
                else
                {
                    _currentSession.totalPlayTimeSeconds = currentPlayTime;
                }
            }
            
            if (_stageManager != null)
            {
                _currentSession.finalStageNumber = _stageManager.GetCurrentStageNumber();
                _currentSession.finalEnemyIndex = _stageManager.GetCurrentEnemyIndex();
            }

            CalculateSessionSummary();

            return _currentSession;
        }

        /// <summary>
        /// 전투 시작 핸들러
        /// </summary>
        private void HandleCombatStarted()
        {
            GameLogger.LogInfo("[GameSessionStatistics] HandleCombatStarted 호출됨!", GameLogger.LogCategory.Combat);
            
            // 세션이 시작되지 않았으면 자동으로 시작 시도
            if (!IsSessionActive)
            {
                GameLogger.LogWarning("[GameSessionStatistics] 세션이 활성화되지 않았습니다. 자동으로 세션을 시작합니다.", GameLogger.LogCategory.Error);
                
                // 캐릭터 이름 가져오기 시도
                string characterName = "Unknown";
                if (_playerManager != null && _playerManager.GetPlayer() != null)
                {
                    var playerData = _playerManager.GetPlayer().CharacterData as Game.CharacterSystem.Data.PlayerCharacterData;
                    if (playerData != null)
                    {
                        characterName = playerData.DisplayName ?? "Unknown";
                    }
                    else
                    {
                        characterName = _playerManager.GetPlayer().GetCharacterName();
                    }
                }
                else
                {
                    // PlayerManager가 null이면 직접 찾기
                    if (_playerManager == null)
                    {
                        _playerManager = FindFirstObjectByType<PlayerManager>(FindObjectsInactive.Include);
                        if (_playerManager != null && _playerManager.GetPlayer() != null)
                        {
                            var playerData = _playerManager.GetPlayer().CharacterData as Game.CharacterSystem.Data.PlayerCharacterData;
                            if (playerData != null)
                            {
                                characterName = playerData.DisplayName ?? "Unknown";
                            }
                            else
                            {
                                characterName = _playerManager.GetPlayer().GetCharacterName();
                            }
                        }
                    }
                }
                
                StartSession(characterName);
            }

            // 세션 시작 후에도 활성화되지 않았으면 경고
            if (!IsSessionActive)
            {
                GameLogger.LogError("[GameSessionStatistics] 세션을 시작할 수 없습니다. 통계 수집을 건너뜁니다.", GameLogger.LogCategory.Error);
                return;
            }

            // CombatStatsAggregator 미리 찾기 및 캐시 (전투 시작 시점에 확실히 찾기)
            EnsureCombatStatsAggregator();

            _currentCombatStats = new CombatStatisticsData
            {
                combatStartTime = DateTime.UtcNow.ToString("o")
            };

            // Dictionary 초기화
            _currentCombatStats.playerSkillUsageByCardId = new Dictionary<string, int>();
            _currentCombatStats.playerSkillUsageByName = new Dictionary<string, int>();
            _currentCombatStats.activeItemUsageByName = new Dictionary<string, int>();

            _combatStartTime = Time.time;
            _isCurrentCombatVictory = false;

            // 스테이지 정보 업데이트
            if (_stageManager != null)
            {
                _currentCombatStats.stageNumber = _stageManager.GetCurrentStageNumber();
                _currentCombatStats.enemyIndex = _stageManager.GetCurrentEnemyIndex();
                GameLogger.LogInfo($"[GameSessionStatistics] 스테이지 정보: {_currentCombatStats.stageNumber}-{_currentCombatStats.enemyIndex}", GameLogger.LogCategory.Combat);
            }
            else
            {
                // StageManager가 없으면 직접 찾기
                _stageManager = FindFirstObjectByType<StageManager>(FindObjectsInactive.Include);
                if (_stageManager != null)
                {
                    _currentCombatStats.stageNumber = _stageManager.GetCurrentStageNumber();
                    _currentCombatStats.enemyIndex = _stageManager.GetCurrentEnemyIndex();
                    GameLogger.LogInfo($"[GameSessionStatistics] StageManager 직접 찾기 성공: {_currentCombatStats.stageNumber}-{_currentCombatStats.enemyIndex}", GameLogger.LogCategory.Combat);
                }
            }

            GameLogger.LogInfo($"[GameSessionStatistics] 전투 시작 (전투 통계 수집 시작) - CombatStatsAggregator: {(_cachedCombatStatsAggregator != null ? "찾음" : "없음")}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 승리 핸들러
        /// </summary>
        private void HandleVictory()
        {
            _isCurrentCombatVictory = true;
            HandleCombatEnded();
        }

        /// <summary>
        /// 패배 핸들러
        /// </summary>
        private void HandleDefeat()
        {
            _isCurrentCombatVictory = false;
            HandleCombatEnded();
        }

        /// <summary>
        /// CombatStatsAggregator 찾기 및 캐시 (전투 시작 시 호출)
        /// </summary>
        private void EnsureCombatStatsAggregator()
        {
            if (_cachedCombatStatsAggregator != null && _cachedCombatStatsAggregator.gameObject.activeInHierarchy)
            {
                GameLogger.LogInfo("[GameSessionStatistics] 캐시된 CombatStatsAggregator 유효함", GameLogger.LogCategory.Combat);
                return;
            }

            // DI로 주입된 것이 있으면 사용
            if (_combatStatsProvider != null && _combatStatsProvider is CombatStatsAggregator aggregator)
            {
                _cachedCombatStatsAggregator = aggregator;
                GameLogger.LogInfo("[GameSessionStatistics] DI로 주입된 CombatStatsAggregator 캐시", GameLogger.LogCategory.Combat);
                return;
            }

            // 직접 찾기 시도 (여러 번 시도)
            for (int attempt = 0; attempt < 3; attempt++)
            {
                var found = FindFirstObjectByType<CombatStatsAggregator>(FindObjectsInactive.Include);
                if (found != null)
                {
                    _cachedCombatStatsAggregator = found;
                    _combatStatsProvider = found;
                    GameLogger.LogInfo($"[GameSessionStatistics] CombatStatsAggregator 찾기 성공 (시도 {attempt + 1}회)", GameLogger.LogCategory.Combat);
                    return;
                }

                // 다음 프레임까지 대기
                if (attempt < 2)
                {
                    GameLogger.LogWarning($"[GameSessionStatistics] CombatStatsAggregator 찾기 실패 (시도 {attempt + 1}회) - 재시도 중...", GameLogger.LogCategory.Error);
                }
            }

            GameLogger.LogError("[GameSessionStatistics] CombatStatsAggregator를 찾을 수 없습니다. 통계 수집이 제한될 수 있습니다.", GameLogger.LogCategory.Error);
        }

        /// <summary>
        /// 전투 종료 핸들러
        /// </summary>
        private void HandleCombatEnded()
        {
            GameLogger.LogInfo("[GameSessionStatistics] HandleCombatEnded 호출됨!", GameLogger.LogCategory.Combat);

            // 전투 통계 데이터가 없으면 생성
            if (_currentCombatStats == null)
            {
                GameLogger.LogWarning("[GameSessionStatistics] 전투 통계 데이터가 없습니다. 기본 데이터를 생성합니다.", GameLogger.LogCategory.Error);
                _currentCombatStats = new CombatStatisticsData
                {
                    combatStartTime = DateTime.UtcNow.ToString("o"),
                    playerSkillUsageByCardId = new Dictionary<string, int>(),
                    playerSkillUsageByName = new Dictionary<string, int>(),
                    activeItemUsageByName = new Dictionary<string, int>()
                };
            }

            _currentCombatStats.combatEndTime = DateTime.UtcNow.ToString("o");
            _currentCombatStats.battleDurationSeconds = Time.time - _combatStartTime;

            // CombatStatsAggregator에서 통계 가져오기
            // 캐시된 aggregator가 없으면 다시 찾기 시도
            if (_cachedCombatStatsAggregator == null)
            {
                EnsureCombatStatsAggregator();
            }

            ICombatStatsProvider statsProvider = _combatStatsProvider ?? _cachedCombatStatsAggregator;

            if (statsProvider != null)
            {
                var snapshot = statsProvider.GetSnapshot();
                if (snapshot != null)
                {
                    GameLogger.LogInfo($"[GameSessionStatistics] 전투 통계 수집 성공: 턴={snapshot.totalTurns}, 데미지={snapshot.totalDamageDealtToEnemies}, 받은데미지={snapshot.totalDamageTakenByPlayer}, 힐={snapshot.totalHealingToPlayer}", GameLogger.LogCategory.Combat);
                    GameLogger.LogInfo($"[GameSessionStatistics] 스킬카드 사용: {snapshot.playerSkillUsageByCardId?.Count ?? 0}개, 스킬 사용: {snapshot.playerSkillUsageByName?.Count ?? 0}개, 아이템 사용: {snapshot.activeItemUsageByName?.Count ?? 0}개", GameLogger.LogCategory.Combat);
                    
                    _currentCombatStats.totalTurns = snapshot.totalTurns;
                    _currentCombatStats.totalDamageDealtToEnemies = snapshot.totalDamageDealtToEnemies;
                    _currentCombatStats.totalDamageTakenByPlayer = snapshot.totalDamageTakenByPlayer;
                    _currentCombatStats.totalHealingToPlayer = snapshot.totalHealingToPlayer;
                    _currentCombatStats.resourceName = snapshot.resourceName;
                    _currentCombatStats.startResource = snapshot.startResource;
                    _currentCombatStats.endResource = snapshot.endResource;
                    _currentCombatStats.maxResource = snapshot.maxResource;
                    _currentCombatStats.totalResourceGained = snapshot.totalResourceGained;
                    _currentCombatStats.totalResourceSpent = snapshot.totalResourceSpent;

                    // 카드 사용 통계 복사 (카드 ID별)
                    if (snapshot.playerSkillUsageByCardId != null)
                    {
                        foreach (var kv in snapshot.playerSkillUsageByCardId)
                        {
                            _currentCombatStats.playerSkillUsageByCardId[kv.Key] = kv.Value;
                        }
                    }

                    // 스킬 사용 통계 복사 (스킬 이름별)
                    if (snapshot.playerSkillUsageByName != null)
                    {
                        foreach (var kv in snapshot.playerSkillUsageByName)
                        {
                            _currentCombatStats.playerSkillUsageByName[kv.Key] = kv.Value;
                        }
                    }

                    // 아이템 사용 통계 복사
                    if (snapshot.activeItemUsageByName != null)
                    {
                        foreach (var kv in snapshot.activeItemUsageByName)
                        {
                            _currentCombatStats.activeItemUsageByName[kv.Key] = kv.Value;
                        }
                    }

                    // 세션 레벨 통계 집계: 스킬카드 생성/사용
                    if (snapshot.playerSkillCardSpawnByCardId != null)
                    {
                        foreach (var kv in snapshot.playerSkillCardSpawnByCardId)
                        {
                            if (!_currentSession.skillCardSpawnCountByCardId.ContainsKey(kv.Key))
                                _currentSession.skillCardSpawnCountByCardId[kv.Key] = 0;
                            _currentSession.skillCardSpawnCountByCardId[kv.Key] += kv.Value;
                        }
                    }

                    if (snapshot.playerSkillUsageByCardId != null)
                    {
                        foreach (var kv in snapshot.playerSkillUsageByCardId)
                        {
                            if (!_currentSession.skillCardUseCountByCardId.ContainsKey(kv.Key))
                                _currentSession.skillCardUseCountByCardId[kv.Key] = 0;
                            _currentSession.skillCardUseCountByCardId[kv.Key] += kv.Value;
                        }
                    }

                    if (snapshot.playerSkillUsageByName != null)
                    {
                        foreach (var kv in snapshot.playerSkillUsageByName)
                        {
                            if (!_currentSession.skillUseCountByName.ContainsKey(kv.Key))
                                _currentSession.skillUseCountByName[kv.Key] = 0;
                            _currentSession.skillUseCountByName[kv.Key] += kv.Value;
                        }
                    }

                    // 세션 레벨 통계 집계: 액티브 아이템 생성
                    if (snapshot.activeItemSpawnByItemId != null)
                    {
                        foreach (var kv in snapshot.activeItemSpawnByItemId)
                        {
                            if (!_currentSession.activeItemSpawnCountByItemId.ContainsKey(kv.Key))
                                _currentSession.activeItemSpawnCountByItemId[kv.Key] = 0;
                            _currentSession.activeItemSpawnCountByItemId[kv.Key] += kv.Value;
                        }
                    }

                    // 세션 레벨 통계 집계: 액티브 아이템 사용 (이름별로 저장)
                    if (snapshot.activeItemUsageByName != null)
                    {
                        foreach (var kv in snapshot.activeItemUsageByName)
                        {
                            if (!_currentSession.activeItemUseCountByName.ContainsKey(kv.Key))
                                _currentSession.activeItemUseCountByName[kv.Key] = 0;
                            _currentSession.activeItemUseCountByName[kv.Key] += kv.Value;
                        }
                    }

                    // 세션 레벨 통계 집계: 액티브 아이템 버리기
                    if (snapshot.activeItemDiscardByItemId != null)
                    {
                        foreach (var kv in snapshot.activeItemDiscardByItemId)
                        {
                            if (!_currentSession.activeItemDiscardCountByItemId.ContainsKey(kv.Key))
                                _currentSession.activeItemDiscardCountByItemId[kv.Key] = 0;
                            _currentSession.activeItemDiscardCountByItemId[kv.Key] += kv.Value;
                        }
                    }

                    if (snapshot.passiveItemAcquiredByItemId != null)
                    {
                        foreach (var kv in snapshot.passiveItemAcquiredByItemId)
                        {
                            if (!_currentSession.passiveItemAcquiredCountByItemId.ContainsKey(kv.Key))
                                _currentSession.passiveItemAcquiredCountByItemId[kv.Key] = 0;
                            _currentSession.passiveItemAcquiredCountByItemId[kv.Key] += kv.Value;
                        }
                    }

                    // 세션 레벨 자원 통계 집계
                    _currentSession.totalResourceGained += snapshot.totalResourceGained;
                    _currentSession.totalResourceSpent += snapshot.totalResourceSpent;
                }
            }
            else
            {
                // CombatStatsAggregator를 찾을 수 없어도 기본 데이터는 수집
                GameLogger.LogWarning("[GameSessionStatistics] 전투 통계 수집기 없음. 기본 데이터만 저장합니다.", GameLogger.LogCategory.Error);
                _currentCombatStats.totalTurns = 0;
                _currentCombatStats.totalDamageDealtToEnemies = 0;
                _currentCombatStats.totalDamageTakenByPlayer = 0;
                _currentCombatStats.totalHealingToPlayer = 0;
            }

            // 전투 결과 결정 (플래그 사용)
            if (_isCurrentCombatVictory)
            {
                _currentCombatStats.result = "Victory";
                _currentSession.totalVictoryCount++;
                GameLogger.LogInfo($"[GameSessionStatistics] 전투 승리 (총 승리 횟수: {_currentSession.totalVictoryCount})", GameLogger.LogCategory.Combat);
            }
            else
            {
                _currentCombatStats.result = "Defeat";
                _currentSession.totalDefeatCount++;
                GameLogger.LogInfo($"[GameSessionStatistics] 전투 패배 (총 패배 횟수: {_currentSession.totalDefeatCount})", GameLogger.LogCategory.Combat);
            }

            // 세션에 추가
            _currentSession.combatStatistics.Add(_currentCombatStats);
            
            // 통계 저장 검증
            int combatCount = _currentSession.combatStatistics.Count;
            GameLogger.LogInfo($"[GameSessionStatistics] 전투 통계 저장 완료 (총 전투 수: {combatCount})", GameLogger.LogCategory.Combat);
            GameLogger.LogInfo($"[GameSessionStatistics] 저장된 전투 통계: 턴={_currentCombatStats.totalTurns}, 데미지={_currentCombatStats.totalDamageDealtToEnemies}, 스킬카드 사용={_currentCombatStats.playerSkillUsageByCardId?.Count ?? 0}개", GameLogger.LogCategory.Combat);
            
            _currentCombatStats = null;
            _isCurrentCombatVictory = false;

            // 전투 종료 후 통계 리셋 (다음 전투를 위해)
            if (statsProvider != null)
            {
                statsProvider.ResetStats();
                GameLogger.LogInfo("[GameSessionStatistics] CombatStatsAggregator 통계 리셋 완료", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 게임 오버 핸들러
        /// </summary>
        private void HandleGameOver()
        {
            if (IsSessionActive)
            {
                EndSession();
            }
        }

        /// <summary>
        /// 액티브 아이템 제거 핸들러 (버리기 통계)
        /// </summary>
        private void HandleActiveItemRemoved(Game.ItemSystem.Data.ActiveItemDefinition def, int slotIndex)
        {
            if (!IsSessionActive || def == null)
                return;

            string itemId = def.ItemId;
            
            // 세션 레벨 통계 집계: 액티브 아이템 버리기
            if (!_currentSession.activeItemDiscardCountByItemId.ContainsKey(itemId))
                _currentSession.activeItemDiscardCountByItemId[itemId] = 0;
            _currentSession.activeItemDiscardCountByItemId[itemId]++;
            
            GameLogger.LogInfo($"[GameSessionStatistics] 액티브 아이템 버리기: {def.DisplayName} (ID: {itemId})", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 액티브 아이템 획득 핸들러 (세션 통계: 생성/획득 카운트)
        /// </summary>
        private void HandleActiveItemAdded(Game.ItemSystem.Data.ActiveItemDefinition def, int slotIndex)
        {
            if (!IsSessionActive || def == null)
                return;

            var itemId = def.ItemId;
            if (string.IsNullOrEmpty(itemId))
                return;

            if (!_currentSession.activeItemSpawnCountByItemId.ContainsKey(itemId))
                _currentSession.activeItemSpawnCountByItemId[itemId] = 0;
            _currentSession.activeItemSpawnCountByItemId[itemId]++;

            GameLogger.LogInfo($"[GameSessionStatistics] 액티브 아이템 획득: {def.DisplayName} (ID: {itemId})", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 세션 요약 계산
        /// </summary>
        private void CalculateSessionSummary()
        {
            if (_currentSession == null || _currentSession.combatStatistics == null)
            {
                return;
            }

            var summary = _currentSession.summary;
            summary.totalDamageDealt = 0;
            summary.totalDamageTaken = 0;
            summary.totalHealing = 0;
            summary.totalTurns = 0;

            Dictionary<string, int> cardUsageCount = new Dictionary<string, int>();
            Dictionary<string, int> skillUsageCount = new Dictionary<string, int>(); // 스킬 이름별 통계
            Dictionary<string, int> itemUsageCount = new Dictionary<string, int>();

            foreach (var combat in _currentSession.combatStatistics)
            {
                summary.totalDamageDealt += combat.totalDamageDealtToEnemies;
                summary.totalDamageTaken += combat.totalDamageTakenByPlayer;
                summary.totalHealing += combat.totalHealingToPlayer;
                summary.totalTurns += combat.totalTurns;

                // 카드 사용 통계 집계 (카드 ID별)
                if (combat.playerSkillUsageByCardId != null)
                {
                    foreach (var kv in combat.playerSkillUsageByCardId)
                    {
                        if (!cardUsageCount.ContainsKey(kv.Key))
                            cardUsageCount[kv.Key] = 0;
                        cardUsageCount[kv.Key] += kv.Value;
                    }
                }

                // 스킬 사용 통계 집계 (스킬 이름별)
                if (combat.playerSkillUsageByName != null)
                {
                    foreach (var kv in combat.playerSkillUsageByName)
                    {
                        if (!skillUsageCount.ContainsKey(kv.Key))
                            skillUsageCount[kv.Key] = 0;
                        skillUsageCount[kv.Key] += kv.Value;
                    }
                }

                // 아이템 사용 통계 집계
                if (combat.activeItemUsageByName != null)
                {
                    foreach (var kv in combat.activeItemUsageByName)
                    {
                        if (!itemUsageCount.ContainsKey(kv.Key))
                            itemUsageCount[kv.Key] = 0;
                        itemUsageCount[kv.Key] += kv.Value;
                    }
                }
            }

            // 가장 많이 사용된 카드 찾기 (카드 ID별)
            string mostUsedCardId = null;
            int mostUsedCardCount = 0;
            foreach (var kv in cardUsageCount)
            {
                if (kv.Value > mostUsedCardCount)
                {
                    mostUsedCardId = kv.Key;
                    mostUsedCardCount = kv.Value;
                }
            }
            summary.mostUsedCardId = mostUsedCardId ?? "None";
            summary.mostUsedCardCount = mostUsedCardCount;

            // 가장 많이 사용된 스킬 찾기 (스킬 이름별)
            string mostUsedSkillName = null;
            int mostUsedSkillCount = 0;
            foreach (var kv in skillUsageCount)
            {
                if (kv.Value > mostUsedSkillCount)
                {
                    mostUsedSkillName = kv.Key;
                    mostUsedSkillCount = kv.Value;
                }
            }
            summary.mostUsedSkillName = mostUsedSkillName ?? "None";
            summary.mostUsedSkillCount = mostUsedSkillCount;

            // 가장 많이 사용된 아이템 찾기
            string mostUsedItemName = null;
            int mostUsedItemCount = 0;
            foreach (var kv in itemUsageCount)
            {
                if (kv.Value > mostUsedItemCount)
                {
                    mostUsedItemName = kv.Key;
                    mostUsedItemCount = kv.Value;
                }
            }
            summary.mostUsedItemName = mostUsedItemName ?? "None";
            summary.mostUsedItemCount = mostUsedItemCount;
        }

        /// <summary>
        /// 미획득 액티브 아이템 수 계산
        /// </summary>
        private void CalculateUnacquiredActiveItemCount()
        {
            try
            {
                // 모든 액티브 아이템 로드
                var allActiveItems = Game.ItemSystem.Cache.ItemResourceCache.GetActiveItems("Data/Item");
                if (allActiveItems == null || allActiveItems.Length == 0)
                {
                    GameLogger.LogWarning("[GameSessionStatistics] 액티브 아이템을 찾을 수 없습니다", GameLogger.LogCategory.Error);
                    _currentSession.unacquiredActiveItemCount = 0;
                    return;
                }

                // 획득한 액티브 아이템 ID 집합 생성
                var acquiredItemIds = new HashSet<string>();
                if (_currentSession.activeItemSpawnCountByItemId != null)
                {
                    foreach (var kv in _currentSession.activeItemSpawnCountByItemId)
                    {
                        if (kv.Value > 0)
                        {
                            acquiredItemIds.Add(kv.Key);
                        }
                    }
                }

                // 획득하지 못한 아이템 수 계산
                int unacquiredCount = 0;
                foreach (var item in allActiveItems)
                {
                    if (item != null && !string.IsNullOrEmpty(item.ItemId))
                    {
                        if (!acquiredItemIds.Contains(item.ItemId))
                        {
                            unacquiredCount++;
                        }
                    }
                }

                _currentSession.unacquiredActiveItemCount = unacquiredCount;
                GameLogger.LogInfo($"[GameSessionStatistics] 미획득 액티브 아이템 수: {unacquiredCount} / {allActiveItems.Length}", GameLogger.LogCategory.Combat);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[GameSessionStatistics] 미획득 액티브 아이템 수 계산 실패: {ex.Message}", GameLogger.LogCategory.Error);
                _currentSession.unacquiredActiveItemCount = 0;
            }
        }

        /// <summary>
        /// 세션 ID 생성
        /// </summary>
        private string GenerateSessionId()
        {
            return $"SESSION_{DateTime.UtcNow:yyyyMMddHHmmss}_{UnityEngine.Random.Range(1000, 9999)}";
        }
    }
}

