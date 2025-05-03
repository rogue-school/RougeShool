using System.Collections.Generic;
using UnityEngine;
using Game.Interface;
using Game.UI;
using Game.Battle;

namespace Game.Battle
{
    /// <summary>
    /// 플레이어와 적의 카드 슬롯을 통합적으로 관리합니다.
    /// 슬롯은 SlotPosition에 따라 제어됩니다.
    /// </summary>
    public class BattleSlotManager : MonoBehaviour
    {
        private Dictionary<SlotPosition, BaseCardSlotUI> playerSlots = new();
        private Dictionary<SlotPosition, BaseCardSlotUI> enemySlots = new();

        /// <summary>
        /// 플레이어 슬롯을 등록합니다.
        /// </summary>
        public void RegisterPlayerSlot(SlotPosition position, BaseCardSlotUI slot)
        {
            playerSlots[position] = slot;
        }

        /// <summary>
        /// 적 슬롯을 등록합니다.
        /// </summary>
        public void RegisterEnemySlot(SlotPosition position, BaseCardSlotUI slot)
        {
            enemySlots[position] = slot;
        }

        /// <summary>
        /// 슬롯에 카드 데이터를 설정합니다.
        /// </summary>
        public void SetSlot(bool isPlayer, SlotPosition position, ISkillCard card)
        {
            var slot = isPlayer ? GetPlayerSlot(position) : GetEnemySlot(position);
            if (slot != null)
            {
                slot.SetCard(card);
            }
        }

        /// <summary>
        /// 슬롯의 카드를 제거합니다.
        /// </summary>
        public void ClearSlot(bool isPlayer, SlotPosition position)
        {
            var slot = isPlayer ? GetPlayerSlot(position) : GetEnemySlot(position);
            if (slot != null)
            {
                slot.ClearSlot();
            }
        }

        /// <summary>
        /// 모든 슬롯을 초기화합니다.
        /// </summary>
        public void ClearAllSlots()
        {
            foreach (var slot in playerSlots.Values)
                slot.ClearSlot();

            foreach (var slot in enemySlots.Values)
                slot.ClearSlot();
        }

        /// <summary>
        /// 특정 플레이어 슬롯을 반환합니다.
        /// </summary>
        public BaseCardSlotUI GetPlayerSlot(SlotPosition position)
        {
            return playerSlots.TryGetValue(position, out var slot) ? slot : null;
        }

        /// <summary>
        /// 특정 적 슬롯을 반환합니다.
        /// </summary>
        public BaseCardSlotUI GetEnemySlot(SlotPosition position)
        {
            return enemySlots.TryGetValue(position, out var slot) ? slot : null;
        }
    }
}
