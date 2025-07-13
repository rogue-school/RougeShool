using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using System.Collections;
using System.Linq;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Initialization
{
    /// <summary>
    /// 전투 시작 시 슬롯 정보를 자동으로 바인딩하고 레지스트리에 등록하는 초기화 스텝입니다.
    /// </summary>
    public class SlotInitializationStep : MonoBehaviour, ICombatInitializerStep
    {
        #region 필드 및 주입

        [Inject] private ISlotRegistry slotRegistry;
        [Inject] private SlotInitializer slotInitializer;

        public int Order => 0;

        #endregion

        #region 초기화 메서드

        public IEnumerator Initialize()
        {
            Debug.Log("<color=cyan>[SlotInitializationStep] 슬롯 초기화 시작</color>");

            slotInitializer.AutoBindAllSlots();

            yield return new WaitUntil(() => slotRegistry != null && slotRegistry.IsInitialized);

            Debug.Log($"[SlotInitializationStep] 핸드 슬롯 수: {slotRegistry.GetHandSlotRegistry()?.GetAllHandSlots()?.Count() ?? 0}");
            Debug.Log($"[SlotInitializationStep] 전투 슬롯 수: {slotRegistry.GetCombatSlotRegistry()?.GetAllCombatSlots()?.Count() ?? 0}");
            Debug.Log($"[SlotInitializationStep] 캐릭터 슬롯 수: {slotRegistry.GetCharacterSlotRegistry()?.GetAllCharacterSlots()?.Count() ?? 0}");

            Debug.Log("<color=cyan>[SlotInitializationStep] 슬롯 초기화 완료</color>");
        }

        #endregion
    }
}
