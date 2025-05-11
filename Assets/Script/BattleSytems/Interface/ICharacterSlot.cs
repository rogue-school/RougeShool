using Game.Slots;
using UnityEngine;

namespace Game.Interface
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
