using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Units;

namespace Game.UI
{
    /// <summary>
    /// 캐릭터 카드 데이터를 UI로 표시합니다 (이름, 이미지, 체력 등)
    /// </summary>
    public class CharacterCardUI : MonoBehaviour
    {
        public TMP_Text nameText;
        public Image portraitImage;
        public TMP_Text hpText;

        private CharacterCardData data;

        public void Initialize(CharacterCardData characterData, int currentHP)
        {
            data = characterData;

            nameText.text = data.characterName;
            portraitImage.sprite = data.portrait;
            hpText.text = $"HP: {currentHP} / {data.maxHP}";
        }

        public void UpdateHP(int currentHP)
        {
            hpText.text = $"HP: {currentHP} / {data.maxHP}";
        }
    }
}