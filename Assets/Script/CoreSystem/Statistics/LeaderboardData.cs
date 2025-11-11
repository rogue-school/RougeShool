using System;
using System.Collections.Generic;
using UnityEngine;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Statistics
{
    /// <summary>
    /// 리더보드 항목 데이터
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        /// <summary>
        /// 세션 ID
        /// </summary>
        public string sessionId;

        /// <summary>
        /// 캐릭터 이름
        /// </summary>
        public string characterName;

        /// <summary>
        /// 총 점수
        /// </summary>
        public int totalScore;

        /// <summary>
        /// 진행도 점수
        /// </summary>
        public int progressScore;

        /// <summary>
        /// 턴 효율 점수
        /// </summary>
        public int turnEfficiencyScore;

        /// <summary>
        /// 데미지 효율 점수
        /// </summary>
        public int damageEfficiencyScore;

        /// <summary>
        /// Speed Run 보너스
        /// </summary>
        public int speedRunBonus;

        /// <summary>
        /// 플레이 시간 (초)
        /// </summary>
        public float playTimeSeconds;

        /// <summary>
        /// 클리어 시간 (ISO 8601 형식)
        /// </summary>
        public string clearTime;

        /// <summary>
        /// 최종 스테이지 번호
        /// </summary>
        public int finalStageNumber;

        /// <summary>
        /// 최종 적 인덱스
        /// </summary>
        public int finalEnemyIndex;

        public LeaderboardEntry() { }

        public LeaderboardEntry(SessionStatisticsData sessionData, ScoreData scoreData)
        {
            if (sessionData == null || scoreData == null)
            {
                GameLogger.LogError("[LeaderboardEntry] 세션 데이터 또는 점수 데이터가 null입니다", GameLogger.LogCategory.Error);
                return;
            }

            sessionId = sessionData.sessionId;
            characterName = sessionData.selectedCharacterName;
            totalScore = scoreData.totalScore;
            progressScore = scoreData.progressScore;
            turnEfficiencyScore = scoreData.turnEfficiencyScore;
            damageEfficiencyScore = scoreData.damageEfficiencyScore;
            speedRunBonus = scoreData.speedRunBonus;
            playTimeSeconds = sessionData.totalPlayTimeSeconds;
            clearTime = sessionData.gameEndTime;
            finalStageNumber = sessionData.finalStageNumber;
            finalEnemyIndex = sessionData.finalEnemyIndex;
        }
    }

    /// <summary>
    /// 캐릭터별 리더보드 데이터
    /// </summary>
    [Serializable]
    public class CharacterLeaderboard
    {
        /// <summary>
        /// 캐릭터 이름
        /// </summary>
        public string characterName;

        /// <summary>
        /// 리더보드 항목 목록 (점수 높은 순으로 정렬)
        /// </summary>
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

        /// <summary>
        /// 총 클리어 횟수
        /// </summary>
        public int totalClearCount;

        public CharacterLeaderboard() { }

        public CharacterLeaderboard(string characterName)
        {
            this.characterName = characterName;
            this.entries = new List<LeaderboardEntry>();
            this.totalClearCount = 0;
        }
    }

    /// <summary>
    /// 전체 리더보드 데이터
    /// </summary>
    [Serializable]
    public class LeaderboardData
    {
        /// <summary>
        /// 버전 (데이터 구조 변경 시 사용)
        /// </summary>
        public int version = 1;

        /// <summary>
        /// 마지막 업데이트 시간 (ISO 8601 형식)
        /// </summary>
        public string lastUpdatedTime;

        /// <summary>
        /// 캐릭터별 리더보드
        /// </summary>
        public Dictionary<string, CharacterLeaderboard> characterLeaderboards = new Dictionary<string, CharacterLeaderboard>();

        /// <summary>
        /// 최대 저장 항목 수 (캐릭터당)
        /// </summary>
        public const int MAX_ENTRIES_PER_CHARACTER = 100;

        public LeaderboardData()
        {
            lastUpdatedTime = DateTime.UtcNow.ToString("o");
            characterLeaderboards = new Dictionary<string, CharacterLeaderboard>();
        }
    }
}

