using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effect;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Context;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Interface;
using Zenject;
using Game.CharacterSystem.Core;

namespace Game.SkillCardSystem.Runtime
{
    /// <summary>
    /// 스킬카드의 런타임 인스턴스를 나타내는 클래스입니다.
    /// 카드 실행, 연출 처리, 효과 관리를 담당합니다.
    /// MonoBehaviour 제거로 성능 최적화 완료.
    /// </summary>
    public class SkillCard : ISkillCard
    {
        #region 필드
        
        private SkillCardDefinition definition;
        private Owner owner;
        private List<ICardEffectCommand> effectCommands = new();
        
        private Dictionary<SkillCardSlotPosition, SkillCardSlotPosition> handSlotMap = new();
        private Dictionary<CombatSlotPosition, CombatSlotPosition> combatSlotMap = new();
        
        // 의존성 주입 (생성자에서 주입)
        private IAudioManager audioManager;
        
        #endregion
        
        #region 생성자
        
        /// <summary>
        /// 스킬카드 생성자
        /// </summary>
        /// <param name="definition">카드 정의</param>
        /// <param name="owner">소유자</param>
        /// <param name="audioManager">오디오 매니저</param>
        public SkillCard(SkillCardDefinition definition, Owner owner, IAudioManager audioManager)
        {
            this.definition = definition;
            this.owner = owner;
            this.audioManager = audioManager;
            
            SetupEffectCommands();
        }
        
        #endregion
        
        #region === 초기화 ===
        
        /// <summary>
        /// 스킬카드를 초기화합니다. (레거시 호환성)
        /// </summary>
        /// <param name="definition">카드 정의</param>
        /// <param name="owner">소유자</param>
        public void Initialize(SkillCardDefinition definition, Owner owner)
        {
            this.definition = definition;
            this.owner = owner;
            
            SetupEffectCommands();
        }
        
        /// <summary>
        /// 효과 명령들을 설정합니다.
        /// </summary>
        private void SetupEffectCommands()
        {
            effectCommands.Clear();
            
            // 데미지 구성이 있으면 데미지 명령 추가
            if (definition.configuration.hasDamage)
            {
                var damageConfig = definition.configuration.damageConfig;
                var damageCommand = new DamageEffectCommand(
                    damageConfig.baseDamage,
                    damageConfig.hits,
                    damageConfig.ignoreGuard
                );
                effectCommands.Add(damageCommand);
            }
            
            // 효과 구성이 있으면 효과 명령들 추가
            if (definition.configuration.hasEffects)
            {
                foreach (var effectConfig in definition.configuration.effects)
                {
                    if (effectConfig.effectSO != null)
                    {
                        var customSettings = effectConfig.useCustomSettings ? effectConfig.customSettings : null;
                        var command = effectConfig.effectSO.CreateEffectCommand(0);
                        if (command != null)
                        {
                            effectCommands.Add(command);
                        }
                    }
                }
            }
            
            // 실행 순서로 정렬
            effectCommands.Sort((a, b) => 
            {
                var orderA = GetExecutionOrder(a);
                var orderB = GetExecutionOrder(b);
                return orderA.CompareTo(orderB);
            });
        }
        
        private int GetExecutionOrder(ICardEffectCommand command)
        {
            // 데미지 명령은 항상 먼저 실행
            if (command is DamageEffectCommand) return 0;
            
            // 다른 명령들은 정의에서 순서 가져오기
            foreach (var effectConfig in definition.configuration.effects)
            {
                if (effectConfig.effectSO != null)
                {
                    var testCommand = effectConfig.effectSO.CreateEffectCommand(0);
                    if (testCommand != null && testCommand.GetType() == command.GetType())
                    {
                        return effectConfig.executionOrder;
                    }
                }
            }
            
            return 999; // 기본값
        }
        
        #endregion
        
        #region === ISkillCard 구현 ===
        
        public SkillCardDefinition CardDefinition => definition;
        
        public string GetCardName() => definition?.displayName ?? "[Unnamed Card]";
        public string GetDescription() => definition?.description ?? "[No Description]";
        public Sprite GetArtwork() => definition?.artwork;
        public int GetCoolTime() => 0; // 쿨타임 제거됨
        public int GetMaxCoolTime() => 0;
        public int GetCurrentCoolTime() => 0;
        public void SetCurrentCoolTime(int value) { } // 쿨타임 제거됨
        
        public int GetEffectPower(SkillCardEffectSO effect)
        {
            // 데미지 효과의 경우 데미지 설정에서 가져오기
            if (effect is DamageEffectSO && definition.configuration.hasDamage)
            {
                return definition.configuration.damageConfig.baseDamage;
            }
            
            // 다른 효과의 경우 커스텀 설정에서 가져오기
            foreach (var effectConfig in definition.configuration.effects)
            {
                if (effectConfig.effectSO == effect)
                {
                    if (effectConfig.useCustomSettings)
                    {
                        return GetCustomEffectPower(effectConfig.customSettings, effect);
                    }
                    return 0; // 기본값
                }
            }
            
            return 0;
        }
        
        private int GetCustomEffectPower(EffectCustomSettings settings, SkillCardEffectSO effect)
        {
            if (effect is DamageEffectSO) return settings.damageAmount;
            if (effect is GuardEffectSO) return 0; // 가드 효과는 파워 없음
            if (effect is BleedEffectSO) return settings.bleedAmount;
            // 기타 효과 타입들은 기본값 반환
            
            return 0;
        }
        
        public List<SkillCardEffectSO> CreateEffects()
        {
            var effects = new List<SkillCardEffectSO>();
            
            if (definition.configuration.hasEffects)
            {
                foreach (var effectConfig in definition.configuration.effects)
                {
                    if (effectConfig.effectSO != null)
                    {
                        effects.Add(effectConfig.effectSO);
                    }
                }
            }
            
            return effects;
        }
        
        #endregion
        
        #region === 슬롯 관련 ===
        
        public void SetHandSlot(SkillCardSlotPosition slot) => handSlotMap[slot] = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlotMap.Count > 0 ? handSlotMap.Keys.GetEnumerator().Current : null;
        
        public void SetCombatSlot(CombatSlotPosition slot) => combatSlotMap[slot] = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlotMap.Count > 0 ? combatSlotMap.Keys.GetEnumerator().Current : null;
        
        #endregion
        
        #region === 소유자 정보 ===
        
        public SlotOwner GetOwner() => owner == Owner.Player ? SlotOwner.PLAYER : SlotOwner.ENEMY;
        public bool IsFromPlayer() => owner == Owner.Player;
        
        #endregion
        
        #region === 실행 관련 ===
        
        /// <summary>
        /// 스킬을 실행합니다 (소스와 타겟이 사전에 정해진 상태).
        /// </summary>
        public void ExecuteSkill()
        {
            Debug.LogWarning("[SkillCard] ExecuteSkill() without parameters is not supported in new structure. Use ExecuteSkill(source, target) instead.");
        }
        
        /// <summary>
        /// 스킬을 실행합니다.
        /// </summary>
        /// <param name="source">시전자</param>
        /// <param name="target">대상</param>
        public void ExecuteSkill(ICharacter source, ICharacter target)
        {
            var context = new DefaultCardExecutionContext(this, source, target);
            ExecuteCardAutomatically(context);
        }
        
        /// <summary>
        /// 카드를 자동 실행합니다.
        /// </summary>
        /// <param name="context">실행 컨텍스트</param>
        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            if (context?.Source is not CharacterBase || context.Target is not CharacterBase targetChar)
            {
                Debug.LogWarning("[SkillCard] context 또는 대상 타입 오류");
                return;
            }
            
            if (targetChar.IsDead())
            {
                Debug.LogWarning("[SkillCard] 대상자가 이미 사망했습니다.");
                return;
            }
            
            // 연출 시작
            StartPresentation(context);
            
            // 효과 실행
            ExecuteEffects(context);
        }
        
        /// <summary>
        /// 효과들을 실행합니다.
        /// </summary>
        /// <param name="context">실행 컨텍스트</param>
        private void ExecuteEffects(ICardExecutionContext context)
        {
            foreach (var command in effectCommands)
            {
                command.Execute(context, null); // turnManager는 null로 전달
            }
        }
        
        #endregion
        
        #region === 연출 처리 ===
        
        /// <summary>
        /// 연출을 시작합니다.
        /// </summary>
        /// <param name="context">실행 컨텍스트</param>
        private void StartPresentation(ICardExecutionContext context)
        {
            var presentation = definition.presentation;
            
            // 사운드 재생 (즉시)
            if (presentation.sfxClip != null)
            {
                PlaySFX(presentation.sfxClip);
            }
            
            // 비주얼 이펙트 생성 (즉시)
            if (presentation.visualEffectPrefab != null)
            {
                CreateVisualEffect(context, presentation);
            }
        }
        
        private void PlaySFX(AudioClip clip)
        {
            // AudioManager를 통한 사운드 재생
            if (audioManager != null)
            {
                audioManager.PlaySFX(clip);
            }
        }
        
        private void CreateVisualEffect(ICardExecutionContext context, CardPresentation presentation)
        {
            var target = context.Target;
            var targetTransform = (target as MonoBehaviour)?.transform;
            if (targetTransform == null) return;
            
            // 대상 위치에 이펙트 생성
            // TODO: MonoBehaviour가 아니므로 Instantiate 사용 불가, 다른 방법으로 이펙트 생성 필요
            // var effectInstance = Instantiate(presentation.visualEffectPrefab, targetTransform.position, Quaternion.identity);
            
            // 이펙트는 기본적으로 자동 제거되도록 설정 (이펙트 프리팹에서 처리)
            // 필요시 이펙트 프리팹에 자동 제거 컴포넌트 추가
        }
        
        #endregion
        
        #region === 대상 정보 ===
        
        /// <summary>
        /// 컨텍스트에 따라 현재 카드의 소유 캐릭터를 반환합니다.
        /// </summary>
        /// <param name="context">실행 컨텍스트</param>
        /// <returns>소유 캐릭터</returns>
        public ICharacter GetOwner(ICardExecutionContext context) =>
            owner == Owner.Player ? context.GetPlayer() : context.GetEnemy();
        
        /// <summary>
        /// 컨텍스트에 따라 현재 카드의 대상 캐릭터를 반환합니다.
        /// </summary>
        /// <param name="context">실행 컨텍스트</param>
        /// <returns>대상 캐릭터</returns>
        public ICharacter GetTarget(ICardExecutionContext context) =>
            context.Target;
        
        #endregion
        
        #region === 유틸리티 ===
        
        /// <summary>
        /// 카드 정의를 반환합니다.
        /// </summary>
        /// <returns>카드 정의</returns>
        public SkillCardDefinition GetDefinition() => definition;
        
        /// <summary>
        /// 연출 설정을 반환합니다.
        /// </summary>
        /// <returns>연출 설정</returns>
        public CardPresentation GetPresentation() => definition.presentation;
        
        /// <summary>
        /// 효과 명령을 추가합니다.
        /// </summary>
        /// <param name="command">효과 명령</param>
        /// <param name="order">실행 순서</param>
        public void AddEffectCommand(ICardEffectCommand command, int order = 0)
        {
            effectCommands.Add(command);
            SetupEffectCommands(); // 재정렬
        }
        
        #endregion
    }
}
