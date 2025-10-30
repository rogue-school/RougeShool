using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
	/// <summary>
	/// 자원 수급 효과 커맨드 생성 전략입니다.
	/// </summary>
	public class ResourceEffectStrategy : IEffectCommandStrategy
	{
		public bool CanHandle(SkillCardEffectSO effectSO)
		{
			return effectSO is ResourceGainEffectSO;
		}

		public ICardEffectCommand CreateCommand(EffectConfiguration config)
		{
			if (config.effectSO is not ResourceGainEffectSO)
				return null;

			// 커스텀 설정에 resourceDelta가 있으면 우선 사용
			if (config.useCustomSettings && config.customSettings != null)
			{
				int amount = config.customSettings.resourceDelta;
				return new ResourceGainEffectCommand(amount);
			}

			// SO 기본값 사용
			return config.effectSO.CreateEffectCommand(0);
		}
	}
}


