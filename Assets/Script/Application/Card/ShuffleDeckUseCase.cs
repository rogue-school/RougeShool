using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Game.Domain.Card.Interfaces;

namespace Game.Application.Card
{
    /// <summary>
    /// 카드 덱을 무작위로 섞는 애플리케이션 유스케이스입니다.
    /// Fisher-Yates 알고리즘을 사용하여 컬렉션을 인플레이스 섞습니다.
    /// </summary>
    public sealed class ShuffleDeckUseCase
    {
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        /// <summary>
        /// 주어진 덱 컬렉션을 인플레이스 방식으로 섞습니다.
        /// </summary>
        /// <param name="deck">섞을 덱 컬렉션</param>
        /// <exception cref="ArgumentNullException">deck이 null인 경우</exception>
        public void Execute(IList<ISkillCard> deck)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck), "덱 컬렉션은 null일 수 없습니다.");
            }

            int count = deck.Count;
            if (count <= 1)
            {
                return;
            }

            byte[] buffer = new byte[4];

            for (int i = count - 1; i > 0; i--)
            {
                _rng.GetBytes(buffer);
                int rand = BitConverter.ToInt32(buffer, 0) & int.MaxValue;
                int j = rand % (i + 1);

                if (i == j)
                {
                    continue;
                }

                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }
    }
}


