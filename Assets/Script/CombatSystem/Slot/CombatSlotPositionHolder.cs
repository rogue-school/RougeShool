using UnityEngine;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Slot
{
    public class CombatSlotPositionHolder : MonoBehaviour
    {
        [field: SerializeField] public CombatSlotPosition SlotPosition { get; private set; }
        [field: SerializeField] public CombatFieldSlotPosition FieldSlotPosition { get; private set; }
    }
}
