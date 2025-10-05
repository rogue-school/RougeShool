using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 효과 커맨드 팩토리입니다.
    /// Strategy 패턴을 사용하여 효과 타입별로 커맨드를 생성합니다.
    /// </summary>
    public class EffectCommandFactory
    {
        private readonly List<IEffectCommandStrategy> strategies = new();

        public EffectCommandFactory()
        {
            // 전략 등록
            strategies.Add(new GuardEffectStrategy());
            strategies.Add(new BleedEffectStrategy());
            strategies.Add(new DamageEffectStrategy());
            strategies.Add(new CounterEffectStrategy());
            strategies.Add(new StunEffectStrategy());
            strategies.Add(new HealEffectStrategy());
            strategies.Add(new CardUseStackEffectStrategy());
        }

        /// <summary>
        /// 효과 설정으로부터 커맨드를 생성합니다.
        /// </summary>
        /// <param name="config">효과 설정</param>
        /// <returns>생성된 커맨드 (없으면 null)</returns>
        public ICardEffectCommand CreateCommand(EffectConfiguration config)
        {
            if (config?.effectSO == null)
                return null;

            // 적합한 전략 찾기
            foreach (var strategy in strategies)
            {
                if (strategy.CanHandle(config.effectSO))
                {
                    return strategy.CreateCommand(config);
                }
            }

            // 전략이 없으면 기본 방식 사용
            return CreateDefaultCommand(config);
        }

        /// <summary>
        /// 기본 커맨드 생성 (폴백)
        /// </summary>
        private ICardEffectCommand CreateDefaultCommand(EffectConfiguration config)
        {
            var power = 0; // 기본 파워 (필요시 계산)
            return config.effectSO.CreateEffectCommand(power);
        }
    }
}
