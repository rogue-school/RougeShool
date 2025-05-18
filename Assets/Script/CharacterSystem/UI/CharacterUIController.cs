using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;

namespace Game.CharacterSystem.UI
{
    public class CharacterUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image portraitImage;

        public void Initialize(ICharacter character)
        {
            if (character == null) return;

            nameText.text = character.GetName();

            if (character is CharacterBase baseChar)
                SetHP(baseChar.GetCurrentHP(), baseChar.GetMaxHP());
        }

        public void SetHP(int current, int max)
        {
            hpText.text = $"{current} / {max}";
            // 슬라이더 사용 안 하므로 관련 코드 제거
        }

        public void SetPortrait(Sprite sprite)
        {
            if (portraitImage != null)
                portraitImage.sprite = sprite;
        }

        public void Clear()
        {
            nameText.text = "";
            hpText.text = "";
        }
    }
}
