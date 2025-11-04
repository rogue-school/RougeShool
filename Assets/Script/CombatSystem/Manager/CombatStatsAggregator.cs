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
        public Dictionary<string, int> activeItemUsageByName = new Dictionary<string, int>();

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
        private readonly Dictionary<string, int> _activeItemUsage = new Dictionary<string, int>();

        private string _resourceName;
        private int _startResource;
        private int _endResource;
        private int _maxResource;
        private int _resourceGained;
        private int _resourceSpent;

        private void Awake()
        {
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
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

            // Active item usage
            if (_itemService != null)
                _itemService.OnActiveItemUsed += HandleActiveItemUsed;

            // Resource consume/restore from PlayerManager
            if (_playerManager != null)
            {
                _playerManager.OnResourceConsumed += HandleResourceConsumed;
                _playerManager.OnResourceRestored += HandleResourceRestored;
            }
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

            if (_itemService != null)
                _itemService.OnActiveItemUsed -= HandleActiveItemUsed;

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

        private void HandlePlayerCardUsed(string cardId, GameObject obj)
        {
            if (string.IsNullOrEmpty(cardId)) return;
            if (!_skillUsage.ContainsKey(cardId)) _skillUsage[cardId] = 0;
            _skillUsage[cardId]++;
        }

        private void HandleActiveItemUsed(ActiveItemDefinition def, int slotIndex)
        {
            string key = def != null ? def.DisplayName : "Unknown";
            if (!_activeItemUsage.ContainsKey(key)) _activeItemUsage[key] = 0;
            _activeItemUsage[key]++;
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
            foreach (var kv in _activeItemUsage)
                snapshot.activeItemUsageByName[kv.Key] = kv.Value;

            return snapshot;
        }

        public void ResetStats()
        {
            _combatStartTime = 0f;
            _damageDealt = 0;
            _damageTaken = 0;
            _healed = 0;
            _skillUsage.Clear();
            _activeItemUsage.Clear();
            _resourceName = string.Empty;
            _startResource = 0;
            _endResource = 0;
            _maxResource = 0;
            _resourceGained = 0;
            _resourceSpent = 0;
        }
    }
}


