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
using Game.VFXSystem.Manager;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 적 캐릭터의 구체 구현 클래스입니다.
    /// 체력, UI, 스킬 덱, 패시브 효과, 사망 처리 등 적 전용 로직을 포함합니다.
    /// </summary>
    public class EnemyCharacter : CharacterBase, ICharacter
    {
        [Header("Character Data")]
        [SerializeField]
        private EnemyCharacterData _characterData;
        
        public new EnemyCharacterData CharacterData 
        { 
            get => _characterData;
            private set 
            {
                var oldValue = _characterData;
                _characterData = value;
                
                // CharacterData 변경 추적
                if (oldValue != value)
                {
                    var oldName = oldValue?.DisplayName ?? "null";
                    var newName = value?.DisplayName ?? "null";
                    GameLogger.LogInfo($"[EnemyCharacter] CharacterData 변경: {oldName} → {newName} (스택: {System.Environment.StackTrace.Split('\n')[1].Trim()})", GameLogger.LogCategory.Character);
                }
            }
        }

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

        private System.Collections.Generic.List<ICharacterEffect> characterEffects = new System.Collections.Generic.List<ICharacterEffect>();

        [Inject(Optional = true)] private VFXManager vfxManager;

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

            GameLogger.LogInfo($"[EnemyCharacter] Initialize 시작: {data.DisplayName} (스택: {System.Environment.StackTrace.Split('\n')[1].Trim()})", GameLogger.LogCategory.Character);
            
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
            
            GameLogger.LogInfo($"[EnemyCharacter] Initialize 완료: {data.DisplayName}, CharacterData: {CharacterData?.DisplayName ?? "null"}", GameLogger.LogCategory.Character);
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
            bool wasGuarded = IsGuarded();
            int previousHP = GetCurrentHP();

            base.TakeDamage(amount);
            int currentHP = GetCurrentHP();
            RefreshUI();

            // 소환 이펙트는 가드 상태와 관계없이 항상 체크
            NotifyHealthChanged(previousHP, currentHP);

            if (wasGuarded)
            {
                GameLogger.LogInfo($"[{GetCharacterName()}] 가드로 데미지 차단됨 - 데미지 텍스트 표시 안함", GameLogger.LogCategory.Character);
                return;
            }

            ShowDamageText(amount);

            if (IsDead() && !isDead)
            {
                MarkAsDead();
            }
        }

        /// <summary>
        /// 가드 무시 데미지 처리 후 UI 갱신
        /// </summary>
        /// <param name="amount">피해량</param>
        public override void TakeDamageIgnoreGuard(int amount)
        {
            int previousHP = GetCurrentHP();

            base.TakeDamageIgnoreGuard(amount);
            int currentHP = GetCurrentHP();
            RefreshUI();

            ShowDamageText(amount);
            NotifyHealthChanged(previousHP, currentHP);

            if (IsDead() && !isDead)
            {
                MarkAsDead();
            }
        }

        /// <summary>
        /// 데미지 텍스트를 표시합니다.
        /// </summary>
        /// <param name="amount">데미지량</param>
        private void ShowDamageText(int amount)
        {
            // 데미지가 0이면 텍스트 표시하지 않음
            if (amount <= 0) return;

            // 데미지 텍스트 표시 (VFXManager를 통한 Object Pooling)
            if (vfxManager != null && hpTextAnchor != null)
            {
                vfxManager.ShowDamageText(amount, hpTextAnchor.position, hpTextAnchor);
            }
            else if (damageTextPrefab != null && hpTextAnchor != null)
            {
                // Fallback: VFXManager가 없으면 기존 방식 사용
                var instance = Instantiate(damageTextPrefab);
                instance.transform.SetParent(hpTextAnchor, false);

                var damageUI = instance.GetComponent<DamageTextUI>();
                damageUI?.Show(amount, Color.red, "-");
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

            // 회복 텍스트 표시 (VFXManager를 통한 Object Pooling)
            if (vfxManager != null && hpTextAnchor != null)
            {
                vfxManager.ShowDamageText(amount, hpTextAnchor.position, hpTextAnchor);
            }
            else if (damageTextPrefab != null && hpTextAnchor != null)
            {
                // Fallback: VFXManager가 없으면 기존 방식 사용
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
        /// 캐릭터에 설정된 이펙트를 적용합니다.
        /// </summary>
        private void ApplyPassiveEffects()
        {
            if (CharacterData?.CharacterEffects == null) return;

            characterEffects.Clear();

            foreach (var entry in CharacterData.CharacterEffects)
            {
                if (entry.effectSO == null) continue;

                characterEffects.Add(entry.effectSO);

                // 커스텀 설정 사용 여부에 따라 초기화
                if (entry.useCustomSettings && entry.effectSO is Effect.SummonEffectSO summonEffectWithCustom)
                {
                    summonEffectWithCustom.InitializeWithCustomSettings(this, entry.customSettings);
                    summonEffectWithCustom.OnSummonTriggered += HandleSummonTriggered;
                }
                else
                {
                    entry.effectSO.Initialize(this);

                    // SummonEffectSO 지원
                    if (entry.effectSO is Effect.SummonEffectSO summonEffectSO)
                    {
                        summonEffectSO.OnSummonTriggered += HandleSummonTriggered;
                    }
                }

                GameLogger.LogInfo($"[{GetCharacterName()}] 캐릭터 이펙트 적용: {entry.effectSO.GetEffectName()} (커스텀: {entry.useCustomSettings})", GameLogger.LogCategory.Character);
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
            
            GameLogger.LogInfo($"[EnemyCharacter] SetCharacterData 호출: {data.DisplayName} (스택: {System.Environment.StackTrace.Split('\n')[1].Trim()})", GameLogger.LogCategory.Character);
            
            CharacterData = data;
            this.gameObject.name = CharacterData.name;
            Initialize(data);
            
            GameLogger.LogInfo($"[EnemyCharacter] SetCharacterData 완료: {data.DisplayName}, CharacterData: {CharacterData?.DisplayName ?? "null"}", GameLogger.LogCategory.Character);
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

        #region 캐릭터 이펙트 시스템

        private void NotifyHealthChanged(int previousHP, int currentHP)
        {
            GameLogger.LogInfo($"[{GetCharacterName()}] 체력 변경 알림: {previousHP} → {currentHP} (이펙트 수: {characterEffects.Count})", GameLogger.LogCategory.Character);
            
            foreach (var effect in characterEffects)
            {
                GameLogger.LogInfo($"[{GetCharacterName()}] 이펙트 체력 변경 처리: {effect.GetEffectName()}", GameLogger.LogCategory.Character);
                effect.OnHealthChanged(this, previousHP, currentHP);
            }
        }

        private void HandleSummonTriggered(EnemyCharacterData summonTarget, int currentHP)
        {
            GameLogger.LogInfo($"[{GetCharacterName()}] 소환 요청 전달: {summonTarget.DisplayName}, 현재 체력: {currentHP}", GameLogger.LogCategory.Character);
            
            // StageManager에 소환 요청 전달 (상태 패턴에서 처리)
            var stageManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
            if (stageManager != null)
            {
                // 원본 적 정보 저장
                stageManager.SetOriginalEnemyData(CharacterData as EnemyCharacterData);
                stageManager.SetOriginalEnemyHP(currentHP);
                stageManager.SetSummonTarget(summonTarget);
                
                // 소환 상태 플래그 설정
                stageManager.SetSummonedEnemyActive(true);
                
                GameLogger.LogInfo($"[{GetCharacterName()}] StageManager에 소환 요청 전달 완료", GameLogger.LogCategory.Character);
            }
            else
            {
                GameLogger.LogError($"[{GetCharacterName()}] StageManager를 찾을 수 없습니다", GameLogger.LogCategory.Character);
            }
        }

        private void CleanupEffects()
        {
            foreach (var effect in characterEffects)
            {
                // SummonEffectSO 지원
                if (effect is Effect.SummonEffectSO summonEffectSO)
                {
                    summonEffectSO.OnSummonTriggered -= HandleSummonTriggered;
                }
                effect.Cleanup(this);
            }
            characterEffects.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanupEffects();
        }

        #endregion
    }
}
