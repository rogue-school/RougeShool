using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Characters;
using Game.Data;
using Game.Effect;
using Game.Interface;
using Game.Managers;
using Game.Slots;

namespace Game.Enemy
{
    public class EnemyCharacter : CharacterBase
    {
        [SerializeField] private EnemyCharacterData characterData;

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image portraitImage;

        public EnemyCharacterData Data => characterData;

        public void Initialize(EnemyCharacterData data)
        {
            SetCharacterData(data);
        }

        public void SetCharacterData(EnemyCharacterData data)
        {
            characterData = data;

            if (characterData != null)
            {
                SetMaxHP(characterData.maxHP);
                UpdateUI();
                ApplyPassiveEffects();
                Debug.Log($"[EnemyCharacter] {characterData.displayName} 초기화 완료");
            }
            else
            {
                Debug.LogWarning("[EnemyCharacter] characterData가 null입니다!");
            }
        }

        private void UpdateUI()
        {
            nameText.text = characterData.displayName;
            hpText.text = $"HP {GetCurrentHP()} / {GetMaxHP()}";
            if (portraitImage != null && characterData.portrait != null)
            {
                portraitImage.sprite = characterData.portrait;
            }
        }

        private void ApplyPassiveEffects()
        {
            var effects = characterData?.GetPassiveEffects();
            if (effects == null) return;

            foreach (var obj in effects)
            {
                if (obj is ICardEffect effect)
                {
                    effect.ExecuteEffect(this, this, 0);
                }
            }
        }

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            UpdateUI();
        }

        public override void Heal(int amount)
        {
            base.Heal(amount);
            UpdateUI();
        }

        public override void Die()
        {
            Debug.Log($"[EnemyCharacter] 사망 → 다음 적 소환은 CombatTurnManager가 담당함");

            // 핸드 슬롯 정리
            EnemyHandManager.Instance.ClearAllSlots();

            // 캐릭터 슬롯 정리
            foreach (var slot in SlotRegistry.Instance.GetCharacterSlots(SlotOwner.ENEMY))
                slot.Clear();

            // 전투 슬롯 정리 (자신이 올렸던 카드 포함)
            foreach (var slot in SlotRegistry.Instance.GetAllCombatSlots())
            {
                if (slot.GetOwner() == SlotOwner.ENEMY)
                    slot.Clear();
            }
        }
    }
}
