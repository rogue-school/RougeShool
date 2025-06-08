namespace Game.CombatSystem.Interface
{
    using Game.SkillCardSystem.Interface;
    using Game.SkillCardSystem.UI;

    /// <summary>
    /// 전투 슬롯 내 기존 카드를 새로운 카드로 교체하는 기능을 담당하는 인터페이스입니다.
    /// 기존 카드 제거 및 새로운 카드 + UI 등록 처리를 포함합니다.
    /// </summary>
    public interface ICardReplacementHandler
    {
        /// <summary>
        /// 슬롯에 있는 기존 카드를 교체하고 새로운 카드와 UI를 설정합니다.
        /// </summary>
        /// <param name="slot">카드가 배치된 전투 슬롯</param>
        /// <param name="newCard">새로 교체할 스킬 카드</param>
        /// <param name="newCardUI">새로운 카드에 해당하는 UI 오브젝트</param>
        void ReplaceSlotCard(ICombatCardSlot slot, ISkillCard newCard, SkillCardUI newCardUI);
    }
}
