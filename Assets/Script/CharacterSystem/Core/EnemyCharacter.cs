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
using DG.Tweening;

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

            Debug.Log($"[EnemyCharacter] '{characterData.DisplayName}' 초기화 완료");
        }

        /// <summary>
        /// 캐릭터 데이터를 설정합니다.
        /// </summary>
        /// <param name="data">설정할 캐릭터 데이터</param>
        public void SetCharacterData(EnemyCharacterData data)
        {
            Initialize(data);
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
            if (isDead) 
            {
                Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 이미 사망 상태입니다.");
                return;
            }

            isDead = true;
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 사망 처리 (MarkAsDead 호출)");

            // 스킬카드 소멸 애니메이션 실행
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 스킬카드 소멸 애니메이션 시작");
            StartCoroutine(VanishEnemySkillCardsOnDeath());
            
            // 사망 리스너 호출
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 사망 리스너 호출");
            deathListener?.OnCharacterDied(this);
        }

        /// <summary>
        /// 적 캐릭터 사망 시 해당 캐릭터의 스킬카드들을 소멸시킵니다.
        /// </summary>
        private System.Collections.IEnumerator VanishEnemySkillCardsOnDeath()
        {
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 스킬카드 소멸 시작");
            
            // 적 핸드의 모든 스킬카드들을 찾아서 소멸 애니메이션 적용
            var enemyCards = FindEnemySkillCards();
            
            if (enemyCards.Count == 0)
            {
                Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 소멸할 스킬카드가 없습니다.");
                yield break;
            }
            
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 소멸할 스킬카드 수: {enemyCards.Count}");
            
            // 모든 스킬카드에 소멸 애니메이션 적용
            int completedCount = 0;
            int totalCount = enemyCards.Count;
            
            foreach (var skillCard in enemyCards)
            {
                if (skillCard == null) continue;
                
                Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 스킬카드 소멸 애니메이션 적용: {skillCard.name}");
                
                // 간단한 페이드 아웃 효과 적용
                var canvasGroup = skillCard.GetComponent<UnityEngine.CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = skillCard.AddComponent<UnityEngine.CanvasGroup>();
                
                // 간단한 페이드 아웃 애니메이션
                canvasGroup.DOFade(0f, 0.5f).OnComplete(() => {
                    completedCount++;
                    Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 스킬카드 소멸 완료: {completedCount}/{totalCount} - {skillCard.name}");
                });
            }
            
            // 모든 애니메이션이 완료될 때까지 대기
            yield return new WaitUntil(() => completedCount >= totalCount);
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 모든 스킬카드 소멸 완료");
        }
        
        /// <summary>
        /// 적 스킬카드들을 찾습니다.
        /// </summary>
        /// <returns>적 스킬카드 GameObject 리스트</returns>
        private System.Collections.Generic.List<GameObject> FindEnemySkillCards()
        {
            var enemyCards = new System.Collections.Generic.List<GameObject>();
            
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 적 스킬카드 검색 시작");
            
            // EnemyHandCardSlotUI 컴포넌트를 가진 슬롯들을 찾기
            var enemyHandSlots = GameObject.FindObjectsByType<Game.CombatSystem.UI.EnemyHandCardSlotUI>(FindObjectsSortMode.None);
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 적 핸드 슬롯 수: {enemyHandSlots.Length}");
            
            foreach (var slot in enemyHandSlots)
            {
                Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 적 슬롯 발견: {slot.name}");
                
                // 슬롯에 카드가 있는지 확인
                if (slot.HasCard())
                {
                    var cardUI = slot.GetCardUI();
                    if (cardUI != null)
                    {
                        // ISkillCardUI를 SkillCardUI로 캐스팅하여 gameObject에 접근
                        var skillCardUI = cardUI as Game.SkillCardSystem.UI.SkillCardUI;
                        if (skillCardUI != null && skillCardUI.gameObject != null)
                        {
                            enemyCards.Add(skillCardUI.gameObject);
                            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 적 스킬카드 추가: {skillCardUI.gameObject.name}");
                        }
                        else
                        {
                            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' SkillCardUI 캐스팅 실패 또는 gameObject가 null: {slot.name}");
                        }
                    }
                    else
                    {
                        Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 슬롯에 카드가 있지만 UI가 null: {slot.name}");
                    }
                }
                else
                {
                    Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 슬롯에 카드 없음: {slot.name}");
                }
            }
            
            Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 찾은 적 스킬카드 수: {enemyCards.Count}");
            return enemyCards;
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
