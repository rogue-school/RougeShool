using UnityEngine;
using Game.Cards;
using Game.Characters;
using Game.Interface;

namespace Game.Managers
{
    /// <summary>
    /// 카드 실행을 담당하는 매니저입니다.
    /// </summary>
    public class CardExecutor : MonoBehaviour
    {
        public static CardExecutor Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// 스킬 카드를 실행합니다.
        /// </summary>
        public void ExecuteCard(ISkillCard card, CharacterBase caster, CharacterBase target)
        {
            foreach (var effect in card.CreateEffects())
            {
                int value = card.GetEffectPower(effect);
                effect.ExecuteEffect(caster, target, value);
            }
        }
    }
}
