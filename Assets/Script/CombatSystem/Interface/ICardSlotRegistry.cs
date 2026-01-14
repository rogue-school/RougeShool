using System;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Data;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 카드 레지스트리 전담 인터페이스
    /// 슬롯별 카드 및 UI 관리를 담당합니다.
    /// </summary>
    public interface ICardSlotRegistry
    {
        /// <summary>
        /// 카드를 슬롯에 등록합니다.
        /// </summary>
        /// <param name="position">등록할 슬롯 위치</param>
        /// <param name="card">등록할 카드</param>
        /// <param name="ui">카드 UI</param>
        /// <param name="owner">카드 소유자</param>
        void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner);

        /// <summary>
        /// 슬롯의 카드를 반환합니다.
        /// </summary>
        /// <param name="slot">슬롯 위치</param>
        /// <returns>해당 슬롯의 카드 (없으면 null)</returns>
        ISkillCard GetCardInSlot(CombatSlotPosition slot);

        /// <summary>
        /// 슬롯의 카드 UI를 반환합니다.
        /// </summary>
        /// <param name="slot">슬롯 위치</param>
        /// <returns>해당 슬롯의 UI (없으면 null)</returns>
        SkillCardUI GetCardUIInSlot(CombatSlotPosition slot);

        /// <summary>
        /// 슬롯을 클리어합니다 (UI 포함)
        /// </summary>
        /// <param name="slot">클리어할 슬롯</param>
        void ClearSlot(CombatSlotPosition slot);

        /// <summary>
        /// 모든 슬롯을 완전히 정리합니다.
        /// </summary>
        void ClearAllSlots();

        /// <summary>
        /// 적 카드만 제거하고 플레이어 카드 보존
        /// </summary>
        void ClearEnemyCardsOnly();

        /// <summary>
        /// 대기 슬롯(Wait1~4)만 클리어합니다.
        /// </summary>
        void ClearWaitSlots();

        /// <summary>
        /// 플레이어 카드가 있는지 확인합니다.
        /// </summary>
        bool HasPlayerCard();

        /// <summary>
        /// 적 카드가 있는지 확인합니다.
        /// </summary>
        bool HasEnemyCard();

        /// <summary>
        /// 특정 슬롯에 카드가 있는지 확인합니다.
        /// </summary>
        bool HasCardInSlot(CombatSlotPosition slot);

        /// <summary>
        /// 예약된 적 슬롯을 반환합니다.
        /// </summary>
        CombatSlotPosition? GetReservedEnemySlot();

        /// <summary>
        /// 다음 적 슬롯을 예약합니다.
        /// </summary>
        void ReserveNextEnemySlot(CombatSlotPosition slot);

        /// <summary>
        /// 카드를 다른 슬롯으로 이동합니다 (데이터만)
        /// </summary>
        void MoveCardData(CombatSlotPosition from, CombatSlotPosition to);

        /// <summary>
        /// 카드 상태가 변경될 때 발생하는 이벤트
        /// </summary>
        event Action OnCardStateChanged;
    }
}
