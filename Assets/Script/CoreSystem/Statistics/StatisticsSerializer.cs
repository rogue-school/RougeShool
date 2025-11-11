using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.SkillCardSystem.Deck;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Statistics
{
    /// <summary>
    /// 통계 데이터 직렬화 담당 클래스
    /// 전투 집계와 전체 통계를 JSON 출력 형식으로 변환합니다.
    /// </summary>
    public static class StatisticsSerializer
    {
        /// <summary>
        /// 세션 통계 데이터를 직렬화 준비 상태로 변환
        /// </summary>
        public static void PrepareForSerialization(SessionStatisticsData sessionData, PlayerSkillDeck playerDeck = null)
        {
            if (sessionData == null)
            {
                GameLogger.LogError("[StatisticsSerializer] 세션 데이터가 null입니다", GameLogger.LogCategory.Error);
                return;
            }

            // 안전 가드: null → 0 집계
            sessionData._skillCardSpawnCountByCardIdDict ??= new Dictionary<string, int>();
            sessionData._skillCardUseCountByCardIdDict ??= new Dictionary<string, int>();
            sessionData._skillUseCountByNameDict ??= new Dictionary<string, int>();
            sessionData._activeItemSpawnCountByItemIdDict ??= new Dictionary<string, int>();
            sessionData._activeItemUseCountByNameDict ??= new Dictionary<string, int>();
            sessionData._activeItemDiscardCountByItemIdDict ??= new Dictionary<string, int>();
            sessionData._passiveItemAcquiredCountByItemIdDict ??= new Dictionary<string, int>();

            // 전체집계 채우기
            PopulateOverallAggregation(sessionData, playerDeck);

            // 전투별집계 채우기
            PopulateCombatAggregation(sessionData);

            // 전투별 통계도 직렬화 준비
            if (sessionData.combatStatistics != null)
            {
                foreach (var combat in sessionData.combatStatistics)
                {
                    CombatStatisticsDataExtensions.PrepareForSerialization(combat);
                }
            }
        }

        /// <summary>
        /// 전체집계 구조 채우기
        /// </summary>
        private static void PopulateOverallAggregation(SessionStatisticsData sessionData, PlayerSkillDeck playerDeck)
        {
            var spawnDict = sessionData._skillCardSpawnCountByCardIdDict;
            var useDict = sessionData._skillCardUseCountByCardIdDict;
            var skillUseDict = sessionData._skillUseCountByNameDict;
            var activeItemSpawnDict = sessionData._activeItemSpawnCountByItemIdDict;
            var activeItemUseDict = sessionData._activeItemUseCountByNameDict;
            var activeItemDiscardDict = sessionData._activeItemDiscardCountByItemIdDict;
            var passiveItemAcquiredDict = sessionData._passiveItemAcquiredCountByItemIdDict;

            // 생성된 스킬카드
            sessionData.전체집계.생성된스킬카드 = ConvertCardDictToNameValueList(spawnDict, playerDeck);

            // 사용한 스킬카드
            sessionData.전체집계.사용한스킬카드 = ConvertCardDictToNameValueList(useDict, playerDeck);

            // 사용하지 않은 스킬카드 (생성 - 사용)
            var unusedCards = new Dictionary<string, int>();
            foreach (var kv in spawnDict)
            {
                string cardName = SessionStatisticsData.GetCardDisplayNameStatic(kv.Key);
                int spawnCount = kv.Value;
                int useCount = useDict.ContainsKey(kv.Key) ? useDict[kv.Key] : 0;
                int unusedCount = spawnCount - useCount;
                if (unusedCount > 0)
                {
                    if (!unusedCards.ContainsKey(cardName))
                        unusedCards[cardName] = 0;
                    unusedCards[cardName] += unusedCount;
                }
            }
            sessionData.전체집계.사용하지않은스킬카드 = unusedCards.Select(kv => new SerializableNameValue(kv.Key, kv.Value)).ToList();

            // 생성된 액티브아이템
            sessionData.전체집계.생성된액티브아이템 = activeItemSpawnDict.Select(kv => 
                new SerializableNameValue(GetItemDisplayName(kv.Key), kv.Value)).ToList();

            // 사용한 액티브아이템
            sessionData.전체집계.사용한액티브아이템 = activeItemUseDict.Select(kv => 
                new SerializableNameValue(kv.Key, kv.Value)).ToList();

            // 버린 액티브아이템
            sessionData.전체집계.버린액티브아이템 = activeItemDiscardDict.Select(kv => 
                new SerializableNameValue(GetItemDisplayName(kv.Key), kv.Value)).ToList();

            // 생성된 패시브아이템
            sessionData.전체집계.생성된패시브아이템 = passiveItemAcquiredDict.Select(kv => 
                new SerializableNameValue(GetItemDisplayName(kv.Key), kv.Value)).ToList();

            // null 값은 value 0으로 집계
            EnsureNonEmptyLists(sessionData.전체집계);
        }

        /// <summary>
        /// 전투별집계 구조 채우기
        /// </summary>
        private static void PopulateCombatAggregation(SessionStatisticsData sessionData)
        {
            if (sessionData.combatStatistics == null || sessionData.combatStatistics.Count == 0)
            {
                // 전투 데이터가 없으면 모든 항목을 0으로 초기화
                EnsureNonEmptyLists(sessionData.전투별집계);
                return;
            }

            var lastCombat = sessionData.combatStatistics[sessionData.combatStatistics.Count - 1];

            // 생성된 스킬카드 (전투별)
            var combatCardSpawn = lastCombat.playerSkillCardSpawnByCardId ?? new Dictionary<string, int>();
            sessionData.전투별집계.생성된스킬카드 = combatCardSpawn
                .GroupBy(kv => SessionStatisticsData.GetCardDisplayNameStatic(kv.Key))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Value))
                .Select(kv => new SerializableNameValue(kv.Key, kv.Value)).ToList();

            // 사용한 스킬카드 (전투별)
            var combatSkillUsage = (lastCombat.playerSkillUsageByCardId ?? new Dictionary<string, int>())
                .GroupBy(kv => SessionStatisticsData.GetCardDisplayNameStatic(kv.Key))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
            sessionData.전투별집계.사용한스킬카드 = combatSkillUsage.Select(kv => 
                new SerializableNameValue(kv.Key, kv.Value)).ToList();

            // 사용하지 않은 스킬카드 (전투별: 생성 - 사용)
            var combatUnusedCards = new Dictionary<string, int>();
            foreach (var kv in combatCardSpawn)
            {
                string cardName = SessionStatisticsData.GetCardDisplayNameStatic(kv.Key);
                int spawnCount = kv.Value;
                int useCount = combatSkillUsage.ContainsKey(cardName) ? combatSkillUsage[cardName] : 0;
                int unusedCount = spawnCount - useCount;
                if (unusedCount > 0)
                {
                    if (!combatUnusedCards.ContainsKey(cardName))
                        combatUnusedCards[cardName] = 0;
                    combatUnusedCards[cardName] += unusedCount;
                }
            }
            sessionData.전투별집계.사용하지않은스킬카드 = combatUnusedCards.Select(kv => 
                new SerializableNameValue(kv.Key, kv.Value)).ToList();

            // 생성된 액티브아이템 (전투별)
            var combatItemSpawn = lastCombat.activeItemSpawnByItemId ?? new Dictionary<string, int>();
            sessionData.전투별집계.생성된액티브아이템 = combatItemSpawn.Select(kv => 
                new SerializableNameValue(GetItemDisplayName(kv.Key), kv.Value)).ToList();

            // 사용한 액티브아이템 (전투별)
            var combatItemUsage = lastCombat.activeItemUsageByName ?? new Dictionary<string, int>();
            sessionData.전투별집계.사용한액티브아이템 = combatItemUsage.Select(kv => 
                new SerializableNameValue(kv.Key, kv.Value)).ToList();

            // 버린 액티브아이템 (전투별)
            var combatItemDiscard = lastCombat.activeItemDiscardByItemId ?? new Dictionary<string, int>();
            sessionData.전투별집계.버린액티브아이템 = combatItemDiscard.Select(kv => 
                new SerializableNameValue(GetItemDisplayName(kv.Key), kv.Value)).ToList();

            // 생성된 패시브아이템 (전투별)
            var combatPassiveItem = lastCombat.passiveItemAcquiredByItemId ?? new Dictionary<string, int>();
            sessionData.전투별집계.생성된패시브아이템 = combatPassiveItem.Select(kv => 
                new SerializableNameValue(GetItemDisplayName(kv.Key), kv.Value)).ToList();

            // null 값은 value 0으로 집계
            EnsureNonEmptyLists(sessionData.전투별집계);
        }

        /// <summary>
        /// 카드 Dictionary를 이름-값 리스트로 변환 (덱 순서 고려)
        /// </summary>
        private static List<SerializableNameValue> ConvertCardDictToNameValueList(
            Dictionary<string, int> cardDict, PlayerSkillDeck playerDeck)
        {
            if (cardDict == null || cardDict.Count == 0)
                return new List<SerializableNameValue>();

            var result = new List<SerializableNameValue>();

            // 덱 순서로 정렬 (덱이 제공된 경우)
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

                // 덱 순서대로 추가
                foreach (var cardId in deckCardIds)
                {
                    if (cardDict.ContainsKey(cardId))
                    {
                        string displayName = SessionStatisticsData.GetCardDisplayNameStatic(cardId);
                        result.Add(new SerializableNameValue(displayName, cardDict[cardId]));
                    }
                }

                // 덱에 없는 카드도 추가 (덱 순서 뒤에)
                foreach (var kv in cardDict)
                {
                    if (!deckCardIds.Contains(kv.Key))
                    {
                        string displayName = SessionStatisticsData.GetCardDisplayNameStatic(kv.Key);
                        result.Add(new SerializableNameValue(displayName, kv.Value));
                    }
                }
            }
            else
            {
                // 덱이 없으면 기본 순서로 변환
                foreach (var kv in cardDict)
                {
                    string displayName = SessionStatisticsData.GetCardDisplayNameStatic(kv.Key);
                    result.Add(new SerializableNameValue(displayName, kv.Value));
                }
            }

            return result;
        }

        /// <summary>
        /// 아이템 ID로 아이템 이름 조회
        /// </summary>
        private static string GetItemDisplayName(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return string.Empty;

            try
            {
                var item = Game.ItemSystem.Cache.ItemResourceCache.FindActiveItemById(itemId);
                if (item != null)
                {
                    return item.DisplayName;
                }
                
                // 패시브 아이템도 시도
                var passiveItem = Game.ItemSystem.Cache.ItemResourceCache.FindPassiveItemById(itemId);
                if (passiveItem != null)
                {
                    return passiveItem.DisplayName;
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[StatisticsSerializer] 아이템 이름 로드 실패: {itemId}, {ex.Message}", GameLogger.LogCategory.Save);
            }

            return itemId; // 실패 시 아이템 ID 반환
        }

        /// <summary>
        /// 리스트가 비어있지 않도록 보장 (null 값은 value 0으로 집계)
        /// </summary>
        private static void EnsureNonEmptyLists(SessionStatisticsData.전체집계구조 aggregation)
        {
            if (aggregation.생성된스킬카드.Count == 0)
                aggregation.생성된스킬카드.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.사용한스킬카드.Count == 0)
                aggregation.사용한스킬카드.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.사용하지않은스킬카드.Count == 0)
                aggregation.사용하지않은스킬카드.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.생성된액티브아이템.Count == 0)
                aggregation.생성된액티브아이템.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.사용한액티브아이템.Count == 0)
                aggregation.사용한액티브아이템.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.버린액티브아이템.Count == 0)
                aggregation.버린액티브아이템.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.생성된패시브아이템.Count == 0)
                aggregation.생성된패시브아이템.Add(new SerializableNameValue(string.Empty, 0));
        }

        /// <summary>
        /// 리스트가 비어있지 않도록 보장 (전투별집계용)
        /// </summary>
        private static void EnsureNonEmptyLists(SessionStatisticsData.전투별집계구조 aggregation)
        {
            if (aggregation.생성된스킬카드.Count == 0)
                aggregation.생성된스킬카드.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.사용한스킬카드.Count == 0)
                aggregation.사용한스킬카드.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.사용하지않은스킬카드.Count == 0)
                aggregation.사용하지않은스킬카드.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.생성된액티브아이템.Count == 0)
                aggregation.생성된액티브아이템.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.사용한액티브아이템.Count == 0)
                aggregation.사용한액티브아이템.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.버린액티브아이템.Count == 0)
                aggregation.버린액티브아이템.Add(new SerializableNameValue(string.Empty, 0));
            if (aggregation.생성된패시브아이템.Count == 0)
                aggregation.생성된패시브아이템.Add(new SerializableNameValue(string.Empty, 0));
        }
    }
}

