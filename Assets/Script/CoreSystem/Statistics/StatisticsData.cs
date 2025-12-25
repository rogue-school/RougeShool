using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.SkillCardSystem.Deck;
using Game.CoreSystem.Utility;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.CoreSystem.Statistics
{
    /// <summary>
    /// JSON 직렬화를 위한 Key-Value Pair (카드 ID와 이름 포함)
    /// </summary>
    [Serializable]
    public class SerializableKeyValuePair
    {
        public string displayName; // 카드/아이템 표시 이름 (KEY 값과 함께 표시)
        public string key;
        public int value;

        public SerializableKeyValuePair() { }

        public SerializableKeyValuePair(string key, int value)
        {
            this.key = key;
            this.value = value;
            this.displayName = string.Empty;
        }

        public SerializableKeyValuePair(string key, int value, string displayName)
        {
            this.key = key;
            this.value = value;
            this.displayName = displayName ?? string.Empty;
        }
    }
    
    /// <summary>
    /// JSON 직렬화를 위한 이름-값 쌍 (key 없이 출력)
    /// </summary>
    [Serializable]
    public class SerializableNameValue
    {
        public string displayName;
        public int value;

        public SerializableNameValue() { }
        public SerializableNameValue(string name, int value)
        {
            this.displayName = name ?? string.Empty;
            this.value = value < 0 ? 0 : value;
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
        public string 세션ID;

        /// <summary>
        /// 게임 시작 시간 (ISO 8601 형식)
        /// </summary>
        public string 게임시작시간;

        /// <summary>
        /// 게임 종료 시간 (ISO 8601 형식)
        /// </summary>
        public string 게임종료시간;

        /// <summary>
        /// 총 플레이 시간 (초)
        /// </summary>
        public float 총플레이시간초;

        /// <summary>
        /// 선택된 캐릭터 이름 (예: "아케인", "세레나")
        /// </summary>
        public string 선택된캐릭터이름;

        /// <summary>
        /// 최종 스테이지 번호
        /// </summary>
        public int 최종스테이지번호;

        /// <summary>
        /// 최종 적 인덱스
        /// </summary>
        public int 최종적인덱스;

        /// <summary>
        /// 총 승리 횟수
        /// </summary>
        public int 총승리횟수;

        /// <summary>
        /// 총 패배 횟수
        /// </summary>
        public int 총패배횟수;

        /// <summary>
        /// 총 획득한 자원 (세션 레벨)
        /// </summary>
        public int 총획득한자원;

        /// <summary>
        /// 총 사용한 자원 (세션 레벨)
        /// </summary>
        public int 총사용한자원;

        /// <summary>
        /// (비직렬화) 미획득 액티브 아이템 수 - JSON 출력에서 제외
        /// </summary>
        [NonSerialized]
        public int unacquiredActiveItemCount;

        /// <summary>
        /// 최종 전투의 턴수 (마지막 전투의 턴수)
        /// </summary>
        public int 최종턴수;

        // 기존 필드 이름과의 호환성을 위한 프로퍼티
        public string sessionId { get => 세션ID; set => 세션ID = value; }
        public string gameStartTime { get => 게임시작시간; set => 게임시작시간 = value; }
        public string gameEndTime { get => 게임종료시간; set => 게임종료시간 = value; }
        public float totalPlayTimeSeconds { get => 총플레이시간초; set => 총플레이시간초 = value; }
        public string selectedCharacterName { get => 선택된캐릭터이름; set => 선택된캐릭터이름 = value; }
        public int finalStageNumber { get => 최종스테이지번호; set => 최종스테이지번호 = value; }
        public int finalEnemyIndex { get => 최종적인덱스; set => 최종적인덱스 = value; }
        public int totalVictoryCount { get => 총승리횟수; set => 총승리횟수 = value; }
        public int totalDefeatCount { get => 총패배횟수; set => 총패배횟수 = value; }
        public int totalResourceGained { get => 총획득한자원; set => 총획득한자원 = value; }
        public int totalResourceSpent { get => 총사용한자원; set => 총사용한자원 = value; }
        public int finalTurns { get => 최종턴수; set => 최종턴수 = value; }

        /// <summary>
        /// 전투별 통계 데이터
        /// </summary>
        public List<CombatStatisticsData> 전투별통계 = new List<CombatStatisticsData>();

        /// <summary>
        /// 전체 세션 통계 요약
        /// </summary>
        public SessionSummaryData 요약 = new SessionSummaryData();

        // 기존 필드 이름과의 호환성을 위한 프로퍼티
        public List<CombatStatisticsData> combatStatistics { get => 전투별통계; set => 전투별통계 = value; }
        public SessionSummaryData summary { get => 요약; set => 요약 = value; }

        // JSON 직렬화를 위한 Dictionary -> List 변환 필드
        // ID 기반 목록은 JSON에서 제외 (NonSerialized)
        [System.NonSerialized] private List<SerializableKeyValuePair> 스킬카드생성수_카드ID별 = new List<SerializableKeyValuePair>();
        [System.NonSerialized] private List<SerializableKeyValuePair> 스킬카드사용수_카드ID별 = new List<SerializableKeyValuePair>();
        [System.NonSerialized] private List<SerializableKeyValuePair> 액티브아이템획득수_아이템ID별 = new List<SerializableKeyValuePair>();
        [System.NonSerialized] private List<SerializableKeyValuePair> 액티브아이템버리기수_아이템ID별 = new List<SerializableKeyValuePair>();
        [System.NonSerialized] private List<SerializableKeyValuePair> 패시브아이템획득수_아이템ID별 = new List<SerializableKeyValuePair>();
        // 세션 총 집계 (이름 기반만 JSON에 출력)
        [System.NonSerialized] private List<SerializableKeyValuePair> 생성된스킬카드_이름별 = new List<SerializableKeyValuePair>();
        [System.NonSerialized] private List<SerializableKeyValuePair> 사용한스킬카드_이름별 = new List<SerializableKeyValuePair>();
        [System.NonSerialized] private List<SerializableKeyValuePair> 사용하지않은스킬카드_이름별 = new List<SerializableKeyValuePair>();
        [System.NonSerialized] private List<SerializableKeyValuePair> 스킬사용수_이름별 = new List<SerializableKeyValuePair>();
        [System.NonSerialized] private List<SerializableKeyValuePair> 생성된액티브아이템_이름별 = new List<SerializableKeyValuePair>();
        [System.NonSerialized] private List<SerializableKeyValuePair> 사용한액티브아이템_이름별 = new List<SerializableKeyValuePair>();
        [System.NonSerialized] private List<SerializableKeyValuePair> 버린액티브아이템_이름별 = new List<SerializableKeyValuePair>();
        [System.NonSerialized] private List<SerializableKeyValuePair> 생성된패시브아이템_이름별 = new List<SerializableKeyValuePair>();
        // 호환 필드 (세션 getter에서 사용)
        [System.NonSerialized] private List<SerializableKeyValuePair> 액티브아이템사용수_이름별 = new List<SerializableKeyValuePair>();

        // 새로운 출력 구조: 전투별집계 / 전체집계 (displayName + value 만 노출)
        [Serializable]
        public class 전투별집계구조
        {
            public List<SerializableNameValue> 생성된스킬카드 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 사용한스킬카드 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 사용하지않은스킬카드 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 생성된액티브아이템 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 사용한액티브아이템 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 버린액티브아이템 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 생성된패시브아이템 = new List<SerializableNameValue>();
        }

        [Serializable]
        public class 전체집계구조
        {
            public List<SerializableNameValue> 생성된스킬카드 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 사용한스킬카드 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 사용하지않은스킬카드 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 생성된액티브아이템 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 사용한액티브아이템 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 버린액티브아이템 = new List<SerializableNameValue>();
            public List<SerializableNameValue> 생성된패시브아이템 = new List<SerializableNameValue>();
        }

        [SerializeField] public 전투별집계구조 전투별집계 = new 전투별집계구조();
        [SerializeField] public 전체집계구조 전체집계 = new 전체집계구조();

        // 런타임 Dictionary (직렬화되지 않음, StatisticsSerializer에서 접근)
        [System.NonSerialized]
        internal Dictionary<string, int> _skillCardSpawnCountByCardIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        internal Dictionary<string, int> _skillCardUseCountByCardIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        internal Dictionary<string, int> _skillUseCountByNameDict = new Dictionary<string, int>();
        [System.NonSerialized]
        internal Dictionary<string, int> _activeItemSpawnCountByItemIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        internal Dictionary<string, int> _activeItemUseCountByNameDict = new Dictionary<string, int>();
        [System.NonSerialized]
        internal Dictionary<string, int> _activeItemDiscardCountByItemIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        internal Dictionary<string, int> _passiveItemAcquiredCountByItemIdDict = new Dictionary<string, int>();

        /// <summary>
        /// 세션 레벨 통계: 각 스킬카드별 생성 횟수 (카드 ID -> 생성 횟수)
        /// </summary>
        public Dictionary<string, int> skillCardSpawnCountByCardId
        {
            get
            {
                if (_skillCardSpawnCountByCardIdDict.Count == 0 && 스킬카드생성수_카드ID별.Count > 0)
                {
                    foreach (var kv in 스킬카드생성수_카드ID별)
                        _skillCardSpawnCountByCardIdDict[kv.key] = kv.value;
                }
                return _skillCardSpawnCountByCardIdDict;
            }
            set
            {
                _skillCardSpawnCountByCardIdDict = value ?? new Dictionary<string, int>();
                // 출력은 StatisticsSerializer가 전투별집계/전체집계에서 처리
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 스킬카드별 사용 횟수 (카드 ID -> 사용 횟수)
        /// </summary>
        public Dictionary<string, int> skillCardUseCountByCardId
        {
            get
            {
                if (_skillCardUseCountByCardIdDict.Count == 0 && 스킬카드사용수_카드ID별.Count > 0)
                {
                    foreach (var kv in 스킬카드사용수_카드ID별)
                        _skillCardUseCountByCardIdDict[kv.key] = kv.value;
                }
                return _skillCardUseCountByCardIdDict;
            }
            set
            {
                _skillCardUseCountByCardIdDict = value ?? new Dictionary<string, int>();
                // 출력은 StatisticsSerializer가 전투별집계/전체집계에서 처리
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 스킬별 사용 횟수 (스킬 이름 -> 사용 횟수)
        /// </summary>
        public Dictionary<string, int> skillUseCountByName
        {
            get
            {
                if (_skillUseCountByNameDict.Count == 0 && 스킬사용수_이름별.Count > 0)
                {
                    foreach (var kv in 스킬사용수_이름별)
                        _skillUseCountByNameDict[kv.key] = kv.value;
                }
                return _skillUseCountByNameDict;
            }
            set
            {
                _skillUseCountByNameDict = value ?? new Dictionary<string, int>();
                // 출력은 StatisticsSerializer가 전투별집계/전체집계에서 처리
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 액티브 아이템별 생성 횟수 (아이템 ID -> 생성 횟수)
        /// </summary>
        public Dictionary<string, int> activeItemSpawnCountByItemId
        {
            get
            {
                if (_activeItemSpawnCountByItemIdDict.Count == 0 && 액티브아이템획득수_아이템ID별.Count > 0)
                {
                    foreach (var kv in 액티브아이템획득수_아이템ID별)
                        _activeItemSpawnCountByItemIdDict[kv.key] = kv.value;
                }
                return _activeItemSpawnCountByItemIdDict;
            }
            set
            {
                _activeItemSpawnCountByItemIdDict = value ?? new Dictionary<string, int>();
                // 출력은 StatisticsSerializer가 전투별집계/전체집계에서 처리
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 액티브 아이템별 사용 횟수 (아이템 이름 -> 사용 횟수)
        /// </summary>
        public Dictionary<string, int> activeItemUseCountByName
        {
            get
            {
                if (_activeItemUseCountByNameDict.Count == 0 && 액티브아이템사용수_이름별.Count > 0)
                {
                    foreach (var kv in 액티브아이템사용수_이름별)
                        _activeItemUseCountByNameDict[kv.key] = kv.value;
                }
                return _activeItemUseCountByNameDict;
            }
            set
            {
                _activeItemUseCountByNameDict = value ?? new Dictionary<string, int>();
                // 전환: 세션 출력은 PrepareForSerialization에서 채움
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 액티브 아이템별 버리기 횟수 (아이템 ID -> 버리기 횟수)
        /// </summary>
        public Dictionary<string, int> activeItemDiscardCountByItemId
        {
            get
            {
                if (_activeItemDiscardCountByItemIdDict.Count == 0 && 액티브아이템버리기수_아이템ID별.Count > 0)
                {
                    foreach (var kv in 액티브아이템버리기수_아이템ID별)
                        _activeItemDiscardCountByItemIdDict[kv.key] = kv.value;
                }
                return _activeItemDiscardCountByItemIdDict;
            }
            set
            {
                _activeItemDiscardCountByItemIdDict = value ?? new Dictionary<string, int>();
                // 출력은 StatisticsSerializer가 전투별집계/전체집계에서 처리
            }
        }

        /// <summary>
        /// 세션 레벨 통계: 각 패시브 아이템별 획득 횟수 (아이템 ID -> 획득 횟수)
        /// </summary>
        public Dictionary<string, int> passiveItemAcquiredCountByItemId
        {
            get
            {
                if (_passiveItemAcquiredCountByItemIdDict.Count == 0 && 패시브아이템획득수_아이템ID별.Count > 0)
                {
                    foreach (var kv in 패시브아이템획득수_아이템ID별)
                        _passiveItemAcquiredCountByItemIdDict[kv.key] = kv.value;
                }
                return _passiveItemAcquiredCountByItemIdDict;
            }
            set
            {
                _passiveItemAcquiredCountByItemIdDict = value ?? new Dictionary<string, int>();
                // 출력은 StatisticsSerializer가 전투별집계/전체집계에서 처리
            }
        }

        /// <summary>
        /// 카드 이름 캐시
        /// </summary>
        private static Dictionary<string, string> _cardNameCache = new Dictionary<string, string>();

        /// <summary>
        /// 카드 정의 캐시
        /// </summary>
        private static Dictionary<string, Game.SkillCardSystem.Data.SkillCardDefinition> _cachedDefinitions = new Dictionary<string, Game.SkillCardSystem.Data.SkillCardDefinition>();
        private static readonly HashSet<string> _loadingDefinitions = new HashSet<string>();
        
        /// <summary>
        /// 카드 ID로 스킬 이름 조회 (정적 메서드, 캐싱 지원)
        /// </summary>
        public static string GetCardDisplayNameStatic(string cardId)
                {
            if (string.IsNullOrEmpty(cardId)) return string.Empty;

            // 캐시 확인
            if (_cardNameCache.TryGetValue(cardId, out string cachedName))
                        {
                return cachedName;
                        }

            try
            {
                Game.SkillCardSystem.Data.SkillCardDefinition definition = null;
                
                // 캐시 확인
                if (_cachedDefinitions.TryGetValue(cardId, out Game.SkillCardSystem.Data.SkillCardDefinition cachedDefinition))
                {
                    definition = cachedDefinition;
                }
                else
                {
                    // 로딩 중인지 확인
                    if (_loadingDefinitions.Contains(cardId))
                    {
                        return cardId; // 로딩 중이면 ID 반환
                    }

                    try
                    {
                        _loadingDefinitions.Add(cardId);
                        
                        // 먼저 SkillCards/{cardId} 경로로 시도
                        string address = $"SkillCards/{cardId}";
                        var handle = Addressables.LoadAssetAsync<Game.SkillCardSystem.Data.SkillCardDefinition>(address);
                        definition = handle.WaitForCompletion();
                        
                        // 실패 시 Data/SkillCard/Skill/{cardId} 경로로 시도
                        if (definition == null)
                        {
                            address = $"Data/SkillCard/Skill/{cardId}";
                            handle = Addressables.LoadAssetAsync<Game.SkillCardSystem.Data.SkillCardDefinition>(address);
                            definition = handle.WaitForCompletion();
                        }
                        
                        if (definition != null)
                        {
                            _cachedDefinitions[cardId] = definition;
                        }
                    }
                    catch (Exception ex)
                    {
                        GameLogger.LogError($"[StatisticsData] 카드 정의 로드 중 오류: {ex.Message}", GameLogger.LogCategory.Core);
                    }
                    finally
                    {
                        _loadingDefinitions.Remove(cardId);
                    }
                }
                
                if (definition != null)
                {
                    // 한국어 이름이 있으면 한국어 이름 우선, 없으면 영문 이름, 둘 다 없으면 카드 ID
                    string displayName = !string.IsNullOrEmpty(definition.displayNameKO) 
                        ? definition.displayNameKO 
                        : (!string.IsNullOrEmpty(definition.displayName) 
                            ? definition.displayName 
                            : cardId);
                    
                    // 캐시에 저장
                    _cardNameCache[cardId] = displayName;
                    return displayName;
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[StatisticsData] 카드 이름 로드 실패: {cardId}, {ex.Message}", GameLogger.LogCategory.Save);
            }

            // 실패 시 카드 ID 반환 및 캐시에 저장
            _cardNameCache[cardId] = cardId;
            return cardId;
        }
        

        /// <summary>
        /// 저장 전에 Dictionary를 List로 변환 및 정렬 (카드/아이템 이름 포함)
        /// StatisticsSerializer를 사용하여 직렬화 준비
        /// </summary>
        public void PrepareForSerialization(PlayerSkillDeck playerDeck = null)
            {
            StatisticsSerializer.PrepareForSerialization(this, playerDeck);
        }
    }

    /// <summary>
    /// 전투 통계 데이터 직렬화 준비
    /// </summary>
    public static class CombatStatisticsDataExtensions
    {
        /// <summary>
        /// 카드 ID로 스킬 이름 조회 (정적 메서드)
        /// </summary>
        private static string GetCardDisplayName(string cardId)
        {
            return SessionStatisticsData.GetCardDisplayNameStatic(cardId);
        }

        public static void PrepareForSerialization(this CombatStatisticsData combat)
        {
            // 전투별 통계: 이름 기반만 출력 (ID 기반은 제외)
            var cardIdDict = combat.playerSkillUsageByCardId ?? new Dictionary<string, int>();
            var itemNameDict = combat.activeItemUsageByName ?? new Dictionary<string, int>();

            // 스킬카드 사용 통계 (ID -> 이름 변환)
            var skillUsageByName = new Dictionary<string, int>();
            foreach (var kv in cardIdDict)
            {
                string cardName = GetCardDisplayName(kv.Key);
                if (!skillUsageByName.ContainsKey(cardName))
                    skillUsageByName[cardName] = 0;
                skillUsageByName[cardName] += kv.Value;
            }
            
            combat.사용한스킬카드_이름별_전투 = skillUsageByName.Count > 0
                ? skillUsageByName.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value, kv.Key)).ToList()
                : new List<SerializableKeyValuePair>();

            // 액티브 아이템 사용 통계 (이름 그대로)
            combat.사용한액티브아이템_이름별_전투 = itemNameDict.Count > 0
                ? itemNameDict.Select(kv => new SerializableKeyValuePair(kv.Key, kv.Value, kv.Key)).ToList()
                : new List<SerializableKeyValuePair>();
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
        public string 전투시작시간;

        /// <summary>
        /// 전투 종료 시간 (ISO 8601 형식)
        /// </summary>
        public string 전투종료시간;

        /// <summary>
        /// 스테이지 번호
        /// </summary>
        public int 스테이지번호;

        /// <summary>
        /// 적 인덱스
        /// </summary>
        public int 적인덱스;

        /// <summary>
        /// 전투 결과 (Victory, Defeat)
        /// </summary>
        public string 전투결과;

        /// <summary>
        /// 전투 지속 시간 (초)
        /// </summary>
        public float 전투지속시간초;

        /// <summary>
        /// 총 턴 수
        /// </summary>
        public int 총턴수;

        /// <summary>
        /// 적에게 준 총 데미지
        /// </summary>
        public int 적에게준총데미지;

        /// <summary>
        /// 플레이어가 받은 총 데미지
        /// </summary>
        public int 플레이어가받은총데미지;

        /// <summary>
        /// 플레이어가 받은 총 힐링
        /// </summary>
        public int 플레이어가받은총힐링;

        // 기존 필드 이름과의 호환성을 위한 프로퍼티
        public string combatStartTime { get => 전투시작시간; set => 전투시작시간 = value; }
        public string combatEndTime { get => 전투종료시간; set => 전투종료시간 = value; }
        public int stageNumber { get => 스테이지번호; set => 스테이지번호 = value; }
        public int enemyIndex { get => 적인덱스; set => 적인덱스 = value; }
        public string result { get => 전투결과; set => 전투결과 = value; }
        public float battleDurationSeconds { get => 전투지속시간초; set => 전투지속시간초 = value; }
        public int totalTurns { get => 총턴수; set => 총턴수 = value; }
        public int totalDamageDealtToEnemies { get => 적에게준총데미지; set => 적에게준총데미지 = value; }
        public int totalDamageTakenByPlayer { get => 플레이어가받은총데미지; set => 플레이어가받은총데미지 = value; }
        public int totalHealingToPlayer { get => 플레이어가받은총힐링; set => 플레이어가받은총힐링 = value; }

        // 전투별 통계 (이름 기반만 JSON에 출력)
        [System.NonSerialized] private List<SerializableKeyValuePair> 플레이어스킬사용수_카드ID별 = new List<SerializableKeyValuePair>();
        [SerializeField] internal List<SerializableKeyValuePair> 사용한스킬카드_이름별_전투 = new List<SerializableKeyValuePair>();
        [SerializeField] internal List<SerializableKeyValuePair> 사용한액티브아이템_이름별_전투 = new List<SerializableKeyValuePair>();

        // 런타임 Dictionary (직렬화되지 않음)
        [System.NonSerialized]
        private Dictionary<string, int> _playerSkillUsageByCardIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _playerSkillUsageByNameDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _activeItemUsageByNameDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _playerSkillCardSpawnByCardIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _activeItemSpawnByItemIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _activeItemDiscardByItemIdDict = new Dictionary<string, int>();
        [System.NonSerialized]
        private Dictionary<string, int> _passiveItemAcquiredByItemIdDict = new Dictionary<string, int>();

        /// <summary>
        /// 카드별 사용 횟수 (카드 ID -> 사용 횟수)
        /// </summary>
        public Dictionary<string, int> playerSkillUsageByCardId
        {
            get
            {
                if (_playerSkillUsageByCardIdDict.Count == 0 && 플레이어스킬사용수_카드ID별.Count > 0)
                {
                    foreach (var kv in 플레이어스킬사용수_카드ID별)
                        _playerSkillUsageByCardIdDict[kv.key] = kv.value;
                }
                return _playerSkillUsageByCardIdDict;
            }
            set
            {
                _playerSkillUsageByCardIdDict = value ?? new Dictionary<string, int>();
                플레이어스킬사용수_카드ID별 = _playerSkillUsageByCardIdDict.Count > 0
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
                if (_playerSkillUsageByNameDict.Count == 0 && 사용한스킬카드_이름별_전투.Count > 0)
                {
                    foreach (var kv in 사용한스킬카드_이름별_전투)
                        _playerSkillUsageByNameDict[kv.key] = kv.value;
                }
                return _playerSkillUsageByNameDict;
            }
            set
            {
                _playerSkillUsageByNameDict = value ?? new Dictionary<string, int>();
                // 전투별 통계는 PrepareForSerialization에서 처리
            }
        }

        /// <summary>
        /// 액티브 아이템 사용 횟수 (아이템 이름 -> 사용 횟수)
        /// </summary>
        public Dictionary<string, int> activeItemUsageByName
        {
            get
            {
                if (_activeItemUsageByNameDict.Count == 0 && 사용한액티브아이템_이름별_전투.Count > 0)
                {
                    foreach (var kv in 사용한액티브아이템_이름별_전투)
                        _activeItemUsageByNameDict[kv.key] = kv.value;
                }
                return _activeItemUsageByNameDict;
            }
            set
            {
                _activeItemUsageByNameDict = value ?? new Dictionary<string, int>();
                // 전투별 통계는 PrepareForSerialization에서 처리
            }
        }

        /// <summary>
        /// 스킬카드 생성 횟수 (카드 ID -> 생성 횟수)
        /// </summary>
        public Dictionary<string, int> playerSkillCardSpawnByCardId
        {
            get => _playerSkillCardSpawnByCardIdDict;
            set => _playerSkillCardSpawnByCardIdDict = value ?? new Dictionary<string, int>();
        }

        /// <summary>
        /// 액티브 아이템 생성 횟수 (아이템 ID -> 생성 횟수)
        /// </summary>
        public Dictionary<string, int> activeItemSpawnByItemId
        {
            get => _activeItemSpawnByItemIdDict;
            set => _activeItemSpawnByItemIdDict = value ?? new Dictionary<string, int>();
        }

        /// <summary>
        /// 액티브 아이템 버리기 횟수 (아이템 ID -> 버리기 횟수)
        /// </summary>
        public Dictionary<string, int> activeItemDiscardByItemId
        {
            get => _activeItemDiscardByItemIdDict;
            set => _activeItemDiscardByItemIdDict = value ?? new Dictionary<string, int>();
        }

        /// <summary>
        /// 패시브 아이템 획득 횟수 (아이템 ID -> 획득 횟수)
        /// </summary>
        public Dictionary<string, int> passiveItemAcquiredByItemId
        {
            get => _passiveItemAcquiredByItemIdDict;
            set => _passiveItemAcquiredByItemIdDict = value ?? new Dictionary<string, int>();
        }

        /// <summary>
        /// 리소스 이름
        /// </summary>
        public string 자원이름;

        /// <summary>
        /// 시작 리소스
        /// </summary>
        public int 시작자원;

        /// <summary>
        /// 종료 리소스
        /// </summary>
        public int 종료자원;

        /// <summary>
        /// 최대 리소스
        /// </summary>
        public int 최대자원;

        /// <summary>
        /// 총 획득 리소스
        /// </summary>
        public int 총획득자원;

        /// <summary>
        /// 총 소모 리소스
        /// </summary>
        public int 총소모자원;

        // 기존 필드 이름과의 호환성을 위한 프로퍼티
        public string resourceName { get => 자원이름; set => 자원이름 = value; }
        public int startResource { get => 시작자원; set => 시작자원 = value; }
        public int endResource { get => 종료자원; set => 종료자원 = value; }
        public int maxResource { get => 최대자원; set => 최대자원 = value; }
        public int totalResourceGained { get => 총획득자원; set => 총획득자원 = value; }
        public int totalResourceSpent { get => 총소모자원; set => 총소모자원 = value; }
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
        public int 총적에게준데미지 = 0;

        /// <summary>
        /// 총 받은 데미지
        /// </summary>
        public int 총받은데미지 = 0;

        /// <summary>
        /// 총 힐링량
        /// </summary>
        public int 총힐링량 = 0;

        /// <summary>
        /// 총 턴 수
        /// </summary>
        public int 총턴수 = 0;

        /// <summary>
        /// 가장 많이 사용된 카드 ID와 사용 횟수 (JSON 제외)
        /// </summary>
        [System.NonSerialized]
        public string 가장많이사용된카드ID = "None";

        /// <summary>
        /// 가장 많이 사용된 카드 사용 횟수 (JSON 제외)
        /// </summary>
        [System.NonSerialized]
        public int 가장많이사용된카드사용횟수 = 0;

        /// <summary>
        /// 가장 많이 사용된 스킬 이름과 사용 횟수
        /// </summary>
        public string 가장많이사용된스킬이름 = "None";

        /// <summary>
        /// 가장 많이 사용된 스킬 사용 횟수
        /// </summary>
        public int 가장많이사용된스킬사용횟수 = 0;

        /// <summary>
        /// 가장 많이 사용된 아이템 이름과 사용 횟수
        /// </summary>
        public string 가장많이사용된아이템이름 = "None";

        /// <summary>
        /// 가장 많이 사용된 아이템 사용 횟수
        /// </summary>
        public int 가장많이사용된아이템사용횟수 = 0;

        // 기존 필드 이름과의 호환성을 위한 프로퍼티
        public int totalDamageDealt { get => 총적에게준데미지; set => 총적에게준데미지 = value; }
        public int totalDamageTaken { get => 총받은데미지; set => 총받은데미지 = value; }
        public int totalHealing { get => 총힐링량; set => 총힐링량 = value; }
        public int totalTurns { get => 총턴수; set => 총턴수 = value; }
        public string mostUsedCardId { get => 가장많이사용된카드ID; set => 가장많이사용된카드ID = value; }
        public int mostUsedCardCount { get => 가장많이사용된카드사용횟수; set => 가장많이사용된카드사용횟수 = value; }
        public string mostUsedSkillName { get => 가장많이사용된스킬이름; set => 가장많이사용된스킬이름 = value; }
        public int mostUsedSkillCount { get => 가장많이사용된스킬사용횟수; set => 가장많이사용된스킬사용횟수 = value; }
        public string mostUsedItemName { get => 가장많이사용된아이템이름; set => 가장많이사용된아이템이름 = value; }
        public int mostUsedItemCount { get => 가장많이사용된아이템사용횟수; set => 가장많이사용된아이템사용횟수 = value; }
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
        public int 버전 = 1;

        /// <summary>
        /// 마지막 업데이트 시간 (ISO 8601 형식)
        /// </summary>
        public string 마지막업데이트시간;

        /// <summary>
        /// 총 세션 수
        /// </summary>
        public int 총세션수;

        /// <summary>
        /// 모든 세션 통계 데이터
        /// </summary>
        public List<SessionStatisticsData> 세션목록 = new List<SessionStatisticsData>();

        // 기존 필드 이름과의 호환성을 위한 프로퍼티
        public int version { get => 버전; set => 버전 = value; }
        public string lastUpdatedTime { get => 마지막업데이트시간; set => 마지막업데이트시간 = value; }
        public int totalSessionCount { get => 총세션수; set => 총세션수 = value; }
        public List<SessionStatisticsData> sessions { get => 세션목록; set => 세션목록 = value; }
    }
}

