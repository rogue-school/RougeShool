using UnityEngine;
using System.Collections.Generic;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.UI;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.IManager;
using Game.CharacterSystem.Interface;

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
            if (slotRegistry == null)
            {
                Debug.LogError("[PlayerHandManager] SlotRegistry가 주입되지 않았습니다.");
                return;
            }

            if (player == null)
            {
                Debug.LogError("[PlayerHandManager] Player가 주입되지 않았습니다. 초기화를 중단합니다.");
                return;
            }

            handSlots = new Dictionary<SkillCardSlotPosition, IHandCardSlot>();
            runtimeCards = new Dictionary<SkillCardSlotPosition, PlayerSkillCardRuntime>();

            var slots = slotRegistry.GetHandSlots(SlotOwner.PLAYER);
            if (slots == null)
            {
                Debug.LogError("[PlayerHandManager] Player 슬롯 조회 실패");
                return;
            }

            foreach (var slot in slots)
            {
                if (slot == null) continue;
                handSlots[slot.GetSlotPosition()] = slot;
            }

            Debug.Log($"[PlayerHandManager] Player 슬롯 {handSlots.Count}개 초기화 완료");
        }

        public void GenerateInitialHand()
        {
            ClearAll();

            if (player == null || player.Data == null)
            {
                Debug.LogWarning("[PlayerHandManager] 플레이어 데이터가 없어서 핸드를 생성하지 않습니다.");
                return;
            }

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
            {
                UpdateCardUI(runtimeCard, uiSlot);
            }
        }

        public void TickCoolTime()
        {
            foreach (var kvp in runtimeCards)
            {
                var card = kvp.Value;
                int newCoolTime = Mathf.Max(0, card.GetCoolTime() - 1);
                card.SetCoolTime(newCoolTime);

                if (handSlots.TryGetValue(kvp.Key, out var slot) && slot is PlayerHandCardSlotUI uiSlot)
                {
                    UpdateCardUI(card, uiSlot);
                }
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
    }
}
