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

			// 현재 구조에서 리소스 조작은 PlayerManager가 담당하므로 전역 인스턴스를 통해 위임합니다.
			var playerManager = Game.CharacterSystem.Manager.PlayerManager.Instance;
			if (playerManager == null)
			{
				GameLogger.LogWarning("[ResourceGainEffect] PlayerManager 인스턴스를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
				return;
			}

            playerManager.RestoreResource(amount);
            GameLogger.LogInfo($"[ResourceGainEffect] 자원 수급 +{amount} (현재: {playerManager.CurrentResource}/{playerManager.MaxResource})", GameLogger.LogCategory.SkillCard);

            // 사운드 재생 (가능할 때만)
            if (sfxClip != null)
            {
                var audioManager = Game.CoreSystem.Audio.AudioManager.Instance;
                if (audioManager != null)
                {
                    audioManager.PlaySFXWithPool(sfxClip, 0.9f);
                    GameLogger.LogInfo($"[ResourceGainEffect] 자원 수급 사운드 재생: {sfxClip.name}", GameLogger.LogCategory.SkillCard);
                }
                else
                {
                    GameLogger.LogWarning("[ResourceGainEffect] AudioManager 인스턴스를 찾을 수 없습니다. 자원 수급 사운드 재생을 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                }
            }
		}
	}
}


