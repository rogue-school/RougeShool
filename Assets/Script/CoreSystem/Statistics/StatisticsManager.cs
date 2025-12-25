using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Statistics
{
    /// <summary>
    /// 통계 저장/로드 관리 인터페이스
    /// </summary>
    public interface IStatisticsManager
    {
        /// <summary>
        /// 현재 세션 통계 저장
        /// </summary>
        Task SaveSessionStatistics(SessionStatisticsData sessionData);

        /// <summary>
        /// 모든 통계 데이터 로드
        /// </summary>
        Task<StatisticsSaveData> LoadAllStatistics();

        /// <summary>
        /// 통계 파일 존재 여부 확인
        /// </summary>
        bool HasStatisticsFile();

        /// <summary>
        /// 통계 파일 경로 가져오기
        /// </summary>
        string GetStatisticsFilePath();
    }

    /// <summary>
    /// 통계 저장/로드 매니저
    /// 지스타 플레이 데이터를 수집하고 저장합니다.
    /// </summary>
    public class StatisticsManager : MonoBehaviour, IStatisticsManager
    {
        [Header("통계 저장 설정")]
        [Tooltip("통계 저장 파일 이름")]
        [SerializeField] private string statisticsFileName = "GameStatistics.json";

        [Inject(Optional = true)]
        private Game.CharacterSystem.Manager.PlayerManager playerManager;

        /// <summary>
        /// 통계 파일 전체 경로
        /// </summary>
        private string StatisticsFilePath => Path.Combine(Application.persistentDataPath, statisticsFileName);

        /// <summary>
        /// 현재 세션 통계 저장
        /// </summary>
        public async Task SaveSessionStatistics(SessionStatisticsData sessionData)
        {
            if (sessionData == null)
            {
                GameLogger.LogError("[StatisticsManager] 저장할 세션 데이터가 null입니다", GameLogger.LogCategory.Error);
                return;
            }

            try
            {
                GameLogger.LogInfo($"[StatisticsManager] 세션 통계 저장 시작: {sessionData.sessionId}", GameLogger.LogCategory.Save);

                // 기존 통계 데이터 로드
                var statisticsData = await LoadAllStatistics();
                if (statisticsData == null)
                {
                    statisticsData = new StatisticsSaveData();
                }

                // 세션 추가 (중복 체크)
                var existingSession = statisticsData.sessions.FirstOrDefault(s => s.sessionId == sessionData.sessionId);
                if (existingSession != null)
                {
                    // 기존 세션 업데이트 (통계 누적)
                    var index = statisticsData.sessions.IndexOf(existingSession);
                    
                    // 기존 세션 데이터와 새 데이터 병합 (누적)
                    var mergedSession = MergeSessionData(existingSession, sessionData);
                    statisticsData.sessions[index] = mergedSession;
                    GameLogger.LogInfo($"[StatisticsManager] 기존 세션 업데이트 (통계 누적): {sessionData.sessionId}", GameLogger.LogCategory.Save);
                }
                else
                {
                    // 새 세션 추가
                    statisticsData.sessions.Add(sessionData);
                    statisticsData.totalSessionCount = statisticsData.sessions.Count;
                    GameLogger.LogInfo($"[StatisticsManager] 새 세션 추가: {sessionData.sessionId}", GameLogger.LogCategory.Save);
                }

                // 마지막 업데이트 시간 갱신
                statisticsData.lastUpdatedTime = DateTime.UtcNow.ToString("o");

                // 저장 전에 Dictionary를 List로 변환 (덱 순서로 정렬)
                // 플레이어 덱 가져오기 시도
                Game.SkillCardSystem.Deck.PlayerSkillDeck playerDeck = null;
                try
                {
                    // DI로 주입받은 playerManager 사용 (FindFirstObjectByType 제거)
                    var pm = playerManager;
                    if (pm != null && pm.GetPlayer() != null)
                    {
                        var playerData = pm.GetPlayer().CharacterData as Game.CharacterSystem.Data.PlayerCharacterData;
                        if (playerData != null && playerData.SkillDeck != null)
                        {
                            playerDeck = playerData.SkillDeck;
                        }
                    }
                }
                catch (Exception ex)
                {
                    GameLogger.LogWarning($"[StatisticsManager] 플레이어 덱 가져오기 실패: {ex.Message}", GameLogger.LogCategory.Save);
                }

                StatisticsSerializer.PrepareForSerialization(sessionData, playerDeck);

                // JSON으로 직렬화
                string jsonData = JsonUtility.ToJson(statisticsData, true);

                // 파일로 저장
                await File.WriteAllTextAsync(StatisticsFilePath, jsonData);

                GameLogger.LogInfo($"[StatisticsManager] 세션 통계 저장 완료: {StatisticsFilePath}", GameLogger.LogCategory.Save);
                Debug.Log($"📊 통계 파일 저장 위치: {StatisticsFilePath}");
                GameLogger.LogInfo($"📊 통계 파일 저장 위치: {StatisticsFilePath}", GameLogger.LogCategory.Save);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[StatisticsManager] 세션 통계 저장 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 모든 통계 데이터 로드
        /// </summary>
        public async Task<StatisticsSaveData> LoadAllStatistics()
        {
            try
            {
                if (!File.Exists(StatisticsFilePath))
                {
                    GameLogger.LogInfo("[StatisticsManager] 통계 파일이 존재하지 않습니다", GameLogger.LogCategory.Save);
                    return new StatisticsSaveData();
                }

                string jsonData = await File.ReadAllTextAsync(StatisticsFilePath);
                if (string.IsNullOrEmpty(jsonData))
                {
                    GameLogger.LogWarning("[StatisticsManager] 통계 파일이 비어있습니다", GameLogger.LogCategory.Save);
                    return new StatisticsSaveData();
                }

                var statisticsData = JsonUtility.FromJson<StatisticsSaveData>(jsonData);
                if (statisticsData == null)
                {
                    GameLogger.LogWarning("[StatisticsManager] 통계 데이터 파싱 실패", GameLogger.LogCategory.Save);
                    return new StatisticsSaveData();
                }

                GameLogger.LogInfo($"[StatisticsManager] 통계 데이터 로드 완료: {statisticsData.totalSessionCount}개 세션", GameLogger.LogCategory.Save);
                return statisticsData;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[StatisticsManager] 통계 데이터 로드 실패: {ex.Message}", GameLogger.LogCategory.Error);
                return new StatisticsSaveData();
            }
        }

        /// <summary>
        /// 통계 파일 존재 여부 확인
        /// </summary>
        /// <returns>파일 존재 여부</returns>
        public bool HasStatisticsFile()
        {
            return File.Exists(StatisticsFilePath);
        }

        /// <summary>
        /// 통계 파일 경로 가져오기
        /// </summary>
        /// <returns>통계 파일 전체 경로</returns>
        public string GetStatisticsFilePath()
        {
            return StatisticsFilePath;
        }

        /// <summary>
        /// 기존 세션 데이터와 새 세션 데이터를 병합 (통계 누적)
        /// </summary>
        private SessionStatisticsData MergeSessionData(SessionStatisticsData existing, SessionStatisticsData newData)
        {
            if (existing == null) return newData;
            if (newData == null) return existing;

            // 기본 정보는 새 데이터로 업데이트 (최신 정보)
            existing.gameEndTime = newData.gameEndTime;
            existing.finalStageNumber = newData.finalStageNumber;
            existing.finalEnemyIndex = newData.finalEnemyIndex;
            existing.finalTurns = newData.finalTurns;

            // 통계 누적
            existing.totalVictoryCount += newData.totalVictoryCount;
            existing.totalDefeatCount += newData.totalDefeatCount;
            existing.totalResourceGained += newData.totalResourceGained;
            existing.totalResourceSpent += newData.totalResourceSpent;

            // 플레이 시간 누적
            existing.totalPlayTimeSeconds += newData.totalPlayTimeSeconds;

            // 전투 통계 병합 (중복 체크 후 새 전투만 추가)
            if (newData.combatStatistics != null && newData.combatStatistics.Count > 0)
            {
                if (existing.combatStatistics == null)
                {
                    existing.combatStatistics = new List<CombatStatisticsData>();
                }
                
                // 중복 체크: combatStartTime과 stageNumber, enemyIndex로 중복 판단
                foreach (var newCombat in newData.combatStatistics)
                {
                    bool isDuplicate = false;
                    if (!string.IsNullOrEmpty(newCombat.combatStartTime))
                    {
                        foreach (var existingCombat in existing.combatStatistics)
                        {
                            if (existingCombat.combatStartTime == newCombat.combatStartTime &&
                                existingCombat.stageNumber == newCombat.stageNumber &&
                                existingCombat.enemyIndex == newCombat.enemyIndex)
                            {
                                isDuplicate = true;
                                break;
                            }
                        }
                    }
                    
                    if (!isDuplicate)
                    {
                        existing.combatStatistics.Add(newCombat);
                    }
                }
            }

            // Dictionary 통계 누적
            MergeDictionary(existing.skillCardSpawnCountByCardId, newData.skillCardSpawnCountByCardId);
            MergeDictionary(existing.skillCardUseCountByCardId, newData.skillCardUseCountByCardId);
            MergeDictionary(existing.skillUseCountByName, newData.skillUseCountByName);
            MergeDictionary(existing.activeItemSpawnCountByItemId, newData.activeItemSpawnCountByItemId);
            MergeDictionary(existing.activeItemUseCountByName, newData.activeItemUseCountByName);
            MergeDictionary(existing.activeItemDiscardCountByItemId, newData.activeItemDiscardCountByItemId);
            MergeDictionary(existing.passiveItemAcquiredCountByItemId, newData.passiveItemAcquiredCountByItemId);

            // 요약 재계산
            existing.summary = newData.summary; // 최신 요약으로 업데이트

            return existing;
        }

        /// <summary>
        /// Dictionary 통계 누적
        /// </summary>
        private void MergeDictionary(Dictionary<string, int> existing, Dictionary<string, int> newData)
        {
            if (existing == null || newData == null) return;

            foreach (var kv in newData)
            {
                if (existing.ContainsKey(kv.Key))
                {
                    existing[kv.Key] += kv.Value;
                }
                else
                {
                    existing[kv.Key] = kv.Value;
                }
            }
        }

        /// <summary>
        /// 통계 파일 삭제 (테스트/디버그용)
        /// </summary>
        public void ClearStatisticsFile()
        {
            try
            {
                if (File.Exists(StatisticsFilePath))
                {
                    File.Delete(StatisticsFilePath);
                    GameLogger.LogInfo("[StatisticsManager] 통계 파일 삭제 완료", GameLogger.LogCategory.Save);
                }
                else
                {
                    GameLogger.LogInfo("[StatisticsManager] 삭제할 통계 파일이 없습니다", GameLogger.LogCategory.Save);
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[StatisticsManager] 통계 파일 삭제 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 통계 요약 정보 가져오기
        /// </summary>
        /// <returns>통계 요약 정보</returns>
        public async Task<StatisticsSummary> GetStatisticsSummary()
        {
            var statisticsData = await LoadAllStatistics();
            if (statisticsData == null || statisticsData.sessions == null || statisticsData.sessions.Count == 0)
            {
                return new StatisticsSummary();
            }

            var summary = new StatisticsSummary
            {
                totalSessions = statisticsData.totalSessionCount,
                totalPlayTimeSeconds = 0f,
                totalVictories = 0,
                totalDefeats = 0,
                mostPlayedCharacter = "None",
                averagePlayTimeSeconds = 0f
            };

            Dictionary<string, int> characterPlayCount = new Dictionary<string, int>();
            float totalTime = 0f;

            foreach (var session in statisticsData.sessions)
            {
                totalTime += session.totalPlayTimeSeconds;
                summary.totalVictories += session.totalVictoryCount;
                summary.totalDefeats += session.totalDefeatCount;

                if (!string.IsNullOrEmpty(session.selectedCharacterName))
                {
                    if (!characterPlayCount.ContainsKey(session.selectedCharacterName))
                        characterPlayCount[session.selectedCharacterName] = 0;
                    characterPlayCount[session.selectedCharacterName]++;
                }
            }

            summary.totalPlayTimeSeconds = totalTime;
            summary.averagePlayTimeSeconds = statisticsData.totalSessionCount > 0 ? totalTime / statisticsData.totalSessionCount : 0f;

            // 가장 많이 플레이된 캐릭터 찾기
            string mostPlayedCharacter = "None";
            int maxPlayCount = 0;
            foreach (var kv in characterPlayCount)
            {
                if (kv.Value > maxPlayCount)
                {
                    mostPlayedCharacter = kv.Key;
                    maxPlayCount = kv.Value;
                }
            }
            summary.mostPlayedCharacter = mostPlayedCharacter;

            return summary;
        }
    }

    /// <summary>
    /// 통계 요약 정보
    /// </summary>
    [Serializable]
    public class StatisticsSummary
    {
        public int totalSessions;
        public float totalPlayTimeSeconds;
        public int totalVictories;
        public int totalDefeats;
        public string mostPlayedCharacter;
        public float averagePlayTimeSeconds;
    }
}

