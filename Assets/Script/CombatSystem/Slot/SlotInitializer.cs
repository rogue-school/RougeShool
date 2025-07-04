using System.Linq;
using UnityEngine;
using Game.CombatSystem.Interface;
using Zenject;
using Game.CharacterSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 씬 내 모든 슬롯 컴포넌트를 자동으로 탐색하고 슬롯 레지스트리에 바인딩하는 초기화 클래스입니다.
    /// </summary>
    public class SlotInitializer : MonoBehaviour, ISlotInitializer
    {
        #region 의존성 주입

        [Inject] private ISlotRegistry _slotRegistry;

        #endregion

        #region 슬롯 자동 바인딩

        /// <summary>
        /// 씬 내 모든 핸드 슬롯, 전투 슬롯, 캐릭터 슬롯을 자동 탐색하여 슬롯 레지스트리에 등록합니다.
        /// </summary>
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

        #endregion

        #region 슬롯 탐색 유틸리티

        /// <summary>
        /// 현재 씬에서 비활성 오브젝트 포함하여 주어진 타입의 컴포넌트를 모두 탐색합니다.
        /// </summary>
        private T[] FindAll<T>() where T : class
        {
            return Object.FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .OfType<T>()
                .ToArray();
        }

        #endregion
    }
}
