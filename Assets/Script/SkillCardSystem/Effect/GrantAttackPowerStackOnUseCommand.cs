using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 카드 사용 시점에 해당 카드의 공격 스택을 +1 증가시키는 커맨드입니다.
    /// </summary>
    public class GrantAttackPowerStackOnUseCommand : ICardEffectCommand
    {
        private readonly int max;

        public GrantAttackPowerStackOnUseCommand(int max = 5)
        {
            this.max = max;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Card is IAttackPowerStackProvider provider)
            {
                provider.IncrementAttackPowerStack(max);
                GameLogger.LogInfo($"[AttackPowerStack] 카드 '{context.Card.GetCardName()}' 스택 증가", GameLogger.LogCategory.SkillCard);
            }
        }
    }
}


