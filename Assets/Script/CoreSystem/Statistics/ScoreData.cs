using System;
using UnityEngine;

namespace Game.CoreSystem.Statistics
{
    /// <summary>
    /// 점수 계산 결과 데이터
    /// </summary>
    [Serializable]
    public class ScoreData
    {
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
        /// 점수 계산 상세 정보
        /// </summary>
        public ScoreBreakdown breakdown = new ScoreBreakdown();
    }

    /// <summary>
    /// 점수 계산 상세 내역
    /// </summary>
    [Serializable]
    public class ScoreBreakdown
    {
        /// <summary>
        /// 진행도 (finalEnemyIndex)
        /// </summary>
        public int progressValue;

        /// <summary>
        /// 총 턴 수
        /// </summary>
        public int totalTurns;

        /// <summary>
        /// 총 가한 피해
        /// </summary>
        public int totalDamageDealt;

        /// <summary>
        /// 총 받은 피해
        /// </summary>
        public int totalDamageTaken;

        /// <summary>
        /// 플레이 시간 (초)
        /// </summary>
        public float playTimeSeconds;

        /// <summary>
        /// 데미지 효율 비율
        /// </summary>
        public float damageEfficiencyRatio;
    }
}





