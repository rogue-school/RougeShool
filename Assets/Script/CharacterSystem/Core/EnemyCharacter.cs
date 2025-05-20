using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.CharacterSystem.Interface;

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

        /// <summary>
        /// 초기화: 데이터 설정 및 패시브/스킬카드 준비
        /// </summary>
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
                skillCardEntries = new List<EnemyCharacterData.SkillCardEntry>(characterData.GetAllCards());
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
            if (nameText != null)
                nameText.text = characterData.displayName;

            if (hpText != null)
                hpText.text = $"HP {GetCurrentHP()} / {GetMaxHP()}";

            if (portraitImage != null && characterData.portrait != null)
                portraitImage.sprite = characterData.portrait;
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

        /// <summary>
        /// 스킬 카드 풀에서 랜덤한 카드를 가져옵니다.
        /// </summary>
        public EnemyCharacterData.SkillCardEntry GetRandomCardEntry()
        {
            if (skillCardEntries == null || skillCardEntries.Count == 0)
            {
                Debug.LogWarning("[EnemyCharacter] 스킬 카드가 존재하지 않습니다.");
                return null;
            }

            int index = Random.Range(0, skillCardEntries.Count);
            return skillCardEntries[index];
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
            base.Die();
            UpdateUI();

            Debug.Log($"[EnemyCharacter] 사망 처리 완료 → 다음 적 소환은 외부 매니저에서 진행");
        }

        public string GetCharacterName()
        {
            return characterData?.displayName ?? "Unnamed Enemy";
        }
    }
}
