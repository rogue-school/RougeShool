using System.Linq;
using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 씬에 존재하는 모든 슬롯을 자동 탐색하고 SlotRegistry에 등록합니다.
    /// (실행은 외부에서 명시적으로 호출해야 합니다)
    /// </summary>
    public class SlotInitializer : MonoBehaviour
    {
        /// <summary>
        /// 씬 내 존재하는 모든 슬롯을 자동으로 탐색하고 등록합니다.
        /// 핸드 슬롯, 전투 슬롯, 캐릭터 슬롯을 포함합니다.
        /// </summary>
        public void AutoBindAllSlots()
        {
            var monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            var handSlots = monoBehaviours.OfType<IHandCardSlot>().ToArray();
            var combatSlots = monoBehaviours.OfType<ICombatCardSlot>().ToArray();
            var characterSlots = monoBehaviours.OfType<ICharacterSlot>().ToArray();

            SlotRegistry.Instance.RegisterHandSlots(handSlots);
            SlotRegistry.Instance.RegisterCombatSlots(combatSlots);
            SlotRegistry.Instance.RegisterCharacterSlots(characterSlots);

            Debug.Log($"[SlotInitializer] 슬롯 자동 등록 완료: 핸드({handSlots.Length}), 전투({combatSlots.Length}), 캐릭터({characterSlots.Length})");
        }
    }
}
