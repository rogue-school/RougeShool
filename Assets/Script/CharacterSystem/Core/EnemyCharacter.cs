using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Context;
using Game.CombatSystem.Interface;

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
        private ICharacterDeathListener deathListener;

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
            nameText.text = GetCharacterName();
            hpText.text = $"HP {currentHP} / {GetMaxHP()}";
            portraitImage.sprite = characterData.Portrait;
        }

        private void ApplyPassiveEffects()
        {
            foreach (var effect in characterData?.GetPassiveEffects())
            {
                if (effect is ICardEffect cardEffect)
                    cardEffect.ApplyEffect(new DefaultCardExecutionContext(null, this, this), 0);
            }
        }

        public EnemyCharacterData.SkillCardEntry GetRandomCardEntry()
        {
            if (skillCardEntries.Count == 0) return null;
            return skillCardEntries[Random.Range(0, skillCardEntries.Count)];
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
        public void SetDeathListener(ICharacterDeathListener listener)
        {
            deathListener = listener;
        }

        public override void Die()
        {
            base.Die();
            RefreshUI();
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 사망 처리");

            deathListener?.OnCharacterDied(this);
        }

        public override string GetCharacterName() => characterData?.DisplayName ?? "Unnamed Enemy";
    }
}
