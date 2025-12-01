using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CombatSystem.Manager;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 같은 캐릭터가 이전 턴에 사용했던 비-연계 스킬 카드를
    /// 지정된 횟수만큼 재실행하는 명령입니다.
    /// </summary>
    public class ReplayPreviousTurnCardCommand : ICardEffectCommand
    {
        private readonly int _repeatCount;

        public ReplayPreviousTurnCardCommand(int repeatCount)
        {
            _repeatCount = repeatCount < 0 ? 0 : repeatCount;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context == null || context.Card == null)
            {
                GameLogger.LogWarning("[ReplayPreviousTurnCardCommand] 컨텍스트 또는 카드가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (_repeatCount <= 0)
            {
                GameLogger.LogWarning("[ReplayPreviousTurnCardCommand] 재실행 횟수가 0 이하입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            var source = context.Source;
            var target = context.Target;

            if (source == null || target == null)
            {
                GameLogger.LogWarning("[ReplayPreviousTurnCardCommand] 시전자 또는 대상이 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            var executionManager = Object.FindFirstObjectByType<CombatExecutionManager>();
            if (executionManager == null)
            {
                GameLogger.LogError("[ReplayPreviousTurnCardCommand] CombatExecutionManager를 찾을 수 없습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            var previousCard = executionManager.GetPreviousNonLinkCardForOwner(context.Card);
            if (previousCard == null)
            {
                GameLogger.LogInfo("[ReplayPreviousTurnCardCommand] 재실행할 이전 턴 카드가 없습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            string previousName = previousCard.GetCardName();
            GameLogger.LogInfo(
                $"[ReplayPreviousTurnCardCommand] 이전 턴 카드 재실행 시작 - 카드: {previousName}, 횟수: {_repeatCount}",
                GameLogger.LogCategory.SkillCard);

            for (int i = 0; i < _repeatCount; i++)
            {
                try
                {
                    previousCard.ExecuteSkill(source, target);
                    GameLogger.LogInfo(
                        $"[ReplayPreviousTurnCardCommand] 재실행 완료 {i + 1}/{_repeatCount} - 카드: {previousName}",
                        GameLogger.LogCategory.SkillCard);
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogError(
                        $"[ReplayPreviousTurnCardCommand] 재실행 중 예외 발생: {ex.Message}",
                        GameLogger.LogCategory.Error);
                    break;
                }
            }
        }
    }
}


