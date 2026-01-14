using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Manager;

namespace Game.SkillCardSystem.Effect
{
	/// <summary>
	/// 자원 수급 효과 커맨드 생성 전략입니다.
	/// </summary>
	public class ResourceEffectStrategy : IEffectCommandStrategy
	{
		/// <summary>
		/// 주어진 효과 SO를 처리할 수 있는지 확인합니다.
		/// </summary>
		/// <param name="effectSO">확인할 효과 SO</param>
		/// <returns>ResourceGainEffectSO이면 true</returns>
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
                // 의존성 찾기
                // TODO: Strategy 패턴 특성상 DI가 어려우므로 FindFirstObjectByType 사용
                // 향후 EffectCommandFactory에 의존성 주입하여 Strategy에 전달하도록 리팩토링 필요
                var playerManager = UnityEngine.Object.FindFirstObjectByType<PlayerManager>();
                var audioManager = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
                return new ResourceGainEffectCommand(amount, config.customSettings.resourceGainSfxClip, playerManager, audioManager as Game.CoreSystem.Interface.IAudioManager);
			}

			// SO 기본값 사용
            return config.effectSO.CreateEffectCommand(0);
		}
	}
}


