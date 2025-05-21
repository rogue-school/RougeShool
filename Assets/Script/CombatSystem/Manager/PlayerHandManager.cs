using UnityEngine;
using System.Collections.Generic;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.IManager;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.UI;
using Game.SkillCardSystem.Data;
using Game.CharacterSystem.Data;

namespace Game.CombatSystem.Manager
{
    public class PlayerHandManager : MonoBehaviour, IPlayerHandManager
    {
        [Header("UI 프리팹")]
        [SerializeField] private SkillCardUI skillCardUIPrefab;

        private Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots;
        private Dictionary<SkillCardSlotPosition, PlayerSkillCardRuntime> runtimeCards;

        private IPlayerCharacter player;
        private ISlotRegistry slotRegistry;
        private ICombatTurnManager turnManager;

        public void Inject(IPlayerCharacter player, ISlotRegistry slotRegistry, ICombatTurnManager turnManager)
        {
            this.player = player;
            this.slotRegistry = slotRegistry;
            this.turnManager = turnManager;
        }

        public void Initialize()
        {
            if (slotRegistry == null || player == null)
            {
                Debug.LogError("[PlayerHandManager] 필수 의존성이 누락되었습니다.");
                return;
            }

            handSlots = new Dictionary<SkillCardSlotPosition, IHandCardSlot>();
            runtimeCards = new Dictionary<SkillCardSlotPosition, PlayerSkillCardRuntime>();

            foreach (var slot in slotRegistry.GetHandSlots(SlotOwner.PLAYER))
            {
                if (slot == null) continue;
                handSlots[slot.GetSlotPosition()] = slot;
            }

            Debug.Log($"[PlayerHandManager] Player 슬롯 {handSlots.Count}개 초기화 완료");
        }

        public void GenerateInitialHand()
        {
            ClearAll();

            SkillCardSlotPosition[] orderedSlots = new[]
            {
                SkillCardSlotPosition.PLAYER_SLOT_1,
                SkillCardSlotPosition.PLAYER_SLOT_2,
                SkillCardSlotPosition.PLAYER_SLOT_3
            };

            var selectedCharacter = player?.Data;
            if (selectedCharacter == null || selectedCharacter.skillDeck == null)
            {
                Debug.LogWarning("[PlayerHandManager] 플레이어 데이터 또는 스킬 덱이 없습니다.");
                return;
            }

            List<PlayerSkillCard> deck = selectedCharacter.skillDeck.GetCards();

            for (int i = 0; i < Mathf.Min(deck.Count, orderedSlots.Length); i++)
            {
                var card = deck[i];
                var slotPos = orderedSlots[i];

                if (!handSlots.TryGetValue(slotPos, out var slot)) continue;

                var runtimeCard = new PlayerSkillCardRuntime(card);
                runtimeCard.SetHandSlot(slotPos);
                runtimeCards[slotPos] = runtimeCard;

                if (slot is PlayerHandCardSlotUI uiSlot)
                    uiSlot.InjectUIFactory(skillCardUIPrefab);

                slot.SetCard(runtimeCard);
                UpdateCardUI(runtimeCard, (PlayerHandCardSlotUI)slot);
            }

            // 플레이어 핸드 슬롯 상태 확인
            LogPlayerHandSlotStates();  // 상태 출력
        }

        public void RestoreCardToHand(PlayerSkillCardRuntime card)
        {
            RestoreCardToHand((ISkillCard)card);
        }

        public void RestoreCardToHand(ISkillCard card)
        {
            var slotPos = card.GetHandSlot();
            if (!slotPos.HasValue || !handSlots.TryGetValue(slotPos.Value, out var slot))
                return;

            slot.SetCard(card);

            if (card is PlayerSkillCardRuntime runtimeCard && slot is PlayerHandCardSlotUI uiSlot)
                UpdateCardUI(runtimeCard, uiSlot);
        }

        public void TickCoolTime()
        {
            foreach (var kvp in runtimeCards)
            {
                var card = kvp.Value;
                int newCoolTime = Mathf.Max(0, card.GetCoolTime() - 1);
                card.SetCoolTime(newCoolTime);

                if (handSlots.TryGetValue(kvp.Key, out var slot) && slot is PlayerHandCardSlotUI uiSlot)
                    UpdateCardUI(card, uiSlot);
            }
        }

        public void EnableCardInteraction(bool isEnabled)
        {
            foreach (var slot in handSlots.Values)
            {
                if (slot is PlayerHandCardSlotUI uiSlot)
                {
                    var card = uiSlot.GetCard();
                    bool canInteract = isEnabled && card is PlayerSkillCardRuntime runtimeCard && runtimeCard.GetCoolTime() == 0;
                    uiSlot.SetInteractable(canInteract);
                }
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
            if (handSlots == null) return;

            foreach (var slot in handSlots.Values)
                slot.Clear();

            runtimeCards?.Clear();
        }

        public IEnumerable<IHandCardSlot> GetAllHandSlots() => handSlots?.Values;

        public void EnableInput(bool isEnabled) => EnableCardInteraction(isEnabled);

        // 플레이어 핸드 슬롯 상태 확인
        public void LogPlayerHandSlotStates()
        {
            Debug.Log("[PlayerHandManager] 슬롯 상태 확인:");

            // 3개의 슬롯에 대해 반복하면서 각 슬롯의 카드 상태를 확인
            for (int i = 0; i < 3; i++)
            {
                SkillCardSlotPosition pos = (SkillCardSlotPosition)(i + 1); // PLAYER_SLOT_1부터 3까지
                var card = GetSlotCard(pos);
                var ui = GetCardUI(i);

                Debug.Log($" → {pos}: 카드 = {card?.CardData.Name ?? "없음"}, UI = {(ui != null ? "있음" : "없음")}");
            }
        }

        private ISkillCard GetSlotCard(SkillCardSlotPosition pos)
        {
            return handSlots.TryGetValue(pos, out var slot) ? slot.GetCard() : null;
        }

        private ISkillCardUI GetCardUI(int index)
        {
            SkillCardSlotPosition pos = (SkillCardSlotPosition)(index + 1);
            return handSlots.TryGetValue(pos, out var slot) && slot.GetCardUI() != null ? slot.GetCardUI() : null;
        }
    }
}
