using System;
using System.Collections.Generic;
using UnityEngine;
using Game.ItemSystem.Data;
using Game.ItemSystem.Cache;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Service
{
    /// <summary>
    /// 기본 보상 생성 서비스
    /// 중복된 기본 보상 생성 로직을 통합합니다.
    /// </summary>
    public static class DefaultRewardService
    {
        private const int DEFAULT_ACTIVE_COUNT = 3;
        private const int DEFAULT_PASSIVE_COUNT = 1;

        /// <summary>
        /// 기본 액티브 보상을 생성합니다.
        /// </summary>
        /// <param name="count">생성할 아이템 수</param>
        /// <param name="path">리소스 경로</param>
        /// <returns>생성된 액티브 아이템 정의 배열</returns>
        public static ActiveItemDefinition[] GenerateDefaultActiveReward(int count = DEFAULT_ACTIVE_COUNT, string path = "Data/Item")
        {
            var allItems = ItemResourceCache.GetActiveItems(path);
            
            if (allItems.Length == 0)
            {
                GameLogger.LogError($"[DefaultRewardService] 액티브 아이템을 찾을 수 없습니다. (경로: {path})", GameLogger.LogCategory.UI);
                return new ActiveItemDefinition[0];
            }

            var selectedCount = Math.Min(count, allItems.Length);
            var selected = new List<ActiveItemDefinition>();
            
            for (int i = 0; i < selectedCount; i++)
            {
                var randomIndex = UnityEngine.Random.Range(0, allItems.Length);
                selected.Add(allItems[randomIndex]);
            }

            GameLogger.LogInfo($"[DefaultRewardService] 기본 액티브 보상 생성: {selected.Count}개 (경로: {path})", GameLogger.LogCategory.UI);
            return selected.ToArray();
        }

        /// <summary>
        /// 기본 패시브 보상을 생성합니다.
        /// </summary>
        /// <param name="count">생성할 아이템 수</param>
        /// <param name="path">리소스 경로</param>
        /// <returns>생성된 패시브 아이템 정의 배열</returns>
        public static PassiveItemDefinition[] GenerateDefaultPassiveReward(int count = DEFAULT_PASSIVE_COUNT, string path = "Data/Item")
        {
            var allItems = ItemResourceCache.GetPassiveItems(path);
            
            if (allItems.Length == 0)
            {
                GameLogger.LogInfo($"[DefaultRewardService] 패시브 아이템을 찾을 수 없습니다. (경로: {path})", GameLogger.LogCategory.UI);
                return new PassiveItemDefinition[0];
            }

            var selectedCount = Math.Min(count, allItems.Length);
            var selected = new List<PassiveItemDefinition>();
            
            for (int i = 0; i < selectedCount; i++)
            {
                var randomIndex = UnityEngine.Random.Range(0, allItems.Length);
                selected.Add(allItems[randomIndex]);
            }

            GameLogger.LogInfo($"[DefaultRewardService] 기본 패시브 보상 생성: {selected.Count}개 (경로: {path})", GameLogger.LogCategory.UI);
            return selected.ToArray();
        }

        /// <summary>
        /// 중복을 허용하지 않는 액티브 보상을 생성합니다.
        /// </summary>
        /// <param name="count">생성할 아이템 수</param>
        /// <param name="path">리소스 경로</param>
        /// <returns>생성된 액티브 아이템 정의 배열</returns>
        public static ActiveItemDefinition[] GenerateUniqueActiveReward(int count = DEFAULT_ACTIVE_COUNT, string path = "Data/Item")
        {
            var allItems = ItemResourceCache.GetActiveItems(path);
            
            if (allItems.Length == 0)
            {
                GameLogger.LogError($"[DefaultRewardService] 액티브 아이템을 찾을 수 없습니다. (경로: {path})", GameLogger.LogCategory.UI);
                return new ActiveItemDefinition[0];
            }

            var availableItems = new List<ActiveItemDefinition>(allItems);
            var selected = new List<ActiveItemDefinition>();
            var selectedCount = Math.Min(count, availableItems.Count);
            
            for (int i = 0; i < selectedCount; i++)
            {
                var randomIndex = UnityEngine.Random.Range(0, availableItems.Count);
                selected.Add(availableItems[randomIndex]);
                availableItems.RemoveAt(randomIndex);
            }

            GameLogger.LogInfo($"[DefaultRewardService] 고유 액티브 보상 생성: {selected.Count}개 (경로: {path})", GameLogger.LogCategory.UI);
            return selected.ToArray();
        }

        /// <summary>
        /// 중복을 허용하지 않는 패시브 보상을 생성합니다.
        /// </summary>
        /// <param name="count">생성할 아이템 수</param>
        /// <param name="path">리소스 경로</param>
        /// <returns>생성된 패시브 아이템 정의 배열</returns>
        public static PassiveItemDefinition[] GenerateUniquePassiveReward(int count = DEFAULT_PASSIVE_COUNT, string path = "Data/Item")
        {
            var allItems = ItemResourceCache.GetPassiveItems(path);
            
            if (allItems.Length == 0)
            {
                GameLogger.LogInfo($"[DefaultRewardService] 패시브 아이템을 찾을 수 없습니다. (경로: {path})", GameLogger.LogCategory.UI);
                return new PassiveItemDefinition[0];
            }

            var availableItems = new List<PassiveItemDefinition>(allItems);
            var selected = new List<PassiveItemDefinition>();
            var selectedCount = Math.Min(count, availableItems.Count);
            
            for (int i = 0; i < selectedCount; i++)
            {
                var randomIndex = UnityEngine.Random.Range(0, availableItems.Count);
                selected.Add(availableItems[randomIndex]);
                availableItems.RemoveAt(randomIndex);
            }

            GameLogger.LogInfo($"[DefaultRewardService] 고유 패시브 보상 생성: {selected.Count}개 (경로: {path})", GameLogger.LogCategory.UI);
            return selected.ToArray();
        }
    }
}
