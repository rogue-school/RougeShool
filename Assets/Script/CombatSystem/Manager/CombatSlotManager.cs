using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.UI;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    public class CombatSlotManager : MonoBehaviour, ICombatSlotManager
    {
        private Dictionary<CombatSlotPosition, ICombatCardSlot> combatSlots = new();

        private void Awake()
        {
            AutoBindSlots();
        }

        private void AutoBindSlots()
        {
            combatSlots.Clear();

            var slotUIs = GetComponentsInChildren<CombatExecutionSlotUI>(true);
            foreach (var slot in slotUIs)
            {
                var pos = slot.GetCombatPosition();
                if (!combatSlots.ContainsKey(pos))
                {
                    combatSlots[pos] = slot;
                    Debug.Log($"[CombatSlotManager] 슬롯 등록 완료: {pos}");
                }
                else
                {
                    Debug.LogWarning($"[CombatSlotManager] 중복 슬롯 발견: {pos}");
                }
            }
        }

        public ICombatCardSlot GetSlot(CombatSlotPosition position)
        {
            combatSlots.TryGetValue(position, out var slot);
            return slot;
        }

        public void ClearAllSlots()
        {
            foreach (var slot in combatSlots.Values)
                slot.Clear();
        }

        public bool IsSlotEmpty(CombatSlotPosition position)
        {
            return !combatSlots.ContainsKey(position) || combatSlots[position].IsEmpty();
        }
    }
}
