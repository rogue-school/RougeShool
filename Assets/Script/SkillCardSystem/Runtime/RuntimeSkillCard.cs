using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;

namespace Game.SkillCardSystem.Runtime
{
    public class RuntimeSkillCard : ISkillCard
    {
        private string cardName;
        private string description;
        private Sprite artwork;
        private int power;
        private int coolTime;
        private List<ICardEffect> effects;
        private SlotOwner owner;

        private SkillCardSlotPosition? handSlot = null;
        private CombatSlotPosition? combatSlot = null;

        public RuntimeSkillCard(
            string name,
            string desc,
            Sprite art,
            List<ICardEffect> effects,
            int power,
            int coolTime,
            SlotOwner owner
        )
        {
            this.cardName = name;
            this.description = desc;
            this.artwork = art;
            this.effects = effects != null ? new List<ICardEffect>(effects) : new List<ICardEffect>();
            this.power = power;
            this.coolTime = coolTime;
            this.owner = owner;
        }

        public string GetCardName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public int GetCoolTime() => coolTime;
        public void SetCoolTime(int time) => coolTime = time;
        public int GetEffectPower(ICardEffect effect) => power;
        public List<ICardEffect> CreateEffects() => new List<ICardEffect>(effects);
        public SlotOwner GetOwner() => owner;

        public void SetPower(int value) => power = value;

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            if (context == null)
            {
                Debug.LogError("[RuntimeSkillCard] context가 null입니다.");
                return;
            }

            CharacterBase caster = GetOwner(context) as CharacterBase;
            CharacterBase target = GetTarget(context) as CharacterBase;

            if (caster == null || target == null || target.IsDead())
            {
                Debug.LogWarning("[RuntimeSkillCard] 유효한 시전자 또는 대상이 없거나 대상이 사망 상태입니다.");
                return;
            }

            Debug.Log($"[RuntimeSkillCard] 자동 실행 시작 → {cardName}, 공격력: {power}, 효과 수: {effects.Count}");

            foreach (var effect in effects)
            {
                if (effect == null) continue;

                int value = GetEffectPower(effect);
                effect.ExecuteEffect(caster, target, value);
                Debug.Log($" → 효과 적용: {effect.GetType().Name}, 값: {value}");
            }
        }

        public ICharacter GetOwner(ICardExecutionContext context)
        {
            return owner == SlotOwner.PLAYER
                ? context.GetPlayer()
                : context.GetEnemy();
        }

        public ICharacter GetTarget(ICardExecutionContext context)
        {
            return owner == SlotOwner.PLAYER
                ? context.GetEnemy()
                : context.GetPlayer();
        }
    }
}
