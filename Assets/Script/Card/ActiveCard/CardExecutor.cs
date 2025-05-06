using UnityEngine;
using Game.Interface;
using Game.Characters;

namespace Game.Cards
{
    public class CardExecutor : MonoBehaviour
    {
        public static CardExecutor Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void ExecuteCard(ISkillCard card, CharacterBase caster, CharacterBase target)
        {
            var effects = card.CreateEffects();
            foreach (var effect in effects)
            {
                int value = card.GetEffectPower(effect);
                effect.ExecuteEffect(caster, target, value);
            }
        }

        /// <summary>
        /// 외부에서 간단히 호출할 수 있도록 제공되는 정적 실행 함수
        /// </summary>
        public static void Execute(ISkillCard card, ICharacter caster, ICharacter target)
        {
            if (Instance == null)
            {
                Debug.LogError("[CardExecutor] 인스턴스가 존재하지 않습니다.");
                return;
            }

            if (card == null || caster == null || target == null)
            {
                Debug.LogWarning("[CardExecutor] 실행 파라미터가 누락되었습니다.");
                return;
            }

            if (caster is CharacterBase casterBase && target is CharacterBase targetBase)
            {
                Instance.ExecuteCard(card, casterBase, targetBase);
            }
            else
            {
                Debug.LogError("[CardExecutor] 캐스터 또는 타겟이 CharacterBase 타입이 아닙니다.");
            }
        }
    }
}
