using UnityEngine;
using Game.Interface;
using Game.Slots;

namespace Game.UI
{
    /// <summary>
    /// 전투 화면에서 캐릭터 정보를 표시하는 UI 슬롯입니다.
    /// </summary>
    public class CharacterCardUI : MonoBehaviour, ICharacterSlot
    {
        [SerializeField] private CharacterSlotPosition position;
        [SerializeField] private SlotOwner owner;

        private ICharacter currentCharacter;

        public CharacterSlotPosition GetSlotPosition() => position;

        public SlotOwner GetOwner() => owner;

        public void SetCharacter(ICharacter character)
        {
            currentCharacter = character;
        }

        public ICharacter GetCharacter()
        {
            return currentCharacter;
        }

        public void Clear()
        {
            currentCharacter = null;
        }
    }
}
