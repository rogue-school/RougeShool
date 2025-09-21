using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;

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

        [System.Serializable]
        public class RewardCard
        {
            [Header("보상 카드")]
            [Tooltip("보상으로 지급할 스킬카드 정의")]
            public SkillCardDefinition cardDefinition;
            [Tooltip("지급할 카드 수량")]
            [Range(1, 5)]
            public int quantity = 1;
        }

        #endregion

        #region 보상 데이터

        [Header("적 처치 보상")]
        [SerializeField] private List<RewardItem> enemyDefeatRewards = new();
        [SerializeField] private List<RewardCurrency> enemyDefeatCurrency = new();
        [SerializeField] private List<RewardCard> enemyDefeatCards = new();

        #endregion

        #region 프로퍼티

        /// <summary>
        /// 적 처치 보상 아이템
        /// </summary>
        public List<RewardItem> EnemyDefeatRewards => enemyDefeatRewards;

        /// <summary>
        /// 적 처치 보상 화폐
        /// </summary>
        public List<RewardCurrency> EnemyDefeatCurrency => enemyDefeatCurrency;

        /// <summary>
        /// 적 처치 보상 카드
        /// </summary>
        public List<RewardCard> EnemyDefeatCards => enemyDefeatCards;

        #endregion

        #region 보상 확인

        /// <summary>
        /// 적 처치 보상이 있는지 확인
        /// </summary>
        public bool HasEnemyDefeatRewards()
        {
            return enemyDefeatRewards.Count > 0 || enemyDefeatCurrency.Count > 0 || enemyDefeatCards.Count > 0;
        }

        #endregion

        #region 초기화 메서드

        /// <summary>
        /// 기본 적 처치 보상으로 초기화
        /// TODO: 향후 인벤토리 아이템 및 스킬카드 강화 시스템 구현 예정
        /// </summary>
        public void InitializeDefaultEnemyDefeatRewards()
        {
            // 현재는 보상 시스템이 구현되지 않음
            // 향후 인벤토리 아이템과 스킬카드 강화 시스템이 추가될 예정
            enemyDefeatRewards.Clear();
            enemyDefeatCurrency.Clear();
            enemyDefeatCards.Clear();
        }

        #endregion
    }
}
