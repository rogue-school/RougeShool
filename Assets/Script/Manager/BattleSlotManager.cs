using UnityEngine;
using System;
using System.Collections.Generic;
using Game.Battle;
using Game.UI;

namespace Game.Managers
{
    /// <summary>
    /// 전투 카드 슬롯을 자동으로 수집하고, 위치(SlotPosition)에 따라 분류 및 관리하는 매니저입니다.
    /// </summary>
    public class BattleSlotManager : MonoBehaviour
    {
        private BattleCardSlotUI[] allSlots;
        private Dictionary<SlotPosition, List<BattleCardSlotUI>> slotGroups;

        private void Awake()
        {
            AutoBindAndAssignSlotPositions();
        }

        /// <summary>
        /// 씬에 존재하는 모든 BattleCardSlotUI를 자동 수집하고 이름 기준으로 포지션을 자동 설정합니다.
        /// </summary>
        private void AutoBindAndAssignSlotPositions()
        {
            allSlots = FindObjectsOfType<BattleCardSlotUI>();
            Array.Sort(allSlots, (a, b) => a.name.CompareTo(b.name));

            slotGroups = new Dictionary<SlotPosition, List<BattleCardSlotUI>>();

            foreach (var slot in allSlots)
            {
                // 슬롯 이름을 기준으로 위치를 유추
                SlotPosition position = InferSlotPositionFromName(slot.name);
                slot.SetSlotPosition(position); // 슬롯에 위치 설정

                if (!slotGroups.ContainsKey(position))
                    slotGroups[position] = new List<BattleCardSlotUI>();

                slotGroups[position].Add(slot);
            }

            Debug.Log($"[BattleSlotManager] 슬롯 자동 수집 및 위치 할당 완료 - 총 {allSlots.Length}개");
        }

        /// <summary>
        /// 슬롯 오브젝트 이름을 분석하여 SlotPosition을 유추합니다.
        /// </summary>
        private SlotPosition InferSlotPositionFromName(string name)
        {
            if (name.ToLower().Contains("front"))
                return SlotPosition.FRONT;
            else if (name.ToLower().Contains("back"))
                return SlotPosition.BACK;
            else if (name.ToLower().Contains("support"))
                return SlotPosition.SUPPORT;
            else
                return SlotPosition.UNKNOWN; // 확장 고려
        }

        /// <summary>
        /// 특정 위치에 해당하는 모든 슬롯 목록을 반환합니다.
        /// </summary>
        public List<BattleCardSlotUI> GetSlotsByPosition(SlotPosition position)
        {
            return slotGroups.TryGetValue(position, out var list) ? list : new List<BattleCardSlotUI>();
        }

        /// <summary>
        /// 전투 슬롯 전체를 반환합니다.
        /// </summary>
        public BattleCardSlotUI[] GetAllSlots()
        {
            return allSlots;
        }

        /// <summary>
        /// 슬롯 위치별 첫 번째 슬롯을 반환합니다 (예: FRONT 선공 슬롯)
        /// </summary>
        public BattleCardSlotUI GetFirstSlot(SlotPosition position)
        {
            var list = GetSlotsByPosition(position);
            return list.Count > 0 ? list[0] : null;
        }
    }
}
