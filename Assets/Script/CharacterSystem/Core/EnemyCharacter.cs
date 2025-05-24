using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Context;

namespace Game.CharacterSystem.Core
{
    public class EnemyCharacter : CharacterBase, IEnemyCharacter
    {
        [SerializeField] private EnemyCharacterData characterData;

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image portraitImage;

        private List<EnemyCharacterData.SkillCardEntry> skillCardEntries = new();

        public EnemyCharacterData Data => characterData;

        public void Initialize(EnemyCharacterData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[EnemyCharacter] 초기화 실패 - null 데이터");
                return;
            }

            characterData = data;
            SetMaxHP(data.MaxHP);
            skillCardEntries = new List<EnemyCharacterData.SkillCardEntry>(data.GetAllCards());

            RefreshUI();
            ApplyPassiveEffects();

            Debug.Log($"[EnemyCharacter] '{characterData.DisplayName}' 초기화 완료");
        }

        private void RefreshUI()
        {
            if (characterData == null) return;

            nameText.text = GetCharacterName();
            hpText.text = $"HP {GetCurrentHP()} / {GetMaxHP()}";
            portraitImage.sprite = characterData.Portrait;
        }

        private void ApplyPassiveEffects()
        {
            var effects = characterData?.GetPassiveEffects();
            if (effects == null) return;

            foreach (var effect in effects)
            {
                if (effect is ICardEffect cardEffect)
                    cardEffect.ApplyEffect(new DefaultCardExecutionContext(null, this, this), 0);
            }
        }

        public EnemyCharacterData.SkillCardEntry GetRandomCardEntry()
        {
            if (skillCardEntries == null || skillCardEntries.Count == 0)
                return null;

            int index = Random.Range(0, skillCardEntries.Count);
            return skillCardEntries[index];
        }

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            RefreshUI();
        }

        public override void Heal(int amount)
        {
            base.Heal(amount);
            RefreshUI();
        }

        public override void Die()
        {
            base.Die();
            RefreshUI();
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 사망 처리");
        }

        public override string GetCharacterName() => characterData?.DisplayName ?? "Unnamed Enemy";
    }
}
