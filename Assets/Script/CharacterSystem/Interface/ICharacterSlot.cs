using UnityEngine;
using Game.CharacterSystem.Slot;
using Game.CombatSystem.Slot;

namespace Game.CharacterSystem.Interface
{
    public interface ICharacterSlot
    {
        void SetCharacter(ICharacter character);
        void Clear();
        ICharacter GetCharacter();

        CharacterSlotPosition GetSlotPosition();
        SlotOwner GetOwner();
        Transform GetTransform();
    }
}
