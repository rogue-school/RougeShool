using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICoolTimeHandler
    {
        /// <summary>
        /// 모든 카드의 현재 쿨타임을 1씩 감소시킵니다.
        /// </summary>
        void ReduceCoolTimes();
    }
}
