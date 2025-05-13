using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;

namespace Game.SkillCardSystem.Interface
{
    public interface ISkillCard
    {
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
    }
}
