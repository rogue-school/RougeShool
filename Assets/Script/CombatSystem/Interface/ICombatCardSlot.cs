using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 슬롯에 카드 및 카드 UI를 관리하는 인터페이스입니다.
    /// 카드 등록, 제거, 실행, 시각적 위치 등 슬롯 기능을 정의합니다.
    /// </summary>
    public interface ICombatCardSlot
    {
        /// <summary>
        /// 슬롯의 전투 포지션 정보입니다.
        /// </summary>
        CombatSlotPosition Position { get; }

        /// <summary>
        /// 슬롯의 전체 전장 위치 정보를 반환합니다.
        /// (예: 선공/후공, 전열/후열 등 복합 정보)
        /// </summary>
        CombatFieldSlotPosition GetCombatPosition();

        /// <summary>
        /// 현재 슬롯에 등록된 카드 데이터를 반환합니다.
        /// </summary>
        ISkillCard GetCard();

        /// <summary>
        /// 슬롯에 카드 데이터를 등록합니다.
        /// </summary>
        /// <param name="card">등록할 카드 객체</param>
        void SetCard(ISkillCard card);

        /// <summary>
        /// 현재 슬롯에 등록된 카드 UI를 반환합니다.
        /// </summary>
        ISkillCardUI GetCardUI();

        /// <summary>
        /// 카드 UI를 슬롯에 등록합니다.
        /// </summary>
        /// <param name="cardUI">카드 UI 객체</param>
        void SetCardUI(ISkillCardUI cardUI);

        /// <summary>
        /// 카드 데이터와 UI를 모두 제거합니다.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// 카드 UI만 제거합니다. 카드 데이터는 유지됩니다.
        /// </summary>
        void ClearCardUI();

        /// <summary>
        /// 슬롯에 카드 데이터가 등록되어 있는지 확인합니다.
        /// </summary>
        /// <returns>true면 카드 존재</returns>
        bool HasCard();

        /// <summary>
        /// 카드 데이터와 UI 모두 등록되지 않은 상태인지 확인합니다.
        /// </summary>
        /// <returns>true면 완전히 비어 있음</returns>
        bool IsEmpty();

        /// <summary>
        /// 슬롯에 등록된 카드 데이터를 기반으로 효과를 자동 실행합니다.
        /// </summary>
        void ExecuteCardAutomatically();

        /// <summary>
        /// 주어진 컨텍스트를 사용하여 슬롯의 카드 효과를 실행합니다.
        /// </summary>
        /// <param name="ctx">카드 실행에 필요한 컨텍스트</param>
        void ExecuteCardAutomatically(ICardExecutionContext ctx);

        /// <summary>
        /// 카드 UI가 위치할 슬롯의 Transform을 반환합니다.
        /// </summary>
        Transform GetTransform();
    }
}
