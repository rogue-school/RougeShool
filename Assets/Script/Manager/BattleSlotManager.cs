using UnityEngine;
using System;
using System.Collections.Generic;
using Game.Battle;
using Game.UI;

namespace Game.Managers
{
    /// <summary>
    /// 전투 카드 슬롯을 자동으로 수집하고, 위치(BattleSlotPosition)에 따라 분류 및 관리하는 매니저입니다.
    /// </summary>
    public class BattleSlotManager : MonoBehaviour
    {
        private BattleCardSlotUI[] allSlots;
        private Dictionary<BattleSlotPosition, List<BattleCardSlotUI>> slotGroups;

        private void Awake()
        {
            AutoBindAndAssignSlotPositions();
        }

        private void AutoBindAndAssignSlotPositions()
        {
            allSlots = FindObjectsOfType<BattleCardSlotUI>();
            Array.Sort(allSlots, (a, b) => a.name.CompareTo(b.name));

            slotGroups = new Dictionary<BattleSlotPosition, List<BattleCardSlotUI>>();

            foreach (var slot in allSlots)
            {
                BattleSlotPosition position = InferSlotPositionFromName(slot.name);
                slot.SetSlotPosition(position);

                if (!slotGroups.ContainsKey(position))
                    slotGroups[position] = new List<BattleCardSlotUI>();

                slotGroups[position].Add(slot);
            }

            Debug.Log($"[BattleSlotManager] 슬롯 자동 수집 및 위치 할당 완료 - 총 {allSlots.Length}개");
        }

        private BattleSlotPosition InferSlotPositionFromName(string name)
        {
            if (name.ToLower().Contains("first"))
                return BattleSlotPosition.FIRST;
            else if (name.ToLower().Contains("second"))
                return BattleSlotPosition.SECOND;
            else
                return BattleSlotPosition.FIRST; // fallback
        }

        public List<BattleCardSlotUI> GetSlotsByPosition(BattleSlotPosition position)
        {
            return slotGroups.TryGetValue(position, out var list) ? list : new List<BattleCardSlotUI>();
        }

        public BattleCardSlotUI[] GetAllSlots() => allSlots;

        public BattleCardSlotUI GetFirstSlot(BattleSlotPosition position)
        {
            var list = GetSlotsByPosition(position);
            return list.Count > 0 ? list[0] : null;
        }
    }
}
