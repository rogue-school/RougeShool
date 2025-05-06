using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Characters;

namespace Game.UI
{
    public enum CharacterUIType
    {
        Player,
        Enemy
    }

    /// <summary>
    /// 전투에 배치된 캐릭터(플레이어 또는 적)의 상태를 UI로 보여주는 컴포넌트입니다.
    /// </summary>
    public class CharacterCardUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;

        [Header("Character Role")]
        [SerializeField] private CharacterUIType uiType;

        private CharacterBase linkedCharacter;

        /// <summary>
        /// 캐릭터 데이터를 기반으로 UI 초기화
        /// </summary>
        public void Initialize(CharacterBase character)
        {
            linkedCharacter = character;

            // 캐릭터 타입 자동 구분
            if (character is PlayerCharacter)
            {
                uiType = CharacterUIType.Player;
            }
            else if (character is EnemyCharacter)
            {
                uiType = CharacterUIType.Enemy;
            }

            // 공통 정보 표시
            portraitImage.sprite = character.portrait;
            nameText.text = character.characterName;
            hpText.text = $"{character.CurrentHP} / {character.MaxHP}";
        }

        /// <summary>
        /// 체력 값 등 UI 갱신
        /// </summary>
        public void UpdateUI()
        {
            if (linkedCharacter == null) return;

            hpText.text = $"{linkedCharacter.CurrentHP} / {linkedCharacter.MaxHP}";

            // 스타일 차별화
            if (uiType == CharacterUIType.Enemy)
            {
                nameText.color = Color.red;
                hpText.color = Color.red;
            }
            else
            {
                nameText.color = Color.white;
                hpText.color = Color.white;
            }
        }
    }
}