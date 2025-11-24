using System;
using System.Collections.Generic;
using Game.Domain.Card.Interfaces;

namespace Game.Application.Card
{
    /// <summary>
    /// 덱에서 카드를 뽑아 손패 컬렉션으로 이동시키는 애플리케이션 유스케이스입니다.
    /// </summary>
    public sealed class DrawCardUseCase
    {
        /// <summary>
        /// 덱에서 한 장의 카드를 뽑아 손패에 추가합니다.
        /// </summary>
        /// <param name="deck">카드 덱 컬렉션</param>
        /// <param name="hand">손패 컬렉션</param>
        /// <returns>뽑은 카드 (없으면 null)</returns>
        /// <exception cref="ArgumentNullException">
        /// deck 또는 hand가 null인 경우
        /// </exception>
        public ISkillCard Execute(IList<ISkillCard> deck, IList<ISkillCard> hand)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck), "덱 컬렉션은 null일 수 없습니다.");
            }

            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand), "손패 컬렉션은 null일 수 없습니다.");
            }

            if (deck.Count == 0)
            {
                return null;
            }

            // 덱의 맨 위(마지막 인덱스)를 한 장 뽑는다.
            int lastIndex = deck.Count - 1;
            ISkillCard drawn = deck[lastIndex];
            deck.RemoveAt(lastIndex);
            hand.Add(drawn);

            return drawn;
        }
    }
}


