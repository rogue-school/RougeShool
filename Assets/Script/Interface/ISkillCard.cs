using System.Collections.Generic;
using UnityEngine;
using Game.Slots;
using Game.Effect;

namespace Game.Interface
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
