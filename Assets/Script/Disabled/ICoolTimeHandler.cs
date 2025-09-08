using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 카드 쿨타임을 관리하는 핸들러 인터페이스입니다.
    /// 전투 턴이 끝날 때 카드들의 쿨타임을 감소시키는 역할을 수행합니다.
    /// </summary>
    public interface ICoolTimeHandler
    {
        /// <summary>
        /// 모든 쿨타임 적용 대상 카드의 현재 쿨타임을 1씩 감소시킵니다.
        /// 쿨타임이 0 이하가 되면 드래그 가능 상태 등으로 복원됩니다.
        /// </summary>
        void ReduceCoolTimes();
    }
}
