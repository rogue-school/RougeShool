using UnityEngine;
using System.Collections.Generic;
using Game.Interface;
using Game.UI.Hand;
using Game.Slots;
using Game.Cards;
using Game.UI;
using Game.Player;
using Game.Combat.Turn;
using Game.Data;

namespace Game.Managers
{
    /// <summary>
    /// 플레이어 핸드 카드 초기화 및 전투 슬롯 연동을 관리합니다.
    /// 카드와 슬롯은 고정된 위치를 유지하며 쿨타임과 데미지를 런타임에 주입합니다.
    /// </summary>
    public class PlayerHandManager : MonoBehaviour
    {
        [Header("UI 프리팹")]
        [SerializeField] private SkillCardUI skillCardUIPrefab;

        private Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots;
        private Dictionary<SkillCardSlotPosition, PlayerSkillCardRuntime> runtimeCards;

        public void Initialize()
        {
            handSlots = new Dictionary<SkillCardSlotPosition, IHandCardSlot>();
            runtimeCards = new Dictionary<SkillCardSlotPosition, PlayerSkillCardRuntime>();

            foreach (var slot in SlotRegistry.Instance.GetHandSlots(SlotOwner.PLAYER))
            {
                handSlots[slot.GetSlotPosition()] = slot;
            }
        }

        public void GenerateInitialHand()
        {
            ClearAll();

            var player = PlayerManager.Instance.GetPlayer();
            if (player == null || player.Data == null)
                return;

            var skillDeck = player.Data.skillDeck;
            SkillCardSlotPosition[] orderedSlots = new[]
            {
                SkillCardSlotPosition.PLAYER_SLOT_1,
                SkillCardSlotPosition.PLAYER_SLOT_2,
                SkillCardSlotPosition.PLAYER_SLOT_3,
            };

            int count = Mathf.Min(skillDeck.Count, orderedSlots.Length);

            for (int i = 0; i < count; i++)
            {
                var entry = skillDeck[i];
                var card = entry.card;
                int damage = entry.damage;
                int coolTime = card.GetCoolTime();
                var slotPos = orderedSlots[i];

                if (!handSlots.TryGetValue(slotPos, out var slot))
                    continue;

                var runtimeCard = new PlayerSkillCardRuntime(card, damage, coolTime);
                runtimeCard.SetHandSlot(slotPos);
                runtimeCards[slotPos] = runtimeCard;

                if (slot is PlayerHandCardSlotUI uiSlot)
                    uiSlot.InjectUIFactory(skillCardUIPrefab);

                slot.SetCard(runtimeCard);
            }
        }

        public void SubmitCardFromHand(ISkillCard selectedCard)
        {
            if (selectedCard == null)
                return;

            var slotPos = selectedCard.GetHandSlot();
            if (slotPos.HasValue && handSlots.TryGetValue(slotPos.Value, out var slot))
            {
                slot.Clear();
            }

            CombatTurnManager.Instance.RegisterPlayerCard(selectedCard);
        }

        public void RestoreCardToHand(ISkillCard card)
        {
            var slotPos = card.GetHandSlot();
            if (!slotPos.HasValue || !handSlots.TryGetValue(slotPos.Value, out var slot))
                return;

            slot.SetCard(card);

            if (card is PlayerSkillCardRuntime runtimeCard && slot is PlayerHandCardSlotUI uiSlot)
            {
                UpdateCardUI(runtimeCard, uiSlot);
            }
        }

        private void UpdateCardUI(PlayerSkillCardRuntime runtimeCard, PlayerHandCardSlotUI uiSlot)
        {
            int coolTime = runtimeCard.GetCoolTime();
            bool isCooldown = coolTime > 0;

            uiSlot.SetInteractable(!isCooldown);
            uiSlot.SetCoolTimeDisplay(coolTime, isCooldown);
        }

        public void ClearAll()
        {
            foreach (var slot in handSlots.Values)
                slot.Clear();

            runtimeCards.Clear();
        }

        public ISkillCard GetSlotCard(SkillCardSlotPosition pos)
        {
            return handSlots.TryGetValue(pos, out var slot) ? slot.GetCard() : null;
        }

        public PlayerSkillCardRuntime GetRuntimeCard(SkillCardSlotPosition pos)
        {
            return runtimeCards.TryGetValue(pos, out var card) ? card : null;
        }
    }
}
