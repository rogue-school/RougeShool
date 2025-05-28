using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using System.Collections.Generic;

public interface IHandSlotRegistry
{
    void RegisterHandSlots(IEnumerable<IHandCardSlot> slots);
    IHandCardSlot GetHandSlot(SkillCardSlotPosition position);
    IEnumerable<IHandCardSlot> GetAllHandSlots();
    IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner);

    // 플레이어 핸드 슬롯만 반환
    IEnumerable<IHandCardSlot> GetPlayerHandSlots() => GetHandSlots(SlotOwner.PLAYER);

    // 특정 슬롯 위치가 플레이어 소유인지 확인하고 반환
    IHandCardSlot GetPlayerHandSlot(SkillCardSlotPosition pos)
    {
        var slot = GetHandSlot(pos);
        return slot != null && slot.GetOwner() == SlotOwner.PLAYER ? slot : null;
    }
}
