using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Statistics
{
    /// <summary>
    /// 리더보드 관리 인터페이스
    /// </summary>
    public interface ILeaderboardManager
    {
        /// <summary>
        /// 리더보드에 점수 추가
        /// </summary>
        Task AddScore(SessionStatisticsData sessionData, ScoreData scoreData);

        /// <summary>
        /// 캐릭터별 리더보드 가져오기
        /// </summary>
        CharacterLeaderboard GetLeaderboard(string characterName);

        /// <summary>
        /// 캐릭터별 현재 순위 가져오기
        /// </summary>
        int GetCurrentRank(string characterName, int score);

        /// <summary>
        /// 캐릭터별 총 클리어 횟수 가져오기
        /// </summary>
        int GetTotalClearCount(string characterName);

        /// <summary>
        /// 전체 캐릭터 통합 총 클리어 횟수 가져오기
        /// </summary>
        int GetTotalClearCountAllCharacters();

        /// <summary>
        /// 처음 클리어 여부를 확인합니다.
        /// </summary>
        bool IsFirstClear(string characterName);

        /// <summary>
        /// 캐릭터별 최고 점수 가져오기
        /// </summary>
        int GetBestScore(string characterName);

        /// <summary>
        /// 캐릭터별 상위 N개 항목 가져오기
        /// </summary>
        List<LeaderboardEntry> GetTopEntries(string characterName, int count = 5);

        /// <summary>
        /// 모든 캐릭터 통합 상위 N개 항목 가져오기
        /// </summary>
        List<LeaderboardEntry> GetTopEntriesAllCharacters(int count = 10);

        /// <summary>
        /// 모든 캐릭터 통합 최고 점수 가져오기
        /// </summary>
        int GetBestScoreAllCharacters();

        /// <summary>
        /// 리더보드 데이터 로드
        /// </summary>
        Task LoadLeaderboard();

        /// <summary>
        /// 리더보드 데이터 저장
        /// </summary>
        Task SaveLeaderboard();
    }

    /// <summary>
    /// 로컬 리더보드 관리자
    /// </summary>
    public class LeaderboardManager : MonoBehaviour, ILeaderboardManager
    {
        [Header("리더보드 저장 설정")]
        [Tooltip("리더보드 저장 파일 이름")]
        [SerializeField] private string leaderboardFileName = "Leaderboard.json";

        /// <summary>
        /// 리더보드 파일 전체 경로
        /// </summary>
        private string LeaderboardFilePath => Path.Combine(Application.persistentDataPath, leaderboardFileName);

        /// <summary>
        /// 현재 리더보드 데이터
        /// </summary>
        private LeaderboardData _leaderboardData;

        private void Awake()
        {
            _leaderboardData = new LeaderboardData();
        }

        private async void Start()
        {
            await LoadLeaderboard();
        }

        /// <summary>
        /// 리더보드에 점수 추가
        /// </summary>
        public async Task AddScore(SessionStatisticsData sessionData, ScoreData scoreData)
        {
            if (sessionData == null || scoreData == null)
            {
                GameLogger.LogError("[LeaderboardManager] 세션 데이터 또는 점수 데이터가 null입니다", GameLogger.LogCategory.Error);
                return;
            }

            string characterName = sessionData.selectedCharacterName;
            if (string.IsNullOrEmpty(characterName))
            {
                GameLogger.LogWarning("[LeaderboardManager] 캐릭터 이름이 없습니다", GameLogger.LogCategory.Error);
                return;
            }

            // 리더보드가 로드되지 않았으면 로드 시도
            if (_leaderboardData == null || _leaderboardData.characterLeaderboards == null)
            {
                GameLogger.LogWarning("[LeaderboardManager] 리더보드 데이터가 초기화되지 않았습니다. 로드를 시도합니다.", GameLogger.LogCategory.Save);
                await LoadLeaderboard();
            }

            // 캐릭터별 리더보드 가져오기 또는 생성
            if (!_leaderboardData.characterLeaderboards.ContainsKey(characterName))
            {
                _leaderboardData.characterLeaderboards[characterName] = new CharacterLeaderboard(characterName);
                GameLogger.LogInfo($"[LeaderboardManager] 새 캐릭터 리더보드 생성: {characterName}", GameLogger.LogCategory.UI);
            }

            var leaderboard = _leaderboardData.characterLeaderboards[characterName];

            // 새 항목 생성
            var entry = new LeaderboardEntry(sessionData, scoreData);

            // 리더보드에 추가 (점수 높은 순으로 정렬)
            leaderboard.entries.Add(entry);
            leaderboard.entries = leaderboard.entries
                .OrderByDescending(e => e.totalScore)
                .ThenBy(e => e.playTimeSeconds)
                .ToList();

            // 최대 항목 수 제한
            if (leaderboard.entries.Count > LeaderboardData.MAX_ENTRIES_PER_CHARACTER)
            {
                leaderboard.entries = leaderboard.entries
                    .Take(LeaderboardData.MAX_ENTRIES_PER_CHARACTER)
                    .ToList();
            }

            // 총 클리어 횟수 증가
            leaderboard.totalClearCount++;

            // 마지막 업데이트 시간 갱신
            _leaderboardData.lastUpdatedTime = DateTime.UtcNow.ToString("o");

            // 저장
            await SaveLeaderboard();

            // 추가 후 순위 계산 (리더보드에 이미 추가된 상태)
            int rank = GetCurrentRank(characterName, scoreData.totalScore);
            GameLogger.LogInfo($"[LeaderboardManager] 리더보드에 점수 추가: {characterName} - {scoreData.totalScore}점 (순위: {rank}위, 총항목수: {leaderboard.entries.Count})", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 캐릭터별 리더보드 가져오기
        /// </summary>
        public CharacterLeaderboard GetLeaderboard(string characterName)
        {
            if (string.IsNullOrEmpty(characterName))
            {
                GameLogger.LogWarning("[LeaderboardManager] GetLeaderboard: 캐릭터 이름이 비어있습니다", GameLogger.LogCategory.UI);
                return null;
            }

            if (_leaderboardData == null)
            {
                GameLogger.LogError("[LeaderboardManager] GetLeaderboard: _leaderboardData가 null입니다", GameLogger.LogCategory.Error);
                return null;
            }

            if (_leaderboardData.characterLeaderboards.ContainsKey(characterName))
            {
                var leaderboard = _leaderboardData.characterLeaderboards[characterName];
                GameLogger.LogInfo($"[LeaderboardManager] GetLeaderboard: 기존 리더보드 반환. 캐릭터={characterName}, 항목수={leaderboard?.entries?.Count ?? 0}", GameLogger.LogCategory.UI);
                return leaderboard;
            }

            GameLogger.LogWarning($"[LeaderboardManager] GetLeaderboard: 새 리더보드 생성. 캐릭터={characterName} (리더보드에 없음)", GameLogger.LogCategory.UI);
            return new CharacterLeaderboard(characterName);
        }

        /// <summary>
        /// 캐릭터별 현재 순위 가져오기
        /// </summary>
        public int GetCurrentRank(string characterName, int score)
        {
            var leaderboard = GetLeaderboard(characterName);
            if (leaderboard == null || leaderboard.entries == null || leaderboard.entries.Count == 0)
            {
                GameLogger.LogWarning($"[LeaderboardManager] GetCurrentRank: 리더보드가 비어있습니다. 캐릭터: {characterName}, 점수: {score}", GameLogger.LogCategory.UI);
                return 1; // 첫 번째 클리어
            }

            // 점수 높은 순으로 정렬된 리스트에서 현재 점수의 순위 찾기
            // 동일한 점수가 여러 개 있을 수 있으므로, 현재 점수보다 높은 점수의 개수 + 1
            int rank = leaderboard.entries.Count(e => e.totalScore > score) + 1;
            
            GameLogger.LogInfo($"[LeaderboardManager] GetCurrentRank: 캐릭터={characterName}, 점수={score}, 순위={rank}, 총항목수={leaderboard.entries.Count}", GameLogger.LogCategory.UI);
            return rank;
        }

        /// <summary>
        /// 캐릭터별 총 클리어 횟수 가져오기
        /// </summary>
        public int GetTotalClearCount(string characterName)
        {
            var leaderboard = GetLeaderboard(characterName);
            if (leaderboard == null)
            {
                return 0;
            }

            return leaderboard.totalClearCount;
        }

        /// <summary>
        /// 전체 캐릭터 통합 총 클리어 횟수 가져오기
        /// </summary>
        public int GetTotalClearCountAllCharacters()
        {
            if (_leaderboardData == null || _leaderboardData.characterLeaderboards == null)
            {
                return 0;
            }

            int totalCount = 0;
            foreach (var leaderboard in _leaderboardData.characterLeaderboards.Values)
            {
                totalCount += leaderboard.totalClearCount;
            }

            return totalCount;
        }

        /// <summary>
        /// 캐릭터별 최고 점수 가져오기
        /// </summary>
        public int GetBestScore(string characterName)
        {
            var leaderboard = GetLeaderboard(characterName);
            if (leaderboard == null || leaderboard.entries == null || leaderboard.entries.Count == 0)
            {
                return 0;
            }

            // entries는 이미 점수 높은 순으로 정렬되어 있음
            return leaderboard.entries[0].totalScore;
        }

        /// <summary>
        /// 캐릭터별 상위 N개 항목 가져오기
        /// </summary>
        public List<LeaderboardEntry> GetTopEntries(string characterName, int count = 5)
        {
            var leaderboard = GetLeaderboard(characterName);
            if (leaderboard == null || leaderboard.entries == null || leaderboard.entries.Count == 0)
            {
                return new List<LeaderboardEntry>();
            }

            // entries는 이미 점수 높은 순으로 정렬되어 있음
            return leaderboard.entries.Take(count).ToList();
        }

        /// <summary>
        /// 모든 캐릭터 통합 상위 N개 항목 가져오기
        /// </summary>
        public List<LeaderboardEntry> GetTopEntriesAllCharacters(int count = 10)
        {
            if (_leaderboardData == null || _leaderboardData.characterLeaderboards == null)
            {
                return new List<LeaderboardEntry>();
            }

            // 모든 캐릭터의 항목을 수집
            List<LeaderboardEntry> allEntries = new List<LeaderboardEntry>();
            foreach (var leaderboard in _leaderboardData.characterLeaderboards.Values)
            {
                if (leaderboard.entries != null && leaderboard.entries.Count > 0)
                {
                    allEntries.AddRange(leaderboard.entries);
                }
            }

            // 점수 높은 순으로 정렬 (동일 점수면 플레이 시간 짧은 순)
            allEntries = allEntries
                .OrderByDescending(e => e.totalScore)
                .ThenBy(e => e.playTimeSeconds)
                .Take(count)
                .ToList();

            GameLogger.LogInfo($"[LeaderboardManager] GetTopEntriesAllCharacters: 통합 상위 {count}개 항목 반환. 총항목수={allEntries.Count}", GameLogger.LogCategory.UI);
            return allEntries;
        }

        /// <summary>
        /// 모든 캐릭터 통합 최고 점수 가져오기
        /// </summary>
        public int GetBestScoreAllCharacters()
        {
            if (_leaderboardData == null || _leaderboardData.characterLeaderboards == null)
            {
                return 0;
            }

            int bestScore = 0;
            foreach (var leaderboard in _leaderboardData.characterLeaderboards.Values)
            {
                if (leaderboard.entries != null && leaderboard.entries.Count > 0)
                {
                    // entries는 이미 점수 높은 순으로 정렬되어 있음
                    int characterBestScore = leaderboard.entries[0].totalScore;
                    if (characterBestScore > bestScore)
                    {
                        bestScore = characterBestScore;
                    }
                }
            }

            return bestScore;
        }

        /// <summary>
        /// 처음 클리어 여부를 확인합니다.
        /// </summary>
        /// <param name="characterName">캐릭터 이름</param>
        /// <returns>처음 클리어 여부</returns>
        public bool IsFirstClear(string characterName)
        {
            var leaderboard = GetLeaderboard(characterName);
            if (leaderboard == null || leaderboard.entries == null || leaderboard.entries.Count == 0)
            {
                return true; // 리더보드가 비어있으면 처음 클리어
            }

            return leaderboard.totalClearCount == 0;
        }

        /// <summary>
        /// 리더보드 데이터 로드
        /// </summary>
        public async Task LoadLeaderboard()
        {
            try
            {
                if (!File.Exists(LeaderboardFilePath))
                {
                    GameLogger.LogInfo("[LeaderboardManager] 리더보드 파일이 존재하지 않습니다. 새 리더보드를 생성합니다.", GameLogger.LogCategory.Save);
                    _leaderboardData = new LeaderboardData();
                    return;
                }

                string jsonData = await File.ReadAllTextAsync(LeaderboardFilePath);
                if (string.IsNullOrEmpty(jsonData))
                {
                    GameLogger.LogWarning("[LeaderboardManager] 리더보드 파일이 비어있습니다", GameLogger.LogCategory.Save);
                    _leaderboardData = new LeaderboardData();
                    return;
                }

                // Dictionary는 JsonUtility로 직접 직렬화되지 않으므로, 커스텀 파싱 필요
                var tempData = JsonUtility.FromJson<LeaderboardDataWrapper>(jsonData);
                if (tempData != null)
                {
                    _leaderboardData = new LeaderboardData
                    {
                        version = tempData.version,
                        lastUpdatedTime = tempData.lastUpdatedTime
                    };

                    // 리스트를 Dictionary로 변환
                    if (tempData.characterLeaderboardsList != null)
                    {
                        foreach (var kv in tempData.characterLeaderboardsList)
                        {
                            _leaderboardData.characterLeaderboards[kv.characterName] = kv;
                        }
                    }
                }
                else
                {
                    _leaderboardData = new LeaderboardData();
                }

                GameLogger.LogInfo($"[LeaderboardManager] 리더보드 데이터 로드 완료: {_leaderboardData.characterLeaderboards.Count}개 캐릭터", GameLogger.LogCategory.Save);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[LeaderboardManager] 리더보드 데이터 로드 실패: {ex.Message}", GameLogger.LogCategory.Error);
                _leaderboardData = new LeaderboardData();
            }
        }

        /// <summary>
        /// 리더보드 데이터 저장
        /// </summary>
        public async Task SaveLeaderboard()
        {
            try
            {
                // Dictionary를 리스트로 변환하여 직렬화
                var wrapper = new LeaderboardDataWrapper
                {
                    version = _leaderboardData.version,
                    lastUpdatedTime = _leaderboardData.lastUpdatedTime,
                    characterLeaderboardsList = _leaderboardData.characterLeaderboards.Values.ToList()
                };

                string jsonData = JsonUtility.ToJson(wrapper, true);

                await File.WriteAllTextAsync(LeaderboardFilePath, jsonData);

                GameLogger.LogInfo($"[LeaderboardManager] 리더보드 데이터 저장 완료: {LeaderboardFilePath}", GameLogger.LogCategory.Save);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[LeaderboardManager] 리더보드 데이터 저장 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// JsonUtility 직렬화를 위한 래퍼 클래스
        /// </summary>
        [Serializable]
        private class LeaderboardDataWrapper
        {
            public int version;
            public string lastUpdatedTime;
            public List<CharacterLeaderboard> characterLeaderboardsList;
        }
    }
}

