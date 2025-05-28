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
    /// 플레이어 스킬 카드의 런타임 구현입니다. 카드 데이터와 효과 실행을 담당합니다.
    /// </summary>
    public class PlayerSkillCardRuntime : ISkillCard
    {
        public SkillCardData CardData { get; private set; }

        private readonly List<SkillCardEffectSO> effects;
        private readonly SlotOwner owner = SlotOwner.PLAYER;

        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;
        private int coolTime;

        public PlayerSkillCardRuntime(SkillCardData data, List<SkillCardEffectSO> effects)
        {
            if (data == null)
            {
                Debug.LogError("[PlayerSkillCardRuntime] SkillCardData가 null입니다. 기본값으로 초기화합니다.");
                CardData = new SkillCardData("Unnamed", "No description", null, 0, 0);
                this.effects = new List<SkillCardEffectSO>();
            }
            else
            {
                CardData = data;
                this.effects = effects ?? new List<SkillCardEffectSO>();
            }

            coolTime = CardData.CoolTime;
        }

        // === 메타 정보 ===
        public string GetCardName() => CardData?.Name ?? "[Unnamed]";
        public string GetDescription() => CardData?.Description ?? "[No Description]";
        public Sprite GetArtwork() => CardData?.Artwork;
        public int GetCoolTime() => coolTime;
        public void SetCoolTime(int time) => coolTime = Mathf.Max(0, time);
        public int GetEffectPower(SkillCardEffectSO effect) => CardData?.Damage ?? 0;

        public List<SkillCardEffectSO> CreateEffects() => new List<SkillCardEffectSO>(effects);

        public SlotOwner GetOwner() => owner;
        public bool IsFromPlayer() => true;

        // === 슬롯 정보 ===
        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        // === 실행 ===
        public void ExecuteSkill()
        {
            throw new System.InvalidOperationException("[PlayerSkillCardRuntime] ExecuteSkill()에는 source/target이 필요합니다.");
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
                Debug.LogError("[PlayerSkillCardRuntime] context가 null입니다.");
                return;
            }

            foreach (var effect in effects)
            {
                int power = GetEffectPower(effect);
                var command = effect.CreateEffectCommand(power);

                if (command != null)
                {
                    command.Execute(context, null);
                    Debug.Log($"[PlayerSkillCardRuntime] {GetCardName()} → {effect.GetType().Name}, power: {power}");
                }
                else
                {
                    Debug.LogWarning($"[PlayerSkillCardRuntime] {GetCardName()} - 효과 명령 생성 실패: {effect.name}");
                }
            }
        }

        // === 대상 정보 ===
        public ICharacter GetOwner(ICardExecutionContext context) => context?.GetPlayer();
        public ICharacter GetTarget(ICardExecutionContext context) => context?.GetEnemy();
    }
}
