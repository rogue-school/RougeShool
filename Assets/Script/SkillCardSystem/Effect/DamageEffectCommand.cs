using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Zenject;

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
        
        public DamageEffectCommand(IAudioManager audioManager)
        {
            this.audioManager = audioManager;
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
            int effectiveDamage = damageAmount + attackBonus;

            // 반격 버프 처리: 대상이 CounterBuff 보유 시, 들어오는 피해의 절반만 받고 나머지 절반을 공격자에게 반사
            // 정수 절삭/올림 규칙: 들어오는 피해를 ceil(절반)은 수신, floor(절반)은 반사
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
                    int receive = Mathf.CeilToInt(effectiveDamage * 0.5f);
                    int reflect = effectiveDamage - receive; // floor
                    ApplyDamageCustom(target, receive);
                    totalDamage += receive;
                    if (reflect > 0)
                    {
                        source.TakeDamageIgnoreGuard(reflect);
                        totalDamage += reflect;
                        Debug.Log($"[DamageEffectCommand] 반격: 대상 {receive} 수신, 공격자 {reflect} 반사");
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
            var totalDamage = 0;

            // 시전자 공격력 보너스 재계산
            int attackBonus = 0;
            if (context.Card is IAttackPowerStackProvider stackProvider)
            {
                attackBonus = stackProvider.GetAttackPowerStack();
            }
            int perHitDamage = damageAmount + attackBonus;
            
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
                    int receive = Mathf.CeilToInt(perHitDamage * 0.5f);
                    int reflect = perHitDamage - receive;
                    ApplyDamageCustom(target, receive);
                    totalDamage += receive;
                    if (reflect > 0) 
                    {
                        source.TakeDamageIgnoreGuard(reflect);
                        totalDamage += reflect;
                    }
                    Debug.Log($"[DamageEffectCommand] 반격(멀티히트) step {i+1}: 대상 {receive}, 반사 {reflect}");
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