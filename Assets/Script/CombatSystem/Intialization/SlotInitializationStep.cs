using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using System.Collections;
using System.Linq;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Initialization
{
    public class SlotInitializationStep : MonoBehaviour, ICombatInitializerStep
    {
        [Inject] private ISlotRegistry slotRegistry;
        [Inject] private SlotInitializer slotInitializer;

        public int Order => 0;

        public IEnumerator Initialize()
        {
            Debug.Log("[SlotInitializationStep] 슬롯 초기화 시작");

            slotInitializer.AutoBindAllSlots();

            yield return new WaitUntil(() => slotRegistry != null && slotRegistry.IsInitialized);

            Debug.Log($"[SlotInitializationStep] 핸드 슬롯 수: {slotRegistry.GetHandSlotRegistry()?.GetAllHandSlots()?.Count() ?? 0}");
            Debug.Log($"[SlotInitializationStep] 전투 슬롯 수: {slotRegistry.GetCombatSlotRegistry()?.GetAllCombatSlots()?.Count() ?? 0}");
            Debug.Log($"[SlotInitializationStep] 캐릭터 슬롯 수: {slotRegistry.GetCharacterSlotRegistry()?.GetAllCharacterSlots()?.Count() ?? 0}");

            Debug.Log("[SlotInitializationStep] 슬롯 초기화 완료");
        }
    }
}
