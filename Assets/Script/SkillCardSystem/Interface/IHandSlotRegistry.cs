using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using System.Collections.Generic;

/// <summary>
/// 핸드 슬롯을 관리하는 레지스트리 인터페이스입니다.
/// 플레이어와 적의 카드 핸드 슬롯을 등록, 조회할 수 있습니다.
/// </summary>
public interface IHandSlotRegistry
{
    /// <summary>
    /// 핸드 슬롯들을 레지스트리에 등록합니다.
    /// </summary>
    /// <param name="slots">등록할 핸드 슬롯 컬렉션</param>
    void RegisterHandSlots(IEnumerable<IHandCardSlot> slots);

    /// <summary>
    /// 지정된 슬롯 위치에 해당하는 핸드 슬롯을 반환합니다.
    /// </summary>
    /// <param name="position">슬롯 위치 열거형</param>
    /// <returns>해당 위치의 핸드 슬롯</returns>
    IHandCardSlot GetHandSlot(SkillCardSlotPosition position);

    /// <summary>
    /// 등록된 모든 핸드 슬롯을 반환합니다.
    /// </summary>
    IEnumerable<IHandCardSlot> GetAllHandSlots();

    /// <summary>
    /// 지정된 소유자(SlotOwner)에 해당하는 핸드 슬롯들만 반환합니다.
    /// </summary>
    /// <param name="owner">플레이어 또는 적</param>
    IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner);

    /// <summary>
    /// 플레이어의 핸드 슬롯만 반환합니다.
    /// </summary>
    /// <returns>플레이어 핸드 슬롯 목록</returns>
    IEnumerable<IHandCardSlot> GetPlayerHandSlots() => GetHandSlots(SlotOwner.PLAYER);

    /// <summary>
    /// 지정된 위치가 플레이어 핸드 슬롯이면 반환하고, 아니면 null을 반환합니다.
    /// </summary>
    /// <param name="pos">슬롯 위치</param>
    /// <returns>플레이어 핸드 슬롯 또는 null</returns>
    IHandCardSlot GetPlayerHandSlot(SkillCardSlotPosition pos)
    {
        var slot = GetHandSlot(pos);
        return slot != null && slot.GetOwner() == SlotOwner.PLAYER ? slot : null;
    }
}
