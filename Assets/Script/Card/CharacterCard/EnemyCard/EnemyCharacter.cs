using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Game.Characters;
using Game.Data;
using Game.Effect;
using Game.Interface;

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

        /// <summary>
        /// 외부에서 데이터 초기화
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
                UpdateUI();
                ApplyPassiveEffects(); // 패시브 이펙트 적용
                Debug.Log($"[EnemyCharacter] {characterData.displayName} 초기화 완료");
            }
            else
            {
                Debug.LogWarning("[EnemyCharacter] characterData가 null입니다!");
            }
        }

        private void Awake()
        {
            if (characterData != null)
            {
                SetMaxHP(characterData.maxHP);
                UpdateUI();
                ApplyPassiveEffects(); // Awake에서도 보장
                Debug.Log($"[EnemyCharacter] {characterData.displayName} 초기화 완료");
            }
        }

        private void UpdateUI()
        {
            if (nameText != null)
                nameText.text = characterData.displayName;

            if (hpText != null)
                hpText.text = $"HP {currentHP} / {characterData.maxHP}";

            if (portraitImage != null)
            {
                if (characterData.portrait != null)
                {
                    portraitImage.sprite = characterData.portrait;
                    Debug.Log($"[EnemyCharacter] 아트워크 설정 완료: {characterData.portrait.name}");
                }
                else
                {
                    Debug.LogWarning("[EnemyCharacter] characterData.portrait가 null입니다.");
                }
            }
            else
            {
                Debug.LogError("[EnemyCharacter] portraitImage 컴포넌트가 연결되지 않았습니다.");
            }
        }

        /// <summary>
        /// 패시브 이펙트를 실행합니다. (Regen 등)
        /// </summary>
        private void ApplyPassiveEffects()
        {
            var effects = characterData?.GetPassiveEffects();

            if (effects == null || effects.Count == 0)
            {
                Debug.Log("[EnemyCharacter] 적용할 패시브 이펙트가 없습니다.");
                return;
            }

            foreach (var effectObject in effects)
            {
                if (effectObject is ICardEffect effect)
                {
                    try
                    {
                        Debug.Log($"[EnemyCharacter] 패시브 이펙트 적용 중: {effect.GetType().Name}");
                        effect.ExecuteEffect(this, this, 0);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[EnemyCharacter] 패시브 이펙트 실행 실패: {effect.GetType().Name}, 에러: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[EnemyCharacter] 잘못된 이펙트 타입: {effectObject?.GetType().Name}");
                }
            }
        }

        public override void Die()
        {
            base.Die();
            Debug.Log("[EnemyCharacter] 사망 → 전투 종료 처리 필요");
        }
    }
}
