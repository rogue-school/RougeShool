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
        private readonly PlayerManager playerManager;
        private readonly Game.CoreSystem.Interface.IAudioManager audioManager;

        public ResourceGainEffectCommand(int amount, UnityEngine.AudioClip sfxClip = null)
		{
			this.amount = amount < 0 ? 0 : amount;
            this.sfxClip = sfxClip;
            this.playerManager = null;
            this.audioManager = null;
		}

        /// <summary>
        /// 자원 수급 효과 명령 생성자 (의존성 포함)
        /// </summary>
        /// <param name="amount">수급량</param>
        /// <param name="sfxClip">사운드 클립 (선택적)</param>
        /// <param name="playerManager">플레이어 매니저 (선택적)</param>
        /// <param name="audioManager">오디오 매니저 (선택적)</param>
        public ResourceGainEffectCommand(int amount, UnityEngine.AudioClip sfxClip, PlayerManager playerManager, Game.CoreSystem.Interface.IAudioManager audioManager)
		{
			this.amount = amount < 0 ? 0 : amount;
            this.sfxClip = sfxClip;
            this.playerManager = playerManager;
            this.audioManager = audioManager;
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

			// PlayerManager 사용 (주입받았으면 사용, 없으면 폴백)
			var pm = playerManager ?? UnityEngine.Object.FindFirstObjectByType<PlayerManager>();
			if (pm == null)
			{
				GameLogger.LogWarning("[ResourceGainEffect] PlayerManager를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
				return;
			}

            pm.RestoreResource(amount);
            GameLogger.LogInfo($"[ResourceGainEffect] 자원 수급 +{amount} (현재: {pm.CurrentResource}/{pm.MaxResource})", GameLogger.LogCategory.SkillCard);

            // 사운드 재생 (가능할 때만)
            if (sfxClip != null)
            {
                var am = audioManager ?? UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>() as Game.CoreSystem.Interface.IAudioManager;
                if (am != null)
                {
                    am.PlaySFXWithPool(sfxClip, 0.9f);
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


