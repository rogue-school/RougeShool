using System.Collections.Generic;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 카드 순환 시스템 인터페이스입니다.
    /// 플레이어 덱에서 랜덤하게 카드를 드로우하는 시스템입니다.
    /// </summary>
    public interface ICardCirculationSystem
    {
        /// <summary>
        /// 턴당 드로우할 카드 수 (기본값: 3)
        /// </summary>
        int CardsPerTurn { get; }

        /// <summary>
        /// 현재 덱의 카드 수
        /// </summary>
        int DeckCardCount { get; }

        /// <summary>
        /// 턴 시작 시 카드를 드로우합니다.
        /// </summary>
        /// <returns>드로우된 카드 리스트</returns>
        List<ISkillCard> DrawCardsForTurn();

        /// <summary>
        /// 카드 순환 시스템을 초기화합니다.
        /// </summary>
        /// <param name="initialCards">초기 카드 리스트</param>
        void Initialize(List<ISkillCard> initialCards);

        /// <summary>
        /// 카드 순환 시스템을 리셋합니다.
        /// </summary>
        void Clear();

        #region 보상 관리

        /// <summary>
        /// 적 캐릭터 처치 보상 카드를 지급합니다.
        /// </summary>
        /// <param name="rewardData">보상 데이터</param>
        void GiveEnemyDefeatCardRewards(Game.StageSystem.Data.StageRewardData rewardData);

        /// <summary>
        /// 특정 카드를 보상으로 지급합니다.
        /// </summary>
        /// <param name="cardDefinition">지급할 카드 정의</param>
        /// <param name="quantity">지급할 수량</param>
        /// <returns>지급 성공 여부</returns>
        bool GiveCardReward(Game.SkillCardSystem.Data.SkillCardDefinition cardDefinition, int quantity = 1);

        #endregion
    }
}