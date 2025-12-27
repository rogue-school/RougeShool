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
        private Game.VFXSystem.Manager.VFXManager vfxManager;
        private static EffectCommandFactory effectFactory = new();
        
        // 데미지 오버라이드 (캐릭터별 데미지 설정용, -1이면 기본값 사용)
        private int damageOverride = -1;
        
        #endregion
        
        #region 생성자
        
        /// <summary>
        /// 스킬카드 생성자
        /// </summary>
        /// <param name="definition">카드 정의</param>
        /// <param name="owner">소유자</param>
        /// <param name="audioManager">오디오 매니저</param>
        /// <param name="vfxManager">VFX 매니저 (옵셔널)</param>
        /// <param name="damageOverride">데미지 오버라이드 (옵셔널, -1이면 기본값 사용)</param>
        public SkillCard(SkillCardDefinition definition, Owner owner, IAudioManager audioManager, Game.VFXSystem.Manager.VFXManager vfxManager = null, int damageOverride = -1)
        {
            this.definition = definition;
            this.owner = owner;
            this.audioManager = audioManager;
            this.vfxManager = vfxManager;
            this.damageOverride = damageOverride;
            
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
        /// 효과 명령들을 설정합니다. (Strategy 패턴 적용)
        /// </summary>
        private void SetupEffectCommands()
        {
            effectCommands.Clear();

            // 데미지 구성 처리 (출혈 효과가 없을 때만)
            AddDamageCommandIfNeeded();

            // 효과 구성 처리 (Strategy 패턴 사용)
            if (definition.configuration.hasEffects)
            {
                foreach (var effectConfig in definition.configuration.effects)
                {
                    if (effectConfig.effectSO == null) continue;

                    var command = effectFactory.CreateCommand(effectConfig);
                    if (command != null)
                    {
                        effectCommands.Add(command);
                    }
                }
            }

            // 실행 순서로 정렬
            SortCommandsByExecutionOrder();
        }

        /// <summary>
        /// 필요 시 데미지 명령을 추가합니다.
        /// </summary>
        private void AddDamageCommandIfNeeded()
        {
            if (!definition.configuration.hasDamage) return;

            var damageConfig = definition.configuration.damageConfig;
            
            // 데미지 오버라이드가 있으면 사용, 없으면 기본값 사용
            int finalDamage = damageOverride >= 0 ? damageOverride : damageConfig.baseDamage;

            bool useRandomDamage = damageOverride < 0 && damageConfig.useRandomDamage;

            var damageCommand = new DamageEffectCommand(
                finalDamage,
                damageConfig.hits,
                damageConfig.ignoreGuard,
                damageConfig.ignoreCounter,
                useRandomDamage,
                damageConfig.minDamage,
                damageConfig.maxDamage
            );
            effectCommands.Add(damageCommand);
        }

        /// <summary>
        /// 출혈 효과가 있는지 확인합니다.
        /// </summary>
        private bool HasBleedEffect()
        {
            if (!definition.configuration.hasEffects) return false;

            foreach (var effectConfig in definition.configuration.effects)
            {
                if (effectConfig.effectSO is BleedEffectSO)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 명령들을 실행 순서로 정렬합니다.
        /// </summary>
        private void SortCommandsByExecutionOrder()
        {
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
            // 데미지 효과의 경우 데미지 설정에서 가져오기 (오버라이드 포함)
            if (effect is DamageEffectSO && definition.configuration.hasDamage)
            {
                return GetBaseDamage();
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

        /// <summary>
        /// 카드의 기본 데미지를 반환합니다 (데미지 오버라이드 포함).
        /// </summary>
        /// <returns>기본 데미지 값 (오버라이드가 있으면 오버라이드 값, 없으면 카드 정의의 기본 데미지)</returns>
        public int GetBaseDamage()
        {
            if (!definition.configuration.hasDamage)
                return 0;

            var damageConfig = definition.configuration.damageConfig;
            
            // 데미지 오버라이드가 있으면 사용, 없으면 기본값 사용
            return damageOverride >= 0 ? damageOverride : damageConfig.baseDamage;
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
            AudioClip sfxClip = null;
            GameObject visualEffectPrefab = null;
            
            // 데미지 설정에서 이펙트/사운드 가져오기 (우선순위 1)
            if (definition.configuration.hasDamage)
            {
                var damageConfig = definition.configuration.damageConfig;
                sfxClip = damageConfig.sfxClip;
                visualEffectPrefab = damageConfig.visualEffectPrefab;
            }
            
            // 데미지 설정에 없으면 presentation 섹션에서 가져오기 (우선순위 2)
            if (sfxClip == null && definition.presentation != null)
            {
                sfxClip = definition.presentation.sfxClip;
            }
            
            if (visualEffectPrefab == null && definition.presentation != null)
            {
                visualEffectPrefab = definition.presentation.visualEffectPrefab;
            }
            
            // 사운드 재생 (즉시, 풀링 우선)
            if (sfxClip != null)
            {
                PlaySFXPooled(sfxClip);
            }
            
            // 비주얼 이펙트 생성 (즉시)
            if (visualEffectPrefab != null)
            {
                CreateVisualEffectFromPrefab(context, visualEffectPrefab);
            }
            else if (definition.configuration.hasDamage)
            {
                // 데미지가 있는 카드인데 VFX가 없으면 경고만 출력 (기존 동작 유지)
                GameLogger.LogWarning("[SkillCard] 비주얼 이펙트 프리팹이 설정되지 않음", GameLogger.LogCategory.SkillCard);
            }
            
            // 데미지가 없는 카드는 가드 이펙트를 여기서 재생하지 않음
            // 가드 차단 이펙트는 실제로 가드가 차단할 때만 재생됨:
            // - 데미지: CharacterBase.TakeDamage()에서 가드 차단 시 재생
            // - 상태이상: BleedEffectCommand 등에서 가드 차단 시 재생
        }
        
        private void PlaySFXPooled(AudioClip clip)
        {
            if (audioManager == null || clip == null) return;
            // 전역 오디오 풀을 통한 재생으로 동시 재생/우선순위 대응
            audioManager.PlaySFXWithPool(clip, 0.9f);
        }
        
        /// <summary>
        /// 프리팹을 직접 받아서 비주얼 이펙트를 생성합니다.
        /// </summary>
        private void CreateVisualEffectFromPrefab(ICardExecutionContext context, GameObject visualEffectPrefab)
        {
            var target = context.Target;
            var targetTransform = (target as MonoBehaviour)?.transform;
            if (targetTransform == null)
            {
                GameLogger.LogError("[SkillCard] 대상 Transform이 null입니다", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 반격 버프 확인: 대상이 반격 버프를 가지고 있으면 타겟에게 이펙트를 재생하지 않음
            // (반격 이펙트는 DamageEffectCommand에서 공격자에게 재생됨)
            bool targetHasCounter = false;
            if (target is Game.CharacterSystem.Core.CharacterBase targetCharacter)
            {
                targetHasCounter = targetCharacter.HasEffect<Game.SkillCardSystem.Effect.CounterBuff>();
            }

            if (targetHasCounter)
            {
                return;
            }

            // VFX 매니저를 통한 이펙트 생성
            if (vfxManager != null)
            {
                // 캐릭터의 시각적 중심에서 이펙트 재생
                var effectInstance = vfxManager.PlayEffectAtCharacterCenter(visualEffectPrefab, targetTransform);
                if (effectInstance == null)
                {
                    GameLogger.LogError("[SkillCard] 이펙트 인스턴스 생성 실패", GameLogger.LogCategory.SkillCard);
                }
            }
        }
        
        /// <summary>
        /// DamageConfiguration에서 비주얼 이펙트를 생성합니다 (기존 호환성).
        /// </summary>
        private void CreateVisualEffect(ICardExecutionContext context, DamageConfiguration damageConfig)
        {
            if (damageConfig?.visualEffectPrefab != null)
            {
                CreateVisualEffectFromPrefab(context, damageConfig.visualEffectPrefab);
            }
            else
            {
                GameLogger.LogError("[SkillCard] VFXManager를 찾을 수 없습니다 - DI 바인딩 확인 필요", GameLogger.LogCategory.SkillCard);
            }
        }

        // 주의: PlayGuardEffectIfActive 메서드는 제거되었습니다.
        // 가드 적용 이펙트는 GuardEffectCommand에서 가드 버프 적용 시에만 재생됩니다.
        // 가드 차단 이펙트는 실제로 가드가 차단할 때만 재생됩니다:
        // - 데미지 차단: CharacterBase.TakeDamage()에서 재생
        // - 상태이상 차단: BleedEffectCommand.PlayGuardBlockEffect() 등에서 재생
        
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
        /// 연출 설정을 반환합니다 (호환성을 위해 유지, 데미지 설정에서 반환).
        /// </summary>
        /// <returns>데미지 설정의 연출 정보</returns>
        public CardPresentation GetPresentation()
        {
            // 호환성을 위해 빈 CardPresentation 반환 (실제 사용은 DamageConfiguration 사용)
            return definition.configuration.hasDamage ? 
                new CardPresentation { sfxClip = definition.configuration.damageConfig.sfxClip, visualEffectPrefab = definition.configuration.damageConfig.visualEffectPrefab } : 
                new CardPresentation();
        }
        
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
