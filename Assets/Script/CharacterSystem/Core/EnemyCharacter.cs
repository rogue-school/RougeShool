using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Interface;
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
        /// 적 캐릭터를 데이터 기반으로 초기화합니다.
        /// </summary>
        public void Initialize(EnemyCharacterData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[EnemyCharacter] 초기화 실패 - EnemyCharacterData가 null입니다.");
                return;
            }

            characterData = data;

            SetMaxHP(data.maxHP);

            skillCardEntries = new List<EnemyCharacterData.SkillCardEntry>(data.GetAllCards());

            UpdateUI();
            ApplyPassiveEffects();

            Debug.Log($"[EnemyCharacter] '{characterData.displayName}' 초기화 완료");
        }


        public EnemyCharacterData.SkillCardEntry GetRandomCardEntry()
        {
            if (skillCardEntries == null || skillCardEntries.Count == 0)
            {
                Debug.LogWarning("[EnemyCharacter] 랜덤 카드 추출 실패 - 카드 목록이 비어 있습니다.");
                return null;
            }

            int index = Random.Range(0, skillCardEntries.Count);
            return skillCardEntries[index];
        }

        private void UpdateUI()
        {
            if (characterData == null) return;

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
            if (effects == null || effects.Count == 0) return;

            foreach (var obj in effects)
            {
                if (obj is ICardEffect effect)
                    effect.ExecuteEffect(this, this, 0);
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
            base.Die();
            UpdateUI();
            Debug.Log($"[EnemyCharacter] '{characterData?.displayName}' 사망 처리 완료");
        }

        public string GetCharacterName()
        {
            return characterData?.displayName ?? "Unnamed Enemy";
        }
    }
}
