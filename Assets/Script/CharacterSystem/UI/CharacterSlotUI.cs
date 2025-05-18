using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Core;

namespace Game.CharacterSystem.UI
{
    public class CharacterSlotUI : MonoBehaviour, ICharacterSlot
    {
        [SerializeField] private CharacterSlotPosition slotPosition;
        [SerializeField] private SlotOwner owner;

        private ICharacter character;

        public void SetCharacter(ICharacter character)
        {
            this.character = character;

            if (character is CharacterBase baseChar)
            {
                var uiController = GetComponentInChildren<CharacterUIController>();
                if (uiController != null)
                    baseChar.SetCardUI(uiController);
            }

            if (character is MonoBehaviour mb)
                mb.transform.position = transform.position;
        }

        public ICharacter GetCharacter() => character;
        public void Clear() => character = null;
        public CharacterSlotPosition GetSlotPosition() => slotPosition;
        public SlotOwner GetOwner() => owner;
        public Transform GetTransform() => transform;
    }
}
