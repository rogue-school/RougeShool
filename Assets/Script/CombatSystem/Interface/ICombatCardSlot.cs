using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 실행 슬롯 인터페이스
    /// 카드 UI 및 실행 관련 기능을 제공함
    /// </summary>
    public interface ICombatCardSlot
    {
        /// <summary>
        /// 슬롯의 전투 필드 위치 (왼쪽/오른쪽)
        /// </summary>
        CombatFieldSlotPosition GetCombatPosition();

        /// <summary>
        /// 슬롯에 등록된 카드
        /// </summary>
        ISkillCard GetCard();
        void SetCard(ISkillCard card);

        /// <summary>
        /// 슬롯에 표시되는 카드 UI
        /// </summary>
        SkillCardUI GetCardUI();
        void SetCardUI(SkillCardUI cardUI);

        /// <summary>
        /// 카드 및 UI 제거
        /// </summary>
        void Clear();

        /// <summary>
        /// 카드 존재 여부
        /// </summary>
        bool HasCard();
        bool IsEmpty();

        /// <summary>
        /// 카드 자동 실행
        /// </summary>
        void ExecuteCardAutomatically();
        void ExecuteCardAutomatically(ICardExecutionContext ctx);
    }
}
