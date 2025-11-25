using UnityEngine;
using Game.CombatSystem.Interface;
using System.Collections;
using System.Linq;
using Game.CombatSystem.Slot;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.Initialization
{
    /// <summary>
    /// 전투 시작 시 슬롯 정보를 자동으로 바인딩하고 레지스트리에 등록하는 초기화 스텝입니다.
    /// </summary>
    public class SlotInitializationStep
    {
        #region 필드

        private readonly object slotRegistry; // TODO: 적절한 타입으로 교체 필요

        public int Order => 0;

        #endregion

        #region 생성자

        public SlotInitializationStep(object slotRegistry)
        {
            this.slotRegistry = slotRegistry;
        }

        #endregion

        #region 초기화 메서드

        public IEnumerator Initialize()
        {
            GameLogger.LogInfo("[SlotInitializationStep] 슬롯 초기화 시작", GameLogger.LogCategory.Combat);

            // null 체크 추가
            if (slotRegistry == null)
            {
                GameLogger.LogError("[SlotInitializationStep] slotRegistry가 null입니다.", GameLogger.LogCategory.Combat);
                yield break;
            }

            // 슬롯 자동 바인딩 (SlotInitializer 제거로 인해 직접 처리)
            GameLogger.LogInfo("[SlotInitializationStep] 슬롯 자동 바인딩 기능은 현재 구현되지 않음", GameLogger.LogCategory.Combat);

            // 슬롯 레지스트리 초기화 대기
            float timeout = 5f; // 5초 타임아웃
            float elapsed = 0f;
            
            // TODO: slotRegistry가 object 타입이므로 적절한 캐스팅 필요
            // while ((slotRegistry == null || !slotRegistry.IsInitialized) && elapsed < timeout)
            while ((slotRegistry == null || !((slotRegistry as SlotRegistry)?.IsInitialized ?? false)) && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= timeout)
            {
                GameLogger.LogError("[SlotInitializationStep] 슬롯 레지스트리 초기화 타임아웃", GameLogger.LogCategory.Combat);
                yield break;
            }

            // TODO: slotRegistry가 object 타입이므로 적절한 캐스팅 필요
            GameLogger.LogInfo("[SlotInitializationStep] 슬롯 초기화 완료 (상세 정보는 임시로 비활성화)", GameLogger.LogCategory.Combat);

            GameLogger.LogInfo("[SlotInitializationStep] 슬롯 초기화 완료", GameLogger.LogCategory.Combat);
        }

        #endregion
    }
}
