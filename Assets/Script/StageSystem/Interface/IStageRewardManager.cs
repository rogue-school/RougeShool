using Game.StageSystem.Data;

namespace Game.StageSystem.Interface
{
    /// <summary>
    /// 스테이지 보상 관리 인터페이스
    /// </summary>
    public interface IStageRewardManager
    {
        #region 보상 지급

        /// <summary>
        /// 준보스 보상 지급
        /// </summary>
        void GiveSubBossRewards();

        /// <summary>
        /// 보스 보상 지급
        /// </summary>
        void GiveBossRewards();

        /// <summary>
        /// 스테이지 완료 보상 지급
        /// </summary>
        void GiveStageCompletionRewards();

        /// <summary>
        /// 특정 보상 데이터로 보상 지급
        /// </summary>
        /// <param name="rewards">지급할 보상 데이터</param>
        void GiveRewards(StageRewardData rewards);

        #endregion

        #region 보상 확인

        /// <summary>
        /// 적 처치 보상이 있는지 확인
        /// </summary>
        bool HasEnemyDefeatRewards();

        #endregion

        #region 보상 데이터

        /// <summary>
        /// 현재 스테이지의 보상 데이터 설정
        /// </summary>
        /// <param name="rewards">보상 데이터</param>
        void SetCurrentRewards(StageRewardData rewards);

        /// <summary>
        /// 현재 스테이지의 보상 데이터 가져오기
        /// </summary>
        /// <returns>현재 보상 데이터</returns>
        StageRewardData GetCurrentRewards();

        #endregion

        #region 이벤트

        /// <summary>
        /// 보상 지급 이벤트
        /// </summary>
        event System.Action<StageRewardData.RewardItem> OnItemRewardGiven;

        /// <summary>
        /// 화폐 보상 지급 이벤트
        /// </summary>
        event System.Action<StageRewardData.RewardCurrency> OnCurrencyRewardGiven;

        #endregion
    }
}
