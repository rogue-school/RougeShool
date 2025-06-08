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
    /// 플레이어 스킬 카드의 런타임 구현체입니다.
    /// 쿨타임, 슬롯 상태, 효과 실행 기능을 포함합니다.
    /// </summary>
    public class PlayerSkillCardRuntime : ISkillCard
    {
        /// <inheritdoc/>
        public SkillCardData CardData { get; private set; }

        private readonly List<SkillCardEffectSO> effects;
        private readonly SlotOwner owner = SlotOwner.PLAYER;

        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;

        private int currentCoolTime;

        /// <summary>
        /// 생성자: 카드 데이터와 이펙트 목록을 받아 초기화합니다.
        /// </summary>
        public PlayerSkillCardRuntime(SkillCardData data, List<SkillCardEffectSO> effects)
        {
            if (data == null)
            {
                Debug.LogError("[PlayerSkillCardRuntime] SkillCardData가 null입니다.");
                CardData = new SkillCardData("Unnamed", "No description", null, 0, 0);
                this.effects = new List<SkillCardEffectSO>();
            }
            else
            {
                CardData = data;
                this.effects = effects ?? new List<SkillCardEffectSO>();
            }

            currentCoolTime = 0; // 시작 시 쿨타임 없음
        }

        #region === 카드 메타 정보 ===

        public string GetCardName() => CardData?.Name ?? "[Unnamed]";
        public string GetDescription() => CardData?.Description ?? "[No Description]";
        public Sprite GetArtwork() => CardData?.Artwork;
        public int GetEffectPower(SkillCardEffectSO effect) => CardData?.Damage ?? 0;
        public List<SkillCardEffectSO> CreateEffects() => new List<SkillCardEffectSO>(effects);

        #endregion

        #region === 슬롯 관련 ===

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        #endregion

        #region === 소유자 정보 ===

        public SlotOwner GetOwner() => owner;
        public bool IsFromPlayer() => true;

        #endregion

        #region === 쿨타임 관련 ===

        public int GetMaxCoolTime() => CardData?.CoolTime ?? 0;
        public int GetCurrentCoolTime() => currentCoolTime;
        public void SetCurrentCoolTime(int value) => currentCoolTime = Mathf.Max(0, value);

        /// <summary>
        /// 쿨타임을 최대값으로 설정하여 사용 불가능 상태로 만듭니다.
        /// </summary>
        public void StartCooldown()
        {
            int max = GetMaxCoolTime();
            SetCurrentCoolTime(max);
            Debug.Log($"[PlayerSkillCardRuntime] {GetCardName()} 쿨타임 시작: {max}");
        }

        /// <summary>
        /// 쿨타임을 1 감소시킵니다.
        /// </summary>
        public void ReduceCooldown()
        {
            if (currentCoolTime > 0)
            {
                currentCoolTime--;
                Debug.Log($"[PlayerSkillCardRuntime] {GetCardName()} 쿨타임 감소: {currentCoolTime}");
            }
        }

        #endregion

        #region === 실행 ===

        /// <summary>
        /// 실행 컨텍스트 없이 호출 시 경고 로그를 출력합니다.
        /// </summary>
        public void ExecuteSkill()
        {
            Debug.LogWarning("[PlayerSkillCardRuntime] ExecuteSkill()는 source/target 필요");
        }

        /// <summary>
        /// 소스와 타겟 캐릭터를 이용해 카드 효과를 실행합니다.
        /// </summary>
        public void ExecuteSkill(ICharacter source, ICharacter target)
        {
            var context = new DefaultCardExecutionContext(this, source, target);
            ExecuteCardAutomatically(context);
        }

        /// <summary>
        /// 지정된 실행 컨텍스트를 기반으로 카드 효과를 자동 실행합니다.
        /// </summary>
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
                    Debug.LogWarning($"[PlayerSkillCardRuntime] {GetCardName()} - 효과 명령 생성 실패");
                }
            }

            StartCooldown(); // 효과 실행 후 쿨타임 시작
        }

        #endregion

        #region === 대상 정보 ===

        public ICharacter GetOwner(ICardExecutionContext context) => context?.GetPlayer();
        public ICharacter GetTarget(ICardExecutionContext context) => context?.GetEnemy();

        #endregion
    }
}
