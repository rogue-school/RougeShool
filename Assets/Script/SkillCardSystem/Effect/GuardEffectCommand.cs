using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 가드 효과를 적용하는 커맨드 클래스입니다.
    /// 캐릭터에게 지정된 턴 동안 가드 버프를 적용합니다.
    /// </summary>
    public class GuardEffectCommand : ICardEffectCommand
    {
        private readonly int duration;
        
        /// <summary>
        /// 가드 효과 커맨드 생성자
        /// </summary>
        /// <param name="duration">가드 지속 턴 수 (기본값: 1)</param>
        public GuardEffectCommand(int duration = 1)
        {
            this.duration = duration;
        }
        
        /// <summary>
        /// 가드 효과를 실행합니다.
        /// 캐릭터에게 지정된 턴 동안 가드 버프를 적용합니다.
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
                // 가드 아이콘 로드 (ScriptableObject에서)
                Sprite guardIcon = null;
                var guardEffectSO = Resources.Load<Game.SkillCardSystem.Effect.GuardEffectSO>("Data/SkillCard/SkillEffect/GuardEffect");
                if (guardEffectSO != null)
                {
                    guardIcon = guardEffectSO.GetIcon();
                    GameLogger.LogInfo($"[GuardEffectCommand] 가드 아이콘 로드 성공: {guardIcon?.name ?? "null"}", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogWarning("[GuardEffectCommand] GuardEffectSO를 찾을 수 없습니다.", GameLogger.LogCategory.Combat);
                }
                
                // 아이콘이 없으면 기본 아이콘 시도
                if (guardIcon == null)
                {
                    guardIcon = Resources.Load<Sprite>("Image/UI (1)/UI/shield_icon");
                    if (guardIcon != null)
                    {
                        GameLogger.LogInfo("[GuardEffectCommand] 대체 가드 아이콘 로드 성공", GameLogger.LogCategory.Combat);
                    }
                    else
                    {
                        GameLogger.LogWarning("[GuardEffectCommand] 대체 가드 아이콘도 찾을 수 없습니다.", GameLogger.LogCategory.Combat);
                    }
                }
                
                var guardBuff = new GuardBuff(duration, guardIcon); // 커스텀 지속 시간과 아이콘으로 생성
                character.RegisterPerTurnEffect(guardBuff);
                // 즉시 보호 활성화: 다음 자신의 턴 시작 시 카운트가 0이 되면 해제됨
                character.SetGuarded(true);
                
                GameLogger.LogInfo($"[GuardEffectCommand] {character.GetCharacterName()}에게 가드 버프 적용 ({duration}턴 지속, 아이콘: {guardIcon?.name ?? "없음"})", GameLogger.LogCategory.Combat);
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
