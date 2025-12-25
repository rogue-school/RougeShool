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
        private readonly CombatExecutionManager executionManager;

        public ReplayPreviousTurnCardCommand(int repeatCount)
        {
            _repeatCount = repeatCount < 0 ? 0 : repeatCount;
            this.executionManager = null;
        }

        /// <summary>
        /// 이전 턴 카드 재실행 명령 생성자 (의존성 포함)
        /// </summary>
        /// <param name="repeatCount">재실행 횟수</param>
        /// <param name="executionManager">전투 실행 매니저 (선택적)</param>
        public ReplayPreviousTurnCardCommand(int repeatCount, CombatExecutionManager executionManager)
        {
            _repeatCount = repeatCount < 0 ? 0 : repeatCount;
            this.executionManager = executionManager;
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

            var em = executionManager ?? Object.FindFirstObjectByType<CombatExecutionManager>();
            if (em == null)
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

            // 시전자가 MonoBehaviour이면 코루틴으로, 아니면 즉시 실행으로 처리
            var sourceMono = source as MonoBehaviour;
            if (sourceMono != null)
            {
                sourceMono.StartCoroutine(ReplayWithDelayRoutine(previousCard, source, target, previousName));
            }
            else
            {
                ReplayImmediately(previousCard, source, target, previousName);
            }
        }

        private System.Collections.IEnumerator ReplayWithDelayRoutine(ISkillCard previousCard, Game.CharacterSystem.Interface.ICharacter source, Game.CharacterSystem.Interface.ICharacter target, string previousName)
        {
            const float interval = 0.15f;

            for (int i = 0; i < _repeatCount; i++)
            {
                if (target == null || target.IsDead())
                {
                    GameLogger.LogInfo(
                        "[ReplayPreviousTurnCardCommand] 대상이 사망하여 연계 재실행을 중단합니다.",
                        GameLogger.LogCategory.SkillCard);
                    yield break;
                }

                try
                {
                    previousCard.ExecuteSkill(source, target);
                    GameLogger.LogInfo(
                        $"[ReplayPreviousTurnCardCommand] 지연 재실행 완료 {i + 1}/{_repeatCount} - 카드: {previousName}",
                        GameLogger.LogCategory.SkillCard);
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogError(
                        $"[ReplayPreviousTurnCardCommand] 지연 재실행 중 예외 발생: {ex.Message}",
                        GameLogger.LogCategory.Error);
                    yield break;
                }

                if (i < _repeatCount - 1)
                {
                    yield return new WaitForSeconds(interval);
                }
            }
        }

        private void ReplayImmediately(ISkillCard previousCard, Game.CharacterSystem.Interface.ICharacter source, Game.CharacterSystem.Interface.ICharacter target, string previousName)
        {
            for (int i = 0; i < _repeatCount; i++)
            {
                if (target == null || target.IsDead())
                {
                    GameLogger.LogInfo(
                        "[ReplayPreviousTurnCardCommand] 대상이 사망하여 즉시 재실행을 중단합니다.",
                        GameLogger.LogCategory.SkillCard);
                    return;
                }

                try
                {
                    previousCard.ExecuteSkill(source, target);
                    GameLogger.LogInfo(
                        $"[ReplayPreviousTurnCardCommand] 즉시 재실행 완료 {i + 1}/{_repeatCount} - 카드: {previousName}",
                        GameLogger.LogCategory.SkillCard);
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogError(
                        $"[ReplayPreviousTurnCardCommand] 즉시 재실행 중 예외 발생: {ex.Message}",
                        GameLogger.LogCategory.Error);
                    return;
                }
            }
        }
    }
}


