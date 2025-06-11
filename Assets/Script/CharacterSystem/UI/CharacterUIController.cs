using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 캐릭터 이름, 체력, 초상화 등 UI 요소를 관리하는 클래스입니다.
    /// ICharacter 인터페이스와 연동되어 시각적 정보를 표시합니다.
    /// </summary>
    public class CharacterUIController : MonoBehaviour
    {
        [Header("UI 참조")]
        [Tooltip("캐릭터 이름 표시용 텍스트")]
        [SerializeField] private TextMeshProUGUI nameText;

        [Tooltip("체력 표시용 텍스트")]
        [SerializeField] private TextMeshProUGUI hpText;

        [Tooltip("초상화 이미지")]
        [SerializeField] private Image portraitImage;

        private ICharacter character;

        #region 초기화

        /// <summary>
        /// 지정된 캐릭터의 정보를 기반으로 UI를 초기화합니다.
        /// </summary>
        /// <param name="character">UI에 연결할 캐릭터</param>
        public void Initialize(ICharacter character)
        {
            this.character = character;

            if (character == null)
            {
                Debug.LogWarning("[CharacterUIController] Initialize() - character가 null입니다.");
                return;
            }

            SetName(character.GetCharacterName());

            // 추가 정보는 CharacterBase를 통해 접근
            if (character is CharacterBase baseChar)
            {
                SetHP(baseChar.GetCurrentHP(), baseChar.GetMaxHP());
                SetPortrait(baseChar.GetPortrait());
            }
        }

        #endregion

        #region UI 설정

        /// <summary>
        /// 이름 텍스트를 설정합니다.
        /// </summary>
        /// <param name="name">캐릭터 이름</param>
        private void SetName(string name)
        {
            if (nameText != null)
                nameText.text = name ?? "";
        }

        /// <summary>
        /// 체력 정보를 UI에 갱신합니다.
        /// </summary>
        /// <param name="current">현재 체력</param>
        /// <param name="max">최대 체력</param>
        public void SetHP(int current, int max)
        {
            if (hpText == null) return;

            // 현재 체력만 표시
            hpText.text = current.ToString();

            // 색상: 최대 체력이면 흰색, 아니면 붉은색
            hpText.color = (current < max) ? Color.red : Color.white;
        }

        /// <summary>
        /// 초상화 이미지를 설정합니다.
        /// </summary>
        /// <param name="sprite">설정할 스프라이트</param>
        public void SetPortrait(Sprite sprite)
        {
            if (portraitImage != null && sprite != null)
                portraitImage.sprite = sprite;
        }

        #endregion

        #region 초기화/제거

        /// <summary>
        /// 모든 UI 요소를 초기 상태로 초기화합니다.
        /// </summary>
        public void Clear()
        {
            if (nameText != null) nameText.text = "";
            if (hpText != null) hpText.text = "";
            if (portraitImage != null) portraitImage.sprite = null;
        }

        #endregion
    }
}
