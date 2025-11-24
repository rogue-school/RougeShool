using System;
using Game.Domain.Character.Interfaces;

namespace Game.Application.Character
{
    /// <summary>
    /// 캐릭터에게 피해를 적용하는 애플리케이션 유스케이스입니다.
    /// 가드 무시 여부에 따라 도메인 캐릭터의 데미지 로직을 호출합니다.
    /// </summary>
    public sealed class TakeDamageUseCase
    {
        /// <summary>
        /// 캐릭터에게 피해를 적용합니다.
        /// </summary>
        /// <param name="target">피해를 받을 캐릭터</param>
        /// <param name="amount">피해량 (1 이상)</param>
        /// <param name="ignoreGuard">가드 상태를 무시하고 피해를 적용할지 여부</param>
        /// <exception cref="ArgumentNullException">target이 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">amount가 1 미만인 경우</exception>
        public void Execute(ICharacter target, int amount, bool ignoreGuard)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "피해를 적용할 캐릭터는 null일 수 없습니다.");
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "피해량은 1 이상이어야 합니다.");
            }

            if (ignoreGuard)
            {
                target.TakeDamageIgnoreGuard(amount);
            }
            else
            {
                target.TakeDamage(amount);
            }
        }
    }
}


