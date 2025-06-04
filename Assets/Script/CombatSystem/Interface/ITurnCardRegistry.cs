using System;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    public interface ITurnCardRegistry
    {
        event Action OnCardStateChanged;

        /// <summary>
        /// 전투 슬롯 위치에 따라 카드 등록
        /// </summary>
        void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner);

        /// <summary>
        /// 해당 슬롯의 카드 가져오기
        /// </summary>
        ISkillCard GetCardInSlot(CombatSlotPosition position);

        /// <summary>
        /// 슬롯 초기화
        /// </summary>
        void ClearSlot(CombatSlotPosition position);

        /// <summary>
        /// 모든 슬롯 초기화
        /// </summary>
        void ClearAll();

        /// <summary>
        /// 등록된 플레이어 카드 여부
        /// </summary>
        bool HasPlayerCard();

        /// <summary>
        /// 등록된 적 카드 여부
        /// </summary>
        bool HasEnemyCard();

        /// <summary>
        /// 적이 사용할 다음 슬롯을 예약
        /// </summary>
        void ReserveNextEnemySlot(CombatSlotPosition slot);

        /// <summary>
        /// 예약된 적 슬롯 위치 반환
        /// </summary>
        CombatSlotPosition? GetReservedEnemySlot();

        /// <summary>
        /// 전체 초기화
        /// </summary>
        void Reset();

        /// <summary>
        /// 적 카드만 제거 (플레이어 카드 보존)
        /// </summary>
        void ClearEnemyCardsOnly();
    }
}
