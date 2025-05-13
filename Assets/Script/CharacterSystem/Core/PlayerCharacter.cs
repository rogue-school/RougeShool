using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.CharacterSystem.Data;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 플레이어 캐릭터. 캐릭터 데이터 기반으로 초기화되며, CharacterBase를 상속받습니다.
    /// </summary>
    public class PlayerCharacter : CharacterBase
    {
        [SerializeField] private PlayerCharacterData characterData;

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Slider hpSlider;

        public PlayerCharacterData Data => characterData;

        /// <summary>
        /// 외부에서 직접 초기화 시 사용
        /// </summary>
        public void Initialize(int maxHP)
        {
            SetMaxHP(maxHP);
            ApplyDataToUI();
        }

        private void Awake()
        {
            if (characterData != null)
            {
                SetMaxHP(characterData.maxHP);
                ApplyDataToUI();
                Debug.Log($"[PlayerCharacter] {characterData.displayName} 초기화 완료");
            }
        }

        /// <summary>
        /// 외부에서 ScriptableObject 데이터 주입 시 사용
        /// </summary>
        public void SetCharacterData(PlayerCharacterData data)
        {
            characterData = data;
            SetMaxHP(data.maxHP);
            ApplyDataToUI();
        }

        /// <summary>
        /// ScriptableObject 기반 UI 요소 반영
        /// </summary>
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
    }
}
