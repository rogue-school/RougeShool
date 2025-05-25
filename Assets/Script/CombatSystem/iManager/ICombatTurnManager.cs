using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 턴 상태의 흐름을 제어하는 컨트롤러
    /// 상태 전이만을 책임짐 (SOLID: SRP)
    /// </summary>
    public interface ICombatTurnManager
    {
        void Initialize();

        /// <summary>
        /// 상태 팩토리를 주입합니다.
        /// </summary>
        void InjectFactory(ICombatStateFactory factory);

        /// <summary>
        /// 현재 상태에서 다음 상태로의 전이를 요청합니다.
        /// </summary>
        void RequestStateChange(ICombatTurnState nextState);

        /// <summary>
        /// 현재 전투 턴 상태를 반환합니다.
        /// </summary>
        ICombatTurnState GetCurrentState();

        /// <summary>
        /// 플레이어 카드 등록 (간단 버전)
        /// </summary>
        void RegisterPlayerCard(ISkillCard card);

        /// <summary>
        /// 적 카드 등록
        /// </summary>
        void RegisterEnemyCard(ISkillCard card);

        /// <summary>
        /// 플레이어 카드 등록 (슬롯 위치 지정 버전)
        /// </summary>
        void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card);
    }
}
