using UnityEngine;
using UnityEngine.UI;
using Game.Interface;
using Game.Battle;

namespace Game.UI
{
    /// <summary>
    /// 모든 카드 슬롯 UI의 기본 클래스입니다.
    /// 슬롯 위치 정보 및 카드 참조 기능을 제공합니다.
    /// </summary>
    public abstract class BaseCardSlotUI : MonoBehaviour
    {
        [Header("슬롯 테두리 이미지")]
        [SerializeField] protected Image frame;

        protected ISkillCard card;

        /// <summary>
        /// 이 슬롯이 담당하는 위치를 반환합니다.
        /// </summary>
        public abstract SlotPosition Position { get; }

        /// <summary>
        /// 카드 설정
        /// </summary>
        public virtual void SetCard(ISkillCard skillCard)
        {
            card = skillCard;
        }

        /// <summary>
        /// 카드 제거
        /// </summary>
        public virtual void Clear()
        {
            card = null;
        }

        /// <summary>
        /// 카드 유무 확인
        /// </summary>
        public bool HasCard()
        {
            return card != null;
        }

        /// <summary>
        /// 카드 반환
        /// </summary>
        public ISkillCard GetCard()
        {
            return card;
        }

        /// <summary>
        /// 슬롯의 위치를 자동 설정합니다. 하위 클래스에서 재정의할 수 있습니다.
        /// </summary>
        public virtual void AutoBind() { }
    }
}
