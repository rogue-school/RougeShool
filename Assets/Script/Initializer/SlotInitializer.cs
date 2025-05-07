using System.Linq;
using UnityEngine;
using Game.Interface;
using Game.Managers;

namespace Game.Initialization
{
    /// <summary>
    /// 씬에 존재하는 모든 슬롯을 자동 탐색하고 SlotRegistry에 등록합니다.
    /// </summary>
    public class SlotInitializer : MonoBehaviour
    {
        private void Awake()
        {
            AutoBindAllSlots();
        }

        public void AutoBindAllSlots()
        {
            var handSlots = FindObjectsOfType<MonoBehaviour>(true).OfType<IHandCardSlot>().ToArray();
            SlotRegistry.Instance.RegisterHandSlots(handSlots);

            var combatSlots = FindObjectsOfType<MonoBehaviour>(true).OfType<ICombatCardSlot>().ToArray();
            SlotRegistry.Instance.RegisterCombatSlots(combatSlots);

            var characterSlots = FindObjectsOfType<MonoBehaviour>(true).OfType<ICharacterSlot>().ToArray();
            SlotRegistry.Instance.RegisterCharacterSlots(characterSlots);

            Debug.Log($"[SlotInitializer] 슬롯 자동 등록 완료: 핸드({handSlots.Length}), 전투({combatSlots.Length}), 캐릭터({characterSlots.Length})");
        }
    }
}
