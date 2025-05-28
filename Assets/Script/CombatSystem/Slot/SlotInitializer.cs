using System.Linq;
using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Slot
{
    public class SlotInitializer : MonoBehaviour, ISlotInitializer
    {
        private SlotRegistry _slotRegistry;

        public void Inject(SlotRegistry slotRegistry)
        {
            _slotRegistry = slotRegistry;
        }

        public void AutoBindAllSlots()
        {
            if (_slotRegistry == null)
            {
                Debug.LogError("[SlotInitializer] SlotRegistry가 주입되지 않았습니다.");
                return;
            }

            Debug.Log("[SlotInitializer] === 슬롯 자동 바인딩 시작 ===");

            var monoBehaviours = Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            var handSlots = monoBehaviours.OfType<IHandCardSlot>().ToArray();
            var combatSlots = monoBehaviours.OfType<ICombatCardSlot>().ToArray();
            var characterSlots = monoBehaviours.OfType<ICharacterSlot>().ToArray();

            Debug.Log($"[SlotInitializer] 탐색된 슬롯 수 - 핸드: {handSlots.Length}, 전투: {combatSlots.Length}, 캐릭터: {characterSlots.Length}");

            _slotRegistry.GetHandSlotRegistry().RegisterHandSlots(handSlots);
            _slotRegistry.GetCombatSlotRegistry().RegisterCombatSlots(combatSlots);
            _slotRegistry.GetCharacterSlotRegistry().RegisterCharacterSlots(characterSlots);

            Debug.Log("[SlotInitializer] === 슬롯 자동 바인딩 완료 ===");
        }
    }
}
