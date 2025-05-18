using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Core;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;

namespace Game.SkillCardSystem.Runtime
{
    public class PlayerSkillCardRuntime : ISkillCard
    {
        private PlayerSkillCard baseCard;
        private int power;
        private int coolTime;

        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;

        public PlayerSkillCardRuntime(PlayerSkillCard card, int power, int coolTime)
        {
            this.baseCard = card;
            this.power = power;
            this.coolTime = coolTime;
        }

        public string GetCardName() => baseCard.GetCardName();
        public string GetDescription() => baseCard.GetDescription();
        public Sprite GetArtwork() => baseCard.GetArtwork();
        public int GetCoolTime() => coolTime;
        public void SetCoolTime(int value) => coolTime = Mathf.Max(0, value);
        public int GetEffectPower(ICardEffect effect) => power;
        public List<ICardEffect> CreateEffects() => baseCard.CreateEffects();

        public void TickCoolTime() => coolTime = Mathf.Max(0, coolTime - 1);
        public void ActivateCoolTime() => coolTime = baseCard.GetCoolTime();
        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;
        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;
        public SlotOwner GetOwner() => SlotOwner.PLAYER;

        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            var caster = GetOwner(context) as CharacterBase;
            var target = GetTarget(context) as CharacterBase;

            if (caster == null || target == null || target.IsDead()) return;

            foreach (var effect in CreateEffects())
            {
                int value = GetEffectPower(effect);
                effect.ExecuteEffect(caster, target, value);
            }
        }

        public ICharacter GetOwner(ICardExecutionContext context)
        {
            return context.GetPlayer(); // 플레이어 카드의 시전자
        }

        public ICharacter GetTarget(ICardExecutionContext context)
        {
            return context.GetEnemy(); // 플레이어 카드의 대상
        }
    }
}
