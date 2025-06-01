using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 턴별 카드 등록 및 조회를 위한 카드 레지스트리 인터페이스입니다.
    /// </summary>
    public interface ITurnCardRegistry
    {
        // 카드 등록
        void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card);
        void RegisterEnemyCard(ISkillCard card);

        // 카드 조회
        ISkillCard GetPlayerCard(CombatSlotPosition position);
        ISkillCard GetEnemyCard();

        // 카드 제거
        void ClearPlayerCard(CombatSlotPosition position);
        void ClearEnemyCard();
        void ClearSlot(CombatSlotPosition position); // 플레이어 슬롯만 정리 (편의 함수)

        // 턴 제어 상태
        CombatSlotPosition? GetReservedEnemySlot();
        void ReserveNextEnemySlot(CombatSlotPosition position);

        // 전체 리셋
        void Reset();
    }
}
