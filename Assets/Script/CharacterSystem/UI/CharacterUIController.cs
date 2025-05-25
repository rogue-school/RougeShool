using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 캐릭터 이름, 체력, 초상화 등 UI 요소를 관리하는 클래스입니다.
    /// </summary>
    public class CharacterUIController : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image portraitImage;

        private ICharacter character;

        /// <summary>
        /// UI에 캐릭터 정보를 설정합니다.
        /// </summary>
        public void Initialize(ICharacter character)
        {
            this.character = character;

            if (character == null)
            {
                Debug.LogWarning("[CharacterUIController] character == null");
                return;
            }

            nameText.text = character.GetCharacterName();

            if (character is CharacterBase baseChar)
            {
                SetHP(baseChar.GetCurrentHP(), baseChar.GetMaxHP());
                SetPortrait(baseChar.GetPortrait());
            }
        }

        /// <summary>
        /// 체력 정보를 UI에 갱신합니다.
        /// </summary>
        public void SetHP(int current, int max)
        {
            if (hpText == null) return;
            hpText.text = $"{current} / {max}";
        }

        /// <summary>
        /// 캐릭터의 초상화 스프라이트를 설정합니다.
        /// </summary>
        public void SetPortrait(Sprite sprite)
        {
            if (portraitImage != null && sprite != null)
                portraitImage.sprite = sprite;
        }

        /// <summary>
        /// 모든 UI 정보를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            if (nameText != null) nameText.text = "";
            if (hpText != null) hpText.text = "";
            if (portraitImage != null) portraitImage.sprite = null;
        }
    }
}
