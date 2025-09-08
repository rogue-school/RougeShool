using System.Collections.Generic;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 카드 순환 시스템 인터페이스입니다.
    /// Unused Storage ↔ Used Storage 간의 무한 순환을 관리합니다.
    /// </summary>
    public interface ICardCirculationSystem
    {
        /// <summary>
        /// 턴당 드로우할 카드 수 (기본값: 3)
        /// </summary>
        int CardsPerTurn { get; }

        /// <summary>
        /// 현재 Unused Storage의 카드 수
        /// </summary>
        int UnusedCardCount { get; }

        /// <summary>
        /// 현재 Used Storage의 카드 수
        /// </summary>
        int UsedCardCount { get; }

        /// <summary>
        /// 턴 시작 시 카드를 드로우합니다.
        /// </summary>
        /// <returns>드로우된 카드 리스트</returns>
        List<ISkillCard> DrawCardsForTurn();

        /// <summary>
        /// 사용한 카드를 Used Storage로 이동시킵니다.
        /// </summary>
        /// <param name="card">사용한 카드</param>
        void MoveCardToUsedStorage(ISkillCard card);

        /// <summary>
        /// 여러 카드를 Used Storage로 이동시킵니다.
        /// </summary>
        /// <param name="cards">사용한 카드들</param>
        void MoveCardsToUsedStorage(List<ISkillCard> cards);

        /// <summary>
        /// Used Storage가 비어있으면 Unused Storage로 순환시킵니다.
        /// </summary>
        void CirculateCardsIfNeeded();

        /// <summary>
        /// 카드 순환 시스템을 초기화합니다.
        /// </summary>
        /// <param name="initialCards">초기 카드 리스트</param>
        void Initialize(List<ISkillCard> initialCards);

        /// <summary>
        /// 카드 순환 시스템을 리셋합니다.
        /// </summary>
        void Reset();

        /// <summary>
        /// 현재 Unused Storage의 모든 카드를 반환합니다. (저장 시스템용)
        /// </summary>
        /// <returns>Unused Storage의 카드 리스트</returns>
        List<ISkillCard> GetUnusedCards();

        /// <summary>
        /// 현재 Used Storage의 모든 카드를 반환합니다. (저장 시스템용)
        /// </summary>
        /// <returns>Used Storage의 카드 리스트</returns>
        List<ISkillCard> GetUsedCards();

        /// <summary>
        /// Unused Storage에 카드들을 복원합니다. (저장 시스템용)
        /// </summary>
        /// <param name="cards">복원할 카드들</param>
        void RestoreUnusedCards(List<ISkillCard> cards);

        /// <summary>
        /// Used Storage에 카드들을 복원합니다. (저장 시스템용)
        /// </summary>
        /// <param name="cards">복원할 카드들</param>
        void RestoreUsedCards(List<ISkillCard> cards);
    }
}
