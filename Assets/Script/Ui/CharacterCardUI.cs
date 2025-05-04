using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Cards;
using Game.Characters;
using System.Collections.Generic;

namespace Game.UI
{
    /// <summary>
    /// 캐릭터 카드 UI (이름, 초상화, 체력 및 상태이상) 출력 전용
    /// </summary>
    public class CharacterCardUI : MonoBehaviour
    {
        [Header("기본 정보 UI")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text hpText;

        [Header("상태이상 아이콘 표시 영역")]
        [SerializeField] private Transform statusEffectContainer;
        [SerializeField] private GameObject statusEffectIconPrefab;

        private CharacterBase targetCharacter;

        public void Initialize(PlayerCharacterData data, CharacterBase character)
        {
            nameText.text = data.characterName;
            portraitImage.sprite = data.portrait;
            targetCharacter = character;
            UpdateHP();
        }

        public void Initialize(EnemyCharacterData data, CharacterBase character)
        {
            nameText.text = data.characterName;
            portraitImage.sprite = data.portrait;
            targetCharacter = character;
            UpdateHP();
        }

        public void UpdateHP()
        {
            if (targetCharacter != null)
            {
                hpText.text = $"HP: {targetCharacter.GetCurrentHP()} / {targetCharacter.GetMaxHP()}";
            }
        }

        public void ClearStatusEffects()
        {
            foreach (Transform child in statusEffectContainer)
            {
                Destroy(child.gameObject);
            }
        }

        public void AddStatusEffectIcon(Sprite icon)
        {
            GameObject iconObj = Instantiate(statusEffectIconPrefab, statusEffectContainer);
            Image image = iconObj.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = icon;
                image.enabled = true;
            }
        }

        /// <summary>
        /// UI 전체 갱신용 메서드입니다. 외부 파사드에서 통합 호출용
        /// </summary>
        public void UpdateUI()
        {
            UpdateHP();
            Debug.Log("[CharacterCardUI] 전체 UI 업데이트 완료");
        }
    }
}
