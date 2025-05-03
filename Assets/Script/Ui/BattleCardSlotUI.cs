using UnityEngine;
using UnityEngine.UI;
using Game.Cards;
using Game.Units;
using Game.Interface;

namespace Game.Battle
{
    /// <summary>
    /// 전투 슬롯에서 카드 효과를 실행하는 UI 슬롯입니다.
    /// </summary>
    public class BattleCardSlotUI : MonoBehaviour, ICardSlot
    {
        public Image cardImage;
        private ISkillCard currentCard;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            cardImage.enabled = true;
            // cardImage.sprite = card.GetArtwork(); // 추가 연동 가능
        }

        public ISkillCard GetCard() => currentCard;

        public bool HasCard() => currentCard != null;

        public void Clear()
        {
            currentCard = null;
            cardImage.sprite = null;
            cardImage.enabled = false;
        }

        public void ExecuteEffect(Unit caster, Unit target)
        {
            currentCard?.CreateEffect()?.ExecuteEffect(caster, target);
        }
    }
}
