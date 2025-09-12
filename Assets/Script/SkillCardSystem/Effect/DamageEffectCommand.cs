using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;

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
        private bool pierceable;
        private float critChance;
        
        /// <summary>
        /// 데미지 효과 명령을 생성합니다.
        /// </summary>
        /// <param name="damageAmount">데미지량</param>
        /// <param name="hits">히트 수</param>
        /// <param name="pierceable">가드 관통 여부</param>
        /// <param name="critChance">크리티컬 확률</param>
        public DamageEffectCommand(int damageAmount, int hits = 1, bool pierceable = false, float critChance = 0f)
        {
            this.damageAmount = damageAmount;
            this.hits = hits;
            this.pierceable = pierceable;
            this.critChance = critChance;
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
            
            // 히트 수만큼 데미지 적용
            for (int i = 0; i < hits; i++)
            {
                var damage = damageAmount;
                
                // 크리티컬 계산
                if (critChance > 0f && Random.Range(0f, 1f) < critChance)
                {
                    damage = Mathf.RoundToInt(damage * 1.5f);
                    Debug.Log($"[DamageEffectCommand] 크리티컬! 데미지: {damage}");
                }
                
                // 데미지 적용
                if (pierceable)
                {
                    target.TakeDamage(damage);
                    Debug.Log($"[DamageEffectCommand] 관통 데미지: {damage}");
                }
                else
                {
                    target.TakeDamage(damage);
                    Debug.Log($"[DamageEffectCommand] 일반 데미지: {damage}");
                }
                
                totalDamage += damage;
            }
            
            Debug.Log($"[DamageEffectCommand] {source?.GetCharacterName()} → {target.GetCharacterName()} 총 데미지: {totalDamage} (히트: {hits})");
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
        /// 히트 수를 반환합니다.
        /// </summary>
        /// <returns>히트 수</returns>
        public int GetHits() => hits;
        
        /// <summary>
        /// 가드 관통 여부를 반환합니다.
        /// </summary>
        /// <returns>가드 관통 여부</returns>
        public bool IsPierceable() => pierceable;
        
        /// <summary>
        /// 크리티컬 확률을 반환합니다.
        /// </summary>
        /// <returns>크리티컬 확률</returns>
        public float GetCritChance() => critChance;
    }
}