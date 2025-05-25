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
            if (selectedCharacter == null || selectedCharacter.SkillDeck == null)
            {
                Debug.LogWarning("[PlayerHandManager] 플레이어 데이터 또는 스킬 덱이 없습니다.");
                return;
            }

            List<PlayerSkillCard> deck = selectedCharacter.SkillDeck.GetCards();
            if (deck == null)
            {
                Debug.LogWarning("[PlayerHandManager] SkillDeck.GetCards() 결과가 null입니다.");
                return;
            }

            for (int i = 0; i < Mathf.Min(deck.Count, orderedSlots.Length); i++)
            {
                var card = deck[i];
                var slotPos = orderedSlots[i];

                if (!handSlots.TryGetValue(slotPos, out var slot) || slot == null)
                {
                    Debug.LogWarning($"[PlayerHandManager] 핸드 슬롯 {slotPos} 이 존재하지 않음");
                    continue;
                }

                var runtimeCard = new PlayerSkillCardRuntime(card);
                runtimeCard.SetHandSlot(slotPos);
                runtimeCards[slotPos] = runtimeCard;

                if (slot is PlayerHandCardSlotUI uiSlot)
                    uiSlot.InjectUIFactory(skillCardUIPrefab);

                slot.SetCard(runtimeCard);

                if (slot is PlayerHandCardSlotUI uiSlot2)
                    UpdateCardUI(runtimeCard, uiSlot2);
            }

            LogPlayerHandSlotStates();
        }


        public void RestoreCardToHand(PlayerSkillCardRuntime card) => RestoreCardToHand((ISkillCard)card);

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
                    var card = uiSlot.GetCard() as PlayerSkillCardRuntime;
                    bool canInteract = isEnabled && card != null && card.GetCoolTime() == 0;
                    uiSlot.SetInteractable(canInteract);
                }
            }
        }

        public void EnableInput(bool isEnabled) => EnableCardInteraction(isEnabled);

        public void ClearAll()
        {
            if (handSlots == null) return;

            foreach (var slot in handSlots.Values)
                slot.Clear();

            runtimeCards?.Clear();
        }

        public IEnumerable<IHandCardSlot> GetAllHandSlots() => handSlots?.Values;

        private void UpdateCardUI(PlayerSkillCardRuntime runtimeCard, PlayerHandCardSlotUI uiSlot)
        {
            if (runtimeCard == null || uiSlot == null) return;

            int coolTime = runtimeCard.GetCoolTime();
            bool isCooldown = coolTime > 0;

            uiSlot.SetInteractable(!isCooldown);
            uiSlot.SetCoolTimeDisplay(coolTime, isCooldown);
        }

        public void LogPlayerHandSlotStates()
        {
            Debug.Log("[PlayerHandManager] 슬롯 상태 확인:");

            SkillCardSlotPosition[] positions = new[]
            {
                SkillCardSlotPosition.PLAYER_SLOT_1,
                SkillCardSlotPosition.PLAYER_SLOT_2,
                SkillCardSlotPosition.PLAYER_SLOT_3
            };

            foreach (var pos in positions)
            {
                var card = GetCardInSlot(pos);
                var ui = GetCardUIInSlot(pos);
                Debug.Log($" → {pos}: 카드 = {card?.CardData.Name ?? "없음"}, UI = {(ui != null ? "있음" : "없음")}");
            }
        }

        public ISkillCard GetCardInSlot(SkillCardSlotPosition pos)
        {
            return handSlots.TryGetValue(pos, out var slot) ? slot.GetCard() : null;
        }

        public ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos)
        {
            return handSlots.TryGetValue(pos, out var slot) ? slot.GetCardUI() : null;
        }
    }
}
