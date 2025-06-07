using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface ICombatCardSlot
    {
        CombatSlotPosition Position { get; }
        CombatFieldSlotPosition GetCombatPosition();

        /// <summary> 현재 슬롯에 등록된 카드 데이터 </summary>
        ISkillCard GetCard();

        /// <summary> 슬롯에 카드 데이터를 등록합니다 </summary>
        void SetCard(ISkillCard card);

        /// <summary> 카드 UI를 가져옵니다 </summary>
        ISkillCardUI GetCardUI();

        /// <summary> 카드 UI를 등록합니다 </summary>
        void SetCardUI(ISkillCardUI cardUI);

        /// <summary> 카드 데이터와 UI 모두 제거합니다 </summary>
        void ClearAll();

        /// <summary> 카드 UI만 제거합니다 (카드 정보는 유지) </summary>
        void ClearCardUI();

        /// <summary> 카드가 존재하는지 확인합니다 </summary>
        bool HasCard();

        /// <summary> 카드 정보와 UI가 모두 비어 있는지 확인합니다 </summary>
        bool IsEmpty();

        /// <summary> 슬롯의 카드 데이터를 사용하여 자동으로 카드 효과를 실행합니다 </summary>
        void ExecuteCardAutomatically();

        /// <summary> 슬롯의 카드 데이터를 지정된 컨텍스트로 실행합니다 </summary>
        void ExecuteCardAutomatically(ICardExecutionContext ctx);

        /// <summary> 슬롯 UI 배치용 Transform을 반환합니다 </summary>
        Transform GetTransform();
    }
}
