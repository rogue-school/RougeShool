using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Factory
{
    /// <summary>
    /// 스킬 카드 이펙트 SO에서 커맨드를 생성하는 팩토리 클래스입니다.
    /// </summary>
    public class CardEffectCommandFactory : ICardEffectCommandFactory
    {
        /// <summary>
        /// 주어진 이펙트 SO와 파워 값을 기반으로 이펙트 커맨드를 생성합니다.
        /// </summary>
        /// <param name="effect">실행할 이펙트 ScriptableObject</param>
        /// <param name="power">이펙트에 전달될 파워 값</param>
        /// <returns>생성된 카드 이펙트 커맨드</returns>
        public ICardEffectCommand Create(SkillCardEffectSO effect, int power)
        {
            if (effect == null)
            {
                UnityEngine.Debug.LogWarning("[CardEffectCommandFactory] 효과가 null입니다.");
                return null;
            }

            return effect.CreateEffectCommand(power);
        }
    }
}
