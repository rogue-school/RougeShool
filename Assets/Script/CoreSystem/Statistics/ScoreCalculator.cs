using System;
using UnityEngine;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Statistics
{
    /// <summary>
    /// 게임 세션 통계를 기반으로 점수를 계산하는 클래스입니다.
    /// </summary>
    public static class ScoreCalculator
    {
        #region 상수

        /// <summary>
        /// 진행도 점수 배율 (적 인덱스당 점수)
        /// </summary>
        private const int PROGRESS_SCORE_MULTIPLIER = 100;

        /// <summary>
        /// 턴 효율 점수 배율
        /// </summary>
        private const int TURN_EFFICIENCY_MULTIPLIER = 2;

        /// <summary>
        /// 턴 효율 최대 점수
        /// </summary>
        private const int MAX_TURN_EFFICIENCY_SCORE = 200;

        /// <summary>
        /// 턴 효율 기준 턴 수 (이 턴 수 이상이면 0점)
        /// </summary>
        private const int TURN_EFFICIENCY_BASE = 100;

        /// <summary>
        /// 데미지 효율 점수 배율
        /// </summary>
        private const int DAMAGE_EFFICIENCY_MULTIPLIER = 10;

        /// <summary>
        /// 데미지 효율 최대 점수
        /// </summary>
        private const int MAX_DAMAGE_EFFICIENCY_SCORE = 100;

        /// <summary>
        /// Speed Run 보너스 기준 시간 (초)
        /// </summary>
        private const float SPEED_RUN_TIER_1 = 600f;  // 10분
        private const float SPEED_RUN_TIER_2 = 900f;  // 15분
        private const float SPEED_RUN_TIER_3 = 1200f; // 20분

        /// <summary>
        /// Speed Run 보너스 점수
        /// </summary>
        private const int SPEED_RUN_BONUS_TIER_1 = 100;
        private const int SPEED_RUN_BONUS_TIER_2 = 50;
        private const int SPEED_RUN_BONUS_TIER_3 = 25;

        #endregion

        /// <summary>
        /// 세션 통계 데이터를 기반으로 점수를 계산합니다.
        /// </summary>
        /// <param name="sessionData">세션 통계 데이터</param>
        /// <returns>계산된 점수 데이터</returns>
        public static ScoreData CalculateScore(SessionStatisticsData sessionData)
        {
            if (sessionData == null)
            {
                GameLogger.LogError("[ScoreCalculator] 세션 데이터가 null입니다", GameLogger.LogCategory.Error);
                return CreateEmptyScore();
            }

            var scoreData = new ScoreData();
            var breakdown = scoreData.breakdown;

            // 기본 정보 수집
            breakdown.progressValue = sessionData.finalEnemyIndex;
            breakdown.totalTurns = sessionData.summary?.totalTurns ?? 0;
            breakdown.totalDamageDealt = sessionData.summary?.totalDamageDealt ?? 0;
            breakdown.totalDamageTaken = sessionData.summary?.totalDamageTaken ?? 0;
            breakdown.playTimeSeconds = sessionData.totalPlayTimeSeconds;

            // 1. 진행도 점수 계산
            scoreData.progressScore = CalculateProgressScore(breakdown.progressValue);

            // 2. 턴 효율 점수 계산
            scoreData.turnEfficiencyScore = CalculateTurnEfficiencyScore(breakdown.totalTurns);

            // 3. 데미지 효율 점수 계산
            scoreData.damageEfficiencyScore = CalculateDamageEfficiencyScore(
                breakdown.totalDamageDealt,
                breakdown.totalDamageTaken,
                out breakdown.damageEfficiencyRatio);

            // 4. Speed Run 보너스 계산
            scoreData.speedRunBonus = CalculateSpeedRunBonus(breakdown.playTimeSeconds);

            // 총 점수 계산
            scoreData.totalScore = scoreData.progressScore
                + scoreData.turnEfficiencyScore
                + scoreData.damageEfficiencyScore
                + scoreData.speedRunBonus;

            GameLogger.LogInfo($"[ScoreCalculator] 점수 계산 완료: 총점={scoreData.totalScore} (진행도={scoreData.progressScore}, 턴효율={scoreData.turnEfficiencyScore}, 데미지효율={scoreData.damageEfficiencyScore}, SpeedRun={scoreData.speedRunBonus})", GameLogger.LogCategory.UI);

            return scoreData;
        }

        /// <summary>
        /// 진행도 점수를 계산합니다.
        /// </summary>
        /// <param name="enemyIndex">최종 적 인덱스</param>
        /// <returns>진행도 점수</returns>
        private static int CalculateProgressScore(int enemyIndex)
        {
            // 적 인덱스는 0부터 시작하므로 +1하여 실제 처치한 적 수로 계산
            int defeatedEnemies = Mathf.Max(0, enemyIndex + 1);
            int score = defeatedEnemies * PROGRESS_SCORE_MULTIPLIER;
            
            GameLogger.LogInfo($"[ScoreCalculator] 진행도 점수: {score} (처치한 적: {defeatedEnemies})", GameLogger.LogCategory.UI);
            return score;
        }

        /// <summary>
        /// 턴 효율 점수를 계산합니다.
        /// </summary>
        /// <param name="totalTurns">총 턴 수</param>
        /// <returns>턴 효율 점수</returns>
        private static int CalculateTurnEfficiencyScore(int totalTurns)
        {
            if (totalTurns <= 0)
            {
                return 0;
            }

            // 턴이 적을수록 높은 점수
            // 기준: 100턴 이하일 때만 점수 부여
            int score = Mathf.Max(0, (TURN_EFFICIENCY_BASE - totalTurns) * TURN_EFFICIENCY_MULTIPLIER);
            score = Mathf.Min(score, MAX_TURN_EFFICIENCY_SCORE);

            GameLogger.LogInfo($"[ScoreCalculator] 턴 효율 점수: {score} (총 턴: {totalTurns})", GameLogger.LogCategory.UI);
            return score;
        }

        /// <summary>
        /// 데미지 효율 점수를 계산합니다.
        /// </summary>
        /// <param name="damageDealt">가한 피해</param>
        /// <param name="damageTaken">받은 피해</param>
        /// <param name="efficiencyRatio">데미지 효율 비율 (출력)</param>
        /// <returns>데미지 효율 점수</returns>
        private static int CalculateDamageEfficiencyScore(int damageDealt, int damageTaken, out float efficiencyRatio)
        {
            if (damageDealt <= 0)
            {
                efficiencyRatio = 0f;
                return 0;
            }

            // 받은 피해가 0이면 최대 점수
            if (damageTaken <= 0)
            {
                efficiencyRatio = float.MaxValue;
                int maxScore = MAX_DAMAGE_EFFICIENCY_SCORE;
                GameLogger.LogInfo($"[ScoreCalculator] 데미지 효율 점수: {maxScore} (가한 피해: {damageDealt}, 받은 피해: 0)", GameLogger.LogCategory.UI);
                return maxScore;
            }

            // 효율 비율 = 가한 피해 / 받은 피해
            efficiencyRatio = (float)damageDealt / damageTaken;
            
            // 효율 비율에 비례하여 점수 계산 (최대 100점)
            int score = Mathf.Min((int)(efficiencyRatio * DAMAGE_EFFICIENCY_MULTIPLIER), MAX_DAMAGE_EFFICIENCY_SCORE);

            GameLogger.LogInfo($"[ScoreCalculator] 데미지 효율 점수: {score} (가한 피해: {damageDealt}, 받은 피해: {damageTaken}, 효율: {efficiencyRatio:F2})", GameLogger.LogCategory.UI);
            return score;
        }

        /// <summary>
        /// Speed Run 보너스를 계산합니다.
        /// </summary>
        /// <param name="playTimeSeconds">플레이 시간 (초)</param>
        /// <returns>Speed Run 보너스 점수</returns>
        private static int CalculateSpeedRunBonus(float playTimeSeconds)
        {
            if (playTimeSeconds <= 0)
            {
                return 0;
            }

            int bonus = 0;
            if (playTimeSeconds <= SPEED_RUN_TIER_1)
            {
                bonus = SPEED_RUN_BONUS_TIER_1;
            }
            else if (playTimeSeconds <= SPEED_RUN_TIER_2)
            {
                bonus = SPEED_RUN_BONUS_TIER_2;
            }
            else if (playTimeSeconds <= SPEED_RUN_TIER_3)
            {
                bonus = SPEED_RUN_BONUS_TIER_3;
            }

            if (bonus > 0)
            {
                GameLogger.LogInfo($"[ScoreCalculator] Speed Run 보너스: {bonus} (플레이 시간: {playTimeSeconds:F1}초)", GameLogger.LogCategory.UI);
            }

            return bonus;
        }

        /// <summary>
        /// 빈 점수 데이터를 생성합니다.
        /// </summary>
        /// <returns>빈 점수 데이터</returns>
        private static ScoreData CreateEmptyScore()
        {
            return new ScoreData
            {
                totalScore = 0,
                progressScore = 0,
                turnEfficiencyScore = 0,
                damageEfficiencyScore = 0,
                speedRunBonus = 0,
                breakdown = new ScoreBreakdown()
            };
        }
    }
}

