using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Context;

namespace Game.SkillCardSystem.Runtime
{
    /// <summary>
    /// 런타임에서 사용되는 실제 플레이어 스킬 카드 인스턴스입니다.
    /// 카드 데이터와 이펙트를 포함하며, 쿨타임 및 슬롯 상태를 관리하고 실제 효과를 실행합니다.
    /// </summary>
    public class PlayerSkillCardInstance : ISkillCard
    {
        /// <inheritdoc/>
        public SkillCardData CardData { get; }

        private readonly List<SkillCardEffectSO> effects;
        private readonly SlotOwner owner;

        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;

        private int currentCoolTime = 0;

        /// <summary>
        /// 카드 데이터, 효과 목록, 소유자를 지정해 인스턴스를 생성합니다.
        /// </summary>
        public PlayerSkillCardInstance(SkillCardData cardData, List<SkillCardEffectSO> effects, SlotOwner owner)
        {
            this.CardData = cardData;
            this.effects = effects ?? new List<SkillCardEffectSO>();
            this.owner = owner;
        }

        #region 메타 정보

        public string GetCardName() => CardData?.Name ?? "Unnamed";
        public string GetDescription() => CardData?.Description ?? "";
        public Sprite GetArtwork() => CardData?.Artwork;
        public int GetCoolTime() => CardData?.CoolTime ?? 0;
        public int GetMaxCoolTime() => CardData?.CoolTime ?? 0;
        public int GetCurrentCoolTime() => currentCoolTime;
        public void SetCurrentCoolTime(int value) => currentCoolTime = Mathf.Max(0, value);

        public int GetEffectPower(SkillCardEffectSO effect)
        {
            return CardData?.Damage ?? 0;
        }

        public List<SkillCardEffectSO> CreateEffects() => new List<SkillCardEffectSO>(effects);

        #endregion

        #region 슬롯 정보

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        #endregion

        #region 소유자 정보

        public SlotOwner GetOwner() => owner;
        public bool IsFromPlayer() => owner == SlotOwner.PLAYER;

        #endregion

        #region 실행 관련

        /// <summary>
        /// 카드 실행 컨텍스트를 기반으로 카드 효과들을 실행합니다.
        /// </summary>
        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            var source = context?.Source;
            var target = context?.Target;

            Debug.Log($"[PlayerSkillCardInstance] 실행: {GetCardName()} ({source?.GetCharacterName()} -> {target?.GetCharacterName()})");

            foreach (var effect in effects)
            {
                int power = GetEffectPower(effect);
                effect.ApplyEffect(context, power);
            }

            SetCurrentCoolTime(GetMaxCoolTime());
        }

        /// <summary>
        /// 실행 불가능한 상황에서 호출 시 경고 로그를 출력합니다.
        /// </summary>
        public void ExecuteSkill()
        {
            Debug.LogWarning("[PlayerSkillCardInstance] ExecuteSkill() 호출됨 - context 없음");
        }

        /// <summary>
        /// 지정된 시전자 및 대상 캐릭터를 이용해 카드 실행 컨텍스트를 구성하고 실행합니다.
        /// </summary>
        public void ExecuteSkill(ICharacter source, ICharacter target)
        {
            var context = new DefaultCardExecutionContext(this, source, target);
            ExecuteCardAutomatically(context);
        }

        #endregion

        #region 대상 정보

        public ICharacter GetOwner(ICardExecutionContext context) => context?.Source;
        public ICharacter GetTarget(ICardExecutionContext context) => context?.Target;

        #endregion
    }
}
