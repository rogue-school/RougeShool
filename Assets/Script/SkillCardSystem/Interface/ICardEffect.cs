using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 카드 효과를 정의하는 인터페이스입니다.
    /// </summary>
    public interface ICardEffect
    {
        /// <summary>
        /// 효과를 실행합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트 (시전자, 대상 포함)</param>
        /// <param name="value">이펙트 수치 (예: 데미지)</param>
        /// <param name="controller">턴 상태 컨트롤러 (필요 시)</param>
        void ApplyEffect(ICardExecutionContext context, int value, ITurnStateController controller = null);

        /// <summary>
        /// 이펙트의 이름을 반환합니다.
        /// </summary>
        string GetEffectName();

        /// <summary>
        /// 이펙트 설명을 반환합니다.
        /// </summary>
        string GetDescription();
    }
}
