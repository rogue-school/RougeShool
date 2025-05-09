using UnityEngine;
using System.Collections.Generic;
using Game.Slots;
using Game.UI.Combat;

namespace Game.Managers
{
    /// <summary>
    /// 전투 실행 슬롯을 자동 수집하고, 사전에 설정된 CombatSlotPosition 값으로 그룹화 및 관리하는 매니저입니다.
    /// </summary>
    public class CombatSlotManager : MonoBehaviour
    {
        private CombatExecutionSlotUI[] allSlots;
        private Dictionary<CombatSlotPosition, List<CombatExecutionSlotUI>> slotGroups;

        private void Awake()
        {
            AutoBindAndGroupByPosition();
        }

        /// <summary>
        /// 씬 내 모든 전투 슬롯을 수집하고, 인스펙터에서 수동으로 설정된 CombatSlotPosition을 기준으로 그룹화합니다.
        /// </summary>
        private void AutoBindAndGroupByPosition()
        {
            allSlots = FindObjectsByType<CombatExecutionSlotUI>(FindObjectsSortMode.None);
            slotGroups = new Dictionary<CombatSlotPosition, List<CombatExecutionSlotUI>>();

            foreach (var slot in allSlots)
            {
                CombatSlotPosition position = slot.GetCombatPosition(); // 수동 설정값 사용

                if (!slotGroups.ContainsKey(position))
                    slotGroups[position] = new List<CombatExecutionSlotUI>();

                slotGroups[position].Add(slot);
            }

            Debug.Log($"[CombatSlotManager] 전투 슬롯 자동 수집 및 위치 그룹화 완료 - 총 {allSlots.Length}개");
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
