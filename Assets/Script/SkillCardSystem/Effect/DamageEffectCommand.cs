using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Interface;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 데미지를 주는 효과 명령 클래스입니다.
    /// 새로운 통합 구조에서 데미지 처리를 담당합니다.
    /// </summary>
    public class DamageEffectCommand : ICardEffectCommand
    {
        private int damageAmount;
        private int hits;
        private bool ignoreGuard;
        private bool ignoreCounter;
        private bool useRandomBaseDamage;
        private int minBaseDamage;
        private int maxBaseDamage;
        private readonly IAudioManager audioManager;
        private readonly IItemService itemService;

        public DamageEffectCommand(IAudioManager audioManager, IItemService itemService)
        {
            this.audioManager = audioManager;
            this.itemService = itemService;
        }

        /// <summary>
        /// 데미지 효과 명령을 생성합니다.
        /// </summary>
        /// <param name="damageAmount">데미지량</param>
        /// <param name="hits">공격 횟수</param>
        /// <param name="ignoreGuard">가드 무시 여부</param>
        public DamageEffectCommand(int damageAmount, int hits = 1, bool ignoreGuard = false, bool ignoreCounter = false)
        {
            this.damageAmount = damageAmount;
            this.hits = hits;
            this.ignoreGuard = ignoreGuard;
            this.ignoreCounter = ignoreCounter;
            this.useRandomBaseDamage = false;
            this.minBaseDamage = 0;
            this.maxBaseDamage = 0;
            this.audioManager = null; // 의존성 주입이 아닌 경우
            this.itemService = null; // 의존성 주입이 아닌 경우
        }

        /// <summary>
        /// 데미지 효과 명령을 생성합니다 (의존성 포함).
        /// </summary>
        /// <param name="damageAmount">데미지량</param>
        /// <param name="hits">공격 횟수</param>
        /// <param name="ignoreGuard">가드 무시 여부</param>
        /// <param name="ignoreCounter">반격 무시 여부</param>
        /// <param name="audioManager">오디오 매니저 (선택적)</param>
        /// <param name="itemService">아이템 서비스 (선택적)</param>
        public DamageEffectCommand(int damageAmount, int hits, bool ignoreGuard, bool ignoreCounter, IAudioManager audioManager, IItemService itemService)
        {
            this.damageAmount = damageAmount;
            this.hits = hits;
            this.ignoreGuard = ignoreGuard;
            this.ignoreCounter = ignoreCounter;
            this.useRandomBaseDamage = false;
            this.minBaseDamage = 0;
            this.maxBaseDamage = 0;
            this.audioManager = audioManager;
            this.itemService = itemService;
        }

        /// <summary>
        /// 랜덤 데미지 구간을 포함하는 데미지 효과 명령을 생성합니다.
        /// </summary>
        /// <param name="damageAmount">기본 데미지(랜덤을 사용하지 않을 때)</param>
        /// <param name="hits">공격 횟수</param>
        /// <param name="ignoreGuard">가드 무시 여부</param>
        /// <param name="ignoreCounter">반격 무시 여부</param>
        /// <param name="useRandomBaseDamage">랜덤 데미지 사용 여부</param>
        /// <param name="minBaseDamage">랜덤 데미지 최소값</param>
        /// <param name="maxBaseDamage">랜덤 데미지 최대값</param>
        public DamageEffectCommand(int damageAmount, int hits, bool ignoreGuard, bool ignoreCounter, bool useRandomBaseDamage, int minBaseDamage, int maxBaseDamage)
        {
            this.damageAmount = damageAmount;
            this.hits = hits;
            this.ignoreGuard = ignoreGuard;
            this.ignoreCounter = ignoreCounter;
            this.useRandomBaseDamage = useRandomBaseDamage;
            this.minBaseDamage = minBaseDamage;
            this.maxBaseDamage = maxBaseDamage;
            this.audioManager = null;
            this.itemService = null;
        }

        /// <summary>
        /// 데미지 효과를 실행합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="turnManager">전투 턴 관리자</param>
        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Target == null)
            {
                Debug.LogWarning("[DamageEffectCommand] 대상이 null입니다. 데미지 적용 실패.");
                return;
            }

            var target = context.Target;
            var source = context.Source;
            var totalDamage = 0;

            // 0) 기본 데미지 결정 (랜덤/고정)
            int baseDamageValue = GetBaseDamageValue();

            // 1) 공격력 스택 버프 확인(시전자 기준) → 추가 피해량 = 스택 수
            int attackBonus = 0;
            if (context.Card is IAttackPowerStackProvider stackProvider)
            {
                int currentStacks = stackProvider.GetAttackPowerStack();

                // 방식 1: 선형 증가 (현재 방식)
                attackBonus = currentStacks;

                // 방식 2: 배수 증가 (주석 해제하여 사용 가능)
                // attackBonus = damageAmount * currentStacks;

                // 방식 3: 지수적 증가 (주석 해제하여 사용 가능)
                // attackBonus = damageAmount * (int)Mathf.Pow(2, currentStacks) - damageAmount;

                // 방식 4: 제곱 증가 (주석 해제하여 사용 가능)
                // attackBonus = currentStacks * currentStacks;

                // GameLogger.LogInfo($"[DamageEffectCommand] 스택 기반 데미지 계산 - 기본: {damageAmount}, 스택: {currentStacks}, 보너스: {attackBonus}", 
                //    GameLogger.LogCategory.Combat);
            }

            // 1.5) 아이템 공격력 버프 확인(시전자 기준)
            int itemAttackBonus = 0;
            if (source != null)
            {
                // ICharacter 인터페이스를 통해 GetBuffs() 호출
                var attackBuffEffects = source.GetBuffs();
                if (attackBuffEffects != null)
                {
                    foreach (var effect in attackBuffEffects)
                    {
                        if (effect is Game.ItemSystem.Effect.AttackPowerBuffEffect attackBuff)
                        {
                            int bonus = attackBuff.GetAttackPowerBonus();
                            itemAttackBonus += bonus;
                            GameLogger.LogInfo($"[DamageEffectCommand] 공격력 물약 버프 적용: +{bonus} (총 보너스: {itemAttackBonus})", GameLogger.LogCategory.Combat);
                        }
                    }
                }
                else
                {
                    GameLogger.LogWarning($"[DamageEffectCommand] source.GetBuffs()가 null을 반환했습니다. source: {source.GetCharacterName()}", GameLogger.LogCategory.Combat);
                }
            }
            else
            {
                GameLogger.LogWarning("[DamageEffectCommand] source가 null입니다. 공격력 물약 버프를 확인할 수 없습니다.", GameLogger.LogCategory.Combat);
            }

            // 2) 강화 단계 데미지 보너스 확인 (플레이어가 시전자이고 플레이어 카드일 때만 적용)
            int starBonus = 0;
            IItemService service = itemService;
            if (service == null)
            {
                // 실행 시점에 1회 안전 조회 (Update 루프 아님)
                var svcImpl = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Service.ItemService>();
                if (svcImpl != null) service = svcImpl as IItemService;
            }
            if (service != null && context.Card != null && source != null && 
                source.IsPlayerControlled() && context.Card.IsFromPlayer())
            {
                string skillId = context.Card.GetCardName();
                starBonus = service.GetSkillDamageBonus(skillId);
            }

            int effectiveDamage = baseDamageValue + attackBonus + itemAttackBonus + starBonus;

            // 분신 버프 확인: 시전자가 분신 버프를 가지고 있으면 공격 횟수 2배
            bool sourceHasClone = false;
            int finalHits = hits;
            if (source is Game.CharacterSystem.Core.CharacterBase sourceCharacterBase)
            {
                sourceHasClone = sourceCharacterBase.HasEffect<CloneBuff>() && sourceCharacterBase.GetCloneHP() > 0;
            }
            
            if (sourceHasClone)
            {
                finalHits = hits * 2;
                GameLogger.LogInfo($"[DamageEffectCommand] 분신 버프로 공격 횟수 2배 적용: {hits} → {finalHits}", GameLogger.LogCategory.Combat);
            }

            // 반격 버프 처리: 대상이 CounterBuff 보유 시, 들어오는 피해의 100%를 공격자에게 반사
            // 대상은 데미지를 받지 않고, 공격자가 원래 데미지의 100%를 받음
            // 단일 히트/다단 히트 모두 동일 규칙 적용
            bool targetHasCounter = false;
            if (target is Game.CharacterSystem.Core.CharacterBase cb)
            {
                targetHasCounter = cb.HasEffect<CounterBuff>();
            }
            if (ignoreCounter)
            {
                targetHasCounter = false;
            }

            // 실드 브레이커 디버프 확인: 공격자가 실드 브레이커 효과를 가지고 있으면 반격 무시
            bool hasShieldBreaker = false;
            if (source is Game.CharacterSystem.Core.CharacterBase sourceCharacter)
            {
                var buffs = sourceCharacter.GetBuffs();
                foreach (var effect in buffs)
                {
                    if (effect is Game.ItemSystem.Effect.ShieldBreakerDebuffEffect shieldBreaker)
                    {
                        hasShieldBreaker = shieldBreaker.IsShieldBreakerActive();
                        break;
                    }
                }
            }

            if (hasShieldBreaker)
            {
                targetHasCounter = false;
            }

            // 다단 히트 처리 (시간 간격을 두고 공격)
            // 분신 버프가 있으면 finalHits가 2배가 되므로, 단일 히트도 다단 히트로 처리됨
            if (finalHits > 1)
            {
                // 코루틴으로 다단 히트 실행
                var sourceMono = source as MonoBehaviour;
                if (sourceMono != null)
                {
                    sourceMono.StartCoroutine(ExecuteMultiHitDamage(context, finalHits));
                }
                else
                {
                    // MonoBehaviour가 아닌 경우 즉시 실행
                    ExecuteImmediateDamage(target, finalHits);
                }
            }
            else
            {
                // 단일 히트는 즉시 실행
                if (targetHasCounter && source != null)
                {
                    // 반격: 대상은 데미지를 받지 않고, 공격자가 원래 데미지의 100%를 받음
                    int reflect = effectiveDamage;
                    if (reflect > 0)
                    {
                        source.TakeDamageIgnoreGuard(reflect);
                        totalDamage += reflect;
                        GameLogger.LogDebug($"[DamageEffectCommand] 반격: 대상 0 수신, 공격자 {reflect} 반사", GameLogger.LogCategory.Combat);
                        
                        // 반격 이펙트 재생: 적의 공격 이펙트가 적에게 반사되어 나타남
                        PlayCounterAttackEffect(context, source);
                        
                        // 시공의 폭풍 버프 추적: 반격으로 플레이어가 적에게 데미지를 입힐 때
                        // 반격의 경우: target(플레이어)가 반격 버프를 가지고 있고, source(적)가 데미지를 받음
                        // 이것도 플레이어가 적에게 데미지를 입힌 것으로 간주
                        // 시공의 폭풍 체력을 소모하는 것도 데미지로 인정
                        if (target != null && target.IsPlayerControlled() && source is Game.CharacterSystem.Core.CharacterBase enemyCharacterCounter)
                        {
                            // 데미지 적용 전 시공의 폭풍 체력 확인
                            int stormHPBefore = enemyCharacterCounter.GetStormHP();
                            
                            int actualDamageToNormalHP = CalculateActualDamageToNormalHP(source, reflect, true); // 반격은 가드 무시
                            
                            // 데미지 적용 후 시공의 폭풍 체력 확인
                            int stormHPAfter = enemyCharacterCounter.GetStormHP();
                            int stormHPDamage = Mathf.Max(0, stormHPBefore - stormHPAfter);
                            
                            var stormDebuff = enemyCharacterCounter.GetEffect<StormOfSpaceTimeDebuff>();
                            if (stormDebuff != null)
                            {
                                int totalStormDamage = stormHPDamage + actualDamageToNormalHP;
                                if (totalStormDamage > 0)
                                {
                                    stormDebuff.AddDamage(totalStormDamage);
                                    GameLogger.LogInfo($"[DamageEffectCommand] 시공의 폭풍 데미지 누적 (반격): {totalStormDamage} (시공의 폭풍 체력 소모: {stormHPDamage}, 일반 체력 데미지: {actualDamageToNormalHP}, 총 누적: {stormDebuff.AccumulatedDamage}/{stormDebuff.TargetDamage}, 원래 데미지: {reflect}, 시공의 폭풍 체력: {stormHPBefore} → {stormHPAfter})", GameLogger.LogCategory.Combat);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // 시공의 폭풍 버프 추적을 위해 데미지 적용 전 시공의 폭풍 체력 확인
                    int stormHPBefore = 0;
                    if (source != null && source.IsPlayerControlled() && target is Game.CharacterSystem.Core.CharacterBase enemyCharacterBefore)
                    {
                        stormHPBefore = enemyCharacterBefore.GetStormHP();
                    }
                    
                    // 실제 일반 체력에 들어갈 데미지 계산 (시공의 폭풍 체력, 분신 체력 고려)
                    int actualDamageToNormalHP = CalculateActualDamageToNormalHP(target, effectiveDamage, ignoreGuard);
                    
                    ApplyDamageCustom(target, effectiveDamage);
                    totalDamage += effectiveDamage;
                    
                    // 시공의 폭풍 버프 추적: 플레이어가 적에게 데미지를 입힐 때
                    // 시공의 폭풍은 적(본인)에게 적용되는 버프이므로, 플레이어가 적에게 데미지를 입힐 때 적의 버프를 확인해야 함
                    // 시공의 폭풍 체력을 소모하는 것도 데미지로 인정 (시공의 폭풍 체력 소모량 + 실제 일반 체력에 들어간 데미지)
                    if (source != null && source.IsPlayerControlled() && target is Game.CharacterSystem.Core.CharacterBase enemyCharacter)
                    {
                        var stormDebuff = enemyCharacter.GetEffect<StormOfSpaceTimeDebuff>();
                        if (stormDebuff != null)
                        {
                            // 시공의 폭풍 체력 소모량 계산 (데미지 적용 전후 비교)
                            int stormHPAfter = enemyCharacter.GetStormHP();
                            int stormHPDamage = Mathf.Max(0, stormHPBefore - stormHPAfter);
                            
                            // 시공의 폭풍 체력 소모량 + 실제 일반 체력에 들어간 데미지
                            int totalStormDamage = stormHPDamage + actualDamageToNormalHP;
                            
                            if (totalStormDamage > 0)
                            {
                                stormDebuff.AddDamage(totalStormDamage);
                                GameLogger.LogInfo($"[DamageEffectCommand] 시공의 폭풍 데미지 누적: {totalStormDamage} (시공의 폭풍 체력 소모: {stormHPDamage}, 일반 체력 데미지: {actualDamageToNormalHP}, 총 누적: {stormDebuff.AccumulatedDamage}/{stormDebuff.TargetDamage}, 원래 데미지: {effectiveDamage}, 시공의 폭풍 체력: {stormHPBefore} → {stormHPAfter})", GameLogger.LogCategory.Combat);
                            }
                            else
                            {
                                GameLogger.LogDebug($"[DamageEffectCommand] 시공의 폭풍 데미지 누적 스킵: totalStormDamage={totalStormDamage} (시공의 폭풍 체력 소모: {stormHPDamage}, 일반 체력 데미지: {actualDamageToNormalHP}, 원래 데미지: {effectiveDamage}, 시공의 폭풍 체력: {stormHPBefore} → {stormHPAfter})", GameLogger.LogCategory.Combat);
                            }
                        }
                    }
                }
            }

            Debug.Log($"[DamageEffectCommand] {source?.GetCharacterName()} → {target.GetCharacterName()} 총 데미지: {totalDamage} (공격 횟수: {hits})");
        }

        /// <summary>
        /// 효과 실행 가능 여부를 확인합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <returns>실행 가능 여부</returns>
        public bool CanExecute(ICardExecutionContext context)
        {
            if (context?.Target == null) return false;

            var target = context.Target;

            // 대상이 이미 사망했으면 실행 불가
            if (target.IsDead()) return false;

            return true;
        }

        /// <summary>
        /// 효과의 비용을 반환합니다.
        /// </summary>
        /// <returns>비용</returns>
        public int GetCost()
        {
            return 0; // 데미지 효과는 비용 없음
        }

        /// <summary>
        /// 기본 데미지 값을 반환합니다. 랜덤 데미지를 사용하는 경우 최소/최대 범위 내에서 무작위 값을 반환합니다.
        /// </summary>
        /// <returns>기본 데미지 값</returns>
        private int GetBaseDamageValue()
        {
            if (!useRandomBaseDamage)
            {
                return damageAmount;
            }

            int min = Mathf.Min(minBaseDamage, maxBaseDamage);
            int max = Mathf.Max(minBaseDamage, maxBaseDamage);

            if (max <= min)
            {
                return min;
            }

            return UnityEngine.Random.Range(min, max + 1);
        }

        /// <summary>
        /// 데미지량을 반환합니다.
        /// </summary>
        /// <returns>데미지량</returns>
        public int GetDamageAmount() => damageAmount;

        /// <summary>
        /// 공격 횟수를 반환합니다.
        /// </summary>
        /// <returns>공격 횟수</returns>
        public int GetHits() => hits;

        /// <summary>
        /// 가드 무시 여부를 반환합니다.
        /// </summary>
        /// <returns>가드 무시 여부</returns>
        public bool IsIgnoreGuard() => ignoreGuard;

        /// <summary>
        /// 다단 히트 데미지를 시간 간격을 두고 실행합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="hitCount">히트 수</param>
        private System.Collections.IEnumerator ExecuteMultiHitDamage(ICardExecutionContext context, int hitCount)
        {
            var target = context.Target;
            var source = context.Source;
            bool targetHasCounter = false;
            if (target is Game.CharacterSystem.Core.CharacterBase cb)
            {
                targetHasCounter = cb.HasEffect<CounterBuff>();
            }

            // 실드 브레이커 디버프 확인: 공격자가 실드 브레이커 효과를 가지고 있으면 반격 무시
            bool hasShieldBreaker = false;
            if (source is Game.CharacterSystem.Core.CharacterBase sourceCharacter)
            {
                var buffs = sourceCharacter.GetBuffs();
                foreach (var effect in buffs)
                {
                    if (effect is Game.ItemSystem.Effect.ShieldBreakerDebuffEffect shieldBreaker)
                    {
                        hasShieldBreaker = shieldBreaker.IsShieldBreakerActive();
                        break;
                    }
                }
            }

            if (hasShieldBreaker)
            {
                targetHasCounter = false;
                GameLogger.LogInfo($"[DamageEffectCommand] 다단 히트에서 실드 브레이커 효과로 반격 무시", GameLogger.LogCategory.Combat);
            }

            var totalDamage = 0;

            // 시전자 공격력 보너스 재계산
            int attackBonus = 0;
            if (context.Card is IAttackPowerStackProvider stackProvider)
            {
                attackBonus = stackProvider.GetAttackPowerStack();
            }

            // 아이템 공격력 버프 확인
            int itemAttackBonus = 0;
            if (source != null)
            {
                // ICharacter 인터페이스를 통해 GetBuffs() 호출
                var attackBuffEffects = source.GetBuffs();
                if (attackBuffEffects != null)
                {
                    foreach (var effect in attackBuffEffects)
                    {
                        if (effect is Game.ItemSystem.Effect.AttackPowerBuffEffect attackBuff)
                        {
                            int bonus = attackBuff.GetAttackPowerBonus();
                            itemAttackBonus += bonus;
                            GameLogger.LogInfo($"[DamageEffectCommand] 다단 히트 - 공격력 물약 버프 적용: +{bonus} (총 보너스: {itemAttackBonus})", GameLogger.LogCategory.Combat);
                        }
                    }
                }
            }

            // 강화 단계 데미지 보너스 확인 (플레이어가 시전자이고 플레이어 카드일 때만 적용)
            int starBonus = 0;
            if (itemService != null && context.Card != null && source != null && 
                source.IsPlayerControlled() && context.Card.IsFromPlayer())
            {
                string skillId = context.Card.GetCardName();
                starBonus = itemService.GetSkillDamageBonus(skillId);
            }

            // 분신 버프 확인: 시전자가 분신 버프를 가지고 있으면 공격 횟수 2배
            // (다단 히트 코루틴 진입 전에 이미 hitCount가 2배로 조정되었으므로 여기서는 추가 처리 불필요)
            bool sourceHasClone = false;
            if (source is Game.CharacterSystem.Core.CharacterBase sourceCharacterBase)
            {
                sourceHasClone = sourceCharacterBase.HasEffect<CloneBuff>() && sourceCharacterBase.GetCloneHP() > 0;
            }

            for (int i = 0; i < hitCount; i++)
            {
                int baseDamageValue = GetBaseDamageValue();
                int perHitDamage = baseDamageValue + attackBonus + itemAttackBonus + starBonus;
                
                // 분신 버프는 공격 횟수로 처리되므로 여기서는 데미지 값 변경 없음
                
                // 대상이 사망했으면 중단
                if (target.IsDead())
                {
                    GameLogger.LogDebug($"[DamageEffectCommand] 대상이 사망하여 다단 히트 중단 (히트: {i}/{hitCount})", GameLogger.LogCategory.Combat);
                    break;
                }

                // 데미지 적용 (반격 고려)
                if (targetHasCounter && source != null)
                {
                    // 반격: 대상은 데미지를 받지 않고, 공격자가 원래 데미지의 100%를 받음
                    int reflect = perHitDamage;
                    if (reflect > 0)
                    {
                        source.TakeDamageIgnoreGuard(reflect);
                        totalDamage += reflect;
                        
                        // 반격 이펙트 재생: 적의 공격 이펙트가 적에게 반사되어 나타남
                        PlayCounterAttackEffect(context, source);
                        
                        // 시공의 폭풍 버프 추적: 반격으로 플레이어가 적에게 데미지를 입힐 때 (다단 히트)
                        // 반격의 경우: target(플레이어)가 반격 버프를 가지고 있고, source(적)가 데미지를 받음
                        // 이것도 플레이어가 적에게 데미지를 입힌 것으로 간주
                        // 시공의 폭풍 체력을 소모하는 것도 데미지로 인정
                        if (target != null && target.IsPlayerControlled() && source is Game.CharacterSystem.Core.CharacterBase enemyCharacterCounterMulti)
                        {
                            // 데미지 적용 전 시공의 폭풍 체력 확인
                            int stormHPBefore = enemyCharacterCounterMulti.GetStormHP();
                            
                            int actualDamageToNormalHP = CalculateActualDamageToNormalHP(source, reflect, true); // 반격은 가드 무시
                            
                            // 데미지 적용 후 시공의 폭풍 체력 확인
                            int stormHPAfter = enemyCharacterCounterMulti.GetStormHP();
                            int stormHPDamage = Mathf.Max(0, stormHPBefore - stormHPAfter);
                            
                            var stormDebuff = enemyCharacterCounterMulti.GetEffect<StormOfSpaceTimeDebuff>();
                            if (stormDebuff != null)
                            {
                                int totalStormDamage = stormHPDamage + actualDamageToNormalHP;
                                if (totalStormDamage > 0)
                                {
                                    stormDebuff.AddDamage(totalStormDamage);
                                    GameLogger.LogInfo($"[DamageEffectCommand] 시공의 폭풍 데미지 누적 (반격, 다단 히트 {i + 1}/{hitCount}): {totalStormDamage} (시공의 폭풍 체력 소모: {stormHPDamage}, 일반 체력 데미지: {actualDamageToNormalHP}, 총 누적: {stormDebuff.AccumulatedDamage}/{stormDebuff.TargetDamage}, 원래 데미지: {reflect}, 시공의 폭풍 체력: {stormHPBefore} → {stormHPAfter})", GameLogger.LogCategory.Combat);
                                }
                            }
                        }
                    }
                    GameLogger.LogDebug($"[DamageEffectCommand] 반격(멀티히트) step {i + 1}: 대상 0, 반사 {reflect}", GameLogger.LogCategory.Combat);
                }
                else
                {
                    // 다단 히트: 각 히트마다 Y 오프셋 적용 (0.10씩 증가)
                    float yOffset = i * 0.10f;
                    ApplyDamageCustom(target, perHitDamage, i, yOffset);
                    totalDamage += perHitDamage;
                    
                    // 시공의 폭풍 버프 추적: 플레이어가 적에게 데미지를 입힐 때 (다단 히트)
                    // 시공의 폭풍은 적(본인)에게 적용되는 버프이므로, 플레이어가 적에게 데미지를 입힐 때 적의 버프를 확인해야 함
                    // 시공의 폭풍 체력을 소모하는 것도 데미지로 인정
                    if (source != null && source.IsPlayerControlled() && target is Game.CharacterSystem.Core.CharacterBase enemyCharacterMulti)
                    {
                        // 데미지 적용 전 시공의 폭풍 체력 확인
                        int stormHPBefore = enemyCharacterMulti.GetStormHP();
                        
                        int actualDamageToNormalHP = CalculateActualDamageToNormalHP(target, perHitDamage, ignoreGuard);
                        
                        // 데미지 적용 후 시공의 폭풍 체력 확인
                        int stormHPAfter = enemyCharacterMulti.GetStormHP();
                        int stormHPDamage = Mathf.Max(0, stormHPBefore - stormHPAfter);
                        
                        var stormDebuff = enemyCharacterMulti.GetEffect<StormOfSpaceTimeDebuff>();
                        if (stormDebuff != null)
                        {
                            int totalStormDamage = stormHPDamage + actualDamageToNormalHP;
                            if (totalStormDamage > 0)
                            {
                                stormDebuff.AddDamage(totalStormDamage);
                                GameLogger.LogInfo($"[DamageEffectCommand] 시공의 폭풍 데미지 누적 (다단 히트 {i + 1}/{hitCount}): {totalStormDamage} (시공의 폭풍 체력 소모: {stormHPDamage}, 일반 체력 데미지: {actualDamageToNormalHP}, 총 누적: {stormDebuff.AccumulatedDamage}/{stormDebuff.TargetDamage}, 원래 데미지: {perHitDamage}, 시공의 폭풍 체력: {stormHPBefore} → {stormHPAfter})", GameLogger.LogCategory.Combat);
                            }
                        }
                    }
                }

                GameLogger.LogDebug($"[DamageEffectCommand] 다단 히트 {i + 1}/{hitCount}: {perHitDamage} 데미지 (총 누적: {totalDamage})", GameLogger.LogCategory.Combat);

                // 마지막 히트가 아니면 대기
                if (i < hitCount - 1)
                {
                    yield return new WaitForSeconds(0.15f); // 0.15초 간격
                }
            }

            GameLogger.LogDebug($"[DamageEffectCommand] 다단 히트 완료 - 총 데미지: {totalDamage}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 즉시 데미지를 적용합니다 (단일 히트 또는 MonoBehaviour가 아닌 경우).
        /// </summary>
        /// <param name="target">대상</param>
        /// <param name="hitCount">히트 수</param>
        private void ExecuteImmediateDamage(ICharacter target, int hitCount)
        {
            var totalDamage = 0;

            // 스택 기반 데미지 계산
            int attackBonus = 0;
            // Note: ExecuteImmediateDamage는 context가 없으므로 스택 계산이 제한적입니다.
            // 이 메서드는 주로 MonoBehaviour가 아닌 경우에 사용되므로 스택은 0으로 가정합니다.

            for (int i = 0; i < hitCount; i++)
            {
                int baseDamageValue = GetBaseDamageValue();
                int perHitDamage = baseDamageValue + attackBonus;

                ApplyDamageCustom(target, perHitDamage);
                totalDamage += perHitDamage;
            }

            GameLogger.LogDebug($"[DamageEffectCommand] 즉시 데미지 적용 - 총 데미지: {totalDamage} (히트: {hitCount})", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 데미지를 대상에게 적용합니다.
        /// </summary>
        /// <param name="target">대상</param>
        private void ApplyDamage(ICharacter target)
        {
            // 사운드 재생 (다단 히트마다)
            PlayHitSound();

            if (ignoreGuard)
            {
                // 가드 무시: TakeDamage를 우회하고 직접 체력 감소
                ApplyDamageDirectly(target, damageAmount);
                GameLogger.LogDebug($"[DamageEffectCommand] 가드 무시 데미지: {damageAmount}", GameLogger.LogCategory.Combat);
            }
            else
            {
                // 일반 데미지: 가드 체크 포함
                target.TakeDamage(damageAmount);
                GameLogger.LogDebug($"[DamageEffectCommand] 일반 데미지: {damageAmount}", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 특정 수치로 즉시 데미지를 적용합니다.
        /// </summary>
        /// <param name="target">대상 캐릭터</param>
        /// <param name="value">데미지량</param>
        /// <param name="hitIndex">히트 인덱스 (다단 히트용, 기본값: -1)</param>
        /// <param name="yOffset">Y 좌표 오프셋 (다단 히트용, 기본값: 0)</param>
        private void ApplyDamageCustom(ICharacter target, int value, int hitIndex = -1, float yOffset = 0f)
        {
            PlayHitSound();
            
            // 다단 히트인 경우 데미지 텍스트를 직접 표시 (Y 오프셋 적용)
            if (hitIndex >= 0)
            {
                // 데미지 적용 (텍스트 표시는 건너뛰기 위해 플래그 설정)
                if (target is CharacterBase characterBase)
                {
                    // 임시 플래그로 ShowDamageText 호출 건너뛰기
                    characterBase.SetSkipDamageTextDisplay(true);
                }
                
                if (ignoreGuard)
                {
                    if (target is CharacterBase characterBaseGuard)
                    {
                        characterBaseGuard.TakeDamageIgnoreGuard(value);
                    }
                    else
                    {
                        target.TakeDamage(value);
                    }
                }
                else
                {
                    target.TakeDamage(value);
                }
                
                // 플래그 해제
                if (target is CharacterBase characterBaseReset)
                {
                    characterBaseReset.SetSkipDamageTextDisplay(false);
                }
                
                // 데미지 텍스트 직접 표시 (Y 오프셋 적용)
                ShowDamageTextWithOffset(target, value, yOffset);
            }
            else
            {
                // 단일 히트는 기존 방식 사용
            if (ignoreGuard)
            {
                if (target is CharacterBase characterBase)
                {
                    characterBase.TakeDamageIgnoreGuard(value);
                }
                else
                {
                    target.TakeDamage(value);
                }
            }
            else
            {
                target.TakeDamage(value);
                }
            }
        }

        /// <summary>
        /// 데미지 텍스트를 Y 오프셋과 함께 표시합니다.
        /// </summary>
        /// <param name="target">대상 캐릭터</param>
        /// <param name="damageAmount">데미지량</param>
        /// <param name="yOffset">Y 좌표 오프셋</param>
        private void ShowDamageTextWithOffset(ICharacter target, int damageAmount, float yOffset)
        {
            if (damageAmount <= 0) return;

            // VFXManager 찾기
            var vfxManager = UnityEngine.Object.FindFirstObjectByType<Game.VFXSystem.Manager.VFXManager>();
            if (vfxManager == null) return;

            // 대상의 hpTextAnchor 찾기
            Transform hpTextAnchor = null;
            Vector3 position = Vector3.zero;

            if (target is PlayerCharacter playerCharacter)
            {
                // 리플렉션으로 hpTextAnchor 접근
                var field = typeof(PlayerCharacter).GetField("hpTextAnchor", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    hpTextAnchor = field.GetValue(playerCharacter) as Transform;
                }
            }
            else if (target is EnemyCharacter enemyCharacter)
            {
                // 리플렉션으로 hpTextAnchor 접근
                var field = typeof(EnemyCharacter).GetField("hpTextAnchor", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    hpTextAnchor = field.GetValue(enemyCharacter) as Transform;
                }
            }

            // hpTextAnchor가 없으면 Transform 위치 사용
            if (hpTextAnchor != null)
            {
                position = hpTextAnchor.position;
            }
            else if (target.Transform != null)
            {
                position = target.Transform.position;
            }

            // Y 오프셋 적용
            position = new Vector3(position.x, position.y + yOffset, position.z);

            // 데미지 텍스트 표시 (Y 오프셋 적용)
            bool success = vfxManager.ShowDamageText(damageAmount, position, hpTextAnchor, yOffset);
            
            // VFXManager가 실패하면 fallback 사용
            if (!success)
            {
                GameObject damageTextPrefab = null;
                
                if (target is PlayerCharacter playerChar)
                {
                    // 리플렉션으로 damageTextPrefab 접근
                    var prefabField = typeof(PlayerCharacter).GetField("damageTextPrefab", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (prefabField != null)
                    {
                        damageTextPrefab = prefabField.GetValue(playerChar) as GameObject;
                    }
                }
                else if (target is EnemyCharacter enemyChar)
                {
                    // 리플렉션으로 damageTextPrefab 접근
                    var prefabField = typeof(EnemyCharacter).GetField("damageTextPrefab", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (prefabField != null)
                    {
                        damageTextPrefab = prefabField.GetValue(enemyChar) as GameObject;
                    }
                }
                
                if (damageTextPrefab != null && hpTextAnchor != null)
                {
                    // 기존 텍스트 개수 확인 (VFXManager를 통해)
                    float initialYOffset = 0f;
                    if (vfxManager != null)
                    {
                        initialYOffset = vfxManager.GetExistingTextCount(hpTextAnchor) * vfxManager.GetDamageTextSpacing();
                    }

                    var instance = UnityEngine.Object.Instantiate(damageTextPrefab);
                    instance.transform.SetParent(hpTextAnchor, false);
                    // RectTransform은 Show()에서 anchoredPosition으로 설정되므로 position 설정 제거

                    var damageUI = instance.GetComponent<Game.CombatSystem.UI.DamageTextUI>();
                    damageUI?.Show(damageAmount, Color.red, "-", initialYOffset);

                    // VFXManager에 등록 (쌓기 효과를 위해)
                    if (vfxManager != null)
                    {
                        vfxManager.RegisterDamageText(instance, hpTextAnchor);
                    }
                }
            }
        }

        /// <summary>
        /// 가드를 무시하고 직접 데미지를 적용합니다.
        /// </summary>
        /// <param name="target">대상</param>
        /// <param name="damage">데미지량</param>
        private void ApplyDamageDirectly(ICharacter target, int damage)
        {
            if (damage <= 0) return;

            // CharacterBase의 가드 무시 데미지 메서드 사용
            if (target is CharacterBase characterBase)
            {
                characterBase.TakeDamageIgnoreGuard(damage);
                GameLogger.LogDebug($"[DamageEffectCommand] 가드 무시 직접 데미지: {damage}", GameLogger.LogCategory.Combat);
            }
            else
            {
                // CharacterBase가 아닌 경우 일반 TakeDamage 사용
                target.TakeDamage(damage);
            }
        }

        /// <summary>
        /// 실제 일반 체력에 들어갈 데미지를 계산합니다.
        /// 시공의 폭풍 체력과 분신 체력을 고려하여 실제 일반 체력에 들어갈 데미지만 반환합니다.
        /// </summary>
        /// <param name="target">대상 캐릭터</param>
        /// <param name="damage">원래 데미지</param>
        /// <param name="ignoreGuard">가드 무시 여부</param>
        /// <returns>실제 일반 체력에 들어갈 데미지</returns>
        private int CalculateActualDamageToNormalHP(ICharacter target, int damage, bool ignoreGuard)
        {
            if (target == null || damage <= 0)
                return 0;

            // CharacterBase가 아니면 전체 데미지 반환
            if (!(target is CharacterBase characterBase))
                return damage;

            // 무적 상태면 0 반환
            if (characterBase.IsInvincible())
                return 0;

            // 가드 상태이고 가드를 무시하지 않으면 0 반환
            if (!ignoreGuard && characterBase.IsGuarded())
                return 0;

            // 시공의 폭풍 체력 확인
            int stormHP = characterBase.GetStormHP();
            int remainingDamage = damage;

            // 시공의 폭풍 체력이 있으면 먼저 소모
            if (stormHP > 0)
            {
                remainingDamage = Mathf.Max(0, remainingDamage - stormHP);
            }

            // 분신 체력 확인
            int cloneHP = characterBase.GetCloneHP();
            
            // 분신 체력이 있으면 소모
            if (cloneHP > 0)
            {
                remainingDamage = Mathf.Max(0, remainingDamage - cloneHP);
            }

            // 실제 일반 체력에 들어갈 데미지 반환
            return remainingDamage;
        }

        /// <summary>
        /// 반격 이펙트를 재생합니다. 적의 공격 이펙트가 적에게 반사되어 나타납니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="source">공격자 (이펙트가 재생될 대상)</param>
        private void PlayCounterAttackEffect(ICardExecutionContext context, ICharacter source)
        {
            if (context?.Card == null || source == null)
            {
                GameLogger.LogWarning("[DamageEffectCommand] 반격 이펙트 재생 실패: context 또는 source가 null입니다.", GameLogger.LogCategory.Combat);
                return;
            }

            // 카드의 시각적 이펙트 프리팹 가져오기 (데미지 설정에서)
            var cardDefinition = context.Card?.CardDefinition;
            if (cardDefinition == null || !cardDefinition.configuration.hasDamage || cardDefinition.configuration.damageConfig.visualEffectPrefab == null)
            {
                GameLogger.LogWarning("[DamageEffectCommand] 반격 이펙트 재생 실패: visualEffectPrefab이 설정되지 않았습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            // 공격자의 Transform 가져오기
            var sourceTransform = (source as MonoBehaviour)?.transform;
            if (sourceTransform == null)
            {
                GameLogger.LogWarning("[DamageEffectCommand] 반격 이펙트 재생 실패: source Transform이 null입니다.", GameLogger.LogCategory.Combat);
                return;
            }

            // VFXManager는 Command 생성 시 주입받지 않으므로 여기서 찾기 (폴백)
            // TODO: EffectCommandFactory에서 VFXManager도 주입받도록 리팩토링 필요
            var vfxManager = UnityEngine.Object.FindFirstObjectByType<Game.VFXSystem.Manager.VFXManager>();
            if (vfxManager != null)
            {
                var visualEffectPrefab = cardDefinition.configuration.damageConfig.visualEffectPrefab;
                
                // 공격자에게 반격 이펙트 재생
                var effectInstance = vfxManager.PlayEffectAtCharacterCenter(visualEffectPrefab, sourceTransform);
                if (effectInstance != null)
                {
                    GameLogger.LogInfo($"[DamageEffectCommand] 반격 이펙트 재생 성공: {visualEffectPrefab.name} → {source.GetCharacterName()}", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogWarning("[DamageEffectCommand] 반격 이펙트 인스턴스 생성 실패", GameLogger.LogCategory.Combat);
                }
            }
            else
            {
                GameLogger.LogWarning("[DamageEffectCommand] 반격 이펙트 재생 실패: VFXManager를 찾을 수 없습니다.", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 히트 사운드를 재생합니다.
        /// </summary>
        private void PlayHitSound()
        {
            // 기본 히트 사운드 재생 (다단 히트마다)
            // TODO: 실제 히트 사운드 클립으로 교체
            if (audioManager != null)
            {
                // 기본 히트 사운드 재생 (임시)
                // AudioManager.Instance.PlaySFX(hitSoundClip);
                GameLogger.LogDebug($"[DamageEffectCommand] 히트 사운드 재생", GameLogger.LogCategory.Audio);
            }
        }

    }
}