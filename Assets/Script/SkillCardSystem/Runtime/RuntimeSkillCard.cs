using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
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

        private List<ICardEffect> effects;
        private SlotOwner owner;
        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;

        public RuntimeSkillCard(SkillCardData cardData, List<ICardEffect> effects, SlotOwner owner)
        {
            CardData = cardData;
            this.effects = effects ?? new();
            this.owner = owner;
        }

        // 기본 정보
        public string GetCardName() => CardData.Name;
        public string GetDescription() => CardData.Description;
        public Sprite GetArtwork() => CardData.Artwork;
        public int GetCoolTime() => CardData.CoolTime;
        public int GetEffectPower(ICardEffect effect) => CardData.Damage;
        public List<ICardEffect> CreateEffects() => new(effects);

        // 슬롯 정보
        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;
        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        public SlotOwner GetOwner() => owner;
        public bool IsFromPlayer() => owner == SlotOwner.PLAYER;

        // ExecuteSkill() 직접 호출 금지
        public void ExecuteSkill()
        {
            Debug.LogWarning("[RuntimeSkillCard] ExecuteSkill() 호출은 금지됩니다. source/target 캐릭터를 명시적으로 넘겨주세요.");
        }

        // 명시적 캐릭터 기반 실행 방식
        public void ExecuteSkill(ICharacter source, ICharacter target)
        {
            var context = new DefaultCardExecutionContext(this, source, target);
            ExecuteCardAutomatically(context);
        }

        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            var caster = context.Source as CharacterBase;
            var targetChar = context.Target as CharacterBase;

            if (caster == null || targetChar == null || targetChar.IsDead())
            {
                Debug.LogWarning("[RuntimeSkillCard] 유효하지 않은 시전자/대상자. 스킬 실행 중단.");
                return;
            }

            foreach (var effect in effects)
            {
                int value = GetEffectPower(effect);
                effect.ApplyEffect(context, value);
            }
        }

        // 컨텍스트 기반 owner/target 추론 (일반적 참조용)
        public ICharacter GetOwner(ICardExecutionContext context)
            => owner == SlotOwner.PLAYER ? context.GetPlayer() : context.GetEnemy();

        public ICharacter GetTarget(ICardExecutionContext context)
            => owner == SlotOwner.PLAYER ? context.GetEnemy() : context.GetPlayer();
    }
}
