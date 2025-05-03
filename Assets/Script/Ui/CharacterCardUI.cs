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

        /// <summary>
        /// 플레이어 캐릭터 UI 초기화
        /// </summary>
        public void Initialize(PlayerCharacterData data, CharacterBase character)
        {
            nameText.text = data.characterName;
            portraitImage.sprite = data.portrait;
            targetCharacter = character;

            UpdateHP();
        }

        /// <summary>
        /// 적 캐릭터 UI 초기화
        /// </summary>
        public void Initialize(EnemyCharacterData data, CharacterBase character)
        {
            nameText.text = data.characterName;
            portraitImage.sprite = data.portrait;
            targetCharacter = character;

            UpdateHP();
        }

        /// <summary>
        /// 외부에서 체력 UI를 수동 갱신할 수 있도록 제공
        /// </summary>
        public void UpdateHP()
        {
            if (targetCharacter != null)
            {
                hpText.text = $"HP: {targetCharacter.GetCurrentHP()} / {targetCharacter.GetMaxHP()}";
            }
        }

        /// <summary>
        /// 현재 상태이상 아이콘을 전부 제거합니다.
        /// </summary>
        public void ClearStatusEffects()
        {
            foreach (Transform child in statusEffectContainer)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// 새로운 상태이상 아이콘을 추가합니다.
        /// </summary>
        /// <param name="icon">표시할 스프라이트</param>
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
    }
}

