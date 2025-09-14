using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.UI;
using Game.IManager;
using Game.CombatSystem.Utility;
using Game.AnimationSystem.Manager;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 슬롯을 자동으로 바인딩하고 슬롯의 상태를 관리하는 클래스입니다.
    /// </summary>
    public class CombatSlotManager : MonoBehaviour, ICombatSlotManager
    {
        #region 슬롯 데이터 저장소

        private Dictionary<CombatSlotPosition, ICombatCardSlot> combatSlots = new();

        // 최적화: 필수 슬롯 위치를 정적 배열로 캐싱
        private static readonly CombatSlotPosition[] RequiredSlots = 
        {
            CombatSlotPosition.SLOT_1,
            CombatSlotPosition.SLOT_2,
            CombatSlotPosition.SLOT_3,
            CombatSlotPosition.SLOT_4
        };

        #endregion

        #region 초기화

        private void Awake()
        {
            AutoBindSlots();
        }

        /// <summary>
        /// 자식 오브젝트의 슬롯 UI를 자동으로 탐색하여 슬롯 정보를 등록합니다.
        /// 중복 슬롯은 경고 로그를 출력합니다.
        /// 4개 슬롯 시스템을 지원합니다.
        /// </summary>
        private void AutoBindSlots()
        {
            combatSlots.Clear();

            CombatExecutionSlotUI[] slotUIs = GetComponentsInChildren<CombatExecutionSlotUI>(true);
            foreach (var slotUI in slotUIs)
            {
                CombatFieldSlotPosition fieldPos = slotUI.GetCombatPosition();
                CombatSlotPosition execPos = SlotPositionUtil.ToExecutionSlot(fieldPos);

                if (!combatSlots.ContainsKey(execPos))
                {
                    combatSlots[execPos] = slotUI;
                }
                else
                {
                    Debug.LogWarning($"[CombatSlotManager] 중복 슬롯 발견: {execPos}");
                }
            }

            // 4개 슬롯 시스템 검증
            ValidateSlotCount();
        }

        /// <summary>
        /// 4개 슬롯이 모두 등록되었는지 확인합니다. (최적화된 버전)
        /// </summary>
        private void ValidateSlotCount()
        {
            // 최적화: 정적 배열 사용으로 메모리 할당 방지
            for (int i = 0; i < RequiredSlots.Length; i++)
            {
                var slot = RequiredSlots[i];
                if (!combatSlots.ContainsKey(slot))
                {
                    Debug.LogWarning($"[CombatSlotManager] 필수 슬롯 누락: {slot}");
                }
            }
        }

        #endregion

        #region 슬롯 접근

        /// <summary>
        /// 실행 슬롯 위치를 기준으로 슬롯을 반환합니다.
        /// 슬롯이 존재하지 않으면 null을 반환합니다.
        /// </summary>
        /// <param name="position">CombatSlotPosition</param>
        public ICombatCardSlot GetSlot(CombatSlotPosition position)
        {
            combatSlots.TryGetValue(position, out var slot);
            return slot;
        }

        /// <summary>
        /// 필드 슬롯 위치를 실행 슬롯 위치로 변환한 후 해당 슬롯을 반환합니다.
        /// </summary>
        /// <param name="fieldPosition">CombatFieldSlotPosition</param>
        public ICombatCardSlot GetSlot(CombatFieldSlotPosition fieldPosition)
        {
            var execPosition = SlotPositionUtil.ToExecutionSlot(fieldPosition);
            return GetSlot(execPosition);
        }

        #endregion

        #region 슬롯 초기화

        /// <summary>
        /// 모든 슬롯에서 카드 및 UI를 제거합니다.
        /// </summary>
        public void ClearAllSlots()
        {
            foreach (var slot in combatSlots.Values)
            {
                slot.ClearAll();
            }
        }

        #endregion

        #region 슬롯 상태 확인

        /// <summary>
        /// 지정한 실행 슬롯이 존재하지 않거나 비어 있는지 확인합니다.
        /// </summary>
        /// <param name="position">CombatSlotPosition</param>
        public bool IsSlotEmpty(CombatSlotPosition position)
        {
            return !combatSlots.ContainsKey(position) || combatSlots[position].IsEmpty();
        }

        /// <summary>
        /// 지정한 필드 슬롯이 비어 있는지 확인합니다.
        /// </summary>
        /// <param name="fieldPosition">CombatFieldSlotPosition</param>
        public bool IsSlotEmpty(CombatFieldSlotPosition fieldPosition)
        {
            var execPosition = SlotPositionUtil.ToExecutionSlot(fieldPosition);
            return IsSlotEmpty(execPosition);
        }

        #endregion

        private void OnSlotCharacterSpawned(string characterId, GameObject characterObject)
        {
            // AnimationFacade.Instance.PlayCharacterAnimation(characterId, "spawn", characterObject); // 제거
        }

        private void OnSlotCharacterDeath(string characterId, GameObject characterObject)
        {
            // AnimationFacade.Instance.PlayCharacterDeathAnimation(characterId, characterObject); // 제거
        }

        private void OnSlotSkillCardUsed(string cardId, GameObject cardObject)
        {
            // AnimationFacade.Instance.PlaySkillCardAnimation(cardId, "use", cardObject); // 제거
        }
    }
}
