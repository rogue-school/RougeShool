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
        /// 기본 점수 (시작 점수)
        /// </summary>
        private const int BASE_SCORE = 10000;

        /// <summary>
        /// 턴당 차감 점수
        /// </summary>
        private const int TURN_PENALTY_PER_TURN = 100;

        /// <summary>
        /// 데미지당 차감 점수 (받은 데미지 1당 차감)
        /// </summary>
        private const int DAMAGE_PENALTY_PER_DAMAGE = 10;

        /// <summary>
        /// 회복량당 차감 점수 (회복량 1당 차감)
        /// </summary>
        private const int HEALING_PENALTY_PER_HEAL = 5;

        /// <summary>
        /// 사용한 엑티브 아이템당 차감 점수 (아이템 1개당 차감)
        /// </summary>
        private const int ACTIVE_ITEM_USE_PENALTY_PER_ITEM = 25;

        /// <summary>
        /// 자원 획득당 차감 점수 (자원 1당 차감) - 사용 안 함
        /// </summary>
        private const float RESOURCE_GAINED_PENALTY_PER_RESOURCE = 0f;

        /// <summary>
        /// 최소 점수 (0점 이하로 내려가지 않음)
        /// </summary>
        private const int MIN_SCORE = 0;

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
            breakdown.totalHealing = sessionData.summary?.totalHealing ?? 0;
            breakdown.playTimeSeconds = sessionData.totalPlayTimeSeconds;

            // 디버그: summary 값 확인
            GameLogger.LogInfo($"[ScoreCalculator] 점수 계산 시작 - summary: 총턴수={breakdown.totalTurns}, 총데미지={breakdown.totalDamageDealt}, 총받은데미지={breakdown.totalDamageTaken}, 총힐={breakdown.totalHealing}, 아이템사용수={sessionData.activeItemUseCountByName?.Count ?? 0}", GameLogger.LogCategory.UI);

            // 기본 점수에서 시작
            int finalScore = BASE_SCORE;

            // 1. 턴 수 차감
            int turnPenalty = CalculateTurnPenalty(breakdown.totalTurns);
            finalScore -= turnPenalty;
            scoreData.turnEfficiencyScore = -turnPenalty; // 차감량을 음수로 표시

            // 2. 받은 데미지 차감
            int damagePenalty = CalculateDamagePenalty(breakdown.totalDamageTaken);
            finalScore -= damagePenalty;
            scoreData.damageEfficiencyScore = -damagePenalty; // 차감량을 음수로 표시

            // 3. 회복량 차감
            int healingPenalty = CalculateHealingPenalty(breakdown.totalHealing);
            finalScore -= healingPenalty;
            scoreData.healthBonus = -healingPenalty; // 차감량을 음수로 표시

            // 4. 사용한 엑티브 아이템 차감
            int itemPenalty = CalculateActiveItemUsePenalty(sessionData);
            finalScore -= itemPenalty;
            scoreData.stageClearBonus = -itemPenalty; // 차감량을 음수로 표시

            // 5. 자원 획득량 차감
            int resourcePenalty = CalculateResourcePenalty(sessionData.totalResourceGained);
            finalScore -= resourcePenalty;
            scoreData.speedRunBonus = -resourcePenalty; // 차감량을 음수로 표시

            // 무패 보너스 제거 (사용 안 함)
            scoreData.noDamageBonus = 0;

            // 최소 점수 보장
            finalScore = Mathf.Max(finalScore, MIN_SCORE);
            scoreData.totalScore = finalScore;

            // 진행도 점수는 차감이 아닌 정보로만 표시
            scoreData.progressScore = breakdown.progressValue;

            GameLogger.LogInfo($"[ScoreCalculator] 점수 계산 완료: 총점={scoreData.totalScore} (기본={BASE_SCORE}, 턴차감={turnPenalty}, 데미지차감={damagePenalty}, 회복차감={healingPenalty}, 아이템차감={itemPenalty}, 자원차감={resourcePenalty})", GameLogger.LogCategory.UI);

            return scoreData;
        }

        /// <summary>
        /// 턴 수 차감을 계산합니다.
        /// </summary>
        /// <param name="totalTurns">총 턴 수</param>
        /// <returns>턴 수 차감량</returns>
        private static int CalculateTurnPenalty(int totalTurns)
        {
            if (totalTurns <= 0)
            {
                return 0;
            }

            int penalty = totalTurns * TURN_PENALTY_PER_TURN;
            GameLogger.LogInfo($"[ScoreCalculator] 턴 수 차감: {penalty} (총 턴: {totalTurns}, 턴당: {TURN_PENALTY_PER_TURN})", GameLogger.LogCategory.UI);
            return penalty;
        }

        /// <summary>
        /// 받은 데미지 차감을 계산합니다.
        /// </summary>
        /// <param name="totalDamageTaken">총 받은 데미지</param>
        /// <returns>데미지 차감량</returns>
        private static int CalculateDamagePenalty(int totalDamageTaken)
        {
            if (totalDamageTaken <= 0)
            {
                return 0;
            }

            int penalty = totalDamageTaken * DAMAGE_PENALTY_PER_DAMAGE;
            GameLogger.LogInfo($"[ScoreCalculator] 데미지 차감: {penalty} (받은 데미지: {totalDamageTaken}, 데미지당: {DAMAGE_PENALTY_PER_DAMAGE})", GameLogger.LogCategory.UI);
            return penalty;
        }

        /// <summary>
        /// 회복량 차감을 계산합니다.
        /// </summary>
        /// <param name="totalHealing">총 회복량</param>
        /// <returns>회복량 차감량</returns>
        private static int CalculateHealingPenalty(int totalHealing)
        {
            if (totalHealing <= 0)
            {
                return 0;
            }

            int penalty = totalHealing * HEALING_PENALTY_PER_HEAL;
            GameLogger.LogInfo($"[ScoreCalculator] 회복량 차감: {penalty} (총 회복량: {totalHealing}, 회복당: {HEALING_PENALTY_PER_HEAL})", GameLogger.LogCategory.UI);
            return penalty;
        }

        /// <summary>
        /// 사용한 엑티브 아이템 차감을 계산합니다.
        /// </summary>
        /// <param name="sessionData">세션 통계 데이터</param>
        /// <returns>아이템 차감량</returns>
        private static int CalculateActiveItemUsePenalty(SessionStatisticsData sessionData)
        {
            if (sessionData == null)
            {
                return 0;
            }

            int totalUsedItems = 0;

            // 사용한 엑티브 아이템 수 (이름별)
            if (sessionData.activeItemUseCountByName != null)
            {
                foreach (var count in sessionData.activeItemUseCountByName.Values)
                {
                    totalUsedItems += count;
                }
            }

            int penalty = totalUsedItems * ACTIVE_ITEM_USE_PENALTY_PER_ITEM;
            GameLogger.LogInfo($"[ScoreCalculator] 사용한 엑티브 아이템 차감: {penalty} (총 사용한 아이템: {totalUsedItems}, 아이템당: {ACTIVE_ITEM_USE_PENALTY_PER_ITEM})", GameLogger.LogCategory.UI);
            return penalty;
        }

        /// <summary>
        /// 자원 획득량 차감을 계산합니다.
        /// </summary>
        /// <param name="totalResourceGained">총 획득한 자원</param>
        /// <returns>자원 차감량</returns>
        private static int CalculateResourcePenalty(int totalResourceGained)
        {
            if (totalResourceGained <= 0)
            {
                return 0;
            }

            int penalty = Mathf.RoundToInt(totalResourceGained * RESOURCE_GAINED_PENALTY_PER_RESOURCE);
            GameLogger.LogInfo($"[ScoreCalculator] 자원 차감: {penalty} (총 획득 자원: {totalResourceGained}, 자원당: {RESOURCE_GAINED_PENALTY_PER_RESOURCE})", GameLogger.LogCategory.UI);
            return penalty;
        }

        /// <summary>
        /// 빈 점수 데이터를 생성합니다.
        /// </summary>
        /// <returns>빈 점수 데이터</returns>
        private static ScoreData CreateEmptyScore()
        {
            return new ScoreData
            {
                totalScore = BASE_SCORE,
                progressScore = 0,
                stageClearBonus = 0,
                turnEfficiencyScore = 0,
                damageEfficiencyScore = 0,
                noDamageBonus = 0,
                healthBonus = 0,
                speedRunBonus = 0,
                breakdown = new ScoreBreakdown()
            };
        }
    }
}

