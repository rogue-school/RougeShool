using UnityEngine;
using System.Collections.Generic;

namespace Game.StageSystem.Data
{
    /// <summary>
    /// 스테이지 보상 데이터
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Stage/Stage Reward Data")]
    public class StageRewardData : ScriptableObject
    {
        #region 보상 타입

        [System.Serializable]
        public class RewardItem
        {
            [Header("보상 아이템")]
            public string itemName;
            public int quantity;
            public Sprite itemIcon;
        }

        [System.Serializable]
        public class RewardCurrency
        {
            [Header("보상 화폐")]
            public string currencyType;
            public int amount;
        }

        #endregion

        #region 보상 데이터

        [Header("준보스 보상")]
        [SerializeField] private List<RewardItem> subBossRewards = new();
        [SerializeField] private List<RewardCurrency> subBossCurrency = new();

        [Header("보스 보상")]
        [SerializeField] private List<RewardItem> bossRewards = new();
        [SerializeField] private List<RewardCurrency> bossCurrency = new();

        [Header("스테이지 완료 보상")]
        [SerializeField] private List<RewardItem> stageCompletionRewards = new();
        [SerializeField] private List<RewardCurrency> stageCompletionCurrency = new();

        #endregion

        #region 프로퍼티

        /// <summary>
        /// 준보스 보상 아이템
        /// </summary>
        public List<RewardItem> SubBossRewards => subBossRewards;

        /// <summary>
        /// 준보스 보상 화폐
        /// </summary>
        public List<RewardCurrency> SubBossCurrency => subBossCurrency;

        /// <summary>
        /// 보스 보상 아이템
        /// </summary>
        public List<RewardItem> BossRewards => bossRewards;

        /// <summary>
        /// 보스 보상 화폐
        /// </summary>
        public List<RewardCurrency> BossCurrency => bossCurrency;

        /// <summary>
        /// 스테이지 완료 보상 아이템
        /// </summary>
        public List<RewardItem> StageCompletionRewards => stageCompletionRewards;

        /// <summary>
        /// 스테이지 완료 보상 화폐
        /// </summary>
        public List<RewardCurrency> StageCompletionCurrency => stageCompletionCurrency;

        #endregion

        #region 보상 확인

        /// <summary>
        /// 준보스 보상이 있는지 확인
        /// </summary>
        public bool HasSubBossRewards()
        {
            return subBossRewards.Count > 0 || subBossCurrency.Count > 0;
        }

        /// <summary>
        /// 보스 보상이 있는지 확인
        /// </summary>
        public bool HasBossRewards()
        {
            return bossRewards.Count > 0 || bossCurrency.Count > 0;
        }

        /// <summary>
        /// 스테이지 완료 보상이 있는지 확인
        /// </summary>
        public bool HasStageCompletionRewards()
        {
            return stageCompletionRewards.Count > 0 || stageCompletionCurrency.Count > 0;
        }

        #endregion
    }
}
