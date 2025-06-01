using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Core;

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

            // 캐릭터 UI 컨트롤러 연결
            if (character is CharacterBase baseChar)
            {
                var uiController = GetComponentInChildren<CharacterUIController>();
                if (uiController != null)
                    baseChar.SetCardUI(uiController);
            }

            // 슬롯에 캐릭터 오브젝트 배치
            if (character is MonoBehaviour mb)
            {
                var trans = mb.transform;
                trans.SetParent(transform, false); // keepLocal = false

                if (trans is RectTransform rt)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.localRotation = Quaternion.identity;
                    rt.localScale = Vector3.one;
                }
                else
                {
                    trans.localPosition = Vector3.zero;
                    trans.localRotation = Quaternion.identity;
                    trans.localScale = Vector3.one;
                }
            }
        }

        public ICharacter GetCharacter() => character;

        public void Clear()
        {
            character = null;
        }

        public CharacterSlotPosition GetSlotPosition() => slotPosition;

        public SlotOwner GetOwner() => owner;

        public Transform GetTransform() => transform;
    }
}
