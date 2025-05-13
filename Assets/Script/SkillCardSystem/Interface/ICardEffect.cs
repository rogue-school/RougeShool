using Game.CharacterSystem.Core;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 카드 효과를 정의하는 인터페이스입니다.
    /// 모든 효과는 이 인터페이스를 구현해야 합니다.
    /// </summary>
    public interface ICardEffect
    {
        /// <summary>
        /// 효과를 실행합니다.
        /// </summary>
        /// <param name="caster">시전자</param>
        /// <param name="target">대상</param>
        /// <param name="value">효과 수치 (예: 공격력)</param>
        void ExecuteEffect(CharacterBase caster, CharacterBase target, int value);
    }
}
