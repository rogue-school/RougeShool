using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 카드가 사용될 때마다 해당 카드의 공격력 스택을 증가시키는 명령입니다.
    /// 데미지는 SkillCardDefinition의 hasDamage 시스템에서 자동으로 처리됩니다.
    /// </summary>
    public class CardUseStackCommand : ICardEffectCommand
    {
        private readonly int stackIncreasePerUse;
        private readonly int maxStacks;

        public CardUseStackCommand(int stackIncreasePerUse, int maxStacks = 5)
        {
            this.stackIncreasePerUse = stackIncreasePerUse;
            this.maxStacks = maxStacks;
            
            GameLogger.LogInfo($"[CardUseStackCommand] 생성됨 - 증가량: {stackIncreasePerUse}, 최대: {maxStacks}", 
                GameLogger.LogCategory.SkillCard);
        }
        
        /// <summary>
        /// EffectCustomSettings를 통해 생성하는 생성자
        /// </summary>
        /// <param name="customSettings">커스텀 설정</param>
        public CardUseStackCommand(Game.SkillCardSystem.Data.EffectCustomSettings customSettings)
        {
            this.stackIncreasePerUse = customSettings.stackIncreasePerUse;
            this.maxStacks = customSettings.maxStacks;
            
            GameLogger.LogInfo($"[CardUseStackCommand] 생성됨 (CustomSettings) - 증가량: {stackIncreasePerUse}, 최대: {maxStacks}", 
                GameLogger.LogCategory.SkillCard);
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Card == null)
            {
                GameLogger.LogWarning("[CardUseStackCommand] 컨텍스트 또는 카드가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 현재 카드 정보 가져오기
            string currentCardName = context.Card.GetCardName();
            
            // 데미지가 실제로 들어갔는지 확인
            if (!WasDamageSuccessfullyApplied(context))
            {
                GameLogger.LogInfo($"[CardUseStackCommand] '{currentCardName}' 데미지가 차단됨 - 스택 증가 안함", 
                    GameLogger.LogCategory.SkillCard);
                return;
            }
            
            // 현재 카드가 IAttackPowerStackProvider를 구현하는지 확인
            if (context.Card is IAttackPowerStackProvider stackProvider)
            {
                int currentStacks = stackProvider.GetAttackPowerStack();
                
                // 최대 스택 확인
                if (maxStacks > 0 && currentStacks >= maxStacks)
                {
                    GameLogger.LogInfo($"[CardUseStackCommand] '{currentCardName}' 최대 스택 도달 - 현재: {currentStacks}, 최대: {maxStacks}", 
                        GameLogger.LogCategory.SkillCard);
                    return;
                }

                // 스택 증가 (해당 카드의 독립적인 스택만 증가)
                for (int i = 0; i < stackIncreasePerUse; i++)
                {
                    stackProvider.IncrementAttackPowerStack(maxStacks);
                }

                int newStacks = stackProvider.GetAttackPowerStack();
                
                GameLogger.LogInfo($"[CardUseStackCommand] '{currentCardName}' 스택 증가 완료 - 증가량: +{stackIncreasePerUse}, 현재: {newStacks} (데미지 성공 적용됨)", 
                    GameLogger.LogCategory.SkillCard);
            }
            else
            {
                GameLogger.LogWarning($"[CardUseStackCommand] 카드 '{currentCardName}'가 IAttackPowerStackProvider를 구현하지 않습니다.", 
                    GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 데미지가 실제로 성공적으로 적용되었는지 확인합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <returns>데미지가 성공적으로 적용되었으면 true</returns>
        private bool WasDamageSuccessfullyApplied(ICardExecutionContext context)
        {
            if (context?.Target == null) return false;

            var target = context.Target;
            
            // 대상이 가드 상태인지 확인
            if (target.IsGuarded())
            {
                GameLogger.LogInfo($"[CardUseStackCommand] 대상 '{target.GetCharacterName()}'이 가드 상태 - 데미지 차단됨", 
                    GameLogger.LogCategory.SkillCard);
                return false;
            }

            // 대상이 이미 사망했는지 확인
            if (target.IsDead())
            {
                GameLogger.LogInfo($"[CardUseStackCommand] 대상 '{target.GetCharacterName()}'이 이미 사망 - 데미지 적용 불가", 
                    GameLogger.LogCategory.SkillCard);
                return false;
            }

            // 데미지가 성공적으로 적용될 수 있음
            return true;
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