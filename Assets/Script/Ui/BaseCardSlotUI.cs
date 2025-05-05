using UnityEngine;
using Game.Battle;
using Game.Interface;

namespace Game.UI
{
    /// <summary>
    /// 모든 카드 슬롯 UI의 베이스 클래스입니다. 자동 바인딩, 카드 처리, 슬롯 위치 관리 등 공통 기능을 포함합니다.
    /// </summary>
    public class BaseCardSlotUI : MonoBehaviour, ICardSlot
    {
        /// <summary>
        /// 이 슬롯의 전투 내 위치입니다.
        /// </summary>
        public virtual SlotPosition Position { get; protected set; }

        /// <summary>
        /// 현재 슬롯에 연결된 카드 참조입니다.
        /// </summary>
        protected ISkillCard card;

        /// <summary>
        /// 슬롯 자동 바인딩 여부
        /// </summary>
        protected bool isBound = false;

        /// <summary>
        /// 자동으로 슬롯 위치를 추론하여 설정합니다.
        /// </summary>
        public virtual void AutoBind()
        {
            isBound = true;
        }

        /// <summary>
        /// 슬롯을 초기화합니다. 카드 연결 해제 및 UI 정리.
        /// </summary>
        public virtual void Clear()
        {
            card = null;
        }

        /// <summary>
        /// 슬롯에 카드를 설정합니다.
        /// </summary>
        /// <param name="newCard">슬롯에 연결할 스킬 카드</param>
        public virtual void SetCard(ISkillCard newCard)
        {
            card = newCard;
        }

        /// <summary>
        /// 현재 슬롯에 설정된 카드를 반환합니다.
        /// </summary>
        public virtual ISkillCard GetCard()
        {
            return card;
        }

        /// <summary>
        /// 슬롯의 위치를 외부에서 강제로 설정합니다.
        /// </summary>
        public virtual void SetSlotPosition(SlotPosition newPosition)
        {
            Position = newPosition;
        }
    }
}
