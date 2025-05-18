using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;

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

        void ExecuteCardAutomatically(ICardExecutionContext context);

        /// <summary>
        /// 카드 소유자(시전자)를 반환합니다.
        /// </summary>
        ICharacter GetOwner(ICardExecutionContext context);

        /// <summary>
        /// 카드 대상(타겟)을 반환합니다.
        /// </summary>
        ICharacter GetTarget(ICardExecutionContext context);
    }
}
