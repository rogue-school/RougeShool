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
            this.audioManager = null; // 의존성 주입이 아닌 경우
            this.itemService = null; // 의존성 주입이 아닌 경우
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
            if (source is Game.CharacterSystem.Core.CharacterBase characterBase)
            {
                // AttackPowerBuffEffect가 있는지 확인하고 보너스 적용
                var attackBuffEffects = characterBase.GetBuffs();
                int buffCount = 0;
                foreach (var effect in attackBuffEffects)
                {
                    if (effect is Game.ItemSystem.Effect.AttackPowerBuffEffect attackBuff)
                    {
                        int bonus = attackBuff.GetAttackPowerBonus();
                        itemAttackBonus += bonus;
                        buffCount++;
                        GameLogger.LogInfo($"[DamageCalc] 🔍 AttackPowerBuffEffect #{buffCount} 발견: +{bonus}, 누적: {itemAttackBonus}", GameLogger.LogCategory.Combat);
                    }
                }
                if (buffCount > 0)
                {
                    GameLogger.LogInfo($"[DamageCalc] 🔍 총 {buffCount}개의 공격력 버프 발견, 합계: +{itemAttackBonus}", GameLogger.LogCategory.Combat);
                }
            }

            // 2) 강화 단계 데미지 보너스 확인 (플레이어가 시전자일 때만 적용)
            int starBonus = 0;
            IItemService service = itemService;
            if (service == null)
            {
                // 실행 시점에 1회 안전 조회 (Update 루프 아님)
                var svcImpl = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Service.ItemService>();
                if (svcImpl != null) service = svcImpl as IItemService;
            }
            if (service != null && context.Card != null && source != null && source.IsPlayerControlled())
            {
                string skillId = context.Card.GetCardName();
                starBonus = service.GetSkillDamageBonus(skillId);
            }

            int effectiveDamage = damageAmount + attackBonus + itemAttackBonus + starBonus;

            // 🔍 디버그: 최종 데미지 계산 상세 로그
            GameLogger.LogInfo($"[DamageCalc] 💥 기본:{damageAmount} + 스택:{attackBonus} + 아이템:{itemAttackBonus} + 강화:{starBonus} = 최종:{effectiveDamage}", GameLogger.LogCategory.Combat);

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
                GameLogger.LogInfo($"[DamageEffectCommand] 실드 브레이커 효과로 반격 무시", GameLogger.LogCategory.Combat);
            }

            // 다단 히트 처리 (시간 간격을 두고 공격)
            if (hits > 1)
            {
                // 코루틴으로 다단 히트 실행
                var sourceMono = source as MonoBehaviour;
                if (sourceMono != null)
                {
                    sourceMono.StartCoroutine(ExecuteMultiHitDamage(context, hits));
                }
                else
                {
                    // MonoBehaviour가 아닌 경우 즉시 실행
                    ExecuteImmediateDamage(target, hits);
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
                        Debug.Log($"[DamageEffectCommand] 반격: 대상 0 수신, 공격자 {reflect} 반사");
                        
                        // 반격 이펙트 재생: 적의 공격 이펙트가 적에게 반사되어 나타남
                        PlayCounterAttackEffect(context, source);
                    }
                }
                else
                {
                    ApplyDamageCustom(target, effectiveDamage);
                    totalDamage += effectiveDamage;
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
            if (source is Game.CharacterSystem.Core.CharacterBase characterBase)
            {
                var attackBuffEffects = characterBase.GetBuffs();
                foreach (var effect in attackBuffEffects)
                {
                    if (effect is Game.ItemSystem.Effect.AttackPowerBuffEffect attackBuff)
                    {
                        itemAttackBonus += attackBuff.GetAttackPowerBonus();
                    }
                }
            }

            // 강화 단계 데미지 보너스 확인 (플레이어가 시전자일 때만 적용)
            int starBonus = 0;
            if (itemService != null && context.Card != null && source != null && source.IsPlayerControlled())
            {
                string skillId = context.Card.GetCardName();
                starBonus = itemService.GetSkillDamageBonus(skillId);
            }

            int perHitDamage = damageAmount + attackBonus + itemAttackBonus + starBonus;

            for (int i = 0; i < hitCount; i++)
            {
                // 대상이 사망했으면 중단
                if (target.IsDead())
                {
                    Debug.Log($"[DamageEffectCommand] 대상이 사망하여 다단 히트 중단 (히트: {i}/{hitCount})");
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
                    }
                    Debug.Log($"[DamageEffectCommand] 반격(멀티히트) step {i + 1}: 대상 0, 반사 {reflect}");
                }
                else
                {
                    ApplyDamageCustom(target, perHitDamage);
                    totalDamage += perHitDamage;
                }

                Debug.Log($"[DamageEffectCommand] 다단 히트 {i + 1}/{hitCount}: {perHitDamage} 데미지 (총 누적: {totalDamage})");

                // 마지막 히트가 아니면 대기
                if (i < hitCount - 1)
                {
                    yield return new WaitForSeconds(0.15f); // 0.15초 간격
                }
            }

            Debug.Log($"[DamageEffectCommand] 다단 히트 완료 - 총 데미지: {totalDamage}");
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

            // 강화 단계 보너스는 context가 없으므로 적용하지 않음
            int perHitDamage = damageAmount + attackBonus;

            for (int i = 0; i < hitCount; i++)
            {
                ApplyDamageCustom(target, perHitDamage);
                totalDamage += perHitDamage;
            }

            Debug.Log($"[DamageEffectCommand] 즉시 데미지 적용 - 총 데미지: {totalDamage} (히트: {hitCount})");
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
                Debug.Log($"[DamageEffectCommand] 가드 무시 데미지: {damageAmount}");
            }
            else
            {
                // 일반 데미지: 가드 체크 포함
                target.TakeDamage(damageAmount);
                Debug.Log($"[DamageEffectCommand] 일반 데미지: {damageAmount}");
            }
        }

        /// <summary>
        /// 특정 수치로 즉시 데미지를 적용합니다.
        /// </summary>
        private void ApplyDamageCustom(ICharacter target, int value)
        {
            PlayHitSound();
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
                Debug.Log($"[DamageEffectCommand] 가드 무시 직접 데미지: {damage}");
            }
            else
            {
                // CharacterBase가 아닌 경우 일반 TakeDamage 사용
                target.TakeDamage(damage);
            }
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

            // 카드의 시각적 이펙트 프리팹 가져오기
            var cardDefinition = context.Card?.CardDefinition;
            if (cardDefinition == null || cardDefinition.presentation == null || cardDefinition.presentation.visualEffectPrefab == null)
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

            // VFXManager 찾기
            var vfxManager = UnityEngine.Object.FindFirstObjectByType<Game.VFXSystem.Manager.VFXManager>();
            if (vfxManager != null)
            {
                var visualEffectPrefab = cardDefinition.presentation.visualEffectPrefab;
                
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
                Debug.Log($"[DamageEffectCommand] 히트 사운드 재생");
            }
        }

    }
}