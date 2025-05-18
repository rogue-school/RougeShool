using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Core;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;

namespace Game.SkillCardSystem.Runtime
{
    public class EnemySkillCardRuntime : ISkillCard
    {
        private EnemySkillCard baseCard;
        private int power;
        private CombatSlotPosition? combatSlot;

        public EnemySkillCardRuntime(EnemySkillCard baseCard, int power)
        {
            this.baseCard = baseCard;
            this.power = power;
        }

        public string GetCardName() => baseCard.GetCardName();
        public string GetDescription() => baseCard.GetDescription();
        public Sprite GetArtwork() => baseCard.GetArtwork();
        public int GetCoolTime() => 0;
        public int GetEffectPower(ICardEffect effect) => power;
        public List<ICardEffect> CreateEffects() => baseCard.CreateEffects();

        public SlotOwner GetOwner() => SlotOwner.ENEMY;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        public void SetHandSlot(SkillCardSlotPosition slot) { }
        public SkillCardSlotPosition? GetHandSlot() => null;

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
            return context.GetEnemy(); // 적 카드의 시전자
        }

        public ICharacter GetTarget(ICardExecutionContext context)
        {
            return context.GetPlayer(); // 적 카드의 대상은 플레이어
        }
    }
}
