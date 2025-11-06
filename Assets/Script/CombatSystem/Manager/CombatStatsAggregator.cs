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
        CombatStatsSnapshot GetSnapshot();
        void ResetStats();
    }

    /// <summary>
    /// 전투 통계 스냅샷(불변 데이터 전송용)
    /// </summary>
    [Serializable]
    public class CombatStatsSnapshot
    {
        public float battleDurationSeconds;
        public int totalTurns;
        public int totalDamageDealtToEnemies;
        public int totalDamageTakenByPlayer;
        public int totalHealingToPlayer;

        public Dictionary<string, int> playerSkillUsageByCardId = new Dictionary<string, int>();
        public Dictionary<string, int> playerSkillUsageByName = new Dictionary<string, int>(); // 스킬 이름별 사용 횟수
        public Dictionary<string, int> playerSkillCardSpawnByCardId = new Dictionary<string, int>(); // 스킬카드 생성 횟수
        public Dictionary<string, int> activeItemUsageByName = new Dictionary<string, int>();
        public Dictionary<string, int> activeItemSpawnByItemId = new Dictionary<string, int>(); // 액티브 아이템 생성 횟수
        public Dictionary<string, int> activeItemDiscardByItemId = new Dictionary<string, int>(); // 액티브 아이템 버리기 횟수
        public Dictionary<string, int> passiveItemAcquiredByItemId = new Dictionary<string, int>(); // 패시브 아이템 획득 횟수

        public string resourceName;
        public int startResource;
        public int endResource;
        public int maxResource;
        public int totalResourceGained;
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
                GameLogger.LogInfo("[CombatStatsAggregator] ItemService 이벤트 구독 완료", GameLogger.LogCategory.Combat);
            }

            // Resource consume/restore from PlayerManager
            if (_playerManager != null)
            {
                _playerManager.OnResourceConsumed += HandleResourceConsumed;
                _playerManager.OnResourceRestored += HandleResourceRestored;
                GameLogger.LogInfo("[CombatStatsAggregator] PlayerManager 이벤트 구독 완료", GameLogger.LogCategory.Combat);
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

            if (_playerManager != null)
            {
                _resourceName = _playerManager.ResourceName;
                _startResource = _playerManager.CurrentResource;
                _maxResource = _playerManager.MaxResource;
            }
            GameLogger.LogInfo("[CombatStats] 전투 집계 시작", GameLogger.LogCategory.Combat);
        }

        private void HandleCombatEnded()
        {
            if (_playerManager != null)
            {
                _endResource = _playerManager.CurrentResource;
            }
            GameLogger.LogInfo("[CombatStats] 전투 집계 종료", GameLogger.LogCategory.Combat);
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
            
            GameLogger.LogInfo($"[CombatStatsAggregator] 스킬카드 생성: {cardId}", GameLogger.LogCategory.Combat);
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
        /// </summary>
        private string GetCardName(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return null;

            try
            {
                // Resources에서 직접 카드 정의 로드
                var definition = Resources.Load<Game.SkillCardSystem.Data.SkillCardDefinition>($"SkillCards/{cardId}");
                if (definition != null)
                {
                    // 한국어 이름이 있으면 한국어 이름 우선, 없으면 영문 이름
                    return !string.IsNullOrEmpty(definition.displayNameKO) 
                        ? definition.displayNameKO 
                        : definition.displayName;
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[CombatStatsAggregator] 카드 이름 로드 실패: {cardId}, {ex.Message}", GameLogger.LogCategory.Combat);
            }

            return cardId; // 실패 시 카드 ID 반환
        }

        private void HandleActiveItemUsed(ActiveItemDefinition def, int slotIndex)
        {
            if (def == null) return;
            
            string itemId = def.ItemId;
            string itemName = def.DisplayName;
            
            // 아이템 이름별 사용 횟수 (기존 호환성 유지)
            if (!_activeItemUsage.ContainsKey(itemName)) _activeItemUsage[itemName] = 0;
            _activeItemUsage[itemName]++;
            
            // 아이템 ID별 사용 횟수도 저장 (세션 레벨 통계용)
            // ID별 통계는 별도 Dictionary에 저장하지 않고, CombatStatisticsData에서 이름->ID 변환 필요
            // 하지만 현재 구조에서는 이름별로만 저장되므로, 추후 개선 필요
            
            GameLogger.LogInfo($"[CombatStatsAggregator] 액티브 아이템 사용: {itemName} (ID: {itemId})", GameLogger.LogCategory.Combat);
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
            var snapshot = new CombatStatsSnapshot
            {
                battleDurationSeconds = _combatStartTime > 0f ? Mathf.Max(0f, Time.time - _combatStartTime) : 0f,
                totalTurns = _turnController != null ? _turnController.TurnCount : 0,
                totalDamageDealtToEnemies = _damageDealt,
                totalDamageTakenByPlayer = _damageTaken,
                totalHealingToPlayer = _healed,
                resourceName = _resourceName,
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

        public void ResetStats()
        {
            _combatStartTime = 0f;
            _damageDealt = 0;
            _damageTaken = 0;
            _healed = 0;
            _skillUsage.Clear();
            _skillUsageByName.Clear();
            _skillCardSpawn.Clear();
            _activeItemUsage.Clear();
            _activeItemSpawn.Clear();
            _activeItemDiscard.Clear();
            _passiveItemAcquired.Clear();
            _resourceName = string.Empty;
            _startResource = 0;
            _endResource = 0;
            _maxResource = 0;
            _resourceGained = 0;
            _resourceSpent = 0;
        }
    }
}


