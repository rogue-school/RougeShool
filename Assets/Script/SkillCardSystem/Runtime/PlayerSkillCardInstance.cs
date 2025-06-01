using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Context;

namespace Game.SkillCardSystem.Runtime
{
    /// <summary>
    /// 런타임에서 사용되는 실제 스킬 카드 인스턴스 (실제 효과를 실행 가능)
    /// </summary>
    public class PlayerSkillCardInstance : ISkillCard
    {
        public SkillCardData CardData { get; }

        private readonly List<SkillCardEffectSO> effects;
        private readonly SlotOwner owner;

        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;

        public PlayerSkillCardInstance(SkillCardData cardData, List<SkillCardEffectSO> effects, SlotOwner owner)
        {
            this.CardData = cardData;
            this.effects = effects ?? new List<SkillCardEffectSO>();
            this.owner = owner;
        }

        public string GetCardName() => CardData?.Name ?? "Unnamed";
        public string GetDescription() => CardData?.Description ?? "";
        public Sprite GetArtwork() => CardData?.Artwork;
        public int GetCoolTime() => CardData?.CoolTime ?? 0;

        public int GetEffectPower(SkillCardEffectSO effect)
        {
            // 예시로 카드의 데미지를 통일된 파워로 반환
            return CardData?.Damage ?? 0;
        }

        public List<SkillCardEffectSO> CreateEffects() => new List<SkillCardEffectSO>(effects);

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        public SlotOwner GetOwner() => owner;
        public bool IsFromPlayer() => owner == SlotOwner.PLAYER;

        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            var source = context?.Source;
            var target = context?.Target;

            Debug.Log($"[PlayerSkillCardInstance] 실행: {GetCardName()} ({source?.GetCharacterName()} -> {target?.GetCharacterName()})");

            foreach (var effect in effects)
            {
                int power = GetEffectPower(effect);
                effect.ApplyEffect(context, power);
            }
        }

        public void ExecuteSkill()
        {
            Debug.LogWarning("[PlayerSkillCardInstance] ExecuteSkill() 호출됨 - context 없음");
        }

        public ICharacter GetOwner(ICardExecutionContext context) => context?.Source;
        public ICharacter GetTarget(ICardExecutionContext context) => context?.Target;
    }
}
