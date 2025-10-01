using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 자신의 체력을 회복시키는 명령입니다.
    /// </summary>
    public class HealEffectCommand : ICardEffectCommand
    {
        private readonly int healAmount;
        private readonly int maxHealAmount;

        public HealEffectCommand(int healAmount, int maxHealAmount = 0)
        {
            this.healAmount = healAmount;
            this.maxHealAmount = maxHealAmount;
            
            GameLogger.LogInfo($"[HealEffectCommand] 생성됨 - 치유량: {healAmount}, 최대: {maxHealAmount}", 
                GameLogger.LogCategory.SkillCard);
        }
        
        /// <summary>
        /// EffectCustomSettings를 통해 생성하는 생성자
        /// </summary>
        /// <param name="customSettings">커스텀 설정</param>
        public HealEffectCommand(Game.SkillCardSystem.Data.EffectCustomSettings customSettings)
        {
            this.healAmount = customSettings.healAmount;
            this.maxHealAmount = 0; // 커스텀 설정에서는 최대치 제한 없음
            
            GameLogger.LogInfo($"[HealEffectCommand] 생성됨 (CustomSettings) - 치유량: {healAmount}", 
                GameLogger.LogCategory.SkillCard);
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source == null)
            {
                GameLogger.LogWarning("[HealEffectCommand] 시전자가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            var source = context.Source;
            string sourceName = source.GetCharacterName();
            
            // 현재 체력과 최대 체력 확인
            int currentHP = source.GetCurrentHP();
            int maxHP = source.GetMaxHP();
            
            // 이미 최대 체력이면 치유 불가
            if (currentHP >= maxHP)
            {
                GameLogger.LogInfo($"[HealEffectCommand] '{sourceName}' 이미 최대 체력입니다. (현재: {currentHP}/{maxHP})", 
                    GameLogger.LogCategory.SkillCard);
                return;
            }
            
            // 실제 치유량 계산 (최대 체력을 넘지 않도록)
            int actualHealAmount = healAmount;
            if (maxHealAmount > 0)
            {
                actualHealAmount = Mathf.Min(healAmount, maxHealAmount);
            }
            
            // 최대 체력을 넘지 않도록 제한
            int maxPossibleHeal = maxHP - currentHP;
            actualHealAmount = Mathf.Min(actualHealAmount, maxPossibleHeal);
            
            if (actualHealAmount <= 0)
            {
                GameLogger.LogWarning($"[HealEffectCommand] '{sourceName}' 치유량이 0 이하입니다.", 
                    GameLogger.LogCategory.SkillCard);
                return;
            }
            
            // 체력 회복 적용
            source.Heal(actualHealAmount);
            
            int newHP = source.GetCurrentHP();
            
            GameLogger.LogInfo($"[HealEffectCommand] '{sourceName}' 체력 회복 완료 - 치유량: {actualHealAmount}, 체력: {currentHP} → {newHP}/{maxHP}", 
                GameLogger.LogCategory.SkillCard);
        }

        /// <summary>
        /// 실행 가능 여부 (항상 true)
        /// </summary>
        public bool CanExecute() => true;

        /// <summary>
        /// 효과 비용 (무료)
        /// </summary>
        public int GetCost() => 0;
    }
}
