using System;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.UI;
using Game.CombatSystem.UI;
using Game.CombatSystem.Context;
using Game.CombatSystem.Interface;
using Game.CombatSystem;
using Game.SkillCardSystem.Deck;
using Game.SkillCardSystem.Interface;
using Game.CoreSystem.Utility;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 적 캐릭터의 구체 구현 클래스입니다.
    /// 체력, UI, 스킬 덱, 패시브 효과, 사망 처리 등 적 전용 로직을 포함합니다.
    /// </summary>
    public class EnemyCharacter : CharacterBase, ICharacter
    {
        [Header("Character Data")]
        [field: SerializeField]
        public new EnemyCharacterData CharacterData { get; private set; }

        // CharacterData 프로퍼티
        public ICharacterData CharacterDataInterface => CharacterData;

        // CharacterName 구현
        public override string CharacterName => CharacterData?.CharacterName ?? "Unknown Enemy";

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image portraitImage;
        
        [Header("HP Bar Controller")]
        [SerializeField] private HPBarController hpBarController;

        [Header("Damage Text UI")]
        [SerializeField] private Transform hpTextAnchor; // 데미지 텍스트 위치
        [SerializeField] private GameObject damageTextPrefab; // 데미지 텍스트 프리팹

        private EnemySkillDeck skillDeck;
        private System.Action<ICharacter> onDeathCallback;
        private bool isDead = false;

        /// <summary>
        /// 적 캐릭터의 데이터 (스크립터블 오브젝트)
        /// </summary>
        public EnemyCharacterData Data => CharacterData;

        /// <summary>
        /// 플레이어 조작 여부 → 적이므로 항상 false
        /// </summary>
        public override bool IsPlayerControlled() => false;

        private void Awake()
        {
            if (CharacterData != null)
                this.gameObject.name = CharacterData.name;
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (CharacterData != null)
                this.gameObject.name = CharacterData.name;
        }
#endif

        /// <summary>
        /// 캐릭터 이름 반환 (표시용 이름)
        /// </summary>
        public override string GetCharacterName()
        {
            if (CharacterData == null)
            {
                return gameObject.name.Replace("(Clone)", string.Empty).Trim();
            }
            // DisplayName이 없으면 ScriptableObject 이름으로 대체
            return string.IsNullOrEmpty(CharacterData.DisplayName)
                ? CharacterData.name
                : CharacterData.DisplayName;
        }

        // CharacterBase에서 사용할 이름 반환
        protected override string GetCharacterDataName()
        {
            return CharacterData?.name ?? "Unknown";
        }

        /// <summary>
        /// 캐릭터 이름 반환 (IEnemyCharacter 인터페이스 구현)
        /// </summary>
        public string GetName() => GetCharacterName();

        /// <summary>
        /// 현재 사망 상태인지 여부
        /// </summary>
        public bool IsMarkedDead => isDead;

        /// <summary>
        /// 사망 리스너를 설정합니다.
        /// </summary>
        /// <param name="listener">사망 리스너</param>
        public void SetDeathListener(object listener)
        {
            if (listener == null)
            {
                GameLogger.LogError("[EnemyCharacter] 사망 리스너가 null입니다.", GameLogger.LogCategory.Character);
                throw new ArgumentNullException(nameof(listener), "사망 리스너는 null일 수 없습니다.");
            }
            
            // TODO: 사망 리스너 구현
        }

        /// <summary>
        /// 적 캐릭터 초기화
        /// </summary>
        /// <param name="data">적 캐릭터 데이터</param>
        public void Initialize(EnemyCharacterData data)
        {
            if (data == null)
            {
                GameLogger.LogError("[EnemyCharacter] 초기화 실패 - null 데이터", GameLogger.LogCategory.Character);
                throw new ArgumentNullException(nameof(data), "적 캐릭터 데이터는 null일 수 없습니다.");
            }

            CharacterData = data;
            skillDeck = data.EnemyDeck;

            SetMaxHP(data.MaxHP);
            ApplyPassiveEffects();
            RefreshUI();
            
            // HP 바 초기화 (적 캐릭터 전용)
            if (hpBarController != null)
            {
                hpBarController.Initialize(this);
            }
        }

        /// <summary>
        /// 사망 시 외부 이벤트 수신자를 설정합니다.
        /// </summary>
        /// <param name="listener">리스너</param>
        public void SetDeathCallback(System.Action<ICharacter> callback)
        {
            onDeathCallback = callback;
        }

        /// <summary>
        /// 피해 처리 후 UI 갱신 및 사망 여부 판단
        /// </summary>
        /// <param name="amount">피해량</param>
        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            RefreshUI();

            // 데미지 텍스트 표시
            if (damageTextPrefab != null && hpTextAnchor != null)
            {
                var instance = Instantiate(damageTextPrefab);
                instance.transform.SetParent(hpTextAnchor, false);

                var damageUI = instance.GetComponent<DamageTextUI>();
                damageUI?.Show(amount, Color.red, "-");
            }

            if (IsDead() && !isDead)
            {
                MarkAsDead();
            }
        }

        /// <summary>
        /// 회복 처리 후 UI 갱신
        /// </summary>
        /// <param name="amount">회복량</param>
        public override void Heal(int amount)
        {
            base.Heal(amount);
            RefreshUI();

            if (damageTextPrefab != null && hpTextAnchor != null)
            {
                var instance = Instantiate(damageTextPrefab);
                instance.transform.SetParent(hpTextAnchor, false);

                var damageUI = instance.GetComponent<DamageTextUI>();
                damageUI?.Show(amount, Color.green, "+");
            }
        }

        /// <summary>
        /// UI 텍스트 및 초상화 갱신
        /// </summary>
        private void RefreshUI()
        {
            if (CharacterData == null) return;

            if (nameText != null)
            {
                nameText.text = GetCharacterName();
            }

            if (hpText != null)
            {
                hpText.text = currentHP.ToString();
                hpText.color = currentHP >= GetMaxHP() ? Color.white : Color.red;
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = CharacterData.Portrait;
            }
            
            // HP 바 업데이트
            if (hpBarController != null)
            {
                hpBarController.UpdateHPBar();
            }
        }

        /// <summary>
        /// 캐릭터에 설정된 패시브 효과를 즉시 적용합니다.
        /// </summary>
        private void ApplyPassiveEffects()
        {
            if (CharacterData?.PassiveEffects == null) return;

            foreach (var effect in CharacterData.PassiveEffects)
            {
                if (effect is ICardEffect cardEffect)
                {
                    var context = new DefaultCardExecutionContext(null, this, this);
                    cardEffect.ApplyEffect(context, 0);
                }
            }
        }

        /// <summary>
        /// 적 스킬 덱에서 무작위 카드 엔트리를 반환합니다.
        /// </summary>
        /// <returns>무작위 카드 엔트리</returns>
        public EnemySkillDeck.CardEntry GetRandomCardEntry()
        {
            if (skillDeck == null)
            {
                GameLogger.LogError("[EnemyCharacter] 스킬 덱이 null입니다.", GameLogger.LogCategory.Character);
                return null;
            }

            var entry = skillDeck.GetRandomEntry();

            if (entry?.definition == null)
            {
                GameLogger.LogError("[EnemyCharacter] 카드 선택 실패: entry 또는 definition이 null입니다.", GameLogger.LogCategory.Character);
            }
            else
            {
                GameLogger.LogInfo($"[EnemyCharacter] 카드 선택 완료: {entry.definition.displayName} (확률: {entry.probability})", GameLogger.LogCategory.Character);
            }

            return entry;
        }

        /// <summary>
        /// 외부에서 사망 처리를 명시적으로 호출할 수 있습니다.
        /// </summary>
        public void MarkAsDead()
        {
            if (isDead) return;

            isDead = true;
            GameLogger.LogInfo($"[EnemyCharacter] '{GetCharacterName()}' 사망 처리 (MarkAsDead 호출)", GameLogger.LogCategory.Character);

            // 사망 애니메이션/이벤트 발행 코드 제거
            // Game.CombatSystem.CombatEvents.RaiseHandSkillCardsVanishOnCharacterDeath(false);
            // Game.CombatSystem.CombatEvents.RaiseEnemyCharacterDeath(CharacterData, this.gameObject);
            onDeathCallback?.Invoke(this);
        }

        /// <summary>
        /// 기본 Die 로직은 MarkAsDead로 대체되며, 별도 처리는 없습니다.
        /// </summary>
        public override void Die()
        {
            MarkAsDead();
        }

        public void SetCharacterData(EnemyCharacterData data)
        {
            if (data == null)
            {
                GameLogger.LogError("[EnemyCharacter] 적 캐릭터 데이터가 null입니다.", GameLogger.LogCategory.Character);
                throw new ArgumentNullException(nameof(data), "적 캐릭터 데이터는 null일 수 없습니다.");
            }
            
            CharacterData = data;
            this.gameObject.name = CharacterData.name;
            Initialize(data);
        }

        /// <summary>
        /// ICharacter(부모) 경유 호출 시에도 EnemyCharacterData를 설정할 수 있도록 오버라이드
        /// </summary>
        public override void SetCharacterData(object data)
        {
            if (data is EnemyCharacterData enemyData)
            {
                SetCharacterData(enemyData);
                return;
            }

            GameLogger.LogError($"[EnemyCharacter] 잘못된 데이터 타입입니다. 예상: EnemyCharacterData, 실제: {data?.GetType().Name ?? "null"}", GameLogger.LogCategory.Character);
        }

        #region 이벤트 처리 오버라이드

        /// <summary>가드 획득 시 이벤트 발행</summary>
        protected override void OnGuarded(int amount)
        {
            CombatEvents.RaiseEnemyCharacterGuarded(CharacterData, this.gameObject, amount);
        }

        /// <summary>회복 시 이벤트 발행</summary>
        protected override void OnHealed(int amount)
        {
            CombatEvents.RaiseEnemyCharacterHealed(CharacterData, this.gameObject, amount);
        }

        /// <summary>피해 시 이벤트 발행</summary>
        protected override void OnDamaged(int amount)
        {
            CombatEvents.RaiseEnemyCharacterDamaged(CharacterData, this.gameObject, amount);
        }

        #endregion
    }
}
