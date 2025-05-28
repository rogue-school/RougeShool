using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

public interface ITurnCardRegistry
{
    void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card);
    void RegisterEnemyCard(ISkillCard card);

    ISkillCard GetPlayerCard(CombatSlotPosition position);
    ISkillCard GetEnemyCard();

    void ClearPlayerCard(CombatSlotPosition position);
    void ClearEnemyCard();

    CombatSlotPosition? GetReservedEnemySlot();
    void ReserveNextEnemySlot(CombatSlotPosition position);
    void Reset();

    // 누락된 메서드 추가
    void ClearSlot(CombatSlotPosition position); // 편의 함수로 정의
}
