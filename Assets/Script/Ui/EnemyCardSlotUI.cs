using UnityEngine;
using Game.Managers;
using Game.Cards;
using Game.Characters;
using Game.Interface;
using Game.Battle;

namespace Game.UI
{
    public class EnemyCardSlotUI : MonoBehaviour
    {
        [SerializeField] private ISkillCard currentCard;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
        }

        public void ExecuteEffect(CharacterBase caster, CharacterBase target)
        {
            if (BattleTurnManager.Instance.ConsumePlayerBlock())
            {
                Debug.Log("[EnemyCardSlotUI] 플레이어의 방어 효과로 적의 공격이 무효화되었습니다.");
                return;
            }

            currentCard?.CreateEffect()?.ExecuteEffect(caster, target);
        }
    }
}
