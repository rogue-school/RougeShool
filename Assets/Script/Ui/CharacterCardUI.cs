using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Cards;
using Game.Characters;

namespace Game.UI
{
    public class CharacterCardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text hpText;

        public void Initialize(PlayerCharacterData data, CharacterBase character)
        {
            nameText.text = data.characterName;
            portraitImage.sprite = data.portrait;
            hpText.text = $"HP: {character.GetCurrentHP()} / {character.GetMaxHP()}";
        }

        public void Initialize(EnemyCharacterData data, CharacterBase character)
        {
            nameText.text = data.characterName;
            portraitImage.sprite = data.portrait;
            hpText.text = $"HP: {character.GetCurrentHP()} / {character.GetMaxHP()}";
        }
    }
}
