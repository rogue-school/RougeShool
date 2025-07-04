using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 전투 슬롯, 핸드 슬롯, 캐릭터 슬롯 레지스트리를 통합 관리하는 클래스입니다.
    /// </summary>
    public class SlotRegistry : MonoBehaviour, ISlotRegistry
    {
        #region 인스펙터에서 할당된 슬롯 레지스트리

        [SerializeField] private HandSlotRegistry handSlotRegistry;
        [SerializeField] private CombatSlotRegistry combatSlotRegistry;
        [SerializeField] private CharacterSlotRegistry characterSlotRegistry;

        #endregion

        #region 프로퍼티

        /// <summary>
        /// 슬롯 레지스트리가 초기화되었는지 여부를 나타냅니다.
        /// </summary>
        public bool IsInitialized { get; private set; }

        #endregion

        #region 슬롯 레지스트리 접근

        /// <summary>
        /// 핸드 슬롯 레지스트리를 반환합니다.
        /// </summary>
        public IHandSlotRegistry GetHandSlotRegistry() => handSlotRegistry;

        /// <summary>
        /// 전투 슬롯 레지스트리를 반환합니다.
        /// </summary>
        public ICombatSlotRegistry GetCombatSlotRegistry() => combatSlotRegistry;

        /// <summary>
        /// 캐릭터 슬롯 레지스트리를 반환합니다.
        /// </summary>
        public ICharacterSlotRegistry GetCharacterSlotRegistry() => characterSlotRegistry;

        #endregion

        #region 전투 슬롯 조회

        /// <summary>
        /// 전투 슬롯 위치를 기준으로 슬롯을 반환합니다.
        /// </summary>
        public ICombatCardSlot GetCombatSlot(CombatSlotPosition position)
        {
            return combatSlotRegistry?.GetSlotByPosition(position);
        }

        /// <summary>
        /// 필드 슬롯 위치를 기준으로 슬롯을 반환합니다.
        /// </summary>
        public ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition position)
        {
            return combatSlotRegistry.GetCombatSlot(position);
        }

        #endregion

        #region 상태 설정

        /// <summary>
        /// 슬롯 레지스트리를 초기화된 상태로 표시합니다.
        /// </summary>
        public void MarkInitialized() => IsInitialized = true;

        #endregion
    }
}
