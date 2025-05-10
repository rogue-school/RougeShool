using UnityEngine;
using Game.Interface;
using Game.Slots;

namespace Game.UI
{
    /// <summary>
    /// 캐릭터(플레이어/적)가 배치되는 슬롯 UI
    /// </summary>
    public class CharacterSlotUI : MonoBehaviour, ICharacterSlot
    {
        [SerializeField] private CharacterSlotPosition slotPosition;
        [SerializeField] private SlotOwner owner;

        private ICharacter character;

        public void SetCharacter(ICharacter character)
        {
            this.character = character;

            // 위치도 자동으로 설정
            if (character is MonoBehaviour mb)
                mb.transform.position = transform.position;
        }

        public ICharacter GetCharacter() => character;
        public CharacterSlotPosition GetSlotPosition() => slotPosition;
        public SlotOwner GetOwner() => owner;
        public Transform GetTransform() => this.transform;

        public void Clear()
        {
            character = null;
        }
    }
}
