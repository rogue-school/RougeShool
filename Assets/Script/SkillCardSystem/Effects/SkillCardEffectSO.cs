using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    /// <summary>
    /// 카드 효과를 정의하는 ScriptableObject 기반 추상 클래스.
    /// </summary>
    [System.Serializable]
    public abstract class SkillCardEffectSO : ScriptableObject, ICardEffect
    {
        [SerializeField] private string effectName;

        [TextArea]
        [SerializeField] private string description;

        public string GetEffectName() => effectName;
        public string GetDescription() => description;

        /// <summary>
        /// 실제 커맨드 객체를 생성하여 반환
        /// </summary>
        public abstract ICardEffectCommand CreateEffectCommand(int power);

        /// <summary>
        /// 직접 이펙트를 적용 (디자인 타임용 또는 테스트 용도)
        /// </summary>
        public abstract void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null);
    }
}
