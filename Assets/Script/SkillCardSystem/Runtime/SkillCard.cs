using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.CoreSystem.Utility;
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
    public class SkillCard : ISkillCard, IAttackPowerStackProvider
    {
        #region 필드
        
        private SkillCardDefinition definition;
        private Owner owner;
        private List<ICardEffectCommand> effectCommands = new();
        private const int MaxAttackPowerStacks = 5;
        
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
            
            // 데미지 구성이 있으면 데미지 명령 추가 (단, 출혈 효과가 있으면 제외)
            if (definition.configuration.hasDamage)
            {
                // 출혈 효과가 있는지 확인
                bool hasBleedEffect = false;
                if (definition.configuration.hasEffects)
                {
                    foreach (var effectConfig in definition.configuration.effects)
                    {
                        if (effectConfig.effectSO is BleedEffectSO)
                        {
                            hasBleedEffect = true;
                            break;
                        }
                    }
                }
                
                // 출혈 효과가 없을 때만 즉시 데미지 추가
                if (!hasBleedEffect)
                {
                    var damageConfig = definition.configuration.damageConfig;
                    var damageCommand = new DamageEffectCommand(
                        damageConfig.baseDamage,
                        damageConfig.hits,
                        damageConfig.ignoreGuard,
                        damageConfig.ignoreCounter
                    );
                    effectCommands.Add(damageCommand);
                }
            }
            
            // 효과 구성이 있으면 효과 명령들 추가
            if (definition.configuration.hasEffects)
            {
                foreach (var effectConfig in definition.configuration.effects)
                {
                    if (effectConfig.effectSO != null)
                    {
                        var customSettings = effectConfig.useCustomSettings ? effectConfig.customSettings : null;

                        // 출혈 효과의 커스텀 설정(수치/지속)을 반영
                        if (effectConfig.effectSO is BleedEffectSO)
                        {
                            if (effectConfig.useCustomSettings && customSettings != null)
                            {
                                // 커스텀 출혈 수치/지속 사용
                                var bleedCommand = new BleedEffectCommand(
                                    customSettings.bleedAmount,
                                    customSettings.bleedDuration,
                                    effectConfig.effectSO.GetIcon()
                                );
                                effectCommands.Add(bleedCommand);
                                continue;
                            }
                        }
                        
                        // 카드 사용 스택 효과의 커스텀 설정을 반영
                        if (effectConfig.effectSO is CardUseStackEffectSO cardUseStackEffect)
                        {
                            if (effectConfig.useCustomSettings && customSettings != null)
                            {
                                // 커스텀 카드 사용 스택 설정 사용
                                var cardUseStackCommand = cardUseStackEffect.CreateEffectCommand(customSettings);
                                if (cardUseStackCommand != null)
                                {
                                    effectCommands.Add(cardUseStackCommand);
                                    continue;  // ← 중요: continue 추가!
                                }
                            }
                        }
                        
                        // 반격 효과의 커스텀 설정을 반영
                        if (effectConfig.effectSO is CounterEffectSO)
                        {
                            if (effectConfig.useCustomSettings && customSettings != null)
                            {
                                // 커스텀 반격 지속 시간 사용
                                var counterCommand = new CounterEffectCommand(
                                    customSettings.counterDuration,
                                    effectConfig.effectSO.GetIcon()
                                );
                                effectCommands.Add(counterCommand);
                                continue;
                            }
                        }
                        
                        // 가드 효과의 커스텀 설정을 반영 (SO를 통해 생성하여 VFX 프리팹 전달 보장)
                        if (effectConfig.effectSO is GuardEffectSO guardEffectSO)
                        {
                            if (effectConfig.useCustomSettings && customSettings != null)
                            {
                                // 커스텀 가드 지속 시간 사용하되 VFX 프리팹은 SO에서 가져오기
                                var guardCommand = new GuardEffectCommand(customSettings.guardDuration, guardEffectSO.visualEffectPrefab);
                                effectCommands.Add(guardCommand);
                                continue;
                            }
                        }

                        // 스턴 효과의 커스텀 설정을 반영
                        if (effectConfig.effectSO is StunEffectSO)
                        {
                            if (effectConfig.useCustomSettings && customSettings != null)
                            {
                                var stunCommand = new StunEffectCommand(customSettings.stunDuration, effectConfig.effectSO.GetIcon());
                                effectCommands.Add(stunCommand);
                                continue;
                            }
                        }
                        
                        // 데미지 효과의 커스텀 설정을 반영
                        if (effectConfig.effectSO is DamageEffectSO)
                        {
                            if (effectConfig.useCustomSettings && customSettings != null)
                            {
                                // 커스텀 데미지 설정 사용
                                var damageCommand = new DamageEffectCommand(
                                    customSettings.damageAmount,
                                    customSettings.damageHits,
                                    customSettings.ignoreGuard,
                                    customSettings.ignoreCounter
                                );
                                effectCommands.Add(damageCommand);
                                continue;
                            }
                        }
                        
                        // 치유 효과의 커스텀 설정을 반영 (SO를 통해 생성하여 VFX 프리팹 전달 보장)
                        if (effectConfig.effectSO is HealEffectSO healEffectSO)
                        {
                            if (effectConfig.useCustomSettings && customSettings != null)
                            {
                                var healCommand = healEffectSO.CreateEffectCommand(customSettings);
                                if (healCommand != null)
                                {
                                    effectCommands.Add(healCommand);
                                    continue;
                                }
                            }
                        }

                        // 기타 효과는 기존 로직(파워 계산 후 SO에서 생성)
                        var power = GetEffectPower(effectConfig.effectSO);
                        var command = effectConfig.effectSO.CreateEffectCommand(power);
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
            GameLogger.LogWarning("[SkillCard] ExecuteSkill() without parameters is not supported in new structure. Use ExecuteSkill(source, target) instead.", GameLogger.LogCategory.SkillCard);
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
                GameLogger.LogWarning("[SkillCard] context 또는 대상 타입 오류", GameLogger.LogCategory.SkillCard);
                return;
            }
            
            // 스턴 상태면 스킬 사용 불가
            if ((context.Source as CharacterBase).HasEffect<StunDebuff>())
            {
                GameLogger.LogInfo($"[SkillCard] 스턴 상태로 스킬 사용 불가: {context.Source.GetCharacterName()}", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (targetChar.IsDead())
            {
                GameLogger.LogWarning("[SkillCard] 대상자가 이미 사망했습니다.", GameLogger.LogCategory.SkillCard);
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
            
            // 사운드 재생 (즉시, 풀링 우선)
            if (presentation.sfxClip != null)
            {
                PlaySFXPooled(presentation.sfxClip);
            }
            
            // 비주얼 이펙트 생성 (즉시)
            if (presentation.visualEffectPrefab != null)
            {
                CreateVisualEffect(context, presentation);
            }
        }
        
        private void PlaySFXPooled(AudioClip clip)
        {
            if (audioManager == null || clip == null) return;
            // 전역 오디오 풀을 통한 재생으로 동시 재생/우선순위 대응
            audioManager.PlaySFXWithPool(clip, 0.9f);
        }
        
        private void CreateVisualEffect(ICardExecutionContext context, CardPresentation presentation)
        {
            var target = context.Target;
            var targetTransform = (target as MonoBehaviour)?.transform;
            if (targetTransform == null) return;

            // VFX 매니저를 통한 이펙트 생성 (Object Pooling 적용)
            var vfxManager = UnityEngine.Object.FindFirstObjectByType<Game.VFXSystem.Manager.VFXManager>();
            if (vfxManager != null && presentation.visualEffectPrefab != null)
            {
                vfxManager.PlayEffect(presentation.visualEffectPrefab, targetTransform.position);
            }
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

        #region === IAttackPowerStackProvider 구현 ===

        public int GetAttackPowerStack() => definition.GetAttackPowerStacks();

        public void IncrementAttackPowerStack(int max)
        {
            int limit = max > 0 ? max : MaxAttackPowerStacks;
            definition.IncrementAttackPowerStacks(limit);
        }
        
        #endregion
    }
}
