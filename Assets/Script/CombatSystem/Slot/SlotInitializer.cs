using System.Linq;
using UnityEngine;
using Game.CombatSystem.Interface;
using Zenject;
using Game.CharacterSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Slot
{
    public class SlotInitializer : MonoBehaviour, ISlotInitializer
    {
        [Inject] private ISlotRegistry _slotRegistry;

        public void AutoBindAllSlots()
        {
            if (_slotRegistry == null)
            {
                Debug.LogError("[SlotInitializer] SlotRegistry가 주입되지 않았습니다.");
                return;
            }

            Debug.Log("[SlotInitializer] === 슬롯 자동 바인딩 시작 ===");

            var handSlots = FindAll<IHandCardSlot>();
            var combatSlots = FindAll<ICombatCardSlot>();
            var characterSlots = FindAll<ICharacterSlot>();

            Debug.Log($"[SlotInitializer] 탐색된 슬롯 수 - 핸드: {handSlots.Length}, 전투: {combatSlots.Length}, 캐릭터: {characterSlots.Length}");

            _slotRegistry.GetHandSlotRegistry()?.RegisterHandSlots(handSlots);
            _slotRegistry.GetCombatSlotRegistry()?.RegisterCombatSlots(combatSlots);
            _slotRegistry.GetCharacterSlotRegistry()?.RegisterCharacterSlots(characterSlots);

            if (handSlots.Length > 0 || combatSlots.Length > 0 || characterSlots.Length > 0)
            {
                _slotRegistry.MarkInitialized();
                Debug.Log("[SlotInitializer] === 슬롯 자동 바인딩 완료 ===");
            }
            else
            {
                Debug.LogWarning("[SlotInitializer] 슬롯이 하나도 탐색되지 않았습니다. 초기화를 완료하지 않습니다.");
            }
        }

        private T[] FindAll<T>() where T : class
        {
            return Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<T>()
                .ToArray();
        }
    }
}
