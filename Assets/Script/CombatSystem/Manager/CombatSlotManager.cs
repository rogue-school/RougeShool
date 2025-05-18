using UnityEngine;
using System.Collections.Generic;
using Game.CombatSystem.Slot;
using Game.CombatSystem.UI;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 실행 슬롯을 자동 수집하고, CombatSlotPosition 기준으로 그룹화 및 관리합니다.
    /// </summary>
    public class CombatSlotManager : MonoBehaviour, ICombatSlotManager
    {
        public static CombatSlotManager Instance { get; private set; }

        private CombatExecutionSlotUI[] allSlots;
        private Dictionary<CombatSlotPosition, List<CombatExecutionSlotUI>> slotGroups;

        private void Awake()
        {
            Instance = this;
            AutoBindAndGroupByPosition();
        }

        public void ReserveSlot(SlotOwner owner, ISkillCard card)
        {
            var positions = new[] { CombatSlotPosition.FIRST, CombatSlotPosition.SECOND };
            CombatSlotPosition selectedPosition = Random.value < 0.5f ? positions[0] : positions[1];
            var slot = GetFirstSlot(selectedPosition);

            if (slot != null)
            {
                slot.SetCard(card);
                Debug.Log($"[CombatSlotManager] {owner}의 카드가 {selectedPosition} 슬롯에 배치됨");
            }
            else
            {
                Debug.LogWarning($"[CombatSlotManager] {selectedPosition} 슬롯에 배치 실패 (비어 있는 슬롯 없음)");
            }
        }

        private void AutoBindAndGroupByPosition()
        {
            allSlots = FindObjectsByType<CombatExecutionSlotUI>(FindObjectsSortMode.None);
            slotGroups = new Dictionary<CombatSlotPosition, List<CombatExecutionSlotUI>>();

            foreach (var slot in allSlots)
            {
                CombatSlotPosition position = slot.GetCombatPosition();
                if (!slotGroups.ContainsKey(position))
                    slotGroups[position] = new List<CombatExecutionSlotUI>();

                slotGroups[position].Add(slot);
            }

            //Debug.Log($"[CombatSlotManager] 전투 슬롯 자동 수집 및 위치 그룹화 완료 - 총 {allSlots.Length}개");
        }

        public List<CombatExecutionSlotUI> GetSlotsByPosition(CombatSlotPosition position)
        {
            return slotGroups.TryGetValue(position, out var list) ? list : new List<CombatExecutionSlotUI>();
        }

        public CombatExecutionSlotUI[] GetAllSlots() => allSlots;

        public CombatExecutionSlotUI GetFirstSlot(CombatSlotPosition position)
        {
            var list = GetSlotsByPosition(position);
            return list.Count > 0 ? list[0] : null;
        }

        public ICombatCardSlot GetSlot(CombatSlotPosition position)
        {
            return GetFirstSlot(position); // CombatExecutionSlotUI가 ICombatCardSlot 구현체여야 함
        }
    }
}
