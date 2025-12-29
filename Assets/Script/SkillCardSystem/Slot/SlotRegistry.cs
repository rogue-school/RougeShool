using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Zenject;
using Game.CoreSystem.Utility;
using System.Linq;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 전투 슬롯, 핸드 슬롯, 캐릭터 슬롯 레지스트리를 통합 관리하는 클래스입니다.
    /// DI 컨테이너에서 슬롯 레지스트리들을 주입받습니다.
    /// </summary>
    public class SlotRegistry : MonoBehaviour
    {
        #region DI로 주입받는 슬롯 레지스트리

        [Inject] private HandSlotRegistry handSlotRegistry;
        [Inject] private CombatSlotRegistry combatSlotRegistry;
        [Inject] private CharacterSlotRegistry characterSlotRegistry;

        #endregion

        #region 프로퍼티

        /// <summary>
        /// 슬롯 레지스트리가 초기화되었는지 여부를 나타냅니다.
        /// </summary>
        public bool IsInitialized { get; private set; }

        #endregion

        #region 초기화

        private void Awake()
        {
            // 씬 내 전투 슬롯 자동 등록
            if (combatSlotRegistry != null)
            {
                var slots = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .OfType<ICombatCardSlot>()
                    .ToList();
                combatSlotRegistry.RegisterCombatSlots(slots);
            }

            // 씬 내 핸드 슬롯 자동 등록
            if (handSlotRegistry != null)
            {
                var handSlots = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .OfType<IHandCardSlot>()
                    .ToList();
                handSlotRegistry.RegisterHandSlots(handSlots);
                GameLogger.LogInfo($"HandSlotRegistry 자동 등록: {handSlots.Count}개", GameLogger.LogCategory.SkillCard);
            }

            MarkInitialized();
        }

        #endregion

        #region 슬롯 레지스트리 접근

        /// <summary>
        /// 핸드 슬롯 레지스트리를 반환합니다.
        /// </summary>
        public HandSlotRegistry GetHandSlotRegistry() => handSlotRegistry;

        /// <summary>
        /// 전투 슬롯 레지스트리를 반환합니다.
        /// </summary>
        public CombatSlotRegistry GetCombatSlotRegistry() => combatSlotRegistry;

        /// <summary>
        /// 캐릭터 슬롯 레지스트리를 반환합니다.
        /// </summary>
        public CharacterSlotRegistry GetCharacterSlotRegistry() => characterSlotRegistry;

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
