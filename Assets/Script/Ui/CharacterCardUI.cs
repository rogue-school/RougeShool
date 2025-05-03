using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Cards;
using Game.Units;

namespace Game.UI
{
    /// <summary>
    /// 플레이어 및 적 캐릭터 카드의 UI를 초기화하고 체력을 표시합니다.
    /// </summary>
    public class CharacterCardUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private TMP_Text nameText;       // 캐릭터 이름
        [SerializeField] private Image portraitImage;     // 캐릭터 초상화
        [SerializeField] private TMP_Text hpText;         // HP 표시

        /// <summary>
        /// 플레이어 캐릭터 UI 초기화
        /// </summary>
        public void Initialize(PlayerCharacterData data, Unit unit)
        {
            nameText.text = data.characterName;
            portraitImage.sprite = data.portrait;
            hpText.text = $"HP: {unit.GetCurrentHP()} / {unit.GetMaxHP()}";
        }

        /// <summary>
        /// 적 캐릭터 UI 초기화
        /// </summary>
        public void Initialize(EnemyCharacterData data, Unit unit)
        {
            nameText.text = data.characterName;
            portraitImage.sprite = data.portrait;
            hpText.text = $"HP: {unit.GetCurrentHP()} / {unit.GetMaxHP()}";
        }
    }
}
