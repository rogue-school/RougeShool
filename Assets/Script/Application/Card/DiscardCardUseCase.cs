using System;
using System.Collections.Generic;
using Game.Domain.Card.Interfaces;

namespace Game.Application.Card
{
    /// <summary>
    /// 손패에서 카드를 버리거나, 버린 카드 더미(discard pile)에 추가하는 애플리케이션 유스케이스입니다.
    /// </summary>
    public sealed class DiscardCardUseCase
    {
        /// <summary>
        /// 손패에서 지정한 카드를 제거하고, 선택적으로 버린 카드 더미에 추가합니다.
        /// </summary>
        /// <param name="hand">손패 컬렉션</param>
        /// <param name="discardPile">버린 카드 더미 컬렉션 (null이면 버린 더미를 사용하지 않음)</param>
        /// <param name="card">버릴 카드</param>
        /// <exception cref="ArgumentNullException">
        /// hand 또는 card가 null인 경우
        /// </exception>
        public void Execute(
            IList<ISkillCard> hand,
            IList<ISkillCard> discardPile,
            ISkillCard card)
        {
            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand), "손패 컬렉션은 null일 수 없습니다.");
            }

            if (card == null)
            {
                throw new ArgumentNullException(nameof(card), "버릴 카드는 null일 수 없습니다.");
            }

            if (!hand.Contains(card))
            {
                return;
            }

            hand.Remove(card);

            if (discardPile != null)
            {
                discardPile.Add(card);
            }
        }
    }
}


