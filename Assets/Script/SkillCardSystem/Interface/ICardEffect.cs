using Game.CharacterSystem.Core;
using Game.CombatSystem.Interface;

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
        /// <param name="controller">턴 상태 컨트롤러 (필요 시)</param>
        void ExecuteEffect(CharacterBase caster, CharacterBase target, int value, ITurnStateController controller = null);

        /// <summary>
        /// 효과의 이름을 반환합니다 (디버깅 및 UI용).
        /// </summary>
        string GetEffectName();

        /// <summary>
        /// 효과 설명을 반환합니다 (툴팁 등).
        /// </summary>
        string GetDescription();
    }
}
