using System.Collections.Generic;
using UnityEngine;
using Game.Interface;
using Game.Slots;

namespace Game.Managers
{
    /// <summary>
    /// 모든 슬롯을 중앙에서 관리하는 싱글톤 클래스입니다.
    /// 핸드 슬롯, 전투 슬롯, 캐릭터 슬롯을 분리하여 저장하고 조회합니다.
    /// </summary>
    public class SlotRegistry : MonoBehaviour
    {
        public static SlotRegistry Instance { get; private set; }

        private Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots = new();
        private Dictionary<CombatSlotPosition, ICombatCardSlot> combatSlots = new();
        private Dictionary<SlotOwner, ICharacterSlot> characterSlots = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #region Hand Slots

        public void RegisterHandSlots(IHandCardSlot[] slots)
        {
            handSlots.Clear();
            foreach (var slot in slots)
            {
                if (!handSlots.ContainsKey(slot.GetSlotPosition()))
                    handSlots.Add(slot.GetSlotPosition(), slot);
            }
            Debug.Log($"[SlotRegistry] Hand 슬롯 {handSlots.Count}개 등록됨");
        }

        public IHandCardSlot GetHandSlot(SkillCardSlotPosition position)
        {
            return handSlots.TryGetValue(position, out var slot) ? slot : null;
        }

        public IEnumerable<IHandCardSlot> GetAllHandSlots() => handSlots.Values;

        #endregion

        #region Combat Slots

        public void RegisterCombatSlots(ICombatCardSlot[] slots)
        {
            combatSlots.Clear();
            foreach (var slot in slots)
            {
                if (!combatSlots.ContainsKey(slot.GetCombatPosition()))
                    combatSlots.Add(slot.GetCombatPosition(), slot);
            }
            Debug.Log($"[SlotRegistry] Combat 슬롯 {combatSlots.Count}개 등록됨");
        }

        public ICombatCardSlot GetCombatSlot(CombatSlotPosition position)
        {
            return combatSlots.TryGetValue(position, out var slot) ? slot : null;
        }

        public IEnumerable<ICombatCardSlot> GetAllCombatSlots() => combatSlots.Values;

        #endregion

        #region Character Slots

        public void RegisterCharacterSlots(ICharacterSlot[] slots)
        {
            characterSlots.Clear();
            foreach (var slot in slots)
            {
                if (!characterSlots.ContainsKey(slot.GetOwner()))
                    characterSlots.Add(slot.GetOwner(), slot);
            }
            Debug.Log($"[SlotRegistry] Character 슬롯 {characterSlots.Count}개 등록됨");
        }

        public ICharacterSlot GetCharacterSlot(SlotOwner owner)
        {
            return characterSlots.TryGetValue(owner, out var slot) ? slot : null;
        }

        public IEnumerable<ICharacterSlot> GetAllCharacterSlots() => characterSlots.Values;

        #endregion
    }
}
