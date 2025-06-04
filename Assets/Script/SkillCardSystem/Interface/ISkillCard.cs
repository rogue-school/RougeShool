using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Effects;
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
        int GetEffectPower(SkillCardEffectSO effect);

        List<SkillCardEffectSO> CreateEffects();

        // 슬롯 및 소유자 정보
        void SetHandSlot(SkillCardSlotPosition slot);
        SkillCardSlotPosition? GetHandSlot();
        void SetCombatSlot(CombatSlotPosition slot);
        CombatSlotPosition? GetCombatSlot();
        SlotOwner GetOwner();
        bool IsFromPlayer();

        // 실행
        void ExecuteCardAutomatically(ICardExecutionContext context);
        void ExecuteSkill();
        void ExecuteSkill(ICharacter source, ICharacter target);

        ICharacter GetOwner(ICardExecutionContext context);
        ICharacter GetTarget(ICardExecutionContext context);

        // 쿨타임 관련
        int GetMaxCoolTime();
        int GetCurrentCoolTime();
        void SetCurrentCoolTime(int value);
    }
}
