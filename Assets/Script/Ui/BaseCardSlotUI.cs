using UnityEngine;
using Game.Cards;
using Game.Interface;

namespace Game.UI
{
    /// <summary>
    /// 카드 슬롯의 기본 동작을 정의한 추상 클래스입니다.
    /// </summary>
    public abstract class BaseCardSlotUI : MonoBehaviour, ICardSlot
    {
        protected ISkillCard currentCard;

        /// <summary>
        /// 카드 데이터를 슬롯에 설정합니다.
        /// </summary>
        public virtual void SetCard(ISkillCard card)
        {
            currentCard = card;
        }

        /// <summary>
        /// 슬롯에 저장된 카드 데이터를 반환합니다.
        /// </summary>
        public virtual ISkillCard GetCard()
        {
            return currentCard;
        }

        /// <summary>
        /// 슬롯을 비웁니다.
        /// </summary>
        public virtual void Clear()
        {
            currentCard = null;
        }
    }
}
