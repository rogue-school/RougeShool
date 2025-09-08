using System.Collections.Generic;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 턴 기반 카드 관리 인터페이스입니다.
    /// 카드 순환 시스템과 연동하여 턴별 카드 관리를 담당합니다.
    /// </summary>
    public interface ITurnBasedCardManager
    {
        /// <summary>
        /// 턴 시작 시 카드를 드로우합니다.
        /// </summary>
        void StartTurn();

        /// <summary>
        /// 턴 종료 시 사용하지 않은 카드들을 Used Storage로 이동시킵니다.
        /// </summary>
        void EndTurn();

        /// <summary>
        /// 현재 턴의 드로우된 카드들을 반환합니다.
        /// </summary>
        List<ISkillCard> GetCurrentTurnCards();

        /// <summary>
        /// 카드를 사용합니다. (Used Storage로 이동)
        /// </summary>
        /// <param name="card">사용할 카드</param>
        void UseCard(ISkillCard card);

        /// <summary>
        /// 턴이 시작되었는지 확인합니다.
        /// </summary>
        bool IsTurnStarted { get; }

        /// <summary>
        /// 현재 턴에서 사용 가능한 카드 수를 반환합니다.
        /// </summary>
        int AvailableCardsCount { get; }
    }
}
