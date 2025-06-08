using Game.CharacterSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 캐릭터에게 매 턴마다 적용되는 지속 효과의 인터페이스입니다.
    /// 예: 출혈, 중독, 회복, 방어력 증가 등.
    /// </summary>
    public interface IPerTurnEffect
    {
        /// <summary>
        /// 해당 효과가 만료되어 제거되어야 하는지 여부를 반환합니다.
        /// true인 경우, 다음 턴 처리 시 리스트에서 제거됩니다.
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// 턴 시작 시 효과를 적용합니다.
        /// 예: 체력 감소, 버프 지속 시간 감소 등.
        /// </summary>
        /// <param name="target">이 효과가 적용될 캐릭터</param>
        void OnTurnStart(ICharacter target);
    }
}
