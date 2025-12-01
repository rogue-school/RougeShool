using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 이전 턴 카드 재실행 효과 커맨드 생성 전략입니다.
    /// </summary>
    public class ReplayPreviousTurnCardEffectStrategy : IEffectCommandStrategy
    {
        public bool CanHandle(SkillCardEffectSO effectSO)
        {
            return effectSO is ReplayPreviousTurnCardEffectSO;
        }

        public ICardEffectCommand CreateCommand(EffectConfiguration config)
        {
            if (config.effectSO is not ReplayPreviousTurnCardEffectSO effectSO)
            {
                return null;
            }

            // 현재는 커스텀 설정 없이 SO에 설정된 재실행 횟수를 그대로 사용합니다.
            return effectSO.CreateEffectCommand(0);
        }
    }
}


