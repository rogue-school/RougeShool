using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.UI;
using Game.IManager;
using Game.CombatSystem.Utility;
using Game.AnimationSystem.Manager;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 슬롯을 자동으로 바인딩하고 슬롯의 상태를 관리하는 클래스입니다.
    /// </summary>
    public class CombatSlotManager : MonoBehaviour, ICombatSlotManager
    {
        #region 슬롯 데이터 저장소

        private Dictionary<CombatSlotPosition, ICombatCardSlot> combatSlots = new();

        // 최적화: 필수 슬롯 위치를 정적 배열로 캐싱 (새로운 5슬롯 시스템)
        private static readonly CombatSlotPosition[] RequiredSlots = 
        {
            CombatSlotPosition.BATTLE_SLOT,
            CombatSlotPosition.WAIT_SLOT_1,
            CombatSlotPosition.WAIT_SLOT_2,
            CombatSlotPosition.WAIT_SLOT_3,
            CombatSlotPosition.WAIT_SLOT_4
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

            // 먼저 자식에서 검색 (더 효율적)
            CombatExecutionSlotUI[] childSlotUIs = GetComponentsInChildren<CombatExecutionSlotUI>(true);
            foreach (var slotUI in childSlotUIs)
            {
                var execPos = slotUI.Position;
                if (!combatSlots.ContainsKey(execPos))
                {
                    combatSlots[execPos] = slotUI;
                    // 자식에서 슬롯 바인딩
                }
                else
                {
                    Debug.LogWarning($"[CombatSlotManager] 중복 슬롯 발견: {execPos}");
                }
            }

            // 보강: 현재 오브젝트의 자식에 슬롯이 없을 수 있으므로, 누락된 슬롯이 있으면 씬 전체에서 한 번 더 검색합니다.
            bool hasMissing = false;
            for (int i = 0; i < RequiredSlots.Length; i++)
            {
                if (!combatSlots.ContainsKey(RequiredSlots[i]))
                {
                    hasMissing = true;
                    break;
                }
            }

            if (hasMissing)
            {
                var allSlotUIs = Object.FindObjectsByType<CombatExecutionSlotUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var slotUI in allSlotUIs)
                {
                    var execPos = slotUI.Position;
                    if (!combatSlots.ContainsKey(execPos))
                    {
                        combatSlots[execPos] = slotUI;
                        // 전역 검색으로 슬롯 바인딩 (정상적인 동작)
                    }
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

        [System.Obsolete("4-슬롯 표준: CombatFieldSlotPosition 대신 CombatSlotPosition 사용")]
        public ICombatCardSlot GetSlot(CombatFieldSlotPosition fieldPosition)
        {
            Debug.LogWarning("[CombatSlotManager] 필드 포지션 기반 조회는 비권장입니다. SLOT_1..SLOT_4를 사용하세요.");
            return null;
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

        [System.Obsolete("4-슬롯 표준: CombatFieldSlotPosition 대신 CombatSlotPosition 사용")]
        public bool IsSlotEmpty(CombatFieldSlotPosition fieldPosition)
        {
            return true;
        }

        #endregion

        #region 새로운 5슬롯 시스템

        /// <summary>
        /// 새로운 5슬롯 시스템: 자식 오브젝트의 슬롯 UI를 자동으로 탐색하여 슬롯 정보를 등록합니다.
        /// 5개 슬롯 시스템을 지원합니다.
        /// </summary>
        public void AutoBindSlotsNew()
        {
            combatSlots.Clear();

            CombatExecutionSlotUI[] slotUIs = GetComponentsInChildren<CombatExecutionSlotUI>(true);
            foreach (var slotUI in slotUIs)
            {
                var execPos = slotUI.Position;
                if (!combatSlots.ContainsKey(execPos))
                {
                    combatSlots[execPos] = slotUI;
                }
                else
                {
                    Debug.LogWarning($"[CombatSlotManager] 중복 슬롯 발견: {execPos}");
                }
            }

            // 보강: 현재 오브젝트의 자식에 슬롯이 없을 수 있으므로, 누락된 슬롯이 있으면 씬 전체에서 한 번 더 검색합니다.
            bool hasMissing = false;
            for (int i = 0; i < RequiredSlots.Length; i++)
            {
                if (!combatSlots.ContainsKey(RequiredSlots[i]))
                {
                    hasMissing = true;
                    break;
                }
            }

            if (hasMissing)
            {
                var allSlotUIs = Object.FindObjectsByType<CombatExecutionSlotUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var slotUI in allSlotUIs)
                {
                    var execPos = slotUI.Position;
                    if (!combatSlots.ContainsKey(execPos))
                    {
                        combatSlots[execPos] = slotUI;
                        // 전역 검색으로 슬롯 바인딩 (정상적인 동작)
                    }
                }
            }

            // 새로운 5슬롯 시스템 검증
            ValidateSlotCountNew();
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 5개 슬롯이 모두 등록되었는지 확인합니다.
        /// </summary>
        private void ValidateSlotCountNew()
        {
            for (int i = 0; i < RequiredSlots.Length; i++)
            {
                var slot = RequiredSlots[i];
                if (!combatSlots.ContainsKey(slot))
                {
                    Debug.LogError($"[CombatSlotManager] 새로운 5슬롯 시스템에서 필수 슬롯이 누락되었습니다: {slot}");
                }
            }

            Debug.Log($"[CombatSlotManager] 새로운 5슬롯 시스템 슬롯 바인딩 완료: {combatSlots.Count}개 슬롯 등록됨");
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 전투슬롯에 카드가 있는지 확인합니다.
        /// </summary>
        /// <returns>전투슬롯에 카드가 있으면 true</returns>
        public bool HasCardInBattleSlot()
        {
            return !IsSlotEmpty(CombatSlotPosition.BATTLE_SLOT);
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 전투슬롯의 카드를 반환합니다.
        /// </summary>
        /// <returns>전투슬롯의 카드, 없으면 null</returns>
        public ISkillCard GetCardInBattleSlot()
        {
            if (combatSlots.TryGetValue(CombatSlotPosition.BATTLE_SLOT, out var slot))
            {
                return slot.GetCard();
            }
            return null;
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 대기슬롯에 카드가 있는지 확인합니다.
        /// </summary>
        /// <param name="waitSlotNumber">대기슬롯 번호 (1~4)</param>
        /// <returns>해당 대기슬롯에 카드가 있으면 true</returns>
        public bool HasCardInWaitSlot(int waitSlotNumber)
        {
            var position = CombatSlotPositionExtensions.FromWaitSlotNumber(waitSlotNumber);
            if (position == CombatSlotPosition.NONE)
            {
                Debug.LogWarning($"[CombatSlotManager] 유효하지 않은 대기슬롯 번호: {waitSlotNumber}");
                return false;
            }
            return !IsSlotEmpty(position);
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 대기슬롯의 카드를 반환합니다.
        /// </summary>
        /// <param name="waitSlotNumber">대기슬롯 번호 (1~4)</param>
        /// <returns>해당 대기슬롯의 카드, 없으면 null</returns>
        public ISkillCard GetCardInWaitSlot(int waitSlotNumber)
        {
            var position = CombatSlotPositionExtensions.FromWaitSlotNumber(waitSlotNumber);
            if (position == CombatSlotPosition.NONE)
            {
                Debug.LogWarning($"[CombatSlotManager] 유효하지 않은 대기슬롯 번호: {waitSlotNumber}");
                return null;
            }

            if (combatSlots.TryGetValue(position, out var slot))
            {
                return slot.GetCard();
            }
            return null;
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 모든 슬롯의 상태를 초기화합니다.
        /// </summary>
        public void ClearAllSlotsNew()
        {
            foreach (var slot in RequiredSlots)
            {
                if (combatSlots.TryGetValue(slot, out var slotComponent))
                {
                    slotComponent.ClearAll();
                }
            }
            Debug.Log("[CombatSlotManager] 새로운 5슬롯 시스템 모든 슬롯 초기화 완료");
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 슬롯 상태를 디버그 출력합니다.
        /// </summary>
        public void DebugSlotsStatusNew()
        {
            Debug.Log("=== 새로운 5슬롯 시스템 슬롯 상태 ===");
            foreach (var slot in RequiredSlots)
            {
                if (combatSlots.TryGetValue(slot, out var slotComponent))
                {
                    var card = slotComponent.GetCard();
                    var cardName = card?.GetCardName() ?? "비어있음";
                    Debug.Log($"{slot}: {cardName}");
                }
                else
                {
                    Debug.Log($"{slot}: 슬롯 없음");
                }
            }
            Debug.Log("=====================================");
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
