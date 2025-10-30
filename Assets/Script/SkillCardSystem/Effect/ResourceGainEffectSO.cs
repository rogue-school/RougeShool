using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
	/// <summary>
	/// 시전자의 자원을 회복(수급)하는 스킬 효과 SO입니다.
	/// </summary>
	[CreateAssetMenu(fileName = "ResourceGainEffect", menuName = "SkillEffects/ResourceGainEffect")]
	public class ResourceGainEffectSO : SkillCardEffectSO
	{
		[Header("자원 수급 설정")]
		[Tooltip("기본 자원 회복량")]
		[SerializeField] private int baseResourceAmount = 1;

		public override ICardEffectCommand CreateEffectCommand(int power)
		{
			int amount = Mathf.Max(0, baseResourceAmount + power);
			return new ResourceGainEffectCommand(amount);
		}

		public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null)
		{
			// 명령 패턴으로 처리하므로 직접 적용하지 않습니다.
		}
	}
}


