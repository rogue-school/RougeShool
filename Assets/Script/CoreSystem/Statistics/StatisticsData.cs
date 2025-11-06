using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.SkillCardSystem.Deck;

namespace Game.CoreSystem.Statistics
{
    /// <summary>
    /// JSON 직렬화를 위한 Key-Value Pair
    /// </summary>
    [Serializable]
    public class SerializableKeyValuePair
    {
        public string key;
        public int value;

        public SerializableKeyValuePair() { }

        public SerializableKeyValuePair(string key, int value)
        {
            this.key = key;
            this.value = value;
        }
    }
    /// <summary>
    /// 게임 세션 통계 데이터 구조
    /// </summary>
    [Serializable]
    public class SessionStatisticsData
    {
        /// <summary>
        /// 세션 고유 ID (타임스탬프 기반)
        /// </summary>
        public string sessionId;

        /// <summary>
        /// 게임 시작 시간 (ISO 8601 형식)
        /// </summary>
        public string gameStartTime;

        /// <summary>
        /// 게임 종료 시간 (ISO 8601 형식)
        /// </summary>
        public string gameEndTime;

        /// <summary>
        /// 총 플레이 시간 (초)
        /// </summary>
        public float totalPlayTimeSeconds;

        /// <summary>
        /// 선택된 캐릭터 이름 (예: "아케인", "세레나")
        /// </summary>
        public string selectedCharacterName;

        /// <summary>
        /// 최종 스테이지 번호
        /// </summary>
        public int finalStageNumber;

        /// <summary>
        /// 최종 적 인덱스
        /// </summary>
        public int finalEnemyIndex;

        /// <summary>
        /// 총 승리 횟수
        /// </summary>
        public int totalVictoryCount;

        /// <summary>
        /// 총 패배 횟수
        /// </summary>
        public int totalDefeatCount;

        /// <summary>
        /// 총 획득한 자원 (세션 레벨)
        /// </summary>
        public int totalResourceGained;

        /// <summary>
        /// 총 사용한 자원 (세션 레벨)
        /// </summary>
        public int totalResourceSpent;

        /// <summary>
        /// (비직렬화) 미획득 액티브 아이템 수 - JSON 출력에서 제외
        /// </summary>
        [NonSerialized]
        public int unacquiredActiveItemCount;

        /// <summary>
        /// 최종 전투의 턴수 (마지막 전투의 턴수)
        /// </summary>
        public int finalTurns;

        /// <summary>
        /// 전투별 통계 데이터
        /// </summary>
        public List<CombatStatisticsData> combatStatistics = new List<CombatStatisticsData>();

        /// <summary>
        /// 전체 세션 통계 요약
        /// </summary>
        public SessionSummaryData summary = new SessionSummaryData();

        // JSON 직렬화를 위한 Dictionary -> List 변환 필드
        [SerializeField] private List<SerializableKeyValuePair> _skillCardSpawnCountByCardId = new List<SerializableKeyValuePair>();
        [SerializeField] private List<SerializableKeyValuePair> _skillCardUseCountByCardId = new List<SerializableKeyValuePair>();
        [SerializeField] private List<SerializableKeyValuePair> _skillUseCountByName = new List<SerializableKeyValuePair>();
        [SerializeField] private List<SerializableKeyValuePair> _activeItemSpawnCountByItemId = new List<SerializableKeyValuePair>();
        [SerializeField] private List<SerializableKeyValuePair> _activeItemUseCountByName = new List<SerializableKeyValuePair>();
        [SerializeField] private List<SerializableKeyValuePair> _activeItemDiscardCountByItemId = new List<SerializableKeyValuePair>();
        [SerializeField] private List<SerializableKeyValuePair> _passiveItemAcquiredCountByItemId = new List<SerializableKeyValuePair>();

        // 런타임 Dictionary (직렬화되지 않음)
        [System.NonSerialized]
        private Dictionary<string, int> _skillCardSpawnCountByCardIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _skillCardUseCountByCardIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _skillUseCountByNameDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _activeItemSpawnCountByItemIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _activeItemUseCountByNameDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _activeItemDiscardCountByItemIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _passiveItemAcquiredCountByItemIdDict = new Dictionary<string, int>();

        /// <summary>
        /// 세션 레벨 통계: 각 스킬카드별 생성 횟수 (카드 ID -> 생성 횟수)
        /// </summary>
        public Dictionary<string, int> skillCardSpawnCountByCardId
        {
            get
            {
                if (_skillCardSpawnCountByCardIdDict.Count == 0 && _skillCardSpawnCountByCardId.Count > 0)
                {
                    foreach (var kv in _skillCardSpawnCountByCardId)
                        _skillCardSpawnCountByCardIdDict[kv.key] = kv.value;
                }
                return _skillCardSpawnCountByCardIdDict;
            }
            set
            {
                _skillCardSpawnCountByCardIdDict = value ?? new Dictionary<string, int>();
                _skillCardSpawnCountByCardId = _skillCardSpawnCountByCardIdDict.Count > 0
                    ? _skillCardSpawnCountByCardIdDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 스킬카드별 사용 횟수 (카드 ID -> 사용 횟수)
        /// </summary>
        public Dictionary<string, int> skillCardUseCountByCardId
        {
            get
            {
                if (_skillCardUseCountByCardIdDict.Count == 0 && _skillCardUseCountByCardId.Count > 0)
                {
                    foreach (var kv in _skillCardUseCountByCardId)
                        _skillCardUseCountByCardIdDict[kv.key] = kv.value;
                }
                return _skillCardUseCountByCardIdDict;
            }
            set
            {
                _skillCardUseCountByCardIdDict = value ?? new Dictionary<string, int>();
                _skillCardUseCountByCardId = _skillCardUseCountByCardIdDict.Count > 0
                    ? _skillCardUseCountByCardIdDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 스킬별 사용 횟수 (스킬 이름 -> 사용 횟수)
        /// </summary>
        public Dictionary<string, int> skillUseCountByName
        {
            get
            {
                if (_skillUseCountByNameDict.Count == 0 && _skillUseCountByName.Count > 0)
                {
                    foreach (var kv in _skillUseCountByName)
                        _skillUseCountByNameDict[kv.key] = kv.value;
                }
                return _skillUseCountByNameDict;
            }
            set
            {
                _skillUseCountByNameDict = value ?? new Dictionary<string, int>();
                _skillUseCountByName = _skillUseCountByNameDict.Count > 0
                    ? _skillUseCountByNameDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 액티브 아이템별 생성 횟수 (아이템 ID -> 생성 횟수)
        /// </summary>
        public Dictionary<string, int> activeItemSpawnCountByItemId
        {
            get
            {
                if (_activeItemSpawnCountByItemIdDict.Count == 0 && _activeItemSpawnCountByItemId.Count > 0)
                {
                    foreach (var kv in _activeItemSpawnCountByItemId)
                        _activeItemSpawnCountByItemIdDict[kv.key] = kv.value;
                }
                return _activeItemSpawnCountByItemIdDict;
            }
            set
            {
                _activeItemSpawnCountByItemIdDict = value ?? new Dictionary<string, int>();
                _activeItemSpawnCountByItemId = _activeItemSpawnCountByItemIdDict.Count > 0
                    ? _activeItemSpawnCountByItemIdDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 액티브 아이템별 사용 횟수 (아이템 이름 -> 사용 횟수)
        /// </summary>
        public Dictionary<string, int> activeItemUseCountByName
        {
            get
            {
                if (_activeItemUseCountByNameDict.Count == 0 && _activeItemUseCountByName.Count > 0)
                {
                    foreach (var kv in _activeItemUseCountByName)
                        _activeItemUseCountByNameDict[kv.key] = kv.value;
                }
                return _activeItemUseCountByNameDict;
            }
            set
            {
                _activeItemUseCountByNameDict = value ?? new Dictionary<string, int>();
                _activeItemUseCountByName = _activeItemUseCountByNameDict.Count > 0
                    ? _activeItemUseCountByNameDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 액티브 아이템별 버리기 횟수 (아이템 ID -> 버리기 횟수)
        /// </summary>
        public Dictionary<string, int> activeItemDiscardCountByItemId
        {
            get
            {
                if (_activeItemDiscardCountByItemIdDict.Count == 0 && _activeItemDiscardCountByItemId.Count > 0)
                {
                    foreach (var kv in _activeItemDiscardCountByItemId)
                        _activeItemDiscardCountByItemIdDict[kv.key] = kv.value;
                }
                return _activeItemDiscardCountByItemIdDict;
            }
            set
            {
                _activeItemDiscardCountByItemIdDict = value ?? new Dictionary<string, int>();
                _activeItemDiscardCountByItemId = _activeItemDiscardCountByItemIdDict.Count > 0
                    ? _activeItemDiscardCountByItemIdDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 패시브 아이템별 획득 횟수 (아이템 ID -> 획득 횟수)
        /// </summary>
        public Dictionary<string, int> passiveItemAcquiredCountByItemId
        {
            get
            {
                if (_passiveItemAcquiredCountByItemIdDict.Count == 0 && _passiveItemAcquiredCountByItemId.Count > 0)
                {
                    foreach (var kv in _passiveItemAcquiredCountByItemId)
                        _passiveItemAcquiredCountByItemIdDict[kv.key] = kv.value;
                }
                return _passiveItemAcquiredCountByItemIdDict;
            }
            set
            {
                _passiveItemAcquiredCountByItemIdDict = value ?? new Dictionary<string, int>();
                _passiveItemAcquiredCountByItemId = _passiveItemAcquiredCountByItemIdDict.Count > 0
                    ? _passiveItemAcquiredCountByItemIdDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
            }
        }

        /// <summary>
        /// 저장 전에 Dictionary를 List로 변환 및 정렬
        /// </summary>
        public void PrepareForSerialization(PlayerSkillDeck playerDeck = null)
        {
            // 스킬카드는 덱 순서로 정렬 (덱이 제공된 경우)
            if (playerDeck != null && playerDeck.CardEntries != null)
            {
                var deckCardIds = new List<string>();
                foreach (var entry in playerDeck.CardEntries)
                {
                    if (entry.IsValid() && entry.cardDefinition != null && !string.IsNullOrEmpty(entry.cardDefinition.cardId))
                    {
                        if (!deckCardIds.Contains(entry.cardDefinition.cardId))
                        {
                            deckCardIds.Add(entry.cardDefinition.cardId);
                        }
                    }
                }

                // 덱 순서로 정렬된 스킬카드 생성/사용 통계
                var sortedSpawn = new List<SerializableKeyValuePair>();
                var sortedUse = new List<SerializableKeyValuePair>();

                // 덱 순서대로 정렬
                foreach (var cardId in deckCardIds)
                {
                    if (_skillCardSpawnCountByCardIdDict.ContainsKey(cardId))
                    {
                        sortedSpawn.Add(new SerializableKeyValuePair(cardId, _skillCardSpawnCountByCardIdDict[cardId]));
                    }
                    if (_skillCardUseCountByCardIdDict.ContainsKey(cardId))
                    {
                        sortedUse.Add(new SerializableKeyValuePair(cardId, _skillCardUseCountByCardIdDict[cardId]));
                    }
                }

                // 덱에 없는 카드도 추가 (덱 순서 뒤에)
                foreach (var kv in _skillCardSpawnCountByCardIdDict)
                {
                    if (!deckCardIds.Contains(kv.Key))
                    {
                        sortedSpawn.Add(new SerializableKeyValuePair(kv.Key, kv.Value));
                    }
                }
                foreach (var kv in _skillCardUseCountByCardIdDict)
                {
                    if (!deckCardIds.Contains(kv.Key))
                    {
                        sortedUse.Add(new SerializableKeyValuePair(kv.Key, kv.Value));
                    }
                }

                _skillCardSpawnCountByCardId = sortedSpawn;
                _skillCardUseCountByCardId = sortedUse;
            }
            else
            {
                // 덱이 없으면 기본 순서로 변환 (빈 경우에도 빈 리스트로 확실히 설정)
                _skillCardSpawnCountByCardId = _skillCardSpawnCountByCardIdDict != null && _skillCardSpawnCountByCardIdDict.Count > 0
                    ? _skillCardSpawnCountByCardIdDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
                    
                _skillCardUseCountByCardId = _skillCardUseCountByCardIdDict != null && _skillCardUseCountByCardIdDict.Count > 0
                    ? _skillCardUseCountByCardIdDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
            }

            // 다른 Dictionary는 기본 순서로 변환 (빈 경우에도 빈 리스트로 확실히 설정)
            _skillUseCountByName = _skillUseCountByNameDict != null && _skillUseCountByNameDict.Count > 0
                ? _skillUseCountByNameDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                : new List<SerializableKeyValuePair>();
                
            _activeItemSpawnCountByItemId = _activeItemSpawnCountByItemIdDict != null && _activeItemSpawnCountByItemIdDict.Count > 0
                ? _activeItemSpawnCountByItemIdDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                : new List<SerializableKeyValuePair>();
                
            _activeItemUseCountByName = _activeItemUseCountByNameDict != null && _activeItemUseCountByNameDict.Count > 0
                ? _activeItemUseCountByNameDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                : new List<SerializableKeyValuePair>();
                
            _activeItemDiscardCountByItemId = _activeItemDiscardCountByItemIdDict != null && _activeItemDiscardCountByItemIdDict.Count > 0
                ? _activeItemDiscardCountByItemIdDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                : new List<SerializableKeyValuePair>();
                
            _passiveItemAcquiredCountByItemId = _passiveItemAcquiredCountByItemIdDict != null && _passiveItemAcquiredCountByItemIdDict.Count > 0
                ? _passiveItemAcquiredCountByItemIdDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                : new List<SerializableKeyValuePair>();

            // 전투별 통계도 직렬화 준비
            if (combatStatistics != null)
            {
                foreach (var combat in combatStatistics)
                {
                    combat.PrepareForSerialization();
                }
            }
        }
    }

    /// <summary>
    /// 전투 통계 데이터 직렬화 준비
    /// </summary>
    public static class CombatStatisticsDataExtensions
    {
        public static void PrepareForSerialization(this CombatStatisticsData combat)
        {
            // Dictionary를 List로 변환 (빈 경우에도 빈 리스트로 확실히 설정)
            combat.playerSkillUsageByCardId = combat.playerSkillUsageByCardId ?? new Dictionary<string, int>();
            combat.playerSkillUsageByName = combat.playerSkillUsageByName ?? new Dictionary<string, int>();
            combat.activeItemUsageByName = combat.activeItemUsageByName ?? new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// 전투 통계 데이터 구조 (CombatStatsSnapshot을 확장)
    /// </summary>
    [Serializable]
    public class CombatStatisticsData
    {
        /// <summary>
        /// 전투 시작 시간 (ISO 8601 형식)
        /// </summary>
        public string combatStartTime;

        /// <summary>
        /// 전투 종료 시간 (ISO 8601 형식)
        /// </summary>
        public string combatEndTime;

        /// <summary>
        /// 스테이지 번호
        /// </summary>
        public int stageNumber;

        /// <summary>
        /// 적 인덱스
        /// </summary>
        public int enemyIndex;

        /// <summary>
        /// 전투 결과 (Victory, Defeat)
        /// </summary>
        public string result;

        /// <summary>
        /// 전투 지속 시간 (초)
        /// </summary>
        public float battleDurationSeconds;

        /// <summary>
        /// 총 턴 수
        /// </summary>
        public int totalTurns;

        /// <summary>
        /// 적에게 준 총 데미지
        /// </summary>
        public int totalDamageDealtToEnemies;

        /// <summary>
        /// 플레이어가 받은 총 데미지
        /// </summary>
        public int totalDamageTakenByPlayer;

        /// <summary>
        /// 플레이어가 받은 총 힐링
        /// </summary>
        public int totalHealingToPlayer;

        // JSON 직렬화를 위한 Dictionary -> List 변환 필드
        [SerializeField] private List<SerializableKeyValuePair> _playerSkillUsageByCardId = new List<SerializableKeyValuePair>();
        [SerializeField] private List<SerializableKeyValuePair> _playerSkillUsageByName = new List<SerializableKeyValuePair>();
        [SerializeField] private List<SerializableKeyValuePair> _activeItemUsageByName = new List<SerializableKeyValuePair>();

        // 런타임 Dictionary (직렬화되지 않음)
        [System.NonSerialized]
        private Dictionary<string, int> _playerSkillUsageByCardIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _playerSkillUsageByNameDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _activeItemUsageByNameDict = new Dictionary<string, int>();

        /// <summary>
        /// 카드별 사용 횟수 (카드 ID -> 사용 횟수)
        /// </summary>
        public Dictionary<string, int> playerSkillUsageByCardId
        {
            get
            {
                if (_playerSkillUsageByCardIdDict.Count == 0 && _playerSkillUsageByCardId.Count > 0)
                {
                    foreach (var kv in _playerSkillUsageByCardId)
                        _playerSkillUsageByCardIdDict[kv.key] = kv.value;
                }
                return _playerSkillUsageByCardIdDict;
            }
            set
            {
                _playerSkillUsageByCardIdDict = value ?? new Dictionary<string, int>();
                _playerSkillUsageByCardId = _playerSkillUsageByCardIdDict.Count > 0
                    ? _playerSkillUsageByCardIdDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
            }
        }

        /// <summary>
        /// 스킬별 사용 횟수 (스킬 이름 -> 사용 횟수) 예: "베기" 2회, "2연격" 1회
        /// </summary>
        public Dictionary<string, int> playerSkillUsageByName
        {
            get
            {
                if (_playerSkillUsageByNameDict.Count == 0 && _playerSkillUsageByName.Count > 0)
                {
                    foreach (var kv in _playerSkillUsageByName)
                        _playerSkillUsageByNameDict[kv.key] = kv.value;
                }
                return _playerSkillUsageByNameDict;
            }
            set
            {
                _playerSkillUsageByNameDict = value ?? new Dictionary<string, int>();
                _playerSkillUsageByName = _playerSkillUsageByNameDict.Count > 0
                    ? _playerSkillUsageByNameDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
            }
        }

        /// <summary>
        /// 액티브 아이템 사용 횟수 (아이템 이름 -> 사용 횟수)
        /// </summary>
        public Dictionary<string, int> activeItemUsageByName
        {
            get
            {
                if (_activeItemUsageByNameDict.Count == 0 && _activeItemUsageByName.Count > 0)
                {
                    foreach (var kv in _activeItemUsageByName)
                        _activeItemUsageByNameDict[kv.key] = kv.value;
                }
                return _activeItemUsageByNameDict;
            }
            set
            {
                _activeItemUsageByNameDict = value ?? new Dictionary<string, int>();
                _activeItemUsageByName = _activeItemUsageByNameDict.Count > 0
                    ? _activeItemUsageByNameDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value)).ToList()
                    : new List<SerializableKeyValuePair>();
            }
        }

        /// <summary>
        /// 리소스 이름
        /// </summary>
        public string resourceName;

        /// <summary>
        /// 시작 리소스
        /// </summary>
        public int startResource;

        /// <summary>
        /// 종료 리소스
        /// </summary>
        public int endResource;

        /// <summary>
        /// 최대 리소스
        /// </summary>
        public int maxResource;

        /// <summary>
        /// 총 획득 리소스
        /// </summary>
        public int totalResourceGained;

        /// <summary>
        /// 총 소모 리소스
        /// </summary>
        public int totalResourceSpent;
    }

    /// <summary>
    /// 세션 통계 요약 데이터
    /// </summary>
    [Serializable]
    public class SessionSummaryData
    {
        /// <summary>
        /// 총 적에게 준 데미지
        /// </summary>
        public int totalDamageDealt = 0;

        /// <summary>
        /// 총 받은 데미지
        /// </summary>
        public int totalDamageTaken = 0;

        /// <summary>
        /// 총 힐링량
        /// </summary>
        public int totalHealing = 0;

        /// <summary>
        /// 총 턴 수
        /// </summary>
        public int totalTurns = 0;

        /// <summary>
        /// 가장 많이 사용된 카드 ID와 사용 횟수
        /// </summary>
        public string mostUsedCardId = "None";

        /// <summary>
        /// 가장 많이 사용된 카드 사용 횟수
        /// </summary>
        public int mostUsedCardCount = 0;

        /// <summary>
        /// 가장 많이 사용된 스킬 이름과 사용 횟수
        /// </summary>
        public string mostUsedSkillName = "None";

        /// <summary>
        /// 가장 많이 사용된 스킬 사용 횟수
        /// </summary>
        public int mostUsedSkillCount = 0;

        /// <summary>
        /// 가장 많이 사용된 아이템 이름과 사용 횟수
        /// </summary>
        public string mostUsedItemName = "None";

        /// <summary>
        /// 가장 많이 사용된 아이템 사용 횟수
        /// </summary>
        public int mostUsedItemCount = 0;
    }

    /// <summary>
    /// 통계 저장 파일 래퍼 (여러 세션 포함)
    /// </summary>
    [Serializable]
    public class StatisticsSaveData
    {
        /// <summary>
        /// 저장 파일 버전
        /// </summary>
        public int version = 1;

        /// <summary>
        /// 마지막 업데이트 시간 (ISO 8601 형식)
        /// </summary>
        public string lastUpdatedTime;

        /// <summary>
        /// 총 세션 수
        /// </summary>
        public int totalSessionCount;

        /// <summary>
        /// 모든 세션 통계 데이터
        /// </summary>
        public List<SessionStatisticsData> sessions = new List<SessionStatisticsData>();
    }
}

