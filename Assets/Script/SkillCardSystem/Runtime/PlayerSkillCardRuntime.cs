using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Core;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillCardRuntime : ISkillCard
{
    public SkillCardData CardData { get; private set; }

    private List<ICardEffect> effects;
    private int coolTime;
    private SlotOwner owner;
    private SkillCardSlotPosition? handSlot;
    private CombatSlotPosition? combatSlot;

    // 기본 생성자
    public PlayerSkillCardRuntime(PlayerSkillCard cardData)
    {
        CardData = cardData.CardData;
        effects = cardData.CreateEffects();
        coolTime = CardData.CoolTime;
        owner = SlotOwner.PLAYER;
    }

    // 2인자 생성자 추가 (PlayerSkillCard, int)
    public PlayerSkillCardRuntime(PlayerSkillCard cardData, int coolTime)
    {
        CardData = cardData.CardData;
        effects = cardData.CreateEffects();
        this.coolTime = Mathf.Max(0, coolTime);
        owner = SlotOwner.PLAYER;
    }

    public string GetCardName() => CardData.Name;
    public string GetDescription() => CardData.Description;
    public Sprite GetArtwork() => CardData.Artwork;
    public int GetCoolTime() => coolTime;
    public void SetCoolTime(int time) => coolTime = Mathf.Max(0, time);
    public int GetEffectPower(ICardEffect effect) => CardData.Damage;
    public List<ICardEffect> CreateEffects() => new List<ICardEffect>(effects);

    public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
    public SkillCardSlotPosition? GetHandSlot() => handSlot;

    public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
    public CombatSlotPosition? GetCombatSlot() => combatSlot;

    public SlotOwner GetOwner() => owner;

    public void ExecuteSkill()
    {
        ExecuteCardAutomatically(new DefaultCardExecutionContext(this));
    }

    public void ExecuteCardAutomatically(ICardExecutionContext context)
    {
        CharacterBase caster = GetOwner(context) as CharacterBase;
        CharacterBase target = GetTarget(context) as CharacterBase;

        if (caster == null || target == null || target.IsDead())
        {
            Debug.LogWarning("[PlayerSkillCardRuntime] 잘못된 시전자/대상자");
            return;
        }

        foreach (var effect in effects)
        {
            int value = GetEffectPower(effect);
            effect.ExecuteEffect(caster, target, value);
        }
    }

    public ICharacter GetOwner(ICardExecutionContext context) => context.GetPlayer();
    public ICharacter GetTarget(ICardExecutionContext context) => context.GetEnemy();
}
