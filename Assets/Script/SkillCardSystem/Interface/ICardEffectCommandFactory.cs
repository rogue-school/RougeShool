using Game.SkillCardSystem.Effects;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// SkillCardEffectSO를 기반으로 ICardEffectCommand 인스턴스를 생성하는 팩토리 인터페이스입니다.
    /// </summary>
    public interface ICardEffectCommandFactory
    {
        /// <summary>
        /// 지정된 이펙트 스크립터블 오브젝트와 파워 값을 기반으로 커맨드를 생성합니다.
        /// </summary>
        /// <param name="effect">적용할 카드 이펙트 데이터 (SkillCardEffectSO).</param>
        /// <param name="power">이펙트의 적용 수치 또는 강도.</param>
        /// <returns>생성된 이펙트 커맨드. 실패 시 null 반환 가능.</returns>
        ICardEffectCommand Create(SkillCardEffectSO effect, int power);
    }
}
