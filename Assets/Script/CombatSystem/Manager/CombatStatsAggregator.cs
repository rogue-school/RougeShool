using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.CoreSystem.Utility;
using Game.CombatSystem;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 시작~종료 동안 플레이어 전용 통계를 집계합니다.
    /// </summary>
    public interface ICombatStatsProvider
    {
        /// <summary>
        /// 현재 전투 통계 스냅샷을 가져옵니다.
        /// </summary>
        /// <returns>전투 통계 스냅샷</returns>
        CombatStatsSnapshot GetSnapshot();
        
        /// <summary>
        /// 전투 통계를 초기화합니다.
        /// </summary>
        void ResetStats();
    }

    /// <summary>
    /// 전투 통계 스냅샷(불변 데이터 전송용)
    /// </summary>
    [Serializable]
    public class CombatStatsSnapshot
    {
        /// <summary>
        /// 전투 지속 시간 (초)
        /// </summary>
        public float battleDurationSeconds;
        
        /// <summary>
        /// 총 턴 수
        /// </summary>
        public int totalTurns;
        
        /// <summary>
        /// 적에게 가한 총 데미지
        /// </summary>
        public int totalDamageDealtToEnemies;
        
        /// <summary>
        /// 플레이어가 받은 총 데미지
        /// </summary>
        public int totalDamageTakenByPlayer;
        
        /// <summary>
        /// 플레이어가 받은 총 회복량
        /// </summary>
        public int totalHealingToPlayer;

        /// <summary>
        /// 카드 ID별 스킬 사용 횟수
        /// </summary>
        public Dictionary<string, int> playerSkillUsageByCardId = new Dictionary<string, int>();
        
        /// <summary>
        /// 스킬 이름별 사용 횟수
        /// </summary>
        public Dictionary<string, int> playerSkillUsageByName = new Dictionary<string, int>();
        
        /// <summary>
        /// 카드 ID별 스킬카드 생성 횟수
        /// </summary>
        public Dictionary<string, int> playerSkillCardSpawnByCardId = new Dictionary<string, int>();
        
        /// <summary>
        /// 아이템 이름별 액티브 아이템 사용 횟수
        /// </summary>
        public Dictionary<string, int> activeItemUsageByName = new Dictionary<string, int>();
        
        /// <summary>
        /// 아이템 ID별 액티브 아이템 생성 횟수
        /// </summary>
        public Dictionary<string, int> activeItemSpawnByItemId = new Dictionary<string, int>();
        
        /// <summary>
        /// 아이템 ID별 액티브 아이템 버리기 횟수
        /// </summary>
        public Dictionary<string, int> activeItemDiscardByItemId = new Dictionary<string, int>();
        
        /// <summary>
        /// 아이템 ID별 패시브 아이템 획득 횟수
        /// </summary>
        public Dictionary<string, int> passiveItemAcquiredByItemId = new Dictionary<string, int>();

        /// <summary>
        /// 리소스 이름
        /// </summary>
        public string resourceName;
        
        /// <summary>
        /// 전투 시작 시 리소스
        /// </summary>
        public int startResource;
        
        /// <summary>
        /// 전투 종료 시 리소스
        /// </summary>
        public int endResource;
        
        /// <summary>
        /// 최대 리소스
        /// </summary>
        public int maxResource;
        
        /// <summary>
        /// 총 획득한 리소스
        /// </summary>
        public int totalResourceGained;
        
        /// <summary>
        /// 총 소모한 리소스
        /// </summary>
        public int totalResourceSpent;
    }

    /// <summary>
    /// MonoBehaviour 기반 집계기: 이벤트 구독/해제를 자동 처리합니다.
    /// </summary>
    public class CombatStatsAggregator : MonoBehaviour, ICombatStatsProvider
    {
        // Optional DI 의존성 (씬에 없을 수도 있음)
        [Inject(Optional = true)] private IItemService _itemService;
        [Inject(Optional = true)] private ITurnController _turnController;
        [Inject(Optional = true)] private PlayerManager _playerManager;

        // 내부 집계 상태
        private float _combatStartTime;
        private int _startTurnCount; // 전투 시작 시점의 턴 수
        private int _damageDealt;
        private int _damageTaken;
        private int _healed;
        private readonly Dictionary<string, int> _skillUsage = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _skillUsageByName = new Dictionary<string, int>(); // 스킬 이름별 사용 횟수
        private readonly Dictionary<string, int> _skillCardSpawn = new Dictionary<string, int>(); // 스킬카드 생성 횟수
        private readonly Dictionary<string, int> _activeItemUsage = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _activeItemSpawn = new Dictionary<string, int>(); // 액티브 아이템 생성 횟수
        private readonly Dictionary<string, int> _activeItemDiscard = new Dictionary<string, int>(); // 액티브 아이템 버리기 횟수
        private readonly Dictionary<string, int> _passiveItemAcquired = new Dictionary<string, int>(); // 패시브 아이템 획득 횟수
        // 사용된 아이템 추적 (사용으로 인해 Remove되는 경우 버리기 집계에서 제외)
        private readonly HashSet<string> _usedActiveItemIds = new HashSet<string>();
        // 전투 종료/정리 중 제거 이벤트 무시
        private bool _suppressDiscardCounting = false;

        private string _resourceName;
        private int _startResource;
        private int _endResource;
        private int _maxResource;
        private int _resourceGained;
        private int _resourceSpent;

        private void Awake()
        {
            // DI 주입 전에는 기본 이벤트만 구독
            SubscribeBasicEvents();
        }

        private void Start()
        {
            // DI 주입 완료 후 모든 이벤트 구독
            SubscribeAllEvents();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        /// <summary>
        /// 기본 이벤트 구독 (DI 주입 전)
        /// </summary>
        private void SubscribeBasicEvents()
        {
            // Combat lifecycle
            CombatEvents.OnCombatStarted += HandleCombatStarted;
            CombatEvents.OnVictory += HandleCombatEnded;
            CombatEvents.OnDefeat += HandleCombatEnded;

            // Damage / Heal (플레이어/적)
            CombatEvents.OnEnemyCharacterDamaged += HandleEnemyDamaged;
            CombatEvents.OnPlayerCharacterDamaged += HandlePlayerDamaged;
            CombatEvents.OnPlayerCharacterHealed += HandlePlayerHealed;

            // Player skill usage by cardId
            CombatEvents.OnPlayerCardUse += HandlePlayerCardUsed;
            CombatEvents.OnPlayerCardSpawn += HandlePlayerCardSpawned;
        }

        /// <summary>
        /// 모든 이벤트 구독 (DI 주입 완료 후)
        /// </summary>
        private void SubscribeAllEvents()
        {
            // 기본 이벤트는 이미 구독됨
            // DI 주입된 서비스 이벤트만 추가 구독

            // Active item usage
            if (_itemService != null)
            {
                _itemService.OnActiveItemUsed += HandleActiveItemUsed;
                _itemService.OnActiveItemAdded += HandleActiveItemAdded;
                _itemService.OnActiveItemRemoved += HandleActiveItemRemoved;
                _itemService.OnPassiveItemAdded += HandlePassiveItemAdded;
            }

            // Resource consume/restore from PlayerManager
            // PlayerManager는 DI로 주입받음
            if (_playerManager != null)
            {
                _playerManager.OnResourceConsumed += HandleResourceConsumed;
                _playerManager.OnResourceRestored += HandleResourceRestored;
            }
            else
            {
                GameLogger.LogWarning("[CombatStatsAggregator] PlayerManager를 찾을 수 없어 자원 이벤트를 구독할 수 없습니다", GameLogger.LogCategory.Error);
            }
        }

        private void Subscribe()
        {
            // 레거시 메서드 (호환성 유지)
            SubscribeAllEvents();
        }

        private void Unsubscribe()
        {
            CombatEvents.OnCombatStarted -= HandleCombatStarted;
            CombatEvents.OnVictory -= HandleCombatEnded;
            CombatEvents.OnDefeat -= HandleCombatEnded;

            CombatEvents.OnEnemyCharacterDamaged -= HandleEnemyDamaged;
            CombatEvents.OnPlayerCharacterDamaged -= HandlePlayerDamaged;
            CombatEvents.OnPlayerCharacterHealed -= HandlePlayerHealed;

            CombatEvents.OnPlayerCardUse -= HandlePlayerCardUsed;
            CombatEvents.OnPlayerCardSpawn -= HandlePlayerCardSpawned;

            if (_itemService != null)
            {
                _itemService.OnActiveItemUsed -= HandleActiveItemUsed;
                _itemService.OnActiveItemAdded -= HandleActiveItemAdded;
                _itemService.OnActiveItemRemoved -= HandleActiveItemRemoved;
                _itemService.OnPassiveItemAdded -= HandlePassiveItemAdded;
            }

            if (_playerManager != null)
            {
                _playerManager.OnResourceConsumed -= HandleResourceConsumed;
                _playerManager.OnResourceRestored -= HandleResourceRestored;
            }
        }

        // Handlers
        private void HandleCombatStarted()
        {
            ResetStats();
            _combatStartTime = Time.time;

            // TurnController와 PlayerManager는 DI로 주입받음
            // 전투 시작 시점의 턴 수 저장
            _startTurnCount = _turnController != null ? _turnController.TurnCount : 1;

            if (_playerManager != null)
            {
                // 이벤트 구독
                _playerManager.OnResourceConsumed += HandleResourceConsumed;
                _playerManager.OnResourceRestored += HandleResourceRestored;
            }

            if (_playerManager != null)
            {
                _resourceName = _playerManager.ResourceName;
                _startResource = _playerManager.CurrentResource;
                _maxResource = _playerManager.MaxResource;
            }
            else
            {
                GameLogger.LogWarning("[CombatStats] PlayerManager를 찾을 수 없어 자원 통계를 수집할 수 없습니다", GameLogger.LogCategory.Error);
            }
            
        }

        private void HandleCombatEnded()
        {
            // 전투 종료 시점부터 인벤토리 정리로 발생하는 Remove는 버리기로 카운트하지 않음
            _suppressDiscardCounting = true;

            // PlayerManager는 DI로 주입받음
            if (_playerManager != null)
            {
                _endResource = _playerManager.CurrentResource;
            }
            else
            {
                GameLogger.LogWarning("[CombatStats] PlayerManager를 찾을 수 없어 종료 자원을 수집할 수 없습니다", GameLogger.LogCategory.Error);
            }
            
        }

        private void HandleEnemyDamaged(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj, int amount)
        {
            if (amount > 0) _damageDealt += amount;
        }

        private void HandlePlayerDamaged(Game.CharacterSystem.Data.PlayerCharacterData data, GameObject obj, int amount)
        {
            if (amount > 0) _damageTaken += amount;
        }

        private void HandlePlayerHealed(Game.CharacterSystem.Data.PlayerCharacterData data, GameObject obj, int amount)
        {
            if (amount > 0) _healed += amount;
        }

        private void HandlePlayerCardSpawned(string cardId, GameObject obj)
        {
            if (string.IsNullOrEmpty(cardId)) return;
            
            // 카드 ID별 생성 횟수
            if (!_skillCardSpawn.ContainsKey(cardId)) _skillCardSpawn[cardId] = 0;
            _skillCardSpawn[cardId]++;
        }

        private void HandlePlayerCardUsed(string cardId, GameObject obj)
        {
            if (string.IsNullOrEmpty(cardId)) return;
            
            // 카드 ID별 사용 횟수 (기존)
            if (!_skillUsage.ContainsKey(cardId)) _skillUsage[cardId] = 0;
            _skillUsage[cardId]++;
            
            // 카드 이름별 사용 횟수 (추가)
            string cardName = GetCardName(cardId);
            if (!string.IsNullOrEmpty(cardName))
            {
                if (!_skillUsageByName.ContainsKey(cardName)) _skillUsageByName[cardName] = 0;
                _skillUsageByName[cardName]++;
            }
        }

        /// <summary>
        /// 카드 ID로부터 카드 이름을 가져옵니다.
        /// StatisticsData의 GetCardDisplayNameStatic()과 동일한 로직 사용
        /// </summary>
        private string GetCardName(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return null;

            // StatisticsData의 정적 메서드 사용 (캐싱 및 여러 경로 시도)
            return Game.CoreSystem.Statistics.SessionStatisticsData.GetCardDisplayNameStatic(cardId);
        }

        private void HandleActiveItemUsed(ActiveItemDefinition def, int slotIndex)
        {
            if (def == null) return;
            
            string itemId = def.ItemId;
            string itemName = def.DisplayName;
            
            // 사용된 아이템 ID 추적 (이후 Remove 이벤트에서 버리기 카운트 제외)
            if (!string.IsNullOrEmpty(itemId))
            {
                _usedActiveItemIds.Add(itemId);
            }
            
            // 아이템 이름별 사용 횟수 (기존 호환성 유지)
            if (!_activeItemUsage.ContainsKey(itemName)) _activeItemUsage[itemName] = 0;
            _activeItemUsage[itemName]++;
        }

        private void HandleActiveItemAdded(ActiveItemDefinition def, int slotIndex)
        {
            if (def == null) return;
            
            string itemId = def.ItemId;
            
            // 아이템 ID별 생성 횟수
            if (!_activeItemSpawn.ContainsKey(itemId)) _activeItemSpawn[itemId] = 0;
            _activeItemSpawn[itemId]++;
            
            GameLogger.LogInfo($"[CombatStatsAggregator] 액티브 아이템 생성: {def.DisplayName} (ID: {itemId})", GameLogger.LogCategory.Combat);
        }

        private void HandleActiveItemRemoved(ActiveItemDefinition def, int slotIndex)
        {
            if (def == null) return;
            
            string itemId = def.ItemId;

            // 전투 종료/정리 중에는 버리기 집계 제외
            if (_suppressDiscardCounting)
            {
                GameLogger.LogInfo($"[CombatStatsAggregator] 아이템 제거(정리 중, 버리기 제외): {def.DisplayName} (ID: {itemId})", GameLogger.LogCategory.Combat);
                return;
            }
            
            // 사용으로 인해 제거된 경우 버리기 통계에서 제외
            if (!string.IsNullOrEmpty(itemId) && _usedActiveItemIds.Contains(itemId))
            {
                _usedActiveItemIds.Remove(itemId); // 한 번만 처리
                GameLogger.LogInfo($"[CombatStatsAggregator] 액티브 아이템 제거 (사용됨, 버리기 제외): {def.DisplayName} (ID: {itemId})", GameLogger.LogCategory.Combat);
                return;
            }
            
            // 아이템 ID별 버리기 횟수
            if (!_activeItemDiscard.ContainsKey(itemId)) _activeItemDiscard[itemId] = 0;
            _activeItemDiscard[itemId]++;
            
            GameLogger.LogInfo($"[CombatStatsAggregator] 액티브 아이템 버리기: {def.DisplayName} (ID: {itemId})", GameLogger.LogCategory.Combat);
        }

        private void HandlePassiveItemAdded(PassiveItemDefinition def)
        {
            if (def == null) return;
            
            string itemId = def.ItemId;
            
            // 아이템 ID별 획득 횟수
            if (!_passiveItemAcquired.ContainsKey(itemId)) _passiveItemAcquired[itemId] = 0;
            _passiveItemAcquired[itemId]++;
            
            GameLogger.LogInfo($"[CombatStatsAggregator] 패시브 아이템 획득: {def.DisplayName} (ID: {itemId})", GameLogger.LogCategory.Combat);
        }

        private void HandleResourceConsumed(int amount)
        {
            if (amount > 0) _resourceSpent += amount;
        }

        private void HandleResourceRestored(int amount)
        {
            if (amount > 0) _resourceGained += amount;
        }

        // API
        public CombatStatsSnapshot GetSnapshot()
        {
            // TurnController와 PlayerManager는 DI로 주입받음
            if (_playerManager != null && string.IsNullOrEmpty(_resourceName))
            {
                _resourceName = _playerManager.ResourceName;
                _maxResource = _playerManager.MaxResource;
            }

            // 최신 종료 자원 정보 업데이트
            if (_playerManager != null && _endResource == 0 && _combatStartTime > 0f)
            {
                _endResource = _playerManager.CurrentResource;
            }

            // 전투 종료 시점의 턴 수 계산 (현재 턴 수 - 시작 시점 턴 수)
            int currentTurnCount = _turnController != null ? _turnController.TurnCount : 0;
            int combatTurns = Mathf.Max(0, currentTurnCount - _startTurnCount + 1); // +1은 시작 턴 포함
            
            GameLogger.LogInfo($"[CombatStatsAggregator] GetSnapshot: 시작턴수={_startTurnCount}, 현재턴수={currentTurnCount}, 전투턴수={combatTurns}", GameLogger.LogCategory.Combat);

            var snapshot = new CombatStatsSnapshot
            {
                battleDurationSeconds = _combatStartTime > 0f ? Mathf.Max(0f, Time.time - _combatStartTime) : 0f,
                totalTurns = combatTurns,
                totalDamageDealtToEnemies = _damageDealt,
                totalDamageTakenByPlayer = _damageTaken,
                totalHealingToPlayer = _healed,
                resourceName = _resourceName ?? string.Empty,
                startResource = _startResource,
                endResource = _endResource,
                maxResource = _maxResource,
                totalResourceGained = _resourceGained,
                totalResourceSpent = _resourceSpent
            };

            // 복사본 생성 (외부에서 변경 불가하도록)
            foreach (var kv in _skillUsage)
                snapshot.playerSkillUsageByCardId[kv.Key] = kv.Value;
            foreach (var kv in _skillUsageByName)
                snapshot.playerSkillUsageByName[kv.Key] = kv.Value;
            foreach (var kv in _skillCardSpawn)
                snapshot.playerSkillCardSpawnByCardId[kv.Key] = kv.Value;
            foreach (var kv in _activeItemUsage)
                snapshot.activeItemUsageByName[kv.Key] = kv.Value;
            foreach (var kv in _activeItemSpawn)
                snapshot.activeItemSpawnByItemId[kv.Key] = kv.Value;
            foreach (var kv in _activeItemDiscard)
                snapshot.activeItemDiscardByItemId[kv.Key] = kv.Value;
            foreach (var kv in _passiveItemAcquired)
                snapshot.passiveItemAcquiredByItemId[kv.Key] = kv.Value;

            return snapshot;
        }

        /// <summary>
        /// 전투 통계를 초기화합니다.
        /// </summary>
        public void ResetStats()
        {
            _combatStartTime = 0f;
            _startTurnCount = 1; // 기본값 1로 초기화
            _damageDealt = 0;
            _damageTaken = 0;
            _suppressDiscardCounting = false;
            _healed = 0;
            _skillUsage.Clear();
            _skillUsageByName.Clear();
            _skillCardSpawn.Clear();
            _activeItemUsage.Clear();
            _activeItemSpawn.Clear();
            _activeItemDiscard.Clear();
            _passiveItemAcquired.Clear();
            _usedActiveItemIds.Clear();
            _resourceName = string.Empty;
            _startResource = 0;
            _endResource = 0;
            _maxResource = 0;
            _resourceGained = 0;
            _resourceSpent = 0;
        }
    }
}


