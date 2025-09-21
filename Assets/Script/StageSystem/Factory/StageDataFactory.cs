using UnityEngine;
using Game.StageSystem.Data;

namespace Game.StageSystem.Factory
{
    /// <summary>
    /// 스테이지 데이터 생성 팩토리
    /// 로그 스쿨 시스템: 준보스/보스 구성 스테이지 생성
    /// </summary>
    public static class StageDataFactory
    {

        #region 보상 데이터 생성

        /// <summary>
        /// 기본 적 처치 보상 데이터 생성
        /// </summary>
        /// <returns>생성된 StageRewardData</returns>
        public static StageRewardData CreateDefaultRewards()
        {
            var rewardData = ScriptableObject.CreateInstance<StageRewardData>();
            rewardData.InitializeDefaultEnemyDefeatRewards();

            Debug.Log("[StageDataFactory] 기본 적 처치 보상 데이터 생성 완료");
            return rewardData;
        }

        #endregion

    }
}
