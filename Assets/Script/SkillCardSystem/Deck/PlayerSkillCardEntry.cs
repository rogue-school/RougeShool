using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Slot;
using UnityEngine;

namespace Game.SkillCardSystem.Deck
{
    [System.Serializable]
    public class PlayerSkillCardEntry
    {
        [field: SerializeField] public SkillCardSlotPosition Slot { get; private set; }
        [field: SerializeField] public PlayerSkillCard Card { get; private set; }
    }
}
