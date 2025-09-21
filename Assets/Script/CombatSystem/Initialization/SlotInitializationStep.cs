using UnityEngine;
using Game.CombatSystem.Interface;
using System.Collections;
using System.Linq;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Intialization
{
    /// <summary>
    /// 전투 시작 시 슬롯 정보를 자동으로 바인딩하고 레지스트리에 등록하는 초기화 스텝입니다.
    /// </summary>
    public class SlotInitializationStep : ICombatInitializerStep
    {
        #region 필드

        private readonly ISlotRegistry slotRegistry;

        public int Order => 0;

        #endregion

        #region 생성자

        public SlotInitializationStep(ISlotRegistry slotRegistry)
        {
            this.slotRegistry = slotRegistry;
        }

        #endregion

        #region 초기화 메서드

        public IEnumerator Initialize()
        {
            Debug.Log("<color=cyan>[SlotInitializationStep] 슬롯 초기화 시작</color>");

            // null 체크 추가
            if (slotRegistry == null)
            {
                Debug.LogError("[SlotInitializationStep] slotRegistry가 null입니다.");
                yield break;
            }

            // 슬롯 자동 바인딩 (SlotInitializer 제거로 인해 직접 처리)
            Debug.Log("[SlotInitializationStep] 슬롯 자동 바인딩 기능은 현재 구현되지 않음");

            // 슬롯 레지스트리 초기화 대기
            float timeout = 5f; // 5초 타임아웃
            float elapsed = 0f;
            
            while ((slotRegistry == null || !slotRegistry.IsInitialized) && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= timeout)
            {
                Debug.LogError("[SlotInitializationStep] 슬롯 레지스트리 초기화 타임아웃");
                yield break;
            }

            Debug.Log($"[SlotInitializationStep] 핸드 슬롯 수: {slotRegistry.GetHandSlotRegistry()?.GetAllHandSlots()?.Count() ?? 0}");
            Debug.Log($"[SlotInitializationStep] 전투 슬롯 수: {slotRegistry.GetCombatSlotRegistry()?.GetAllCombatSlots()?.Count() ?? 0}");
            Debug.Log($"[SlotInitializationStep] 캐릭터 슬롯 수: {slotRegistry.GetCharacterSlotRegistry()?.GetAllCharacterSlots()?.Count() ?? 0}");

            Debug.Log("<color=cyan>[SlotInitializationStep] 슬롯 초기화 완료</color>");
        }

        #endregion
    }
}
