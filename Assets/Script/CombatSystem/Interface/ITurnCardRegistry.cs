using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    public interface ITurnCardRegistry
    {
        // 카드 등록
        void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card);
        void RegisterEnemyCard(ISkillCard card);

        // 카드 등록 UI 포함 (CombatFlowCoordinator에서 사용됨)
        void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI cardUI); // 새로 추가됨

        // 카드 조회
        ISkillCard GetPlayerCard(CombatSlotPosition position);
        ISkillCard GetEnemyCard();

        // 카드 제거
        void ClearPlayerCard(CombatSlotPosition position);
        void ClearEnemyCard();
        void ClearSlot(CombatSlotPosition position); // 플레이어 슬롯만 정리

        // 턴 제어 상태
        CombatSlotPosition? GetReservedEnemySlot();
        void ReserveNextEnemySlot(CombatSlotPosition position);

        // 전체 리셋
        void Reset();
    }
}
