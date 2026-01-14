using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 효과 커맨드 생성 전략 인터페이스입니다.
    /// 각 효과 타입별로 커맨드 생성 로직을 캡슐화합니다.
    /// </summary>
    public interface IEffectCommandStrategy
    {
        /// <summary>
        /// 효과 설정으로부터 커맨드를 생성합니다.
        /// </summary>
        /// <param name="config">효과 설정</param>
        /// <returns>생성된 커맨드 (실패 시 null)</returns>
        ICardEffectCommand CreateCommand(EffectConfiguration config);

        /// <summary>
        /// 이 전략이 처리 가능한 효과 타입인지 확인합니다.
        /// </summary>
        /// <param name="effectSO">확인할 효과 SO</param>
        /// <returns>처리 가능하면 true</returns>
        bool CanHandle(SkillCardEffectSO effectSO);
    }
}
