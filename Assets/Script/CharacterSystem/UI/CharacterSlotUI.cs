using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.UI;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 캐릭터 슬롯에 캐릭터를 배치하고 UI를 초기화하는 클래스입니다.
    /// </summary>
    public class CharacterSlotUI : MonoBehaviour, ICharacterSlot
    {
        [SerializeField] private CharacterSlotPosition slotPosition;
        [SerializeField] private SlotOwner owner;

        private ICharacter character;

        public void SetCharacter(ICharacter character)
        {
            this.character = character;

            if (character == null)
            {
                Debug.LogWarning($"[CharacterSlotUI] SetCharacter()에 null이 전달되었습니다. 슬롯 위치: {slotPosition}");
                return;
            }

            if (character is CharacterBase baseChar)
            {
                var uiController = GetComponentInChildren<CharacterUIController>();
                if (uiController != null)
                    baseChar.SetCardUI(uiController);
            }

            // 캐릭터 오브젝트를 슬롯 위치로 이동
            if (character is MonoBehaviour mb)
            {
                mb.transform.SetParent(transform, false);
                mb.transform.localPosition = Vector3.zero;
                mb.transform.localRotation = Quaternion.identity;
                mb.transform.localScale = Vector3.one;
            }
        }

        public ICharacter GetCharacter() => character;
        public void Clear() => character = null;
        public CharacterSlotPosition GetSlotPosition() => slotPosition;
        public SlotOwner GetOwner() => owner;
        public Transform GetTransform() => transform;
    }
}
