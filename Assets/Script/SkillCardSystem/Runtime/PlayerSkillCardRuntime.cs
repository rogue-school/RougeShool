using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Context;

public class PlayerSkillCardRuntime : ISkillCard
{
    public SkillCardData CardData { get; private set; }
    private List<ICardEffect> effects;
    private int coolTime;
    private SlotOwner owner = SlotOwner.PLAYER;
    private SkillCardSlotPosition? handSlot;
    private CombatSlotPosition? combatSlot;

    public PlayerSkillCardRuntime(PlayerSkillCard data)
    {
        CardData = data.CardData;
        effects = data.CreateEffects();
        coolTime = CardData.CoolTime;
    }

    public string GetCardName() => CardData.Name;
    public string GetDescription() => CardData.Description;
    public Sprite GetArtwork() => CardData.Artwork;
    public int GetCoolTime() => coolTime;
    public void SetCoolTime(int time) => coolTime = Mathf.Max(0, time);
    public int GetEffectPower(ICardEffect effect) => CardData.Damage;
    public List<ICardEffect> CreateEffects() => new(effects);

    public SlotOwner GetOwner() => owner;
    public bool IsFromPlayer() => true;

    public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
    public SkillCardSlotPosition? GetHandSlot() => handSlot;
    public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
    public CombatSlotPosition? GetCombatSlot() => combatSlot;

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
            Debug.LogWarning("[PlayerSkillCardRuntime] 유효하지 않은 시전자/대상자");
            return;
        }

        foreach (var effect in effects)
        {
            int value = GetEffectPower(effect);
            effect.ApplyEffect(context, value);
        }
    }

    public void ExecuteSkill()
    {
        throw new System.InvalidOperationException("[PlayerSkillCardRuntime] ExecuteSkill()에는 source/target이 필요합니다.");
    }

    public ICharacter GetOwner(ICardExecutionContext context) => context.GetPlayer();
    public ICharacter GetTarget(ICardExecutionContext context) => context.GetEnemy();
}
