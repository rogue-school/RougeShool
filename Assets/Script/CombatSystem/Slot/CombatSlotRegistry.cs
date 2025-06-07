using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using System.Linq;

namespace Game.CombatSystem.Slot
{
    public class CombatSlotRegistry : MonoBehaviour, ICombatSlotRegistry
    {
        private readonly Dictionary<CombatSlotPosition, ICombatCardSlot> _slotByPosition = new();
        private readonly Dictionary<CombatFieldSlotPosition, ICombatCardSlot> _slotByFieldPosition = new();
        private readonly List<ICombatCardSlot> _allSlots = new();

        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;


        public void RegisterCombatSlots(IEnumerable<ICombatCardSlot> slots)
        {
            _slotByPosition.Clear();
            _slotByFieldPosition.Clear();
            _allSlots.Clear();

            int registeredCount = 0;

            foreach (var slot in slots)
            {
                if (slot is not MonoBehaviour monoSlot)
                {
                    Debug.LogWarning($"[CombatSlotRegistry] 슬롯은 MonoBehaviour 기반이어야 합니다: {slot}");
                    continue;
                }

                var holder = monoSlot.GetComponent<CombatSlotPositionHolder>();
                if (holder == null)
                {
                    Debug.LogWarning($"[CombatSlotRegistry] CombatSlotPositionHolder 컴포넌트 누락: {monoSlot.name}");
                    continue;
                }

                if (_slotByPosition.ContainsKey(holder.SlotPosition))
                {
                    Debug.LogError($"[CombatSlotRegistry] 중복된 CombatSlotPosition: {holder.SlotPosition} - {monoSlot.name}");
                    continue;
                }

                if (_slotByFieldPosition.ContainsKey(holder.FieldSlotPosition))
                {
                    Debug.LogError($"[CombatSlotRegistry] 중복된 CombatFieldSlotPosition: {holder.FieldSlotPosition} - {monoSlot.name}");
                    continue;
                }

                _slotByPosition.Add(holder.SlotPosition, slot);
                _slotByFieldPosition.Add(holder.FieldSlotPosition, slot);
                _allSlots.Add(slot);
                registeredCount++;
            }

            _isInitialized = true;
            Debug.Log($"[CombatSlotRegistry] 슬롯 등록 완료 - 총 등록 수: {registeredCount}");
        }

        public ICombatCardSlot GetCombatSlot(CombatSlotPosition position)
        {
            _slotByPosition.TryGetValue(position, out var slot);
            return slot;
        }

        public ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition fieldPosition)
        {
            _slotByFieldPosition.TryGetValue(fieldPosition, out var slot);
            return slot;
        }

        public IEnumerable<ICombatCardSlot> GetAllCombatSlots() => _allSlots;

        // 호환성 유지용
        public ICombatCardSlot GetSlotByPosition(CombatSlotPosition position) => GetCombatSlot(position);
    }
}
