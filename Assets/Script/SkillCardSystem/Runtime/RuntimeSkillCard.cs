using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Context;

namespace Game.SkillCardSystem.Runtime
{
    public class RuntimeSkillCard : ISkillCard
    {
        public SkillCardData CardData { get; private set; }

        private List<SkillCardEffectSO> effects;
        private SlotOwner owner;
        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;

        public RuntimeSkillCard(SkillCardData cardData, List<SkillCardEffectSO> effects, SlotOwner owner)
        {
            CardData = cardData;
            this.effects = effects ?? new();
            this.owner = owner;
        }

        public string GetCardName() => CardData.Name;
        public string GetDescription() => CardData.Description;
        public Sprite GetArtwork() => CardData.Artwork;
        public int GetCoolTime() => CardData.CoolTime;
        public int GetEffectPower(SkillCardEffectSO effect) => CardData.Damage;
        public List<SkillCardEffectSO> CreateEffects() => new(effects);

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;
        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        public SlotOwner GetOwner() => owner;
        public bool IsFromPlayer() => owner == SlotOwner.PLAYER;

        public void ExecuteSkill() =>
            Debug.LogWarning("[RuntimeSkillCard] source/target을 명시적으로 전달해야 합니다.");

        public void ExecuteSkill(ICharacter source, ICharacter target)
        {
            var context = new DefaultCardExecutionContext(this, source, target);
            ExecuteCardAutomatically(context);
        }

        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            if (context?.Source is not CharacterBase || context.Target is not CharacterBase targetChar)
            {
                Debug.LogWarning("[RuntimeSkillCard] context 또는 대상 오류");
                return;
            }

            if (targetChar.IsDead())
            {
                Debug.LogWarning("[RuntimeSkillCard] 대상자가 사망 상태입니다.");
                return;
            }

            foreach (var effect in effects)
            {
                int power = GetEffectPower(effect);
                var command = effect.CreateEffectCommand(power);
                command.Execute(context, null);
            }
        }

        public ICharacter GetOwner(ICardExecutionContext context) =>
            owner == SlotOwner.PLAYER ? context.GetPlayer() : context.GetEnemy();

        public ICharacter GetTarget(ICardExecutionContext context) =>
            owner == SlotOwner.PLAYER ? context.GetEnemy() : context.GetPlayer();
    }
}
