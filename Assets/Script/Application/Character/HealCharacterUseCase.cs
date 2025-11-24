using System;
using Game.Domain.Character.Interfaces;

namespace Game.Application.Character
{
    /// <summary>
    /// 캐릭터의 체력을 회복시키는 애플리케이션 유스케이스입니다.
    /// </summary>
    public sealed class HealCharacterUseCase
    {
        /// <summary>
        /// 캐릭터의 체력을 회복시킵니다.
        /// </summary>
        /// <param name="target">회복할 캐릭터</param>
        /// <param name="amount">회복량 (1 이상)</param>
        /// <exception cref="ArgumentNullException">target이 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">amount가 1 미만인 경우</exception>
        public void Execute(ICharacter target, int amount)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "회복할 캐릭터는 null일 수 없습니다.");
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "회복량은 1 이상이어야 합니다.");
            }

            target.Heal(amount);
        }
    }
}


