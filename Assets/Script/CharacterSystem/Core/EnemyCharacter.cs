using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.UI;
using Game.CombatSystem.Context;
using Game.CombatSystem.Interface;
using Game.CombatSystem;
using Game.SkillCardSystem.Deck;
using Game.SkillCardSystem.Interface;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 적 캐릭터의 구체 구현 클래스입니다.
    /// 체력, UI, 스킬 덱, 패시브 효과, 사망 처리 등 적 전용 로직을 포함합니다.
    /// </summary>
    public class EnemyCharacter : CharacterBase, IEnemyCharacter
    {
        [Header("Character Data")]
        [SerializeField] private EnemyCharacterData characterData;

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image portraitImage;

        [Header("Damage Text UI")]
        [SerializeField] private Transform hpTextAnchor; // 데미지 텍스트 위치
        [SerializeField] private GameObject damageTextPrefab; // 데미지 텍스트 프리팹

        private EnemySkillDeck skillDeck;
        private ICharacterDeathListener deathListener;
        private bool isDead = false;

        /// <summary>
        /// 적 캐릭터의 데이터 (스크립터블 오브젝트)
        /// </summary>
        public EnemyCharacterData Data => characterData;

        /// <summary>
        /// 플레이어 조작 여부 → 적이므로 항상 false
        /// </summary>
        public override bool IsPlayerControlled() => false;

        /// <summary>
        /// 캐릭터 이름 반환 (표시용 이름)
        /// </summary>
        public override string GetCharacterName()
        {
            return characterData?.DisplayName ?? "Unnamed Enemy";
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
        /// 적 캐릭터 초기화
        /// </summary>
        /// <param name="data">적 캐릭터 데이터</param>
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
            ApplyPassiveEffects();
            RefreshUI();
        }

        /// <summary>
        /// 사망 시 외부 이벤트 수신자를 설정합니다.
        /// </summary>
        /// <param name="listener">리스너</param>
        public void SetDeathListener(ICharacterDeathListener listener)
        {
            deathListener = listener;
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
            if (characterData == null) return;

            nameText.text = GetCharacterName();
            hpText.text = currentHP.ToString(); // 현재 체력만 표시
            portraitImage.sprite = characterData.Portrait;

            // 체력 색상 설정: 최대 체력이면 흰색, 아니면 붉은색
            if (currentHP >= GetMaxHP())
            {
                hpText.color = Color.white;
            }
            else
            {
                hpText.color = Color.red;
            }
        }

        /// <summary>
        /// 캐릭터에 설정된 패시브 효과를 즉시 적용합니다.
        /// </summary>
        private void ApplyPassiveEffects()
        {
            if (characterData?.PassiveEffects == null) return;

            foreach (var effect in characterData.PassiveEffects)
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
                Debug.LogError("[EnemyCharacter] 스킬 덱이 null입니다.");
                return null;
            }

            var entry = skillDeck.GetRandomEntry();

            if (entry?.card == null)
            {
                Debug.LogError("[EnemyCharacter] 카드 선택 실패: entry 또는 card가 null입니다.");
            }
            else
            {
                Debug.Log($"[EnemyCharacter] 카드 선택 완료: {entry.card.name} (확률: {entry.probability})");
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
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 사망 처리 (MarkAsDead 호출)");

            // 1. AnimationFacade를 통한 사망 애니메이션 호출
            AnimationSystem.Manager.AnimationFacade.Instance.PlayEnemyCharacterDeathAnimation(
                characterData.name, // ScriptableObject의 name
                this.gameObject,
                () => {
                    // 2. 애니메이션 종료 후 이벤트 및 후처리
                    Game.CombatSystem.CombatEvents.RaiseEnemyCharacterDeath(characterData, this.gameObject);
                    deathListener?.OnCharacterDied(this);
                }
            );
        }

        /// <summary>
        /// 기본 Die 로직은 MarkAsDead로 대체되며, 별도 처리는 없습니다.
        /// </summary>
        public override void Die()
        {
            MarkAsDead();
        }
    }
}
