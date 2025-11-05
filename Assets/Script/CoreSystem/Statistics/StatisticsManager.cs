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
                    // 기존 세션 업데이트
                    var index = statisticsData.sessions.IndexOf(existingSession);
                    statisticsData.sessions[index] = sessionData;
                    GameLogger.LogInfo($"[StatisticsManager] 기존 세션 업데이트: {sessionData.sessionId}", GameLogger.LogCategory.Save);
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
                    var playerManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>(UnityEngine.FindObjectsInactive.Include);
                    if (playerManager != null && playerManager.GetPlayer() != null)
                    {
                        var playerData = playerManager.GetPlayer().CharacterData as Game.CharacterSystem.Data.PlayerCharacterData;
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

                sessionData.PrepareForSerialization(playerDeck);

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
        public bool HasStatisticsFile()
        {
            return File.Exists(StatisticsFilePath);
        }

        /// <summary>
        /// 통계 파일 경로 가져오기
        /// </summary>
        public string GetStatisticsFilePath()
        {
            return StatisticsFilePath;
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

