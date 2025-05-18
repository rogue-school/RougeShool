using System.Linq;
using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 씬에 존재하는 모든 슬롯을 자동 탐색하고 SlotRegistry에 등록합니다.
    /// (실행은 외부에서 명시적으로 호출해야 합니다)
    /// </summary>
    public class SlotInitializer : MonoBehaviour, ISlotInitializer
    {
        public void AutoBindAllSlots()
        {
            //Debug.Log("[SlotInitializer] === 슬롯 자동 바인딩 시작 ===");

            var monoBehaviours = Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            //Debug.Log($"[SlotInitializer] 탐색된 MonoBehaviour 수: {monoBehaviours.Length}");

            var handSlots = monoBehaviours.OfType<IHandCardSlot>().ToArray();
            var combatSlots = monoBehaviours.OfType<ICombatCardSlot>().ToArray();
            var characterSlots = monoBehaviours.OfType<ICharacterSlot>().ToArray();

            //Debug.Log($"[SlotInitializer] 탐색된 슬롯 수 - 핸드: {handSlots.Length}, 전투: {combatSlots.Length}, 캐릭터: {characterSlots.Length}");

            var registry = SlotRegistry.Instance;
            if (registry == null)
            {
                //Debug.LogError("[SlotInitializer] SlotRegistry.Instance가 null입니다. 슬롯 등록 중단");
                return;
            }

            registry.RegisterHandSlots(handSlots);
            //Debug.Log("[SlotInitializer] 핸드 슬롯 등록 완료");

            registry.RegisterCombatSlots(combatSlots);
            //Debug.Log("[SlotInitializer] 전투 슬롯 등록 완료");

            registry.RegisterCharacterSlots(characterSlots);
            //Debug.Log("[SlotInitializer] 캐릭터 슬롯 등록 완료");

            foreach (var slot in characterSlots)
            {
                if (slot is MonoBehaviour mb)
                {
                    //Debug.Log($" → 등록된 CharacterSlot: {mb.gameObject.name}, Owner: {slot.GetOwner()}, Position: {slot.GetSlotPosition()}");
                }
            }

            //Debug.Log("[SlotInitializer] === 슬롯 자동 바인딩 완료 ===");
        }
    }
}
