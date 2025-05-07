using UnityEngine;
using System;
using System.Collections.Generic;
using Game.Slots;
using Game.UI.Combat;

namespace Game.Managers
{
    /// <summary>
    /// 전투 실행 슬롯을 자동 수집하고, CombatSlotPosition 기준으로 그룹화 및 관리하는 매니저입니다.
    /// </summary>
    public class CombatSlotManager : MonoBehaviour
    {
        private CombatExecutionSlotUI[] allSlots;
        private Dictionary<CombatSlotPosition, List<CombatExecutionSlotUI>> slotGroups;

        private void Awake()
        {
            AutoBindAndAssignSlotPositions();
        }

        /// <summary>
        /// 씬 상의 모든 전투 슬롯을 자동으로 수집하고, 이름 기반으로 CombatSlotPosition을 추론하여 그룹화합니다.
        /// </summary>
        private void AutoBindAndAssignSlotPositions()
        {
            allSlots = FindObjectsOfType<CombatExecutionSlotUI>();
            Array.Sort(allSlots, (a, b) => a.name.CompareTo(b.name));

            slotGroups = new Dictionary<CombatSlotPosition, List<CombatExecutionSlotUI>>();

            foreach (var slot in allSlots)
            {
                CombatSlotPosition position = InferSlotPositionFromName(slot.name);
                slot.SetCombatPosition(position);

                if (!slotGroups.ContainsKey(position))
                    slotGroups[position] = new List<CombatExecutionSlotUI>();

                slotGroups[position].Add(slot);
            }

            Debug.Log($"[CombatSlotManager] 전투 슬롯 자동 수집 및 위치 할당 완료 - 총 {allSlots.Length}개");
        }

        /// <summary>
        /// 슬롯 이름에서 전투 포지션을 추론합니다. 예: "FirstSlot", "SecondSlot"
        /// </summary>
        private CombatSlotPosition InferSlotPositionFromName(string name)
        {
            name = name.ToLower();
            if (name.Contains("first"))
                return CombatSlotPosition.FIRST;
            else if (name.Contains("second"))
                return CombatSlotPosition.SECOND;
            else
                return CombatSlotPosition.FIRST; // fallback
        }

        /// <summary>
        /// 지정된 전투 포지션의 슬롯 리스트 반환
        /// </summary>
        public List<CombatExecutionSlotUI> GetSlotsByPosition(CombatSlotPosition position)
        {
            return slotGroups.TryGetValue(position, out var list) ? list : new List<CombatExecutionSlotUI>();
        }

        /// <summary>
        /// 전체 전투 슬롯 배열 반환
        /// </summary>
        public CombatExecutionSlotUI[] GetAllSlots() => allSlots;

        /// <summary>
        /// 지정 포지션의 첫 번째 슬롯 반환
        /// </summary>
        public CombatExecutionSlotUI GetFirstSlot(CombatSlotPosition position)
        {
            var list = GetSlotsByPosition(position);
            return list.Count > 0 ? list[0] : null;
        }
    }
}
