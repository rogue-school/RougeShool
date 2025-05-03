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
        public static BattleSlotManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private Dictionary<SlotPosition, BaseCardSlotUI> playerSlots = new();
        private Dictionary<SlotPosition, BaseCardSlotUI> enemySlots = new();

        public void RegisterPlayerSlot(SlotPosition position, BaseCardSlotUI slot)
        {
            playerSlots[position] = slot;
        }

        public void RegisterEnemySlot(SlotPosition position, BaseCardSlotUI slot)
        {
            enemySlots[position] = slot;
        }

        public void SetSlot(bool isPlayer, SlotPosition position, ISkillCard card)
        {
            var slot = isPlayer ? GetPlayerSlot(position) : GetEnemySlot(position);
            if (slot != null)
            {
                slot.SetCard(card);
            }
        }

        public void ClearSlot(bool isPlayer, SlotPosition position)
        {
            var slot = isPlayer ? GetPlayerSlot(position) : GetEnemySlot(position);
            if (slot != null)
            {
                slot.ClearSlot();
            }
        }

        public void ClearAllSlots()
        {
            foreach (var slot in playerSlots.Values)
                slot.ClearSlot();

            foreach (var slot in enemySlots.Values)
                slot.ClearSlot();
        }

        public BaseCardSlotUI GetPlayerSlot(SlotPosition position)
        {
            return playerSlots.TryGetValue(position, out var slot) ? slot : null;
        }

        public BaseCardSlotUI GetEnemySlot(SlotPosition position)
        {
            return enemySlots.TryGetValue(position, out var slot) ? slot : null;
        }
    }
}
