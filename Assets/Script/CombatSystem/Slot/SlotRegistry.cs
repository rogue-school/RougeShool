using UnityEngine;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Slot
{
    public class SlotRegistry : MonoBehaviour
    {
        public static SlotRegistry Instance { get; private set; }

        public IHandSlotRegistry HandSlots { get; private set; }
        public ICombatSlotRegistry CombatSlots { get; private set; }
        public ICharacterSlotRegistry CharacterSlots { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            HandSlots = new HandSlotRegistry();
            CombatSlots = new CombatSlotRegistry();
            CharacterSlots = new CharacterSlotRegistry();
        }
    }
}
