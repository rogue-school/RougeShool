using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Manager;

namespace Game.SkillCardSystem.Effect
{
	/// <summary>
	/// 시전자의 자원을 회복(수급)하는 명령입니다.
	/// </summary>
    public class ResourceGainEffectCommand : ICardEffectCommand
	{
		private readonly int amount;
        private readonly UnityEngine.AudioClip sfxClip;

        public ResourceGainEffectCommand(int amount, UnityEngine.AudioClip sfxClip = null)
		{
			this.amount = amount < 0 ? 0 : amount;
            this.sfxClip = sfxClip;
		}

		public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
		{
			if (context?.Source == null)
			{
				GameLogger.LogWarning("[ResourceGainEffect] 시전자가 null입니다.", GameLogger.LogCategory.SkillCard);
				return;
			}

			if (amount <= 0)
			{
				GameLogger.LogWarning("[ResourceGainEffect] 수급량이 0 이하입니다.", GameLogger.LogCategory.SkillCard);
				return;
			}

			// 현재 구조에서 리소스 조작은 PlayerManager가 담당하므로 일회성 조회로 위임합니다.
			// (Update에서 호출하지 않으며, DI가 없다면 안전한 폴백)
			var playerManager = UnityEngine.Object.FindFirstObjectByType<PlayerManager>();
			if (playerManager == null)
			{
				GameLogger.LogWarning("[ResourceGainEffect] PlayerManager를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
				return;
			}

            playerManager.RestoreResource(amount);
            GameLogger.LogInfo($"[ResourceGainEffect] 자원 수급 +{amount} (현재: {playerManager.CurrentResource}/{playerManager.MaxResource})", GameLogger.LogCategory.SkillCard);

            // 사운드 재생 (가능할 때만)
            if (sfxClip != null)
            {
                var audioManager = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
                if (audioManager != null)
                {
                    audioManager.PlaySFXWithPool(sfxClip, 0.9f);
                    GameLogger.LogInfo($"[ResourceGainEffect] 자원 수급 사운드 재생: {sfxClip.name}", GameLogger.LogCategory.SkillCard);
                }
                else
                {
                    GameLogger.LogWarning("[ResourceGainEffect] AudioManager를 찾을 수 없습니다. 자원 수급 사운드 재생을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                }
            }
		}
	}
}


