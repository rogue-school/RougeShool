using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Core;

namespace Game.SkillCardSystem.Runtime
{
    public class RuntimeSkillCard : ISkillCard
    {
        public SkillCardData CardData { get; private set; }

        private List<ICardEffect> effects;
        private SlotOwner owner;

        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;

        public RuntimeSkillCard(SkillCardData cardData, List<ICardEffect> effects, SlotOwner owner)
        {
            this.CardData = cardData;
            this.effects = effects ?? new List<ICardEffect>();
            this.owner = owner;
        }

        // ----- ISkillCard 메서드 구현 -----

        public string GetCardName() => CardData.Name;

        public string GetDescription() => CardData.Description;

        public Sprite GetArtwork() => CardData.Artwork;

        public int GetCoolTime() => CardData.CoolTime;

        public int GetEffectPower(ICardEffect effect) => 10; // 기본값 또는 커스터마이즈

        public List<ICardEffect> CreateEffects() => new List<ICardEffect>(effects);

        public SlotOwner GetOwner() => owner;

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            var caster = GetOwner(context) as CharacterBase;
            var target = GetTarget(context) as CharacterBase;

            if (caster == null || target == null || target.IsDead())
            {
                Debug.LogWarning("[RuntimeSkillCard] 유효하지 않은 대상입니다.");
                return;
            }

            foreach (var effect in effects)
            {
                int value = GetEffectPower(effect);
                effect.ExecuteEffect(caster, target, value);
            }
        }

        public void ExecuteSkill()
        {
            ExecuteCardAutomatically(new DefaultCardExecutionContext(this));
        }

        public ICharacter GetOwner(ICardExecutionContext context)
        {
            return owner == SlotOwner.PLAYER ? context.GetPlayer() : context.GetEnemy();
        }

        public ICharacter GetTarget(ICardExecutionContext context)
        {
            return owner == SlotOwner.PLAYER ? context.GetEnemy() : context.GetPlayer();
        }
    }
}
