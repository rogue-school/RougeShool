using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Context;

public class PlayerSkillCardRuntime : ISkillCard
{
    public SkillCardData CardData { get; private set; }

    private List<SkillCardEffectSO> effects;
    private int coolTime;
    private readonly SlotOwner owner = SlotOwner.PLAYER;
    private SkillCardSlotPosition? handSlot;
    private CombatSlotPosition? combatSlot;

    public PlayerSkillCardRuntime(PlayerSkillCard data)
    {
        if (data == null || data.CardData == null)
        {
            Debug.LogError("[PlayerSkillCardRuntime] null 데이터. 기본값으로 초기화됨.");
            CardData = new SkillCardData("Unnamed", "No description", null, 0, 0);
            effects = new List<SkillCardEffectSO>();
            coolTime = 0;
            return;
        }

        CardData = data.CardData;
        effects = data.CreateEffects() ?? new List<SkillCardEffectSO>();
        coolTime = CardData.CoolTime;
    }

    public string GetCardName() => CardData?.Name ?? "[No Name]";
    public string GetDescription() => CardData?.Description ?? "[No Description]";
    public Sprite GetArtwork() => CardData?.Artwork;
    public int GetCoolTime() => coolTime;
    public void SetCoolTime(int time) => coolTime = Mathf.Max(0, time);
    public int GetEffectPower(SkillCardEffectSO effect) => CardData?.Damage ?? 0;
    public List<SkillCardEffectSO> CreateEffects() => new(effects);

    public SlotOwner GetOwner() => owner;
    public bool IsFromPlayer() => true;

    public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
    public SkillCardSlotPosition? GetHandSlot() => handSlot;
    public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
    public CombatSlotPosition? GetCombatSlot() => combatSlot;

    public void ExecuteSkill() =>
        throw new System.InvalidOperationException("[PlayerSkillCardRuntime] source/target이 필요합니다.");

    public void ExecuteSkill(ICharacter source, ICharacter target)
    {
        var context = new DefaultCardExecutionContext(this, source, target);
        ExecuteCardAutomatically(context);
    }

    public void ExecuteCardAutomatically(ICardExecutionContext context)
    {
        if (context?.Source is not CharacterBase || context.Target is not CharacterBase targetChar)
        {
            Debug.LogWarning("[PlayerSkillCardRuntime] 잘못된 context 또는 대상");
            return;
        }

        if (targetChar.IsDead())
        {
            Debug.LogWarning("[PlayerSkillCardRuntime] 대상이 사망 상태입니다.");
            return;
        }

        foreach (var effect in effects)
        {
            int power = GetEffectPower(effect);
            var command = effect.CreateEffectCommand(power);
            command.Execute(context, null);
            Debug.Log($"[PlayerSkillCardRuntime] {GetCardName()} → {effect.GetType().Name}, power: {power}");
        }
    }

    public ICharacter GetOwner(ICardExecutionContext context) => context?.GetPlayer();
    public ICharacter GetTarget(ICardExecutionContext context) => context?.GetEnemy();
}
