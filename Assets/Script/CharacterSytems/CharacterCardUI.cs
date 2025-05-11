using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

        [Header("UI 연결")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Slider hpSlider;

        private ICharacter currentCharacter;

        public CharacterSlotPosition GetSlotPosition() => position;

        public SlotOwner GetOwner() => owner;

        public void SetCharacter(ICharacter character)
        {
            currentCharacter = character;

            if (character != null)
            {
                nameText.text = character.GetName();
                SetHP(character.GetCurrentHP(), character.GetMaxHP());
            }
        }

        public ICharacter GetCharacter()
        {
            return currentCharacter;
        }
        public Transform GetTransform() => this.transform;

        public void Clear()
        {
            currentCharacter = null;
            nameText.text = "";
            hpText.text = "";
            hpSlider.value = 0;
        }

        /// <summary>
        /// 현재 HP 상태를 UI에 갱신합니다.
        /// </summary>
        public void SetHP(int current, int max)
        {
            if (hpText != null)
                hpText.text = $"{current} / {max}";

            if (hpSlider != null)
                hpSlider.value = (max > 0) ? (float)current / max : 0;
        }
    }
}
