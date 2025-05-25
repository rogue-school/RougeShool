using Game.CharacterSystem.Interface;
using Game.CombatSystem.Context;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkillCardRuntime : ISkillCard
{
    public SkillCardData CardData { get; private set; }
    private List<SkillCardEffectSO> effects;
    private SlotOwner owner = SlotOwner.ENEMY;
    private SkillCardSlotPosition? handSlot;
    private CombatSlotPosition? combatSlot;

    public EnemySkillCardRuntime(SkillCardData data, List<SkillCardEffectSO> effects)
    {
        CardData = data;
        this.effects = effects ?? new List<SkillCardEffectSO>();
    }

    public string GetCardName() => CardData.Name;
    public string GetDescription() => CardData.Description;
    public Sprite GetArtwork() => CardData.Artwork;
    public int GetCoolTime() => CardData.CoolTime;
    public int GetEffectPower(SkillCardEffectSO effect) => CardData.Damage;
    public List<SkillCardEffectSO> CreateEffects() => new(effects);

    public SlotOwner GetOwner() => owner;
    public bool IsFromPlayer() => false;

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
        foreach (var effect in effects)
        {
            int power = GetEffectPower(effect);
            var command = effect.CreateEffectCommand(power);
            command.Execute(context, null);
            Debug.Log($"[EnemySkillCardRuntime] {GetCardName()} → {effect.name}, power: {power}");
        }
    }

    public void ExecuteSkill()
    {
        throw new System.InvalidOperationException("[EnemySkillCardRuntime] ExecuteSkill()에는 source/target이 필요합니다.");
    }

    public ICharacter GetOwner(ICardExecutionContext context) => context.GetEnemy();
    public ICharacter GetTarget(ICardExecutionContext context) => context.GetPlayer();
}
