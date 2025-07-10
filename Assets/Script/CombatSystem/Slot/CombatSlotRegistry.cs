using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using System.Linq;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 전투 슬롯을 등록하고 슬롯 위치에 따라 조회할 수 있도록 관리하는 레지스트리입니다.
    /// </summary>
    public class CombatSlotRegistry : MonoBehaviour, ICombatSlotRegistry
    {
        #region 필드

        private readonly Dictionary<CombatSlotPosition, ICombatCardSlot> _slotByPosition = new();
        private readonly Dictionary<CombatFieldSlotPosition, ICombatCardSlot> _slotByFieldPosition = new();
        private readonly List<ICombatCardSlot> _allSlots = new();

        private bool _isInitialized = false;

        /// <summary>
        /// 슬롯 레지스트리가 초기화되었는지 여부
        /// </summary>
        public bool IsInitialized => _isInitialized;

        #endregion

        #region 슬롯 등록

        /// <summary>
        /// 슬롯을 등록하여 내부 데이터에 저장합니다.
        /// </summary>
        /// <param name="slots">등록할 전투 카드 슬롯 목록</param>
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
        }

        #endregion

        #region 슬롯 조회

        /// <summary>
        /// 실행 슬롯 위치를 기준으로 슬롯을 조회합니다.
        /// </summary>
        public ICombatCardSlot GetCombatSlot(CombatSlotPosition position)
        {
            _slotByPosition.TryGetValue(position, out var slot);
            return slot;
        }

        /// <summary>
        /// 필드 슬롯 위치를 기준으로 슬롯을 조회합니다.
        /// </summary>
        public ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition fieldPosition)
        {
            _slotByFieldPosition.TryGetValue(fieldPosition, out var slot);
            return slot;
        }

        /// <summary>
        /// 모든 전투 슬롯을 반환합니다.
        /// </summary>
        public IEnumerable<ICombatCardSlot> GetAllCombatSlots() => _allSlots;

        /// <summary>
        /// 기존 메서드와 호환을 위한 별칭
        /// </summary>
        public ICombatCardSlot GetSlotByPosition(CombatSlotPosition position) => GetCombatSlot(position);

        #endregion
    }
}
