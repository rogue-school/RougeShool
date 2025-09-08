namespace Game.CombatSystem.Interface
{
    using Game.SkillCardSystem.Interface;
    using Game.SkillCardSystem.UI;

    /// <summary>
    /// 전투 슬롯에 배치된 기존 카드를 새로운 카드로 교체하는 책임을 갖는 인터페이스입니다.
    /// 교체 시 기존 카드 및 UI 제거, 새로운 카드와 UI 등록 처리를 수행합니다.
    /// </summary>
    public interface ICardReplacementHandler
    {
        /// <summary>
        /// 지정된 전투 슬롯에서 기존 카드를 제거하고 새로운 카드와 UI로 교체합니다.
        /// UI 상의 위치나 드래그 가능한 상태, 레지스트리 연동 등을 함께 처리해야 합니다.
        /// </summary>
        /// <param name="slot">
        /// 교체 대상이 되는 전투 슬롯. 기존 카드가 이 슬롯에 배치되어 있어야 합니다.
        /// </param>
        /// <param name="newCard">
        /// 새로 등록할 스킬 카드 인스턴스입니다. <see cref="ISkillCard"/> 참조.
        /// </param>
        /// <param name="newCardUI">
        /// 새 카드에 연결할 UI 오브젝트입니다. <see cref="SkillCardUI"/> 참조.
        /// </param>
        void ReplaceSlotCard(ICombatCardSlot slot, ISkillCard newCard, SkillCardUI newCardUI);
    }
}
