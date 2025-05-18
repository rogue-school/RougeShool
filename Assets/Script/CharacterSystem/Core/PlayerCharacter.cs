using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Runtime;

namespace Game.CharacterSystem.Core
{
    public class PlayerCharacter : CharacterBase, IPlayerCharacter
    {
        [SerializeField] private PlayerCharacterData characterData;

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Slider hpSlider;

        private bool isGuarded = false;
        private PlayerSkillCardRuntime lastUsedCard;

        public PlayerCharacterData Data => characterData;

        private void Awake()
        {
            if (characterData != null)
            {
                SetMaxHP(characterData.maxHP);
                ApplyDataToUI();
                Debug.Log($"[PlayerCharacter] {characterData.displayName} 초기화 완료");
            }
        }

        public void SetCharacterData(PlayerCharacterData data)
        {
            characterData = data;
            SetMaxHP(data.maxHP);
            ApplyDataToUI();
        }

        private void ApplyDataToUI()
        {
            if (nameText != null)
                nameText.text = characterData.displayName;

            if (portraitImage != null)
                portraitImage.sprite = characterData.portrait;

            if (hpText != null)
                hpText.text = $"HP {GetCurrentHP()} / {GetMaxHP()}";

            if (hpSlider != null)
                hpSlider.value = (float)GetCurrentHP() / GetMaxHP();
        }

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            ApplyDataToUI();
        }

        public override void Heal(int amount)
        {
            base.Heal(amount);
            ApplyDataToUI();
        }

        public override void Die()
        {
            base.Die();
            ApplyDataToUI();
            Debug.Log("[PlayerCharacter] 사망 → 게임 오버 처리 필요");
        }

        public void SetGuarded(bool value)
        {
            isGuarded = value;
            Debug.Log($"[PlayerCharacter] 방어 상태 설정됨: {isGuarded}");
        }

        public bool IsGuarded() => isGuarded;

        public void SetLastUsedCard(PlayerSkillCardRuntime card)
        {
            lastUsedCard = card;
        }

        public PlayerSkillCardRuntime GetLastUsedCard() => lastUsedCard;

        public void RestoreCardToHand(PlayerSkillCardRuntime card)
        {
            lastUsedCard = null;
            Debug.Log("[PlayerCharacter] 핸드에 카드 복귀 처리 완료");
        }
    }
}
