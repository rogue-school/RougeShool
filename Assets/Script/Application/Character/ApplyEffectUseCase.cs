using System;
using Game.Domain.Card.Interfaces;
using Game.Domain.Character.Interfaces;

namespace Game.Application.Character
{
    /// <summary>
    /// 캐릭터에게 카드 효과를 적용하는 애플리케이션 유스케이스입니다.
    /// 구체적인 효과 실행 로직은 상위 레이어에서 제공하는 델리게이트에 위임합니다.
    /// </summary>
    public sealed class ApplyEffectUseCase
    {
        /// <summary>
        /// 지정된 카드 효과를 대상 캐릭터에 적용합니다.
        /// </summary>
        /// <param name="target">효과를 적용할 캐릭터</param>
        /// <param name="effect">적용할 카드 효과 메타데이터</param>
        /// <param name="applyAction">실제 효과를 적용하는 델리게이트</param>
        /// <exception cref="ArgumentNullException">
        /// target, effect 또는 applyAction이 null인 경우
        /// </exception>
        public void Execute(
            ICharacter target,
            ICardEffect effect,
            Action<ICharacter, ICardEffect> applyAction)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "효과를 적용할 캐릭터는 null일 수 없습니다.");
            }

            if (effect == null)
            {
                throw new ArgumentNullException(nameof(effect), "적용할 카드 효과는 null일 수 없습니다.");
            }

            if (applyAction == null)
            {
                throw new ArgumentNullException(nameof(applyAction), "효과를 적용할 델리게이트는 null일 수 없습니다.");
            }

            applyAction(target, effect);
        }
    }
}


