using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 슬롯에서 카드 및 UI를 관리하는 인터페이스입니다.
    /// 카드 등록, 제거, 실행 등 슬롯의 핵심 기능을 제공합니다.
    /// </summary>
    public interface ICombatCardSlot
    {
        /// <summary>
        /// 이 슬롯의 전투 위치(예: 선공 1번, 후공 2번 등)를 나타냅니다.
        /// 플레이어와 적이 공용으로 사용하는 위치입니다.
        /// </summary>
        CombatSlotPosition Position { get; }

        /// <summary>
        /// 슬롯의 전체 필드 포지션 정보를 반환합니다.
        /// 예: 시전자/대상 여부, 선공/후공 등이 포함될 수 있습니다.
        /// </summary>
        /// <returns>전장 위치 정보</returns>
        CombatFieldSlotPosition GetCombatPosition();

        /// <summary>
        /// 슬롯에 현재 등록된 스킬 카드 데이터를 반환합니다.
        /// </summary>
        /// <returns>등록된 카드 객체 또는 null</returns>
        ISkillCard GetCard();

        /// <summary>
        /// 슬롯에 스킬 카드 데이터를 등록합니다.
        /// </summary>
        /// <param name="card">등록할 카드 객체</param>
        void SetCard(ISkillCard card);

        /// <summary>
        /// 슬롯에 등록된 카드 UI 객체를 반환합니다.
        /// </summary>
        /// <returns>카드 UI 또는 null</returns>
        ISkillCardUI GetCardUI();

        /// <summary>
        /// 카드 UI를 슬롯에 등록합니다.
        /// </summary>
        /// <param name="cardUI">등록할 카드 UI 객체</param>
        void SetCardUI(ISkillCardUI cardUI);

        /// <summary>
        /// 카드 데이터와 카드 UI 모두를 제거합니다.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// 카드 UI만 제거합니다. 카드 데이터는 유지됩니다.
        /// </summary>
        void ClearCardUI();

        /// <summary>
        /// 슬롯에 카드 데이터가 존재하는지 여부를 반환합니다.
        /// </summary>
        /// <returns>카드가 있으면 true</returns>
        bool HasCard();

        /// <summary>
        /// 슬롯이 완전히 비어 있는지 확인합니다 (카드 + UI 모두 없음).
        /// </summary>
        /// <returns>완전히 비어 있다면 true</returns>
        bool IsEmpty();

        /// <summary>
        /// 슬롯에 등록된 카드의 효과를 자동 실행합니다.
        /// 기본 컨텍스트를 사용하거나 내부 상태에 따라 처리됩니다.
        /// </summary>
        void ExecuteCardAutomatically();

        /// <summary>
        /// 주어진 컨텍스트를 사용하여 카드 효과를 실행합니다.
        /// </summary>
        /// <param name="ctx">카드 실행 컨텍스트</param>
        void ExecuteCardAutomatically(ICardExecutionContext ctx);

        /// <summary>
        /// 카드 UI가 배치될 슬롯의 트랜스폼을 반환합니다.
        /// </summary>
        /// <returns>UI 배치 기준이 되는 Transform</returns>
        Transform GetTransform();
    }
}
