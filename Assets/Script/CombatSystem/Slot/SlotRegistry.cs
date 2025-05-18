using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.IManager;

namespace Game.CombatSystem.Slot
{
    public class SlotRegistry : MonoBehaviour, ISlotRegistry
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
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize()
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        // Hand Slots
        public void RegisterHandSlots(IHandCardSlot[] slots)
        {
            handSlots.Clear();
            foreach (var slot in slots)
            {
                var key = slot.GetSlotPosition();
                if (!handSlots.ContainsKey(key))
                    handSlots.Add(key, slot);
            }
        }

        public IHandCardSlot GetHandSlot(SkillCardSlotPosition position)
            => handSlots.TryGetValue(position, out var slot) ? slot : null;

        public IEnumerable<IHandCardSlot> GetAllHandSlots() => handSlots.Values;

        public IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner)
            => handSlots.Values.Where(slot => slot.GetOwner() == owner);

        // Combat Slots
        public void RegisterCombatSlots(ICombatCardSlot[] slots)
        {
            combatSlots.Clear();
            foreach (var slot in slots)
            {
                var key = slot.GetCombatPosition();
                if (!combatSlots.ContainsKey(key))
                    combatSlots.Add(key, slot);
            }
        }

        public ICombatCardSlot GetCombatSlot(CombatSlotPosition position)
            => combatSlots.TryGetValue(position, out var slot) ? slot : null;

        public IEnumerable<ICombatCardSlot> GetAllCombatSlots() => combatSlots.Values;

        public IEnumerable<ICombatCardSlot> GetCombatSlots() => combatSlots.Values;

        // Character Slots
        public void RegisterCharacterSlots(ICharacterSlot[] slots)
        {
            characterSlots.Clear();
            foreach (var slot in slots)
            {
                var owner = slot.GetOwner();
                if (!characterSlots.ContainsKey(owner))
                    characterSlots.Add(owner, slot);
            }
        }

        public ICharacterSlot GetCharacterSlot(SlotOwner owner)
            => characterSlots.TryGetValue(owner, out var slot) ? slot : null;

        public IEnumerable<ICharacterSlot> GetCharacterSlots() => characterSlots.Values;
    }
}
