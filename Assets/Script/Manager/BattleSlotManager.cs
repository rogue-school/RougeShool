using UnityEngine;
using Game.Cards;

namespace Game.Battle
{
    public class BattleSlotManager : MonoBehaviour
    {
        public CombatSlotUI firstSlot;
        public CombatSlotUI secondSlot;

        public bool IsReady() => firstSlot.HasCard() && secondSlot.HasCard();

        public void StartBattle()
        {
            if (!IsReady()) return;

            Debug.Log("=== 전투 시작 ===");
            firstSlot.ExecuteEffect(null, null);
            secondSlot.ExecuteEffect(null, null);

            firstSlot.Clear();
            secondSlot.Clear();
        }

        public bool TrySetCard(PlayerCardData card)
        {
            if (!firstSlot.HasCard())
            {
                firstSlot.SetCard(card);
                return true;
            }

            if (!secondSlot.HasCard())
            {
                secondSlot.SetCard(card);
                return true;
            }

            return false;
        }

        public bool HasEmptySlot() => !firstSlot.HasCard() || !secondSlot.HasCard();
    }
}
