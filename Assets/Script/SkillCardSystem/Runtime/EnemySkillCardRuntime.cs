using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Core;

namespace Game.SkillCardSystem.Runtime
{
    public class EnemySkillCardRuntime : ISkillCard
    {
        public SkillCardData CardData { get; }
        private List<ICardEffect> effects;
        private int power;
        private SlotOwner owner;
        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;

        public EnemySkillCardRuntime(SkillCardData data, List<ICardEffect> effects, int power)
        {
            CardData = data;
            this.effects = effects ?? new List<ICardEffect>();
            this.power = power;
            this.owner = SlotOwner.ENEMY;
        }

        public string GetCardName() => CardData.Name;
        public string GetDescription() => CardData.Description;
        public Sprite GetArtwork() => CardData.Artwork;
        public int GetCoolTime() => CardData.CoolTime;
        public int GetEffectPower(ICardEffect effect) => power;
        public List<ICardEffect> CreateEffects() => new List<ICardEffect>(effects);
        public SlotOwner GetOwner() => owner;

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            CharacterBase caster = GetOwner(context) as CharacterBase;
            CharacterBase target = GetTarget(context) as CharacterBase;

            foreach (var effect in effects)
            {
                int value = GetEffectPower(effect);
                effect.ExecuteEffect(caster, target, value);
            }
        }

        public void ExecuteSkill() => ExecuteCardAutomatically(new DefaultCardExecutionContext(this));

        public ICharacter GetOwner(ICardExecutionContext context) => context.GetEnemy();
        public ICharacter GetTarget(ICardExecutionContext context) => context.GetPlayer();
    }
}
