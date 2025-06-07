using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Context;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Runtime
{
    /// <summary>
    /// 적 전용 스킬 카드의 런타임 구현체입니다. 실행 시 ICardExecutionContext에 따라 효과를 처리합니다.
    /// </summary>
    public class EnemySkillCardRuntime : ISkillCard
    {
        public SkillCardData CardData { get; private set; }

        private readonly List<SkillCardEffectSO> effects;
        private readonly SlotOwner owner = SlotOwner.ENEMY;

        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;

        // 쿨타임 관리 필드
        private int currentCoolTime = 0;

        public EnemySkillCardRuntime(SkillCardData cardData, List<SkillCardEffectSO> effects)
        {
            CardData = cardData ?? throw new System.ArgumentNullException(nameof(cardData));
            this.effects = effects ?? new List<SkillCardEffectSO>();
        }

        // === 메타 정보 ===
        public string GetCardName() => CardData?.Name ?? "[Unnamed Enemy Card]";
        public string GetDescription() => CardData?.Description ?? "[No Description]";
        public Sprite GetArtwork() => CardData?.Artwork;
        public int GetCoolTime() => CardData?.CoolTime ?? 0;
        public int GetEffectPower(SkillCardEffectSO effect) => CardData?.Damage ?? 0;
        public List<SkillCardEffectSO> CreateEffects() => new List<SkillCardEffectSO>(effects);

        public SlotOwner GetOwner() => owner;
        public bool IsFromPlayer() => false;

        // === 슬롯 정보 ===
        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;
        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        // === 쿨타임 관련 구현 ===
        public int GetMaxCoolTime() => CardData?.CoolTime ?? 0;
        public int GetCurrentCoolTime() => currentCoolTime;
        public void SetCurrentCoolTime(int value) => currentCoolTime = Mathf.Max(0, value);

        // === 실행 ===
        public void ExecuteSkill()
        {
            throw new System.InvalidOperationException("[EnemySkillCardRuntime] source/target이 필요합니다.");
        }

        public void ExecuteSkill(ICharacter source, ICharacter target)
        {
            var context = new DefaultCardExecutionContext(this, source, target);
            ExecuteCardAutomatically(context);
        }

        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            if (context == null)
            {
                Debug.LogError("[EnemySkillCardRuntime] context가 null입니다.");
                return;
            }

            foreach (var effect in effects)
            {
                int power = GetEffectPower(effect);
                var command = effect.CreateEffectCommand(power);

                if (command != null)
                {
                    command.Execute(context, null);
                    Debug.Log($"[EnemySkillCardRuntime] {GetCardName()} → {effect.GetType().Name}, power: {power}");
                }
                else
                {
                    Debug.LogWarning($"[EnemySkillCardRuntime] {GetCardName()} - 효과 명령 생성 실패: {effect.name}");
                }
            }

            // 쿨타임 적용
            SetCurrentCoolTime(GetMaxCoolTime());
        }

        // === 대상 정보 ===
        public ICharacter GetOwner(ICardExecutionContext context) => context?.GetEnemy();
        public ICharacter GetTarget(ICardExecutionContext context) => context?.GetPlayer();
    }
}
