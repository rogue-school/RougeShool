using System;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 턴 동안 슬롯에 등록된 카드 정보를 관리하는 인터페이스입니다.
    /// 카드 등록, 조회, 초기화, 상태 이벤트 처리 등을 제공합니다.
    /// </summary>
    public interface ITurnCardRegistry
    {
        /// <summary>
        /// 카드 상태가 변경될 때 호출되는 이벤트입니다.
        /// </summary>
        event Action OnCardStateChanged;

        // ─────────────── 등록 및 조회 ───────────────

        /// <summary>
        /// 카드 및 UI 정보를 슬롯에 등록합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <param name="card">등록할 카드</param>
        /// <param name="ui">카드에 연결된 UI</param>
        /// <param name="owner">카드 소유자 (플레이어 또는 적)</param>
        void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner);

        /// <summary>
        /// 특정 슬롯에 등록된 카드를 반환합니다.
        /// </summary>
        /// <param name="position">조회할 슬롯 위치</param>
        /// <returns>등록된 카드 또는 null</returns>
        ISkillCard GetCardInSlot(CombatSlotPosition position);

        // ─────────────── 초기화 및 클리어 ───────────────

        /// <summary>
        /// 지정된 슬롯을 초기화합니다.
        /// </summary>
        /// <param name="position">초기화할 슬롯 위치</param>
        void ClearSlot(CombatSlotPosition position);

        /// <summary>
        /// 모든 슬롯을 초기화합니다.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// 등록된 적 카드만 제거합니다. (플레이어 카드는 유지됨)
        /// </summary>
        void ClearEnemyCardsOnly();

        /// <summary>
        /// 전체 초기화를 수행합니다. (상태 및 슬롯 포함)
        /// </summary>
        void Reset();

        // ─────────────── 상태 검사 ───────────────

        /// <summary>
        /// 현재 플레이어 카드가 등록되어 있는지 확인합니다.
        /// </summary>
        /// <returns>플레이어 카드 존재 여부</returns>
        bool HasPlayerCard();

        /// <summary>
        /// 현재 적 카드가 등록되어 있는지 확인합니다.
        /// </summary>
        /// <returns>적 카드 존재 여부</returns>
        bool HasEnemyCard();

        // ─────────────── 슬롯 예약 ───────────────

        /// <summary>
        /// 다음 턴에서 적이 사용할 슬롯을 예약합니다.
        /// </summary>
        /// <param name="slot">예약할 슬롯 위치</param>
        void ReserveNextEnemySlot(CombatSlotPosition slot);

        /// <summary>
        /// 예약된 적 슬롯 위치를 반환합니다.
        /// </summary>
        /// <returns>예약된 슬롯 위치 또는 null</returns>
        CombatSlotPosition? GetReservedEnemySlot();

        /// <summary>
        /// 특정 슬롯에서 카드를 제거합니다.
        /// </summary>
        /// <param name="position">제거할 슬롯 위치</param>
        void RemoveCardFromSlot(CombatSlotPosition position);

        /// <summary>
        /// 카드를 슬롯에 등록합니다.
        /// </summary>
        /// <param name="position">등록할 슬롯 위치</param>
        /// <param name="card">등록할 카드</param>
        /// <param name="ui">카드 UI</param>
        /// <param name="owner">카드 소유자</param>
        void RegisterCardToSlot(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner);
    }
}
