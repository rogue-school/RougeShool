using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 카드 효과를 정의하는 인터페이스입니다.
    /// </summary>
    public interface ICardEffect
    {
        /// <summary>
        /// 카드 효과를 실행합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트. 시전자, 대상 등의 정보를 포함합니다.</param>
        /// <param name="value">이펙트 수치 (예: 피해량, 회복량 등).</param>
        /// <param name="turnManager">필요한 경우 전투 턴 관리자. (예: 가드 처리 등, 선택적)</param>
        void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null);

        /// <summary>
        /// 이펙트의 이름을 반환합니다.
        /// </summary>
        /// <returns>이펙트 이름.</returns>
        string GetEffectName();

        /// <summary>
        /// 이펙트의 설명을 반환합니다.
        /// </summary>
        /// <returns>이펙트 설명 문자열.</returns>
        string GetDescription();
    }
}
