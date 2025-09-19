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

        #region 레거시 호환성 (보관함 시스템 제거됨)

        /// <summary>
        /// 사용한 카드를 처리합니다. (보관함 시스템 제거됨)
        /// </summary>
        /// <param name="card">사용한 카드</param>
        void MoveCardToUsedStorage(ISkillCard card);

        /// <summary>
        /// 여러 카드를 처리합니다. (보관함 시스템 제거됨)
        /// </summary>
        /// <param name="cards">사용한 카드들</param>
        void MoveCardsToUsedStorage(List<ISkillCard> cards);

        /// <summary>
        /// 카드 순환을 처리합니다. (보관함 시스템 제거됨)
        /// </summary>
        void CirculateCardsIfNeeded();

        /// <summary>
        /// 현재 미사용 카드들을 반환합니다. (저장 시스템용, 빈 리스트 반환)
        /// </summary>
        /// <returns>빈 카드 리스트</returns>
        List<ISkillCard> GetUnusedCards();

        /// <summary>
        /// 현재 사용된 카드들을 반환합니다. (저장 시스템용, 빈 리스트 반환)
        /// </summary>
        /// <returns>빈 카드 리스트</returns>
        List<ISkillCard> GetUsedCards();

        /// <summary>
        /// 미사용 카드들을 복원합니다. (저장 시스템용, 아무것도 하지 않음)
        /// </summary>
        /// <param name="cards">복원할 카드들</param>
        void RestoreUnusedCards(List<ISkillCard> cards);

        /// <summary>
        /// 사용된 카드들을 복원합니다. (저장 시스템용, 아무것도 하지 않음)
        /// </summary>
        /// <param name="cards">복원할 카드들</param>
        void RestoreUsedCards(List<ISkillCard> cards);

        #endregion

        #region 핸드 관리

        /// <summary>
        /// 카드를 핸드로 이동시킵니다.
        /// </summary>
        /// <param name="card">이동할 카드</param>
        void MoveCardToHand(ISkillCard card);

        /// <summary>
        /// 카드를 버린 카드 더미로 이동시킵니다.
        /// </summary>
        /// <param name="card">이동할 카드</param>
        void MoveCardToDiscard(ISkillCard card);

        /// <summary>
        /// 카드를 소멸 더미로 이동시킵니다.
        /// </summary>
        /// <param name="card">이동할 카드</param>
        void MoveCardToExhaust(ISkillCard card);

        #endregion
    }
}