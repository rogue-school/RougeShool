using System;
using System.Collections.Generic;
using Game.CombatSystem.Slot;
using Game.CoreSystem.Utility;
using Game.Domain.Combat.Entities;
using Game.Domain.Combat.Interfaces;
using Game.Domain.Combat.ValueObjects;

namespace Game.Infrastructure.Combat
{
    /// <summary>
    /// Unity 전투 슬롯 레지스트리를 도메인 슬롯 레지스트리 인터페이스에 연결하는 어댑터입니다.
    /// </summary>
    public sealed class SlotRegistryAdapter : ISlotRegistry
    {
        private readonly CombatSlotRegistry _combatSlotRegistry;
        private readonly Dictionary<SlotPosition, CombatSlot> _overrides = new Dictionary<SlotPosition, CombatSlot>();

        /// <summary>
        /// 슬롯 레지스트리 어댑터를 생성합니다.
        /// </summary>
        /// <param name="combatSlotRegistry">Unity 전투 슬롯 레지스트리</param>
        /// <exception cref="ArgumentNullException">combatSlotRegistry가 null인 경우</exception>
        public SlotRegistryAdapter(CombatSlotRegistry combatSlotRegistry)
        {
            _combatSlotRegistry = combatSlotRegistry ?? throw new ArgumentNullException(nameof(combatSlotRegistry), "CombatSlotRegistry는 null일 수 없습니다.");
        }

        /// <inheritdoc />
        public CombatSlot GetSlot(SlotPosition position)
        {
            if (_overrides.TryGetValue(position, out var overriddenSlot))
            {
                return overriddenSlot;
            }

            var combatPosition = ToCombatSlotPosition(position);
            var slot = _combatSlotRegistry.GetCombatSlot(combatPosition);

            if (slot == null || slot.IsEmpty())
            {
                return new CombatSlot(position);
            }

            var card = slot.GetCard();
            if (card == null)
            {
                return new CombatSlot(position);
            }

            var definition = card.CardDefinition;
            if (definition == null || string.IsNullOrWhiteSpace(definition.cardId))
            {
                GameLogger.LogWarning("[SlotRegistryAdapter] 카드 정의 또는 cardId가 유효하지 않습니다.", GameLogger.LogCategory.SkillCard);
                return new CombatSlot(position);
            }

            var owner = card.IsFromPlayer() ? TurnType.Player : TurnType.Enemy;
            return new CombatSlot(position, definition.cardId, owner);
        }

        /// <inheritdoc />
        public void SetSlot(CombatSlot slot)
        {
            if (slot == null)
            {
                throw new ArgumentNullException(nameof(slot), "슬롯은 null일 수 없습니다.");
            }

            _overrides[slot.Position] = slot;
        }

        /// <inheritdoc />
        public void ClearAllSlots()
        {
            _overrides.Clear();
        }

        private static CombatSlotPosition ToCombatSlotPosition(SlotPosition position)
        {
            return position switch
            {
                SlotPosition.BattleSlot => CombatSlotPosition.BATTLE_SLOT,
                SlotPosition.WaitSlot1 => CombatSlotPosition.WAIT_SLOT_1,
                SlotPosition.WaitSlot2 => CombatSlotPosition.WAIT_SLOT_2,
                SlotPosition.WaitSlot3 => CombatSlotPosition.WAIT_SLOT_3,
                SlotPosition.WaitSlot4 => CombatSlotPosition.WAIT_SLOT_4,
                _ => CombatSlotPosition.NONE
            };
        }
    }
}


