using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Context;

namespace Game.SkillCardSystem.Runtime
{
    /// <summary>
    /// 플레이어와 적 공용 스킬 카드 런타임 구현입니다.
    /// 효과 실행, 슬롯/쿨타임 정보, 실행 컨텍스트를 관리합니다.
    /// </summary>
    public class RuntimeSkillCard : ISkillCard
    {
        public SkillCardData CardData { get; private set; }

        private readonly List<SkillCardEffectSO> effects;
        private readonly SlotOwner owner;

        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;
        private int currentCoolTime = 0;

        public RuntimeSkillCard(SkillCardData cardData, List<SkillCardEffectSO> effects, SlotOwner owner)
        {
            CardData = cardData;
            this.effects = effects ?? new();
            this.owner = owner;
            // 카드 소유자 이름 할당 (예시: 플레이어/적 이름, 실제 생성부에서 전달 필요)
            // 예: CardData.OwnerCharacterName = "캐릭터이름";
        }

        #region === 카드 메타 정보 ===

        public string GetCardName() => CardData?.Name ?? "[Unnamed Card]";
        public string GetDescription() => CardData?.Description ?? "[No Description]";
        public Sprite GetArtwork() => CardData?.Artwork;
        public int GetCoolTime() => CardData?.CoolTime ?? 0;
        public int GetMaxCoolTime() => CardData?.CoolTime ?? 0;
        public int GetCurrentCoolTime() => currentCoolTime;
        public void SetCurrentCoolTime(int value) => currentCoolTime = Mathf.Max(0, value);

        public int GetEffectPower(SkillCardEffectSO effect) => CardData?.Damage ?? 0;

        /// <summary>
        /// 등록된 이펙트 리스트를 복사하여 반환합니다.
        /// </summary>
        public List<SkillCardEffectSO> CreateEffects() => new(effects);

        #endregion

        #region === 슬롯 관련 ===

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        #endregion

        #region === 소유자 정보 ===

        public SlotOwner GetOwner() => owner;
        public bool IsFromPlayer() => owner == SlotOwner.PLAYER;

        #endregion

        #region === 실행 관련 ===

        /// <summary>
        /// source/target 없이 호출 시 경고 출력
        /// </summary>
        public void ExecuteSkill() =>
            Debug.LogWarning("[RuntimeSkillCard] ExecuteSkill() 호출 - source/target을 직접 지정해야 합니다.");

        /// <summary>
        /// source/target을 지정하여 스킬을 실행합니다.
        /// </summary>
        public void ExecuteSkill(ICharacter source, ICharacter target)
        {
            var context = new DefaultCardExecutionContext(this, source, target);
            ExecuteCardAutomatically(context);
        }

        /// <summary>
        /// 자동 실행: 등록된 이펙트를 순차 적용하며, 쿨타임을 시작합니다.
        /// </summary>
        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            if (context?.Source is not CharacterBase || context.Target is not CharacterBase targetChar)
            {
                Debug.LogWarning("[RuntimeSkillCard] context 또는 대상 타입 오류");
                return;
            }

            if (targetChar.IsDead())
            {
                Debug.LogWarning("[RuntimeSkillCard] 대상자가 이미 사망했습니다.");
                return;
            }

            foreach (var effect in effects)
            {
                int power = GetEffectPower(effect);
                var command = effect.CreateEffectCommand(power);
                command?.Execute(context, null);
            }

            SetCurrentCoolTime(GetMaxCoolTime());
        }

        #endregion

        #region === 대상 정보 ===

        /// <summary>
        /// context에 따라 현재 카드의 소유 캐릭터를 반환합니다.
        /// </summary>
        public ICharacter GetOwner(ICardExecutionContext context) =>
            owner == SlotOwner.PLAYER ? context.GetPlayer() : context.GetEnemy();

        /// <summary>
        /// context에 따라 현재 카드의 대상 캐릭터를 반환합니다.
        /// </summary>
        public ICharacter GetTarget(ICardExecutionContext context) =>
            owner == SlotOwner.PLAYER ? context.GetEnemy() : context.GetPlayer();

        #endregion
    }
}
