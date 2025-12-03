using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 스킬 기반 공격력 버프 효과 커맨드 생성 전략입니다.
    /// </summary>
    public class AttackPowerBuffSkillEffectStrategy : IEffectCommandStrategy
    {
        public bool CanHandle(SkillCardEffectSO effectSO)
        {
            return effectSO is AttackPowerBuffSkillEffectSO;
        }

        public ICardEffectCommand CreateCommand(EffectConfiguration config)
        {
            if (config.effectSO is not AttackPowerBuffSkillEffectSO effectSO)
            {
                return null;
            }

            // 현재는 커스텀 설정 없이 SO에 설정된 수치만 사용합니다.
            return effectSO.CreateEffectCommand(0);
        }
    }
}


