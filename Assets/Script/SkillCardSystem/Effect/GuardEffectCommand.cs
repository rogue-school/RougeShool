using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 가드 효과를 적용하는 커맨드 클래스입니다.
    /// 캐릭터에게 1턴 동안 가드 버프를 적용합니다.
    /// </summary>
    public class GuardEffectCommand : ICardEffectCommand
    {
        /// <summary>
        /// 가드 효과를 실행합니다.
        /// 캐릭터에게 1턴 동안 가드 버프를 적용합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="turnManager">전투 턴 매니저</param>
        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source == null)
            {
                GameLogger.LogWarning("[GuardEffectCommand] 소스가 null입니다.", GameLogger.LogCategory.Combat);
                return;
            }

            // 소스 캐릭터에게 가드 버프 적용
            if (context.Source is ICharacter character)
            {
                var guardBuff = new GuardBuff(1); // 1턴 지속
                character.RegisterPerTurnEffect(guardBuff);
                
                GameLogger.LogInfo($"[GuardEffectCommand] {character.GetCharacterName()}에게 가드 버프 적용 (1턴 지속)", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogWarning("[GuardEffectCommand] 소스가 캐릭터가 아닙니다.", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 가드 효과 실행 가능 여부를 확인합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <returns>실행 가능 여부</returns>
        public bool CanExecute(ICardExecutionContext context)
        {
            return context?.Source != null;
        }

        /// <summary>
        /// 가드 효과의 비용을 반환합니다.
        /// </summary>
        /// <returns>비용 (가드 효과는 비용 없음)</returns>
        public int GetCost()
        {
            return 0;
        }
    }
}
