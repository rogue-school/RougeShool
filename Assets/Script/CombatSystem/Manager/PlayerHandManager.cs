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

            // 슬롯 위치 고정
            SkillCardSlotPosition[] orderedSlots = new[]
            {
        SkillCardSlotPosition.PLAYER_SLOT_1,
        SkillCardSlotPosition.PLAYER_SLOT_2,
        SkillCardSlotPosition.PLAYER_SLOT_3,
    };

            // 여기에서 직접 캐릭터에 따른 카드 3장을 고정으로 배정 (예시)
            var selectedCharacter = player?.Data;
            if (selectedCharacter == null)
            {
                Debug.LogWarning("[PlayerHandManager] 플레이어 데이터가 없습니다.");
                return;
            }

            // 여기에 실제 캐릭터에 따라 카드 지정하는 매핑 로직 필요
            List<PlayerSkillCard> deck = GetFixedDeckForCharacter(selectedCharacter);

            for (int i = 0; i < Mathf.Min(deck.Count, orderedSlots.Length); i++)
            {
                var card = deck[i];
                var slotPos = orderedSlots[i];

                if (!handSlots.TryGetValue(slotPos, out var slot))
                    continue;

                var runtimeCard = new PlayerSkillCardRuntime(card);
                runtimeCard.SetHandSlot(slotPos);
                runtimeCards[slotPos] = runtimeCard;

                if (slot is PlayerHandCardSlotUI uiSlot)
                    uiSlot.InjectUIFactory(skillCardUIPrefab);

                slot.SetCard(runtimeCard);
            }
        }
        private List<PlayerSkillCard> GetFixedDeckForCharacter(PlayerCharacterData data)
        {
            // 여기에 사용할 카드들 직접 연결
            // (예: Resources.Load 또는 Inspector로 참조)
            // 아래는 예시이며, 실제 카드들은 적절히 설정해야 합니다.

            if (data.displayName == "Warrior")
            {
                return new List<PlayerSkillCard>
        {
            Resources.Load<PlayerSkillCard>("Cards/Warrior/Slash"),
            Resources.Load<PlayerSkillCard>("Cards/Warrior/ShieldBash"),
            Resources.Load<PlayerSkillCard>("Cards/Warrior/BattleCry"),
        };
            }
            else if (data.displayName == "Mage")
            {
                return new List<PlayerSkillCard>
        {
            Resources.Load<PlayerSkillCard>("Cards/Mage/Fireball"),
            Resources.Load<PlayerSkillCard>("Cards/Mage/IceBolt"),
            Resources.Load<PlayerSkillCard>("Cards/Mage/ArcaneShield"),
        };
            }

            // 기본값
            return new List<PlayerSkillCard>
    {
        Resources.Load<PlayerSkillCard>("Cards/Common/Strike"),
        Resources.Load<PlayerSkillCard>("Cards/Common/Block"),
        Resources.Load<PlayerSkillCard>("Cards/Common/Focus"),
    };
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
    }
}
