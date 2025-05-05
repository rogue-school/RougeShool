using Game.Cards;

namespace Game.Interface
{
    /// <summary>
    /// 카드 슬롯이 구현해야 할 인터페이스입니다.
    /// </summary>
    public interface ICardSlot
    {
        /// <summary>
        /// 카드 데이터를 슬롯에 설정합니다.
        /// </summary>
        void SetCard(ISkillCard card);

        /// <summary>
        /// 슬롯을 비우는 공통 메서드입니다.
        /// </summary>
        void Clear();

        /// <summary>
        /// 슬롯에 저장된 카드 데이터를 반환합니다.
        /// </summary>
        ISkillCard GetCard();
    }
}
