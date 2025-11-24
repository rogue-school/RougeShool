using System;
using Game.Domain.Card.Interfaces;
using Game.Domain.Character.Interfaces;

namespace Game.Application.Card
{
    /// <summary>
    /// 손패에서 카드를 사용하여 대상에게 효과를 적용하는 애플리케이션 유스케이스입니다.
    /// 실제 효과 적용은 상위 레이어에서 제공하는 델리게이트에 위임합니다.
    /// </summary>
    public sealed class PlayCardUseCase
    {
        /// <summary>
        /// 카드를 사용하여 대상에게 효과를 적용합니다.
        /// </summary>
        /// <param name="card">사용할 카드</param>
        /// <param name="source">카드를 사용하는 캐릭터</param>
        /// <param name="target">카드 효과를 적용할 대상 캐릭터</param>
        /// <param name="applyEffect">카드 효과를 실제로 적용하는 델리게이트</param>
        /// <exception cref="ArgumentNullException">
        /// card, source, target 또는 applyEffect가 null인 경우
        /// </exception>
        public void Execute(
            ISkillCard card,
            ICharacter source,
            ICharacter target,
            Action<ISkillCard, ICharacter, ICharacter> applyEffect)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card), "사용할 카드는 null일 수 없습니다.");
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "카드를 사용하는 캐릭터는 null일 수 없습니다.");
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "카드 효과를 적용할 대상은 null일 수 없습니다.");
            }

            if (applyEffect == null)
            {
                throw new ArgumentNullException(nameof(applyEffect), "카드 효과를 적용할 델리게이트는 null일 수 없습니다.");
            }

            applyEffect(card, source, target);
        }
    }
}


