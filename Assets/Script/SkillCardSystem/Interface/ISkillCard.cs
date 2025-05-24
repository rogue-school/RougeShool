using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.CombatSystem.Context;

namespace Game.SkillCardSystem.Interface
{
    public interface ISkillCard
    {
        SkillCardData CardData { get; }

        string GetCardName();
        string GetDescription();
        Sprite GetArtwork();
        int GetCoolTime();
        int GetEffectPower(ICardEffect effect);
        List<ICardEffect> CreateEffects();

        void SetHandSlot(SkillCardSlotPosition slot);
        SkillCardSlotPosition? GetHandSlot();

        void SetCombatSlot(CombatSlotPosition slot);
        CombatSlotPosition? GetCombatSlot();

        SlotOwner GetOwner();
        bool IsFromPlayer();

        void ExecuteCardAutomatically(ICardExecutionContext context);
        void ExecuteSkill();

        ICharacter GetOwner(ICardExecutionContext context);
        ICharacter GetTarget(ICardExecutionContext context);
        void ExecuteSkill(ICharacter source, ICharacter target)
        {
            var context = new DefaultCardExecutionContext(this, source, target);
            ExecuteCardAutomatically(context);
        }
    }
}
