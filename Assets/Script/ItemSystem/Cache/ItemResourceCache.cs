using System.Collections.Generic;
using UnityEngine;
using Game.ItemSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Cache
{
    /// <summary>
    /// 아이템 리소스 캐싱 시스템
    /// Resources.LoadAll 호출을 캐싱하여 성능을 최적화합니다.
    /// </summary>
    public static class ItemResourceCache
    {
        private static readonly Dictionary<string, ActiveItemDefinition[]> activeItemCache = new Dictionary<string, ActiveItemDefinition[]>();
        private static readonly Dictionary<string, PassiveItemDefinition[]> passiveItemCache = new Dictionary<string, PassiveItemDefinition[]>();
        private static readonly object cacheLock = new object();

        /// <summary>
        /// 액티브 아이템들을 가져옵니다 (캐싱됨).
        /// </summary>
        /// <param name="path">리소스 경로</param>
        /// <returns>액티브 아이템 정의 배열</returns>
        public static ActiveItemDefinition[] GetActiveItems(string path = "Data/Item")
        {
            lock (cacheLock)
            {
                if (!activeItemCache.ContainsKey(path))
                {
                    var items = Resources.LoadAll<ActiveItemDefinition>(path);
                    activeItemCache[path] = items ?? new ActiveItemDefinition[0];
                    GameLogger.LogInfo($"[ItemResourceCache] 액티브 아이템 캐시 로드: {items?.Length ?? 0}개 (경로: {path})", GameLogger.LogCategory.Core);
                }
                return activeItemCache[path];
            }
        }

        /// <summary>
        /// 패시브 아이템들을 가져옵니다 (캐싱됨).
        /// </summary>
        /// <param name="path">리소스 경로</param>
        /// <returns>패시브 아이템 정의 배열</returns>
        public static PassiveItemDefinition[] GetPassiveItems(string path = "Data/Item")
        {
            lock (cacheLock)
            {
                if (!passiveItemCache.ContainsKey(path))
                {
                    var items = Resources.LoadAll<PassiveItemDefinition>(path);
                    passiveItemCache[path] = items ?? new PassiveItemDefinition[0];
                    GameLogger.LogInfo($"[ItemResourceCache] 패시브 아이템 캐시 로드: {items?.Length ?? 0}개 (경로: {path})", GameLogger.LogCategory.Core);
                }
                return passiveItemCache[path];
            }
        }

        /// <summary>
        /// 특정 ID의 액티브 아이템을 찾습니다.
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="path">리소스 경로</param>
        /// <returns>찾은 아이템 정의 또는 null</returns>
        public static ActiveItemDefinition FindActiveItemById(string itemId, string path = "Data/Item")
        {
            var items = GetActiveItems(path);
            foreach (var item in items)
            {
                if (item != null && item.ItemId == itemId)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// 특정 ID의 패시브 아이템을 찾습니다.
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="path">리소스 경로</param>
        /// <returns>찾은 아이템 정의 또는 null</returns>
        public static PassiveItemDefinition FindPassiveItemById(string itemId, string path = "Data/Item")
        {
            var items = GetPassiveItems(path);
            foreach (var item in items)
            {
                if (item != null && item.ItemId == itemId)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// 캐시를 초기화합니다.
        /// </summary>
        public static void ClearCache()
        {
            lock (cacheLock)
            {
                activeItemCache.Clear();
                passiveItemCache.Clear();
                GameLogger.LogInfo("[ItemResourceCache] 캐시 초기화 완료", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 특정 경로의 캐시를 초기화합니다.
        /// </summary>
        /// <param name="path">초기화할 경로</param>
        public static void ClearCache(string path)
        {
            lock (cacheLock)
            {
                if (activeItemCache.ContainsKey(path))
                {
                    activeItemCache.Remove(path);
                    GameLogger.LogInfo($"[ItemResourceCache] 액티브 아이템 캐시 초기화: {path}", GameLogger.LogCategory.Core);
                }

                if (passiveItemCache.ContainsKey(path))
                {
                    passiveItemCache.Remove(path);
                    GameLogger.LogInfo($"[ItemResourceCache] 패시브 아이템 캐시 초기화: {path}", GameLogger.LogCategory.Core);
                }
            }
        }

        /// <summary>
        /// 캐시 상태를 가져옵니다.
        /// </summary>
        /// <returns>캐시 상태 정보</returns>
        public static CacheStatus GetCacheStatus()
        {
            lock (cacheLock)
            {
                return new CacheStatus
                {
                    ActiveItemCacheCount = activeItemCache.Count,
                    PassiveItemCacheCount = passiveItemCache.Count,
                    TotalActiveItems = GetTotalItemCount(activeItemCache),
                    TotalPassiveItems = GetTotalItemCount(passiveItemCache)
                };
            }
        }

        /// <summary>
        /// 총 아이템 수를 계산합니다.
        /// </summary>
        /// <param name="cache">캐시 딕셔너리</param>
        /// <returns>총 아이템 수</returns>
        private static int GetTotalItemCount<T>(Dictionary<string, T[]> cache) where T : ItemDefinition
        {
            int total = 0;
            foreach (var items in cache.Values)
            {
                total += items?.Length ?? 0;
            }
            return total;
        }

        /// <summary>
        /// 캐시 상태 정보 구조체
        /// </summary>
        public struct CacheStatus
        {
            public int ActiveItemCacheCount;
            public int PassiveItemCacheCount;
            public int TotalActiveItems;
            public int TotalPassiveItems;

            public override string ToString()
            {
                return $"캐시 상태 - 액티브: {ActiveItemCacheCount}개 경로, {TotalActiveItems}개 아이템 | " +
                       $"패시브: {PassiveItemCacheCount}개 경로, {TotalPassiveItems}개 아이템";
            }
        }
    }
}
