using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Interface;
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
        public DamageEffectCommand(int damageAmount, int hits = 1, bool ignoreGuard = false)
        {
            this.damageAmount = damageAmount;
            this.hits = hits;
            this.ignoreGuard = ignoreGuard;
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
                ExecuteImmediateDamage(target, hits);
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
            var totalDamage = 0;
            
            for (int i = 0; i < hitCount; i++)
            {
                // 대상이 사망했으면 중단
                if (target.IsDead())
                {
                    Debug.Log($"[DamageEffectCommand] 대상이 사망하여 다단 히트 중단 (히트: {i}/{hitCount})");
                    break;
                }
                
                // 데미지 적용
                ApplyDamage(target);
                totalDamage += damageAmount;
                
                Debug.Log($"[DamageEffectCommand] 다단 히트 {i + 1}/{hitCount}: {damageAmount} 데미지");
                
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
            
            for (int i = 0; i < hitCount; i++)
            {
                ApplyDamage(target);
                totalDamage += damageAmount;
            }
            
            Debug.Log($"[DamageEffectCommand] 즉시 데미지 적용 - 총 데미지: {totalDamage}");
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
                target.TakeDamage(damageAmount);
                Debug.Log($"[DamageEffectCommand] 가드 무시 데미지: {damageAmount}");
            }
            else
            {
                target.TakeDamage(damageAmount);
                Debug.Log($"[DamageEffectCommand] 일반 데미지: {damageAmount}");
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