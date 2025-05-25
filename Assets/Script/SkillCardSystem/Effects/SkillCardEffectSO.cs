using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    [System.Serializable]
    public abstract class SkillCardEffectSO : ScriptableObject, ICardEffect
    {
        [SerializeField] private string effectName;
        [TextArea][SerializeField] private string description;

        public string GetEffectName() => effectName;
        public string GetDescription() => description;

        public abstract ICardEffectCommand CreateEffectCommand(int power);

        public abstract void ApplyEffect(ICardExecutionContext context, int value, ITurnStateController controller = null);
    }
}