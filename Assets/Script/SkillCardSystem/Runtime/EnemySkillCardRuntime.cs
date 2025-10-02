using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Context;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Runtime
{
    /// <summary>
    /// 런타임에서 실행되는 적 전용 스킬 카드 클래스입니다.
    /// 카드 데이터, 이펙트 목록 및 슬롯 정보를 포함하며,
    /// 카드 실행 시 ICardExecutionContext를 통해 동작합니다.
    /// </summary>
    public class EnemySkillCardRuntime : ISkillCard
    {
        /// <inheritdoc/>
        public SkillCardDefinition CardDefinition { get; private set; }

        private readonly List<SkillCardEffectSO> effects;
        private readonly SlotOwner owner = SlotOwner.ENEMY;

        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;

        /// <summary>
        /// 생성자: 카드 정의를 주입받아 초기화합니다.
        /// </summary>
        public EnemySkillCardRuntime(SkillCardDefinition definition)
        {
            CardDefinition = definition ?? throw new System.ArgumentNullException(nameof(definition));
            this.effects = definition.configuration.hasEffects ? 
                definition.configuration.effects.ConvertAll(e => e.effectSO) : 
                new List<SkillCardEffectSO>();
        }

        #region 메타 정보

        public string GetCardName() => CardDefinition?.displayName ?? "[Unnamed Enemy Card]";
        public string GetDescription() => CardDefinition?.description ?? "[No Description]";
        public Sprite GetArtwork() => CardDefinition?.artwork;
        public int GetEffectPower(SkillCardEffectSO effect) => CardDefinition?.configuration.hasDamage == true ? CardDefinition.configuration.damageConfig.baseDamage : 0;
        public List<SkillCardEffectSO> CreateEffects() => new List<SkillCardEffectSO>(effects);

        public SlotOwner GetOwner() => owner;
        public bool IsFromPlayer() => false;

        #endregion

        #region 슬롯 정보

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        #endregion

        #region 실행 메서드

        /// <summary>
        /// source/target을 지정하지 않으면 예외를 발생시킵니다.
        /// </summary>
        public void ExecuteSkill()
        {
            throw new System.InvalidOperationException("[EnemySkillCardRuntime] source/target이 필요합니다.");
        }

        /// <summary>
        /// 지정된 source/target과 함께 카드 실행 컨텍스트를 구성하고 실행합니다.
        /// </summary>
        public void ExecuteSkill(ICharacter source, ICharacter target)
        {
            var context = new DefaultCardExecutionContext(this, source, target);
            ExecuteCardAutomatically(context);
        }

        /// <summary>
        /// 카드의 효과를 자동으로 실행합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
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
                    Debug.LogWarning($"[EnemySkillCardRuntime] {GetCardName()} - 효과 명령 생성 실패: {effect?.name}");
                }
            }
        }

        #endregion

        #region 대상 추출

        public ICharacter GetOwner(ICardExecutionContext context) => context?.GetEnemy();
        public ICharacter GetTarget(ICardExecutionContext context) => context?.GetPlayer();

        #endregion
    }
}
