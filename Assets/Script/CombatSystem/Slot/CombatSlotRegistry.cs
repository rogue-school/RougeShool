using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Slot
{
    public class CombatSlotRegistry : MonoBehaviour, ICombatSlotRegistry
    {
        private readonly Dictionary<CombatSlotPosition, ICombatCardSlot> slotByPosition = new();
        private readonly Dictionary<CombatFieldSlotPosition, ICombatCardSlot> slotByFieldPosition = new();
        private readonly List<ICombatCardSlot> allSlots = new();

        public void RegisterCombatSlots(IEnumerable<ICombatCardSlot> slots)
        {
            slotByPosition.Clear();
            slotByFieldPosition.Clear();
            allSlots.Clear();

            foreach (var slot in slots)
            {
                if (slot is MonoBehaviour mb)
                {
                    var holder = mb.GetComponent<CombatSlotPositionHolder>();
                    if (holder == null)
                    {
                        Debug.LogWarning($"[CombatSlotRegistry] CombatSlotPositionHolder가 없습니다: {mb.name}");
                        continue;
                    }

                    slotByPosition[holder.SlotPosition] = slot;
                    slotByFieldPosition[holder.FieldSlotPosition] = slot;
                    allSlots.Add(slot);
                }
            }
        }

        public ICombatCardSlot GetCombatSlot(CombatSlotPosition position)
        {
            slotByPosition.TryGetValue(position, out var slot);
            return slot;
        }

        public ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition fieldPosition)
        {
            slotByFieldPosition.TryGetValue(fieldPosition, out var slot);
            return slot;
        }

        public IEnumerable<ICombatCardSlot> GetAllCombatSlots()
        {
            return allSlots;
        }

        // 아래 메서드는 기존 SlotRegistry의 간접 호출 대상입니다.
        public ICombatCardSlot GetSlotByPosition(CombatSlotPosition position)
        {
            return GetCombatSlot(position);
        }
    }
}
