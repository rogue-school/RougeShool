using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Context;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Deck;

namespace Game.CharacterSystem.Core
{
    public class EnemyCharacter : CharacterBase, IEnemyCharacter
    {
        [SerializeField] private EnemyCharacterData characterData;

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image portraitImage;

        private EnemySkillDeck skillDeck;
        private ICharacterDeathListener deathListener;

        public EnemyCharacterData Data => characterData;
        public override bool IsPlayerControlled() => false;

        public void Initialize(EnemyCharacterData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[EnemyCharacter] 초기화 실패 - null 데이터");
                return;
            }

            characterData = data;
            skillDeck = data.EnemyDeck;

            SetMaxHP(data.MaxHP);
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
            foreach (var effect in characterData?.PassiveEffects)
            {
                if (effect is ICardEffect cardEffect)
                    cardEffect.ApplyEffect(new DefaultCardExecutionContext(null, this, this), 0);
            }
        }

        public EnemySkillDeck.CardEntry GetRandomCardEntry()
        {
            return skillDeck?.GetRandomEntry();
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
