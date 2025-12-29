using System;
using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.UI;
using Game.CombatSystem.UI;
using Game.CombatSystem.Context;
using Game.CombatSystem.Interface;
using Game.CombatSystem;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Core;
using Game.SkillCardSystem.Deck;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CoreSystem.Utility;
using Game.VFXSystem.Manager;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Zenject;
using DG.Tweening;

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
                    GameLogger.LogDebug($"[EnemyCharacter] CharacterData 변경: {oldName} → {newName}", GameLogger.LogCategory.Character);
                }
            }
        }

        // CharacterData 프로퍼티
        public ICharacterData CharacterDataInterface => CharacterData;

        // CharacterName 구현
        public override string CharacterName => CharacterData?.CharacterName ?? "Unknown Enemy";

        [Header("UI Components")]
        [Tooltip("Portrait 이미지 (자동으로 찾거나 설정됨)")]
        [SerializeField] private Image portraitImage;

        [Tooltip("Portrait가 배치될 부모 Transform (기본 Portrait GameObject의 부모)")]
        [SerializeField] private Transform portraitParent;

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        
        [Header("HP Bar Controller")]
        [SerializeField] private HPBarController hpBarController;

        [Header("Damage Text UI")]
        [SerializeField] private Transform hpTextAnchor; // 데미지 텍스트 위치
        [SerializeField] private GameObject damageTextPrefab; // 데미지 텍스트 프리팹

        [Header("애니메이션 설정")]
        [Tooltip("적 캐릭터 애니메이터")]
        [SerializeField] private Animator enemyAnimator;

        private EnemySkillDeck skillDeck;
        private System.Action<ICharacter> onDeathCallback;
        private bool isDead = false;

        private System.Collections.Generic.List<ICharacterEffect> characterEffects = new System.Collections.Generic.List<ICharacterEffect>();

        /// <summary>
        /// 시공의 폭풍 카드 실행 횟수 추적 (1번째, 2번째, 3번째)
        /// </summary>
        private int stormOfSpaceTimeCardExecutionCount = 0;

        /// <summary>
        /// 시공의 폭풍 카드가 다음 2턴 동안 강제로 생성되어야 하는지 여부
        /// </summary>
        private bool shouldForceStormOfSpaceTimeCard = false;


        /// <summary>
        /// 시공의 폭풍 카드가 이미 트리거되었는지 여부 (중복 트리거 방지)
        /// </summary>
        private bool stormOfSpaceTimeCardTriggered = false;

        [Inject(Optional = true)] private VFXManager vfxManager;
        [Inject(Optional = true)] private Game.CoreSystem.Interface.IAudioManager audioManager;
        [Inject(Optional = true)] private Game.CombatSystem.Interface.ICombatExecutionManager executionManager;
        [Inject(Optional = true)] private Game.CombatSystem.Interface.ICardSlotRegistry slotRegistry;
        [Inject(Optional = true)] private Game.CombatSystem.Interface.ISlotMovementController slotMovementController;
        [Inject(Optional = true)] private Game.StageSystem.Manager.StageManager stageManager;
        [Inject(Optional = true)] private Game.CombatSystem.State.CombatStateMachine combatStateMachine;
        [Inject(Optional = true)] private Game.SkillCardSystem.Interface.ISkillCardFactory cardFactory;
        private Sequence deathSequence;

        #region 페이즈 시스템

        /// <summary>
        /// 현재 페이즈 인덱스 (-1 = 페이즈 시스템 미사용, 0 이상 = 페이즈 인덱스)
        /// </summary>
        private int currentPhaseIndex = -1;

        /// <summary>
        /// 페이즈 전환 대기 중인지 여부 (중복 전환 방지)
        /// </summary>
        private bool isPhaseTransitionPending = false;

        /// <summary>
        /// 페이즈별 기본 정보 캐시
        /// </summary>
        private string cachedPhaseDisplayName;
        private Sprite cachedPhaseIndexIcon;
        private GameObject cachedPhasePortraitPrefab;

        /// <summary>
        /// 현재 페이즈 인덱스를 반환합니다.
        /// </summary>
        public int CurrentPhaseIndex => currentPhaseIndex;

        /// <summary>
        /// 현재 페이즈 이름을 반환합니다.
        /// </summary>
        public string CurrentPhaseName
        {
            get
            {
                if (CharacterData == null || !CharacterData.HasPhases)
                    return "기본";
                
                // currentPhaseIndex = -1: 기본 정보(1페이즈)
                if (currentPhaseIndex < 0)
                    return "1페이즈";
                
                // currentPhaseIndex >= 0: Phases 리스트의 페이즈 (2페이즈, 3페이즈, ...)
                if (currentPhaseIndex >= CharacterData.Phases.Count)
                    return "기본";
                
                return CharacterData.Phases[currentPhaseIndex].phaseName;
            }
        }

        #endregion

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
            // 애니메이터 자동 검색 (수동 설정이 없는 경우)
            if (enemyAnimator == null)
            {
                enemyAnimator = GetComponent<Animator>();
            }

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

            // 페이즈별 DisplayName이 있으면 우선 사용
            if (!string.IsNullOrEmpty(cachedPhaseDisplayName))
            {
                return cachedPhaseDisplayName;
            }

            // DisplayName이 없으면 ScriptableObject 이름으로 대체
            return string.IsNullOrEmpty(CharacterData.DisplayName)
                ? CharacterData.name
                : CharacterData.DisplayName;
        }

        /// <summary>
        /// 캐릭터 초상화 반환
        /// 프리팹을 사용하므로 스프라이트는 반환하지 않습니다.
        /// </summary>
        public override Sprite GetPortrait()
        {
            // 프리팹을 사용하므로 스프라이트 반환하지 않음
            // 다른 시스템과의 호환성을 위해 null 반환
            return null;
        }

        /// <summary>
        /// 인덱스 아이콘 반환 (페이즈별 정보 우선)
        /// </summary>
        public Sprite GetIndexIcon()
        {
            // 페이즈별 IndexIcon이 있으면 우선 사용
            if (cachedPhaseIndexIcon != null)
            {
                return cachedPhaseIndexIcon;
            }

            // 기본 IndexIcon 반환
            if (CharacterData != null)
            {
                return CharacterData.IndexIcon;
            }

            return null;
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

            // Portrait 프리팹 인스턴스화 (데이터에 설정된 경우)
            InitializePortrait(data);

            SetMaxHP(data.MaxHP);
            ApplyPassiveEffects();
            RefreshUI();
            
            // HP 바 초기화 (적 캐릭터 전용)
            ReinitializeHPBarController();
            
            // 적 캐릭터 덱의 스킬카드 스택 초기화
            InitializeEnemyDeckStacks();
            
            // 페이즈 시스템 초기화
            InitializePhases();
            
            // 카드 실행 완료 이벤트 구독
            SubscribeToExecutionEvents();
            
            // 카드 실행 이벤트 구독 (시공의 폭풍 카드 추적용)
            SubscribeToCardExecutedEvents();
            
            // 기본 Idle 시각 효과 시작 (부드러운 호흡)
            StartIdleVisualLoop();
        }

        /// <summary>
        /// Portrait 프리팹을 인스턴스화하고 설정합니다.
        /// </summary>
        /// <param name="data">적 캐릭터 데이터</param>
        private void InitializePortrait(EnemyCharacterData data)
        {
            if (data == null) return;

            InitializePortraitCommon(
                data.PortraitPrefab,
                portraitParent,
                ref portraitImage,
                ref hpTextAnchor,
                transform,
                GetCharacterName());
        }

        /// <summary>
        /// HP 바 컨트롤러를 재초기화합니다.
        /// GameObject 재활성화 후 UI 갱신을 위해 사용됩니다.
        /// </summary>
        public void ReinitializeHPBarController()
        {
            if (hpBarController != null)
            {
                hpBarController.Initialize(this);
            }
        }

        /// <summary>
        /// 적 캐릭터 덱의 스킬카드 스택을 초기화합니다.
        /// </summary>
        private void InitializeEnemyDeckStacks()
        {
            try
            {
                if (CharacterData?.EnemyDeck == null)
                {
                    GameLogger.LogWarning("[EnemyCharacter] EnemyDeck이 null입니다 - 스택 초기화 건너뜀", GameLogger.LogCategory.Character);
                    return;
                }

                int resetCount = 0;
                var deck = CharacterData.EnemyDeck;
                
                // EnemySkillDeck의 모든 카드 엔트리를 순회
                var cardEntries = deck.GetAllCards();
                foreach (var entry in cardEntries)
                {
                    if (entry?.definition != null)
                    {
                        entry.definition.ResetAttackPowerStacks();
                        resetCount++;
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[EnemyCharacter] 적 덱 스택 초기화 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
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

            // 다단 히트인 경우 텍스트 표시 건너뛰기 (DamageEffectCommand에서 직접 처리)
            if (skipDamageTextDisplay) return;

            // 데미지 텍스트 표시 (VFXManager를 통한 Object Pooling)
            bool success = false;
            if (vfxManager != null && hpTextAnchor != null)
            {
                success = vfxManager.ShowDamageText(amount, hpTextAnchor.position, hpTextAnchor);
            }

            // VFXManager가 없거나 damageTextPool이 설정되지 않은 경우 fallback 사용
            if (!success && damageTextPrefab != null && hpTextAnchor != null)
            {
                // 기존 텍스트 개수 확인 (VFXManager를 통해)
                float initialYOffset = 0f;
                if (vfxManager != null)
                {
                    initialYOffset = vfxManager.GetExistingTextCount(hpTextAnchor) * vfxManager.GetDamageTextSpacing();
                }

                var instance = Instantiate(damageTextPrefab);
                instance.transform.SetParent(hpTextAnchor, false);

                var damageUI = instance.GetComponent<DamageTextUI>();
                damageUI?.Show(amount, Color.red, "-", initialYOffset);

                // VFXManager가 있으면 등록 (쌓기 효과를 위해)
                if (vfxManager != null)
                {
                    vfxManager.RegisterDamageText(instance, hpTextAnchor);
            }
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
            bool success = false;
            if (vfxManager != null && hpTextAnchor != null)
            {
                success = vfxManager.ShowDamageText(amount, hpTextAnchor.position, hpTextAnchor);
            }

            // VFXManager가 없거나 damageTextPool이 설정되지 않은 경우 fallback 사용
            if (!success && damageTextPrefab != null && hpTextAnchor != null)
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
	public void RefreshUI()
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

		// Portrait는 프리팹이 자체적으로 관리하므로 여기서 직접 설정하지 않음
		
		// HP 바 업데이트
		if (hpBarController != null)
		{
			hpBarController.UpdateHPBar();
		}
	}

	/// <summary>
	/// 데미지 텍스트를 모두 정리합니다.
	/// </summary>
	public void ClearDamageTexts()
	{
		if (hpTextAnchor == null) return;

		// hpTextAnchor의 모든 자식 데미지 텍스트 제거
		for (int i = hpTextAnchor.childCount - 1; i >= 0; i--)
		{
			Transform child = hpTextAnchor.GetChild(i);
			if (child != null && child.GetComponent<CombatSystem.UI.DamageTextUI>() != null)
			{
				Destroy(child.gameObject);
				GameLogger.LogInfo($"[{GetCharacterName()}] 데미지 텍스트 제거됨", GameLogger.LogCategory.Character);
			}
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
                if (entry.useCustomSettings)
                {
                    if (entry.effectSO is Effect.SummonEffectSO summonEffectWithCustom)
                    {
                        summonEffectWithCustom.InitializeWithCustomSettings(this, entry.customSettings);
                        summonEffectWithCustom.OnSummonTriggered += HandleSummonTriggered;
                    }
                    else if (entry.effectSO is Effect.TriggerSkillOnHealthEffectSO skillEffectWithCustom)
                    {
                        skillEffectWithCustom.InitializeWithCustomSettings(this, entry.customSettings);
                        skillEffectWithCustom.OnSkillTriggered += HandleSkillTriggered;
                        skillEffectWithCustom.OnSkillDefinitionTriggered += HandleSkillDefinitionTriggered;
                    }
                    else
                    {
                        entry.effectSO.Initialize(this);
                    }
                }
                else
                {
                    entry.effectSO.Initialize(this);

                    // SummonEffectSO 지원
                    if (entry.effectSO is Effect.SummonEffectSO summonEffectSO)
                    {
                        summonEffectSO.OnSummonTriggered += HandleSummonTriggered;
                    }
                    // TriggerSkillOnHealthEffectSO 지원
                    else if (entry.effectSO is Effect.TriggerSkillOnHealthEffectSO skillEffect)
                    {
                        skillEffect.OnSkillTriggered += HandleSkillTriggered;
                        skillEffect.OnSkillDefinitionTriggered += HandleSkillDefinitionTriggered;
                        skillEffect.OnSkillDefinitionTriggered += HandleSkillDefinitionTriggered;
                    }
                }

                // 캐릭터 이펙트 적용 완료
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

            // 시공의 폭풍 버프가 존재하는지 먼저 확인 (강제 생성 모드 플래그와 무관하게 체크)
            // 남은 턴수만큼 시공의 폭풍 카드가 생성되도록 보장
            // 현재 슬롯에 있는 시공의 폭풍 카드 수를 계산하여 남은 턴수에서 빼야 함
            var stormDebuff = GetEffect<Game.SkillCardSystem.Effect.StormOfSpaceTimeDebuff>();
            if (stormDebuff != null && stormDebuff.RemainingTurns > 0)
            {
                // 현재 슬롯에 있는 시공의 폭풍 카드 수 카운트 (대기 슬롯 4개 + 배치 슬롯 1개)
                int currentStormCardCount = CountStormOfSpaceTimeCardsInSlots();
                
                // 남은 턴수에서 현재 슬롯에 있는 시공의 폭풍 카드 수를 뺀 값
                int remainingCardsNeeded = stormDebuff.RemainingTurns - currentStormCardCount;
                
                // 추가로 생성해야 할 카드가 있으면 시공의 폭풍 카드 생성
                if (remainingCardsNeeded > 0)
                {
                    // 시공의 폭풍 카드 찾기 (효과 기반 검색)
                    var allCards = skillDeck.GetAllCards();
                    if (allCards != null)
                    {
                        GameLogger.LogInfo($"[EnemyCharacter] 덱에서 시공의 폭풍 카드 검색 중... (덱 카드 수: {allCards.Count})", GameLogger.LogCategory.Character);
                        
                        foreach (var stormEntry in allCards)
                        {
                            if (stormEntry?.definition != null)
                            {
                                // 디버그: 모든 카드 ID 출력
                                if (stormEntry.definition.cardId != null)
                                {
                                    GameLogger.LogDebug($"[EnemyCharacter] 덱 카드 확인: ID={stormEntry.definition.cardId}, Name={stormEntry.definition.displayName}", GameLogger.LogCategory.Character);
                                }
                                
                                // 효과 기반으로 시공의 폭풍 카드 확인
                                if (stormEntry.definition.IsStormOfSpaceTimeCard())
                                {
                                    // 강제 생성 모드 플래그도 활성화 (다음 호출을 위해)
                                    shouldForceStormOfSpaceTimeCard = true;
                                    GameLogger.LogInfo(
                                        $"[EnemyCharacter] 시공의 폭풍 카드 강제 생성 (버프 남은 턴: {stormDebuff.RemainingTurns}, 현재 슬롯에 있는 카드: {currentStormCardCount}개, 추가 생성 필요: {remainingCardsNeeded}개, 목표 달성: {stormDebuff.IsTargetAchieved}, 누적 데미지: {stormDebuff.AccumulatedDamage}/{stormDebuff.TargetDamage})",
                                        GameLogger.LogCategory.Character);
                                    return stormEntry;
                                }
                            }
                        }
                    }
                    else
                    {
                        GameLogger.LogWarning("[EnemyCharacter] skillDeck.GetAllCards()가 null을 반환했습니다.", GameLogger.LogCategory.Character);
                    }
                    
                    // 시공의 폭풍 카드를 찾지 못한 경우 경고
                    GameLogger.LogWarning(
                        $"[EnemyCharacter] 시공의 폭풍 카드를 덱에서 찾을 수 없습니다. 일반 카드 선택으로 폴백 (버프 남은 턴: {stormDebuff.RemainingTurns}, 현재 슬롯에 있는 카드: {currentStormCardCount}개)",
                        GameLogger.LogCategory.Character);
                }
                else
                {
                    // 이미 남은 턴수만큼 시공의 폭풍 카드가 슬롯에 있으므로 일반 카드 생성
                    GameLogger.LogInfo(
                        $"[EnemyCharacter] 시공의 폭풍 카드 충분함 - 일반 카드 생성 (버프 남은 턴: {stormDebuff.RemainingTurns}, 현재 슬롯에 있는 카드: {currentStormCardCount}개)",
                        GameLogger.LogCategory.Character);
                }
            }
            else if (shouldForceStormOfSpaceTimeCard)
            {
                // 강제 생성 모드가 활성화되어 있지만 버프가 만료된 경우 모드 해제
                shouldForceStormOfSpaceTimeCard = false;
                GameLogger.LogInfo($"[EnemyCharacter] 시공의 폭풍 버프가 만료되어 강제 생성 모드 종료", GameLogger.LogCategory.Character);
            }

            var randomEntry = skillDeck.GetRandomEntry();

            if (randomEntry?.definition == null)
            {
                GameLogger.LogError("[EnemyCharacter] 카드 선택 실패: entry 또는 definition이 null입니다.", GameLogger.LogCategory.Character);
            }
            // 카드 선택 완료

            return randomEntry;
        }

        /// <summary>
        /// 현재 슬롯에 있는 시공의 폭풍 카드 수를 카운트합니다.
        /// 대기 슬롯 4개(WAIT_SLOT_1~4)와 배치 슬롯 1개(BATTLE_SLOT)를 확인합니다.
        /// </summary>
        /// <returns>현재 슬롯에 있는 시공의 폭풍 카드 수</returns>
        private int CountStormOfSpaceTimeCardsInSlots()
        {
            if (slotRegistry == null)
            {
                return 0;
            }

            int count = 0;
            var allSlots = new[]
            {
                Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_1,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_2,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_3,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_4
            };

            foreach (var slot in allSlots)
            {
                var card = slotRegistry.GetCardInSlot(slot);
                if (card != null && card.CardDefinition != null && card.CardDefinition.IsStormOfSpaceTimeCard())
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 외부에서 사망 처리를 명시적으로 호출할 수 있습니다.
        /// </summary>
        public void MarkAsDead()
        {
            if (isDead) return;

            isDead = true;
            GameLogger.LogInfo($"[EnemyCharacter] '{GetCharacterName()}' 사망 연출 시작", GameLogger.LogCategory.Character);

            // 사망 연출 실행 (완료 시 콜백)
            PlayDeathPresentation();
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
        protected override void OnDamaged(int amount, bool skipVisualEffects = false)
        {
            CombatEvents.RaiseEnemyCharacterDamaged(CharacterData, this.gameObject, amount);

            // 시각 효과를 건너뛰지 않는 경우에만 피격 효과 재생
            if (!skipVisualEffects)
            {
            // 피격 애니메이션 재생
            PlayHitAnimation();

            // 피격 시각 효과 재생
            PlayHitVisualEffects(amount);
            }
        }

        /// <summary>
        /// 피격 애니메이션을 재생합니다.
        /// </summary>
        private void PlayHitAnimation()
        {
            if (enemyAnimator != null)
            {
                // Hit 트리거 활성화
                enemyAnimator.SetTrigger("Hit");
                GameLogger.LogInfo($"[{GetCharacterName()}] 적 피격 애니메이션 재생", GameLogger.LogCategory.Character);
            }
        }

        #endregion

        #region 캐릭터 이펙트 시스템

        private void NotifyHealthChanged(int previousHP, int currentHP)
        {
            GameLogger.LogDebug($"[{GetCharacterName()}] 체력 변경 알림: {previousHP} → {currentHP}", GameLogger.LogCategory.Character);
            
            // 이펙트에 체력 변경 알림
            foreach (var effect in characterEffects)
            {
                GameLogger.LogDebug($"[{GetCharacterName()}] 이펙트 체력 변경 처리: {effect.GetEffectName()}", GameLogger.LogCategory.Character);
                effect.OnHealthChanged(this, previousHP, currentHP);
            }
            
            // 시공의 폭풍 카드 트리거 체크 (2페이즈, 체력 30 이하)
            if (!stormOfSpaceTimeCardTriggered && currentPhaseIndex == 0 && currentHP <= 30)
            {
                GameLogger.LogInfo($"[{GetCharacterName()}] 시공의 폭풍 카드 트리거 조건 만족 (2페이즈, 체력: {currentHP} <= 30)", GameLogger.LogCategory.Character);
                stormOfSpaceTimeCardTriggered = true;
                StartCoroutine(TriggerStormOfSpaceTimeCardCoroutine());
            }
            
            // 페이즈 전환 체크 (즉시 조건 확인하여 플래그 설정, 이후 지연된 체크로 실제 전환 시작)
            // 즉시 체크하여 SlotMovingState에서 적 턴으로 전환하는 것을 방지
            if (CharacterData != null && CharacterData.HasPhases && !isPhaseTransitionPending && !isDead)
            {
                int maxHP = GetMaxHP();
                
                // 즉시 페이즈 전환 조건 확인 (타이밍 문제 방지)
                // ShouldTransitionPhase()는 isPhaseTransitionPending을 체크하므로, 여기서는 직접 체크
                int startIndex = currentPhaseIndex < 0 ? 0 : currentPhaseIndex + 1;
                bool shouldTransition = false;
                
                for (int i = startIndex; i < CharacterData.Phases.Count; i++)
                {
                    var phase = CharacterData.Phases[i];
                    if (phase != null && phase.IsThresholdReached(currentHP, maxHP))
                    {
                        shouldTransition = true;
                        GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환 조건 즉시 확인 - 플래그 설정 (체력: {currentHP}/{maxHP}, 페이즈: {phase.phaseName})", GameLogger.LogCategory.Character);
                        break;
                    }
                }
                
                // 페이즈 전환이 필요하면 즉시 플래그 설정 (SlotMovingState에서 체크할 수 있도록)
                // 실제 전환은 CheckPhaseTransitionDelayed에서 시작 (VFX/데미지 효과 완료 후)
                if (shouldTransition)
                {
                    isPhaseTransitionPending = true;
                }
                
                // 카드 실행 완료 후 약간의 지연을 두고 실제 페이즈 전환 시작
                StartCoroutine(CheckPhaseTransitionDelayed(currentHP, maxHP));
            }
        }

        private void HandleSummonTriggered(EnemyCharacterData summonTarget, int currentHP)
        {
            GameLogger.LogInfo($"[{GetCharacterName()}] 소환 요청 전달: {summonTarget.DisplayName}, 현재 체력: {currentHP}/{GetMaxHP()}", GameLogger.LogCategory.Character);
            
            // StageManager 주입 시도
            if (stageManager == null)
            {
                EnsureStageManagerInjected();
            }
            
            // StageManager에 소환 요청 전달 (상태 패턴에서 처리)
            if (stageManager != null)
            {
                // 원본 적 정보 저장
                stageManager.SetOriginalEnemyData(CharacterData as EnemyCharacterData);
                stageManager.SetOriginalEnemyHP(currentHP);
                stageManager.SetSummonTarget(summonTarget);
                
                GameLogger.LogInfo($"[{GetCharacterName()}] StageManager에 원본 적 데이터 저장: {CharacterData?.DisplayName ?? "null"}, HP: {currentHP}", GameLogger.LogCategory.Character);
                
                // 소환 상태 플래그 설정
                stageManager.SetSummonedEnemyActive(true);
                
                GameLogger.LogInfo($"[{GetCharacterName()}] StageManager에 소환 요청 전달 완료 - 소환 플래그 활성화", GameLogger.LogCategory.Character);
            }
            else
            {
                GameLogger.LogError($"[{GetCharacterName()}] StageManager를 찾을 수 없습니다", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 스킬 발동 이펙트에서 스킬이 트리거되었을 때 호출됩니다.
        /// </summary>
        private void HandleSkillTriggered(string cardId, int currentHP)
        {
            // 시공의 폭풍 카드 트리거와 동일한 방식으로 처리
            StartCoroutine(TriggerSkillCardCoroutine(cardId));
        }

        /// <summary>
        /// SkillCardDefinition을 사용한 스킬 발동 핸들러입니다.
        /// </summary>
        private void HandleSkillDefinitionTriggered(SkillCardSystem.Data.SkillCardDefinition cardDefinition, int currentHP)
        {
            if (cardDefinition == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] 스킬 발동 요청: SkillCardDefinition이 null입니다", GameLogger.LogCategory.Character);
                return;
            }

            // SkillCardDefinition을 사용하여 카드 트리거
            StartCoroutine(TriggerSkillCardFromDefinitionCoroutine(cardDefinition));
        }

        /// <summary>
        /// 특정 스킬 카드를 트리거하여 강제로 생성하고 배치합니다.
        /// </summary>
        private System.Collections.IEnumerator TriggerSkillCardCoroutine(string cardId)
        {
            // SkillCardFactory 찾기
            if (cardFactory == null)
            {
                // Zenject를 통해 resolve 시도
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null)
                {
                    try
                    {
                        cardFactory = projectContext.Container.Resolve<Game.SkillCardSystem.Interface.ISkillCardFactory>();
                    }
                    catch (System.Exception e)
                    {
                        GameLogger.LogWarning($"[{GetCharacterName()}] SkillCardFactory resolve 실패: {e.Message}", GameLogger.LogCategory.Character);
                    }
                }
            }

            if (cardFactory == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SkillCardFactory를 찾을 수 없습니다. 스킬 카드를 생성할 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // SlotMovementController 찾기
            if (slotMovementController == null)
            {
                EnsureSlotMovementControllerInjected();
            }

            if (slotMovementController == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SlotMovementController를 찾을 수 없습니다. 스킬 카드를 배치할 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // 스킬 카드 생성
            Game.SkillCardSystem.Interface.ISkillCard skillCard = null;
            try
            {
                // SkillCardFactory로 캐스팅하여 CreateFromId 사용
                var factoryImpl = cardFactory as Game.SkillCardSystem.Factory.SkillCardFactory;
                if (factoryImpl != null)
                {
                    skillCard = factoryImpl.CreateFromId(cardId, Game.SkillCardSystem.Data.Owner.Enemy);
                    GameLogger.LogInfo($"[{GetCharacterName()}] 스킬 카드 생성 완료: {cardId}", GameLogger.LogCategory.Character);
                }
                else
                {
                    // 폴백: 덱에서 스킬 카드 정의 찾기
                    if (skillDeck != null)
                    {
                        var allCards = skillDeck.GetAllCards();
                        foreach (var entry in allCards)
                        {
                            if (entry?.definition != null && entry.definition.cardId == cardId)
                            {
                                skillCard = cardFactory.CreateEnemyCard(entry.definition, GetCharacterName());
                                GameLogger.LogInfo($"[{GetCharacterName()}] 스킬 카드 생성 완료 (덱에서 찾음): {cardId}", GameLogger.LogCategory.Character);
                                break;
                            }
                        }
                    }
                    
                    if (skillCard == null)
                    {
                        GameLogger.LogError($"[{GetCharacterName()}] 스킬 카드를 생성할 수 없습니다. SkillCardFactory를 캐스팅할 수 없거나 덱에서 찾을 수 없습니다. (카드 ID: {cardId})", GameLogger.LogCategory.Character);
                        yield break;
                    }
                }
            }
            catch (System.Exception e)
            {
                GameLogger.LogError($"[{GetCharacterName()}] 스킬 카드 생성 실패: {e.Message}", GameLogger.LogCategory.Character);
                yield break;
            }

            if (skillCard == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] 스킬 카드가 null입니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // 카드를 WAIT_SLOT_4에 배치
            var slotController = slotMovementController as Game.CombatSystem.Manager.SlotMovementController;
            if (slotController != null)
            {
                // 프리팹은 SlotMovementController 내부에서 처리
                yield return slotController.PlaceCardInWaitSlot4AndMoveRoutine(
                    skillCard, 
                    Game.CombatSystem.Data.SlotOwner.ENEMY, 
                    null); // 프리팹은 SlotMovementController에서 자동으로 로드
                
                // 스킬 카드 배치 완료
            }
            else
            {
                GameLogger.LogError($"[{GetCharacterName()}] SlotMovementController를 캐스팅할 수 없습니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// SkillCardDefinition을 사용하여 스킬 카드를 트리거하고 배치합니다.
        /// </summary>
        private System.Collections.IEnumerator TriggerSkillCardFromDefinitionCoroutine(SkillCardSystem.Data.SkillCardDefinition cardDefinition)
        {
            // SkillCardFactory 찾기
            if (cardFactory == null)
            {
                // Zenject를 통해 resolve 시도
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null)
                {
                    try
                    {
                        cardFactory = projectContext.Container.Resolve<Game.SkillCardSystem.Interface.ISkillCardFactory>();
                    }
                    catch (System.Exception e)
                    {
                        GameLogger.LogWarning($"[{GetCharacterName()}] SkillCardFactory resolve 실패: {e.Message}", GameLogger.LogCategory.Character);
                    }
                }
            }

            if (cardFactory == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SkillCardFactory를 찾을 수 없습니다. 스킬 카드를 생성할 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // SlotMovementController 찾기
            if (slotMovementController == null)
            {
                EnsureSlotMovementControllerInjected();
            }

            if (slotMovementController == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SlotMovementController를 찾을 수 없습니다. 스킬 카드를 배치할 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // 스킬 카드 생성 (SkillCardDefinition 직접 사용)
            Game.SkillCardSystem.Interface.ISkillCard skillCard = null;
            try
            {
                skillCard = cardFactory.CreateFromDefinition(cardDefinition, Game.SkillCardSystem.Data.Owner.Enemy, GetCharacterName());
            }
            catch (System.Exception e)
            {
                GameLogger.LogError($"[{GetCharacterName()}] 스킬 카드 생성 실패: {e.Message}", GameLogger.LogCategory.Character);
                yield break;
            }

            if (skillCard == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] 스킬 카드가 null입니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // 전투/대기 슬롯에서 적 카드를 찾아 교체
            var slotController = slotMovementController as Game.CombatSystem.Manager.SlotMovementController;
            if (slotController != null)
            {
                // ICardSlotRegistry 가져오기
                var registry = slotController.GetCardSlotRegistry();
                
                if (registry != null)
                {
                    // 교체할 슬롯 찾기: BATTLE_SLOT → WAIT_SLOT_1 → WAIT_SLOT_2 → WAIT_SLOT_3 → WAIT_SLOT_4 순서
                    var slotsToCheck = new[]
                    {
                        Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT,
                        Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_1,
                        Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_2,
                        Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_3,
                        Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_4
                    };

                    Game.CombatSystem.Slot.CombatSlotPosition? targetSlot = null;
                    foreach (var slot in slotsToCheck)
                    {
                        var existingCard = registry.GetCardInSlot(slot);
                        if (existingCard != null && !existingCard.IsFromPlayer())
                        {
                            targetSlot = slot;
                            break;
                        }
                    }

                    // 시공의 폭풍 카드인지 확인 (특수 기믹 스킬은 우선적으로 배틀 슬롯에 배치)
                    bool isStormOfSpaceTimeCard = cardDefinition != null && cardDefinition.IsStormOfSpaceTimeCard();
                    
                    if (targetSlot.HasValue)
                    {
                        // 기존 카드 교체
                        yield return ReplaceCardInSlotCoroutine(registry, targetSlot.Value, skillCard, slotController);
                        // 스킬 카드 교체 완료
                    }
                    else
                    {
                        // 교체할 카드가 없으면 배치
                        // 시공의 폭풍 카드는 우선적으로 배틀 슬롯에 배치 시도
                        if (isStormOfSpaceTimeCard)
                        {
                            // 배틀 슬롯이 비어있으면 배틀 슬롯에 직접 배치
                            var battleCard = registry.GetCardInSlot(Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT);
                            if (battleCard == null)
                            {
                                // 배틀 슬롯에 직접 배치 (리플렉션 사용)
                                var createCardUIMethod = typeof(Game.CombatSystem.Manager.SlotMovementController)
                                    .GetMethod("CreateCardUIForSlot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                
                                var getCachedCardUIPrefabMethod = typeof(Game.CombatSystem.Manager.SlotMovementController)
                                    .GetMethod("GetCachedCardUIPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                
                                Game.SkillCardSystem.UI.SkillCardUI cardUIPrefab = null;
                                if (getCachedCardUIPrefabMethod != null)
                                {
                                    // GetCachedCardUIPrefab이 코루틴을 반환하면 EnemyCharacter에서 시작
                                    var prefabCoroutine = getCachedCardUIPrefabMethod.Invoke(slotController, null) as System.Collections.IEnumerator;
                                    if (prefabCoroutine != null)
                                    {
                                        yield return prefabCoroutine;
                                    }
                                    var cachedPrefabField = typeof(Game.CombatSystem.Manager.SlotMovementController)
                                        .GetField("_cachedCardUIPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                    cardUIPrefab = cachedPrefabField?.GetValue(slotController) as Game.SkillCardSystem.UI.SkillCardUI;
                                }
                                
                                if (createCardUIMethod != null && cardUIPrefab != null)
                                {
                                    var newCardUI = createCardUIMethod.Invoke(slotController, new object[] { skillCard, Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT, cardUIPrefab }) as Game.SkillCardSystem.UI.SkillCardUI;
                                    if (newCardUI != null)
                                    {
                                        registry.RegisterCard(Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT, skillCard, newCardUI, Game.CombatSystem.Data.SlotOwner.ENEMY);
                                        GameLogger.LogInfo($"[{GetCharacterName()}] 시공의 폭풍 카드를 배틀 슬롯에 직접 배치", GameLogger.LogCategory.Character);
                                        
                                        // 배틀 슬롯에 배치되었으므로 즉시 실행 시도
                                        // 적의 턴 중에 시공의 폭풍이 발동하면 즉시 실행되도록 처리
                                        var executionManager = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Manager.CombatExecutionManager>();
                                        var stateMachine = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.State.CombatStateMachine>();
                                        
                                        if (executionManager != null)
                                        {
                                            // 현재 상태가 EnemyTurnState인지 확인
                                            bool isEnemyTurnState = false;
                                            if (stateMachine != null)
                                            {
                                                var currentState = stateMachine.GetCurrentState();
                                                isEnemyTurnState = currentState is Game.CombatSystem.State.EnemyTurnState;
                                            }
                                            
                                            // SlotMovementController의 _turnController를 통해 턴 확인 (폴백)
                                            bool isEnemyTurn = false;
                                            var turnControllerField = typeof(Game.CombatSystem.Manager.SlotMovementController)
                                                .GetField("_turnController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                            
                                            if (turnControllerField != null)
                                            {
                                                var turnController = turnControllerField.GetValue(slotController) as Game.CombatSystem.Interface.ITurnController;
                                                isEnemyTurn = turnController != null && turnController.IsEnemyTurn();
                                            }
                                            
                                            // 적의 턴 중에 시공의 폭풍이 발동한 경우
                                            if (isEnemyTurnState || isEnemyTurn)
                                            {
                                                if (executionManager.IsExecuting)
                                                {
                                                    // 실행 중이면 실행 완료 후 즉시 실행되도록 이벤트 구독
                                                    System.Action<Game.CombatSystem.Interface.ExecutionResult> onExecutionCompleted = null;
                                                    onExecutionCompleted = (result) =>
                                                    {
                                                        executionManager.OnExecutionCompleted -= onExecutionCompleted;
                                                        GameLogger.LogInfo($"[{GetCharacterName()}] 실행 완료 후 시공의 폭풍 카드 즉시 실행: {skillCard.GetCardName()}", GameLogger.LogCategory.Character);
                                                        executionManager.ExecuteCardImmediately(skillCard, Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT);
                                                    };
                                                    executionManager.OnExecutionCompleted += onExecutionCompleted;
                                                    GameLogger.LogInfo($"[{GetCharacterName()}] 적 턴 중 시공의 폭풍 카드 발동 - 다른 카드 실행 완료 후 즉시 실행 예약", GameLogger.LogCategory.Character);
                                                }
                                                    else
                                                    {
                                                        // 실행 중이 아니면 즉시 실행
                                                        // EnemyTurnState가 활성화되어 있으면 CheckAndExecuteEnemyCard를 다시 호출하도록 함
                                                        if (isEnemyTurnState && stateMachine != null)
                                                        {
                                                            // EnemyTurnState의 CheckForStormOfSpaceTimeCard를 호출하여 카드 실행을 다시 체크
                                                            var currentState = stateMachine.GetCurrentState() as Game.CombatSystem.State.EnemyTurnState;
                                                            if (currentState != null)
                                                            {
                                                                // CombatStateContext 가져오기 (리플렉션 사용)
                                                                var contextField = typeof(Game.CombatSystem.State.CombatStateMachine)
                                                                    .GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                                                if (contextField != null)
                                                                {
                                                                    var context = contextField.GetValue(stateMachine) as Game.CombatSystem.State.CombatStateContext;
                                                                    if (context != null)
                                                                    {
                                                                        GameLogger.LogInfo($"[{GetCharacterName()}] 적 턴 중 시공의 폭풍 카드 발동 - EnemyTurnState에 카드 실행 재체크 요청: {skillCard.GetCardName()}", GameLogger.LogCategory.Character);
                                                                        currentState.CheckForStormOfSpaceTimeCard(context);
                                                                        yield break; // EnemyTurnState가 처리하므로 여기서 종료
                                                                    }
                                                                }
                                                            }
                                                            
                                                            // 리플렉션 실패 시 폴백: 직접 실행
                                                            GameLogger.LogInfo($"[{GetCharacterName()}] 적 턴 중 시공의 폭풍 카드 발동 - 즉시 실행 (폴백): {skillCard.GetCardName()}", GameLogger.LogCategory.Character);
                                                            executionManager.ExecuteCardImmediately(skillCard, Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT);
                                                        }
                                                        else
                                                        {
                                                            // 상태 머신이 없거나 EnemyTurnState가 아니면 일반 실행
                                                            GameLogger.LogInfo($"[{GetCharacterName()}] 배틀 슬롯에 배치된 시공의 폭풍 카드 즉시 실행: {skillCard.GetCardName()}", GameLogger.LogCategory.Character);
                                                            executionManager.ExecuteCardImmediately(skillCard, Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT);
                                                        }
                                                    }
                                            }
                                            else
                                            {
                                                // 적의 턴이 아니면 다음 적 턴에 실행되도록 로그만 남김
                                                GameLogger.LogInfo($"[{GetCharacterName()}] 적 턴이 아니므로 시공의 폭풍 카드는 다음 적 턴에 실행됩니다: {skillCard.GetCardName()}", GameLogger.LogCategory.Character);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // 카드 UI 생성 실패 시 폴백
                                        yield return slotController.PlaceCardInWaitSlot4AndMoveRoutine(skillCard, Game.CombatSystem.Data.SlotOwner.ENEMY, cardUIPrefab);
                                    }
                                }
                                else
                                {
                                    // 리플렉션 실패 시 폴백
                                    yield return slotController.PlaceCardInWaitSlot4AndMoveRoutine(skillCard, Game.CombatSystem.Data.SlotOwner.ENEMY, cardUIPrefab);
                                }
                            }
                            else
                            {
                                // 배틀 슬롯이 비어있지 않으면 WAIT_SLOT_4에 배치
                                yield return slotController.PlaceCardInWaitSlot4AndMoveRoutine(skillCard, Game.CombatSystem.Data.SlotOwner.ENEMY, null);
                            }
                        }
                        else
                        {
                            // 일반 카드는 WAIT_SLOT_4에 배치
                            yield return slotController.PlaceCardInWaitSlot4AndMoveRoutine(
                                skillCard, 
                                Game.CombatSystem.Data.SlotOwner.ENEMY, 
                                null);
                        }
                        // 스킬 카드 배치 완료
                    }
                }
                else
                {
                    // 폴백: WAIT_SLOT_4에 배치
                    yield return slotController.PlaceCardInWaitSlot4AndMoveRoutine(
                        skillCard, 
                        Game.CombatSystem.Data.SlotOwner.ENEMY, 
                        null);
                    // 스킬 카드 배치 완료 (레지스트리 접근 실패)
                }
            }
            else
            {
                GameLogger.LogError($"[{GetCharacterName()}] SlotMovementController를 캐스팅할 수 없습니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 배치/대기 슬롯의 모든 적 카드를 시공의 폭풍으로 교체합니다.
        /// </summary>
        /// <param name="stormCardDefinition">시공의 폭풍 카드 정의 (null이면 덱에서 찾음)</param>
        /// <summary>
        /// 남은 턴수만큼 시공의 폭풍 카드를 생성합니다.
        /// 빈 슬롯부터 채우고, 부족하면 기존 카드를 교체합니다.
        /// </summary>
        public System.Collections.IEnumerator GenerateStormOfSpaceTimeCardsForRemainingTurnsCoroutine(Game.SkillCardSystem.Data.SkillCardDefinition stormCardDefinition = null)
        {
            // 시공의 폭풍 버프 확인
            var stormDebuff = GetEffect<Game.SkillCardSystem.Effect.StormOfSpaceTimeDebuff>();
            if (stormDebuff == null || stormDebuff.RemainingTurns <= 0)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] 시공의 폭풍 버프가 없거나 턴이 남아있지 않습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            int targetCount = stormDebuff.RemainingTurns;
            int currentCount = CountStormOfSpaceTimeCardsInSlots();
            int neededCount = targetCount - currentCount;

            if (neededCount <= 0)
            {
                GameLogger.LogInfo($"[{GetCharacterName()}] 시공의 폭풍 카드 충분함 (목표: {targetCount}개, 현재: {currentCount}개)", GameLogger.LogCategory.Character);
                yield break;
            }

            GameLogger.LogInfo($"[{GetCharacterName()}] 시공의 폭풍 카드 생성 시작 (목표: {targetCount}개, 현재: {currentCount}개, 필요: {neededCount}개)", GameLogger.LogCategory.Character);

            // SlotMovementController 찾기
            if (slotMovementController == null)
            {
                EnsureSlotMovementControllerInjected();
            }

            if (slotMovementController == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SlotMovementController를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            var slotController = slotMovementController as Game.CombatSystem.Manager.SlotMovementController;
            if (slotController == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SlotMovementController를 캐스팅할 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // ICardSlotRegistry 가져오기
            var registry = slotController.GetCardSlotRegistry();
            if (registry == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] ICardSlotRegistry를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // SkillCardFactory 찾기
            if (cardFactory == null)
            {
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null)
                {
                    try
                    {
                        cardFactory = projectContext.Container.Resolve<Game.SkillCardSystem.Interface.ISkillCardFactory>();
                    }
                    catch (System.Exception e)
                    {
                        GameLogger.LogWarning($"[{GetCharacterName()}] SkillCardFactory resolve 실패: {e.Message}", GameLogger.LogCategory.Character);
                    }
                }
            }

            if (cardFactory == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SkillCardFactory를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // 시공의 폭풍 카드 정의 찾기 (파라미터가 없으면 덱에서 찾기)
            if (stormCardDefinition == null)
            {
                if (skillDeck != null)
                {
                    var allCards = skillDeck.GetAllCards();
                    foreach (var entry in allCards)
                    {
                        if (entry?.definition != null && entry.definition.IsStormOfSpaceTimeCard())
                        {
                            stormCardDefinition = entry.definition;
                            break;
                        }
                    }
                }
            }

            if (stormCardDefinition == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] 시공의 폭풍 카드 정의를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // 배치/대기 슬롯 확인
            var slotsToCheck = new[]
            {
                Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_1,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_2,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_3,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_4
            };

            int generatedCount = 0;

            // 카드 UI 프리팹 가져오기 (리플렉션 사용)
            Game.SkillCardSystem.UI.SkillCardUI cardUIPrefab = null;
            var cardUIPrefabField = typeof(Game.CombatSystem.Manager.SlotMovementController)
                .GetField("_cachedCardUIPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (cardUIPrefabField != null)
            {
                cardUIPrefab = cardUIPrefabField.GetValue(slotController) as Game.SkillCardSystem.UI.SkillCardUI;
            }

            // 프리팹이 없으면 로드 시도
            if (cardUIPrefab == null)
            {
                var getCachedCardUIPrefabMethod = typeof(Game.CombatSystem.Manager.SlotMovementController)
                    .GetMethod("GetCachedCardUIPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (getCachedCardUIPrefabMethod != null)
                {
                    var prefabCoroutine = getCachedCardUIPrefabMethod.Invoke(slotController, null) as System.Collections.IEnumerator;
                    if (prefabCoroutine != null)
                    {
                        yield return prefabCoroutine;
                        // 코루틴 완료 후 다시 확인
                        if (cardUIPrefabField != null)
                        {
                            cardUIPrefab = cardUIPrefabField.GetValue(slotController) as Game.SkillCardSystem.UI.SkillCardUI;
                        }
                    }
                }
            }

            // CreateCardUIForSlot 메서드 가져오기 (리플렉션 사용)
            var createCardUIMethod = typeof(Game.CombatSystem.Manager.SlotMovementController)
                .GetMethod("CreateCardUIForSlot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // 1단계: 빈 슬롯에 시공의 폭풍 카드 배치
            foreach (var slot in slotsToCheck)
            {
                if (generatedCount >= neededCount)
                    break;

                var existingCard = registry.GetCardInSlot(slot);
                if (existingCard == null)
                {
                    // 빈 슬롯에 시공의 폭풍 카드 생성
                    Game.SkillCardSystem.Interface.ISkillCard stormCard = null;
                    try
                    {
                        stormCard = cardFactory.CreateFromDefinition(stormCardDefinition, Game.SkillCardSystem.Data.Owner.Enemy, GetCharacterName());
                    }
                    catch (System.Exception e)
                    {
                        GameLogger.LogError($"[{GetCharacterName()}] 시공의 폭풍 카드 생성 실패 (슬롯: {slot}): {e.Message}", GameLogger.LogCategory.Character);
                        continue;
                    }

                    if (stormCard != null)
                    {
                        // 빈 슬롯에 직접 카드 UI 생성 및 배치
                        if (createCardUIMethod != null && cardUIPrefab != null)
                        {
                            var newCardUI = createCardUIMethod.Invoke(slotController, new object[] { stormCard, slot, cardUIPrefab }) as Game.SkillCardSystem.UI.SkillCardUI;
                            if (newCardUI != null)
                            {
                                registry.RegisterCard(slot, stormCard, newCardUI, Game.CombatSystem.Data.SlotOwner.ENEMY);
                                generatedCount++;
                                GameLogger.LogInfo($"[{GetCharacterName()}] 빈 슬롯에 시공의 폭풍 카드 배치: {slot} ({generatedCount}/{neededCount})", GameLogger.LogCategory.Character);
                            }
                            else
                            {
                                GameLogger.LogWarning($"[{GetCharacterName()}] 빈 슬롯 {slot}에 카드 UI 생성 실패", GameLogger.LogCategory.Character);
                            }
                        }
                        else
                        {
                            // 폴백: PlaceCardInWaitSlot4AndMoveRoutine 사용
                            yield return slotController.PlaceCardInWaitSlot4AndMoveRoutine(stormCard, Game.CombatSystem.Data.SlotOwner.ENEMY, cardUIPrefab);
                            generatedCount++;
                            GameLogger.LogInfo($"[{GetCharacterName()}] 빈 슬롯에 시공의 폭풍 카드 배치 (폴백): {slot} ({generatedCount}/{neededCount})", GameLogger.LogCategory.Character);
                        }
                    }
                }
            }

            // 2단계: 부족하면 기존 적 카드를 시공의 폭풍 카드로 교체
            if (generatedCount < neededCount)
            {
                foreach (var slot in slotsToCheck)
                {
                    if (generatedCount >= neededCount)
                        break;

                    var existingCard = registry.GetCardInSlot(slot);
                    if (existingCard != null && !existingCard.IsFromPlayer())
                    {
                        // 이미 시공의 폭풍 카드면 스킵
                        if (existingCard.CardDefinition != null && existingCard.CardDefinition.IsStormOfSpaceTimeCard())
                        {
                            continue;
                        }

                        // 시공의 폭풍 카드 생성
                        Game.SkillCardSystem.Interface.ISkillCard stormCard = null;
                        try
                        {
                            stormCard = cardFactory.CreateFromDefinition(stormCardDefinition, Game.SkillCardSystem.Data.Owner.Enemy, GetCharacterName());
                        }
                        catch (System.Exception e)
                        {
                            GameLogger.LogError($"[{GetCharacterName()}] 시공의 폭풍 카드 생성 실패 (슬롯: {slot}): {e.Message}", GameLogger.LogCategory.Character);
                            continue;
                        }

                        if (stormCard != null)
                        {
                            yield return ReplaceCardInSlotCoroutine(registry, slot, stormCard, slotController);
                            generatedCount++;
                            GameLogger.LogInfo($"[{GetCharacterName()}] 기존 카드를 시공의 폭풍 카드로 교체: {slot} ({generatedCount}/{neededCount})", GameLogger.LogCategory.Character);
                        }
                    }
                }
            }

            GameLogger.LogInfo($"[{GetCharacterName()}] 시공의 폭풍 카드 생성 완료 (생성: {generatedCount}개, 목표: {targetCount}개, 현재 총: {CountStormOfSpaceTimeCardsInSlots()}개)", GameLogger.LogCategory.Character);
        }

        public System.Collections.IEnumerator ReplaceAllEnemyCardsWithStormOfSpaceTimeCoroutine(Game.SkillCardSystem.Data.SkillCardDefinition stormCardDefinition = null)
        {
            // SlotMovementController 찾기
            if (slotMovementController == null)
            {
                EnsureSlotMovementControllerInjected();
            }

            if (slotMovementController == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SlotMovementController를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            var slotController = slotMovementController as Game.CombatSystem.Manager.SlotMovementController;
            if (slotController == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SlotMovementController를 캐스팅할 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // ICardSlotRegistry 가져오기
            var registry = slotController.GetCardSlotRegistry();
            if (registry == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] ICardSlotRegistry를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // SkillCardFactory 찾기
            if (cardFactory == null)
            {
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null)
                {
                    try
                    {
                        cardFactory = projectContext.Container.Resolve<Game.SkillCardSystem.Interface.ISkillCardFactory>();
                    }
                    catch (System.Exception e)
                    {
                        GameLogger.LogWarning($"[{GetCharacterName()}] SkillCardFactory resolve 실패: {e.Message}", GameLogger.LogCategory.Character);
                    }
                }
            }

            if (cardFactory == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SkillCardFactory를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // 시공의 폭풍 카드 정의 찾기 (파라미터가 없으면 덱에서 찾기)
            if (stormCardDefinition == null)
            {
                if (skillDeck != null)
                {
                    var allCards = skillDeck.GetAllCards();
                    foreach (var entry in allCards)
                    {
                        if (entry?.definition != null && entry.definition.IsStormOfSpaceTimeCard())
                        {
                            stormCardDefinition = entry.definition;
                            break;
                        }
                    }
                }
            }

            if (stormCardDefinition == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] 시공의 폭풍 카드 정의를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // 배치/대기 슬롯 확인
            var slotsToCheck = new[]
            {
                Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_1,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_2,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_3,
                Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_4
            };

            int replacedCount = 0;
            foreach (var slot in slotsToCheck)
            {
                var existingCard = registry.GetCardInSlot(slot);
                if (existingCard != null && !existingCard.IsFromPlayer())
                {
                    // 이미 시공의 폭풍 카드면 스킵
                    if (existingCard.CardDefinition != null && existingCard.CardDefinition.IsStormOfSpaceTimeCard())
                    {
                        continue;
                    }

                    // 시공의 폭풍 카드 생성
                    Game.SkillCardSystem.Interface.ISkillCard stormCard = null;
                    try
                    {
                        stormCard = cardFactory.CreateFromDefinition(stormCardDefinition, Game.SkillCardSystem.Data.Owner.Enemy, GetCharacterName());
                    }
                    catch (System.Exception e)
                    {
                        GameLogger.LogError($"[{GetCharacterName()}] 시공의 폭풍 카드 생성 실패 (슬롯: {slot}): {e.Message}", GameLogger.LogCategory.Character);
                        continue;
                    }

                    if (stormCard != null)
                    {
                        yield return ReplaceCardInSlotCoroutine(registry, slot, stormCard, slotController);
                        replacedCount++;
                    }
                }
            }
        }

        /// <summary>
        /// 슬롯의 카드를 새로운 카드로 교체합니다.
        /// </summary>
        private System.Collections.IEnumerator ReplaceCardInSlotCoroutine(
            Game.CombatSystem.Interface.ICardSlotRegistry registry,
            Game.CombatSystem.Slot.CombatSlotPosition slotPosition,
            Game.SkillCardSystem.Interface.ISkillCard newCard,
            Game.CombatSystem.Manager.SlotMovementController slotController)
        {
            // 기존 카드 UI 가져오기
            var existingCardUI = registry.GetCardUIInSlot(slotPosition);
            if (existingCardUI != null)
            {
                // 기존 카드 UI의 데이터만 교체 (GameObject는 재사용)
                existingCardUI.SetCard(newCard);
                
                // UI 강제 업데이트: 이미지 컴포넌트를 명시적으로 활성화하고 업데이트
                var cardUIComponent = existingCardUI as Game.SkillCardSystem.UI.SkillCardUI;
                if (cardUIComponent != null)
                {
                    // 리플렉션으로 cardArtImage 필드에 접근하여 강제 업데이트
                    var cardArtImageField = typeof(Game.SkillCardSystem.UI.SkillCardUI)
                        .GetField("cardArtImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (cardArtImageField != null)
                    {
                        var cardArtImage = cardArtImageField.GetValue(cardUIComponent) as UnityEngine.UI.Image;
                        if (cardArtImage != null)
                        {
                            // 이미지 강제 업데이트: 먼저 null로 설정한 후 새 스프라이트 설정
                            var artwork = newCard.GetArtwork();
                            if (artwork != null)
                            {
                                // 기존 스프라이트를 null로 설정하여 강제로 리프레시
                                cardArtImage.sprite = null;
                                // 한 프레임 대기 후 새 스프라이트 설정 (Unity가 스프라이트 변경을 감지하도록)
                                yield return null;
                                cardArtImage.sprite = artwork;
                                cardArtImage.enabled = true;
                                // 이미지 컴포넌트를 비활성화 후 활성화하여 강제 리프레시
                                cardArtImage.gameObject.SetActive(false);
                                cardArtImage.gameObject.SetActive(true);
                            }
                            else
                            {
                                GameLogger.LogWarning($"[{GetCharacterName()}] 카드 아트워크가 null입니다. 카드 ID: {newCard.CardDefinition?.cardId}, 이름: {newCard.CardDefinition?.displayNameKO ?? newCard.CardDefinition?.displayName}", GameLogger.LogCategory.Character);
                                // artwork가 null이어도 기존 스프라이트를 null로 설정하여 UI를 초기화
                                cardArtImage.sprite = null;
                            }
                        }
                    }
                }
                
                registry.RegisterCard(slotPosition, newCard, existingCardUI, Game.CombatSystem.Data.SlotOwner.ENEMY);
                
                // 배틀 슬롯에 교체한 경우, 적 턴이면 즉시 실행
                if (slotPosition == Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT)
                {
                    // SlotMovementController의 _turnController를 통해 턴 확인
                    var turnControllerField = typeof(Game.CombatSystem.Manager.SlotMovementController)
                        .GetField("_turnController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (turnControllerField != null)
                    {
                        var turnController = turnControllerField.GetValue(slotController) as Game.CombatSystem.Interface.ITurnController;
                        if (turnController != null && turnController.IsEnemyTurn())
                        {
                            // CombatExecutionManager를 통해 즉시 실행
                            if (executionManager != null)
                            {
                                // 시공의 폭풍 카드인지 확인 (특수 기믹 스킬은 무조건 실행되어야 함)
                                bool isStormOfSpaceTimeCard = newCard?.CardDefinition?.IsStormOfSpaceTimeCard() == true;
                                
                                // 현재 상태가 EnemyTurnState인지 확인
                                var stateMachine = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.State.CombatStateMachine>();
                                bool isEnemyTurnState = false;
                                if (stateMachine != null)
                                {
                                    var currentState = stateMachine.GetCurrentState();
                                    isEnemyTurnState = currentState is Game.CombatSystem.State.EnemyTurnState;
                                }
                                
                                // 이미 실행 중인지 확인
                                if (executionManager.IsExecuting)
                                {
                                    if (isStormOfSpaceTimeCard)
                                    {
                                        // 시공의 폭풍 카드는 실행 완료 후 즉시 실행되도록 이벤트 구독
                                        System.Action<Game.CombatSystem.Interface.ExecutionResult> onExecutionCompleted = null;
                                        onExecutionCompleted = (result) =>
                                        {
                                            // 이벤트 구독 해제
                                            executionManager.OnExecutionCompleted -= onExecutionCompleted;
                                            
                                            // 실행 완료 후 시공의 폭풍 카드 즉시 실행
                                            GameLogger.LogInfo($"[{GetCharacterName()}] 실행 완료 후 시공의 폭풍 카드 즉시 실행: {newCard.GetCardName()}", GameLogger.LogCategory.Character);
                                            executionManager.ExecuteCardImmediately(newCard, slotPosition);
                                        };
                                        
                                        executionManager.OnExecutionCompleted += onExecutionCompleted;
                                        GameLogger.LogInfo(
                                            $"[{GetCharacterName()}] 배틀 슬롯에 교체된 시공의 폭풍 카드 ({newCard.GetCardName()})는 다른 카드 실행 완료 후 즉시 실행됩니다.",
                                            GameLogger.LogCategory.Character);
                                    }
                                    else
                                    {
                                        // 일반 카드는 다음 적 턴에 자연스럽게 실행되도록 로그만 남김
                                        GameLogger.LogInfo(
                                            $"[{GetCharacterName()}] 배틀 슬롯에 교체된 카드 ({newCard.GetCardName()})는 이미 다른 카드가 실행 중이어서 다음 적 턴에 실행됩니다.",
                                            GameLogger.LogCategory.Character);
                                    }
                                }
                                else
                                {
                                    // 실행 중이 아니면 즉시 실행
                                    // 적의 턴 중에 시공의 폭풍이 발동한 경우 EnemyTurnState에 알림
                                    if (isStormOfSpaceTimeCard && isEnemyTurnState && stateMachine != null)
                                    {
                                        // EnemyTurnState의 CheckForStormOfSpaceTimeCard를 호출하여 카드 실행을 다시 체크
                                        var enemyTurnState = stateMachine.GetCurrentState() as Game.CombatSystem.State.EnemyTurnState;
                                        if (enemyTurnState != null)
                                        {
                                            // CombatStateContext 가져오기 (리플렉션 사용)
                                            var contextField = typeof(Game.CombatSystem.State.CombatStateMachine)
                                                .GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                            if (contextField != null)
                                            {
                                                var context = contextField.GetValue(stateMachine) as Game.CombatSystem.State.CombatStateContext;
                                                if (context != null)
                                                {
                                                    GameLogger.LogInfo($"[{GetCharacterName()}] 적 턴 중 시공의 폭풍 카드 교체 - EnemyTurnState에 카드 실행 재체크 요청: {newCard.GetCardName()}", GameLogger.LogCategory.Character);
                                                    enemyTurnState.CheckForStormOfSpaceTimeCard(context);
                                                    yield break; // EnemyTurnState가 처리하므로 여기서 종료
                                                }
                                            }
                                        }
                                        
                                        // 리플렉션 실패 시 폴백: 직접 실행
                                        GameLogger.LogInfo($"[{GetCharacterName()}] 적 턴 중 시공의 폭풍 카드 교체 - 즉시 실행 (폴백): {newCard.GetCardName()}", GameLogger.LogCategory.Character);
                                        executionManager.ExecuteCardImmediately(newCard, slotPosition);
                                    }
                                    else
                                    {
                                        // 일반 실행
                                        GameLogger.LogInfo($"[{GetCharacterName()}] 배틀 슬롯에 교체된 카드 즉시 실행: {newCard.GetCardName()}", GameLogger.LogCategory.Character);
                                        executionManager.ExecuteCardImmediately(newCard, slotPosition);
                                    }
                                }
                            }
                            else
                            {
                                GameLogger.LogWarning($"[{GetCharacterName()}] CombatExecutionManager가 null입니다. 카드 자동 실행 불가", GameLogger.LogCategory.Character);
                            }
                        }
                    }
                }
                
                yield break;
            }

            // 기존 카드 UI가 없으면 새로 생성
            // SlotMovementController의 private 메서드에 접근하기 위해 reflection 사용
            var getCachedCardUIPrefabMethod = typeof(Game.CombatSystem.Manager.SlotMovementController)
                .GetMethod("GetCachedCardUIPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (getCachedCardUIPrefabMethod != null)
            {
                var prefabCoroutine = getCachedCardUIPrefabMethod.Invoke(slotController, null) as System.Collections.IEnumerator;
                if (prefabCoroutine != null)
                {
                    yield return prefabCoroutine;
                }
            }

            // 캐시된 프리팹 가져오기
            var cachedPrefabField = typeof(Game.CombatSystem.Manager.SlotMovementController)
                .GetField("_cachedCardUIPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cardUIPrefab = cachedPrefabField?.GetValue(slotController) as Game.SkillCardSystem.UI.SkillCardUI;
            
            if (cardUIPrefab == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] SkillCardUI 프리팹을 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // CreateCardUIForSlot 메서드 호출
            var createCardUIMethod = typeof(Game.CombatSystem.Manager.SlotMovementController)
                .GetMethod("CreateCardUIForSlot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (createCardUIMethod != null)
            {
                var newCardUI = createCardUIMethod.Invoke(slotController, new object[] { newCard, slotPosition, cardUIPrefab }) as Game.SkillCardSystem.UI.SkillCardUI;
                if (newCardUI != null)
                {
                    registry.RegisterCard(slotPosition, newCard, newCardUI, Game.CombatSystem.Data.SlotOwner.ENEMY);
                    GameLogger.LogInfo($"[{GetCharacterName()}] 새 카드 UI 생성 및 교체 완료: {slotPosition}", GameLogger.LogCategory.Character);
                }
            }
            else
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] CreateCardUIForSlot 메서드를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
            }
        }
        
        /// <summary>
        /// StageManager가 null이면 주입을 시도합니다.
        /// </summary>
        private void EnsureStageManagerInjected()
        {
            if (stageManager != null) return;

            try
            {
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    // SceneContext에서 먼저 시도
                    Zenject.DiContainer sceneContainer = null;
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[EnemyCharacter] SceneContextRegistry를 찾을 수 없거나 씬 컨테이너 획득 중 오류: {ex.Message}", GameLogger.LogCategory.Character);
                    }

                    // SceneContext에서 먼저 시도
                    if (sceneContainer != null)
                    {
                        var resolvedManager = sceneContainer.TryResolve<Game.StageSystem.Manager.StageManager>();
                        if (resolvedManager != null)
                        {
                            stageManager = resolvedManager;
                            GameLogger.LogInfo("[EnemyCharacter] StageManager 주입 완료 (SceneContext)", GameLogger.LogCategory.Character);
                            return;
                        }
                    }

                    // ProjectContext에서 시도
                    var projectResolvedManager = projectContext.Container.TryResolve<Game.StageSystem.Manager.StageManager>();
                    if (projectResolvedManager != null)
                    {
                        stageManager = projectResolvedManager;
                        GameLogger.LogInfo("[EnemyCharacter] StageManager 주입 완료 (ProjectContext)", GameLogger.LogCategory.Character);
                        return;
                    }
                }

                var foundManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>(UnityEngine.FindObjectsInactive.Include);
                if (foundManager != null)
                {
                    stageManager = foundManager;
                    GameLogger.LogInfo("[EnemyCharacter] StageManager 직접 찾기 완료", GameLogger.LogCategory.Character);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[EnemyCharacter] StageManager 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// SlotMovementController가 null이면 주입을 시도합니다.
        /// </summary>
        private void EnsureSlotMovementControllerInjected()
        {
            if (slotMovementController != null) return;

            try
            {
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    // SceneContext에서 먼저 시도
                    Zenject.DiContainer sceneContainer = null;
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[EnemyCharacter] SceneContextRegistry를 찾을 수 없거나 씬 컨테이너 획득 중 오류: {ex.Message}", GameLogger.LogCategory.Character);
                    }

                    // SceneContext에서 먼저 시도
                    if (sceneContainer != null)
                    {
                        var resolvedController = sceneContainer.TryResolve<Game.CombatSystem.Interface.ISlotMovementController>();
                        if (resolvedController != null)
                        {
                            slotMovementController = resolvedController;
                            GameLogger.LogInfo("[EnemyCharacter] SlotMovementController 주입 완료 (SceneContext)", GameLogger.LogCategory.Character);
                            return;
                        }
                    }

                    // ProjectContext에서 시도
                    var projectResolvedController = projectContext.Container.TryResolve<Game.CombatSystem.Interface.ISlotMovementController>();
                    if (projectResolvedController != null)
                    {
                        slotMovementController = projectResolvedController;
                        GameLogger.LogInfo("[EnemyCharacter] SlotMovementController 주입 완료 (ProjectContext)", GameLogger.LogCategory.Character);
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[EnemyCharacter] SlotMovementController 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Character);
            }

            GameLogger.LogWarning("[EnemyCharacter] SlotMovementController를 찾을 수 없습니다", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// CombatStateMachine이 null이면 주입을 시도합니다.
        /// </summary>
        private void EnsureCombatStateMachineInjected()
        {
            if (combatStateMachine != null) return;

            try
            {
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    // SceneContext에서 먼저 시도
                    Zenject.DiContainer sceneContainer = null;
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[EnemyCharacter] SceneContextRegistry를 찾을 수 없거나 씬 컨테이너 획득 중 오류: {ex.Message}", GameLogger.LogCategory.Character);
                    }

                    // SceneContext에서 먼저 시도
                    if (sceneContainer != null)
                    {
                        var resolvedStateMachine = sceneContainer.TryResolve<Game.CombatSystem.State.CombatStateMachine>();
                        if (resolvedStateMachine != null)
                        {
                            combatStateMachine = resolvedStateMachine;
                            GameLogger.LogInfo("[EnemyCharacter] CombatStateMachine 주입 완료 (SceneContext)", GameLogger.LogCategory.Character);
                            return;
                        }
                    }

                    // ProjectContext에서 시도
                    var projectResolvedStateMachine = projectContext.Container.TryResolve<Game.CombatSystem.State.CombatStateMachine>();
                    if (projectResolvedStateMachine != null)
                    {
                        combatStateMachine = projectResolvedStateMachine;
                        GameLogger.LogInfo("[EnemyCharacter] CombatStateMachine 주입 완료 (ProjectContext)", GameLogger.LogCategory.Character);
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[EnemyCharacter] CombatStateMachine 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Character);
            }

            // 직접 찾기
            var foundStateMachine = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.State.CombatStateMachine>(UnityEngine.FindObjectsInactive.Include);
            if (foundStateMachine != null)
            {
                combatStateMachine = foundStateMachine;
                GameLogger.LogInfo("[EnemyCharacter] CombatStateMachine 직접 찾기 완료", GameLogger.LogCategory.Character);
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
                else if (effect is Effect.TriggerSkillOnHealthEffectSO skillEffect)
                {
                    skillEffect.OnSkillTriggered -= HandleSkillTriggered;
                    skillEffect.OnSkillDefinitionTriggered -= HandleSkillDefinitionTriggered;
                }
                effect.Cleanup(this);
            }
            characterEffects.Clear();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // DOTween 시퀀스 정리
            if (deathSequence != null && deathSequence.IsActive())
            {
                deathSequence.Kill();
                deathSequence = null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // DOTween 시퀀스 정리
            if (deathSequence != null && deathSequence.IsActive())
            {
                deathSequence.Kill();
                deathSequence = null;
            }
            CleanupEffects();
            UnsubscribeFromExecutionEvents();
            UnsubscribeFromCardExecutedEvents();
        }

        /// <summary>
        /// 피격 시각 효과를 적용할 비주얼 루트를 반환합니다 (Portrait 기준으로 한정)
        /// </summary>
        protected override Transform GetHitVisualRoot()
        {
            if (portraitImage != null) return portraitImage.transform;
            var portrait = transform.Find("Portrait");
            return portrait != null ? portrait : transform;
        }

        /// <summary>
        /// 사망 연출(VFX/애니메이션)을 재생하고 완료 후 사망 처리 콜백을 호출합니다.
        /// </summary>
        private void PlayDeathPresentation()
        {
            // 기존 시퀀스가 있으면 정리
            if (deathSequence != null && deathSequence.IsActive())
            {
                deathSequence.Kill();
                deathSequence = null;
            }

            // 데미지 텍스트 정리
            ClearDamageTexts();

            // Animator가 있으면 "Die" 트리거 우선 사용
            bool animatorPlayed = false;
            if (enemyAnimator != null)
            {
                try
                {
                    enemyAnimator.SetTrigger("Die");
                    animatorPlayed = true;
                    GameLogger.LogDebug($"[{GetCharacterName()}] 적 사망 애니메이션 트리거 재생", GameLogger.LogCategory.Character);
                }
                catch (Exception ex)
                {
                    GameLogger.LogWarning($"[{GetCharacterName()}] 사망 애니메이션 트리거 실패: {ex.Message}", GameLogger.LogCategory.Character);
                }
            }

            // 비주얼 루트 기준으로 페이드/스케일 아웃 시퀀스
            Transform visualRoot = GetHitVisualRoot();
            var target = visualRoot != null ? visualRoot : transform;

            // CanvasGroup이 있으면 알파 페이드 사용, 없으면 스케일만 사용
            CanvasGroup cg = target.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = target.gameObject.AddComponent<CanvasGroup>();
            }

            // 시퀀스 구성
            deathSequence = DOTween.Sequence()
                // 약간의 준비 동작: 미세한 스케일 업으로 타격 여운
                .Append(target.DOScale(1.04f, 0.10f).SetEase(Ease.OutQuad))
                .AppendInterval(animatorPlayed ? 0.15f : 0.08f)
                // 페이드 아웃과 동시에 오른쪽(화면 바깥쪽)으로 밀려나며 퇴장
                .Append(cg.DOFade(0f, 0.40f).SetEase(Ease.InQuad))
                .Join(target.DOLocalMoveX(target.localPosition.x + 1.2f, 0.40f).SetEase(Ease.InQuad))
                // 퇴장 중 살짝 스케일 다운으로 원근감 부여
                .Join(target.DOScale(0.96f, 0.40f).SetEase(Ease.InQuad))
                .SetUpdate(false)
                .SetAutoKill(true)
                .OnComplete(() =>
                {
                    deathSequence = null;
                    HandleDeathPresentationComplete();
                });
        }

        /// <summary>
        /// 사망 연출 완료 핸들러: 사망 관련 이벤트 발행 및 외부 콜백 호출
        /// </summary>
        private void HandleDeathPresentationComplete()
        {
            // 핸드 카드 소멸 트리거 (적)
            CombatEvents.RaiseHandSkillCardsVanishOnCharacterDeath(false);
            // 적 사망 이벤트 발행
            CombatEvents.RaiseEnemyCharacterDeath(CharacterData, this.gameObject);

            GameLogger.LogDebug($"[EnemyCharacter] '{GetCharacterName()}' 사망 연출 완료 → 콜백 호출", GameLogger.LogCategory.Character);
            onDeathCallback?.Invoke(this);
        }

        #endregion

        #region 페이즈 시스템 구현

        /// <summary>
        /// 페이즈 시스템을 초기화합니다.
        /// 기본 정보 = 1페이즈 (currentPhaseIndex = -1)
        /// Phases[0] = 2페이즈, Phases[1] = 3페이즈, ...
        /// 항상 기본 정보(1페이즈)로 시작합니다.
        /// </summary>
        private void InitializePhases()
        {
            if (CharacterData == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] CharacterData가 null입니다 - 페이즈 시스템 초기화 실패", GameLogger.LogCategory.Character);
                currentPhaseIndex = -1;
                return;
            }

            if (!CharacterData.HasPhases)
            {
                GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 시스템 없음 - 기본 정보만 사용", GameLogger.LogCategory.Character);
                currentPhaseIndex = -1;
                return;
            }

            // 항상 기본 정보(1페이즈)로 시작
            // currentPhaseIndex = -1: 기본 정보 사용
            currentPhaseIndex = -1;
            
            // 페이즈별 정보 캐시 초기화 (기본 정보 사용)
            cachedPhaseDisplayName = null;
            cachedPhaseIndexIcon = null;
            cachedPhasePortraitPrefab = null;

            int currentHP = GetCurrentHP();
            int maxHP = GetMaxHP();
            float currentHealthRatio = maxHP > 0 ? (float)currentHP / maxHP : 1.0f;

            // 페이즈 정보 로그
            string phaseInfo = $"페이즈 수: {CharacterData.Phases.Count}";
            for (int i = 0; i < CharacterData.Phases.Count; i++)
            {
                var phase = CharacterData.Phases[i];
                phaseInfo += $", Phases[{i}]: {phase.phaseName} (임계값: {phase.healthThreshold:P0})";
            }

            // 페이즈 초기화 완료
        }

        /// <summary>
        /// 카드 실행 완료 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeToExecutionEvents()
        {
            if (executionManager == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] ICombatExecutionManager가 null입니다 - 페이즈 전환 체크 불가", GameLogger.LogCategory.Character);
                return;
            }

            executionManager.OnExecutionCompleted += OnCardExecutionCompleted;
        }

        /// <summary>
        /// 카드 실행 완료 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeFromExecutionEvents()
        {
            if (executionManager != null)
            {
                executionManager.OnExecutionCompleted -= OnCardExecutionCompleted;
            }
        }

        /// <summary>
        /// 카드 실행 이벤트를 구독합니다 (시공의 폭풍 카드 추적용).
        /// </summary>
        private void SubscribeToCardExecutedEvents()
        {
            if (executionManager == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] ICombatExecutionManager가 null입니다 - 시공의 폭풍 카드 추적 불가", GameLogger.LogCategory.Character);
                return;
            }

            executionManager.OnCardExecuted += OnCardExecuted;
        }

        /// <summary>
        /// 카드 실행 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeFromCardExecutedEvents()
        {
            if (executionManager != null)
            {
                executionManager.OnCardExecuted -= OnCardExecuted;
            }
        }

        /// <summary>
        /// 카드 실행 시 호출되는 콜백입니다 (시공의 폭풍 카드 추적용).
        /// 주의: 실행 횟수 증가는 StormOfSpaceTimeEffectCommand에서 처리하므로, 여기서는 로그만 남깁니다.
        /// </summary>
        private void OnCardExecuted(ISkillCard card, ICharacter source, ICharacter target)
        {
            // 적이 사용한 카드인지 확인
            if (!ReferenceEquals(source, this) || card == null)
            {
                return;
            }

            // 시공의 폭풍 카드인지 확인 (효과 기반)
            if (card.CardDefinition != null && card.CardDefinition.IsStormOfSpaceTimeCard())
            {
                // 실행 횟수는 StormOfSpaceTimeEffectCommand에서 이미 증가했으므로, 여기서는 현재 값만 로그
                GameLogger.LogInfo($"[{GetCharacterName()}] 시공의 폭풍 카드 실행 감지 (현재 카운트: {stormOfSpaceTimeCardExecutionCount})", GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 시공의 폭풍 카드를 트리거하여 강제로 생성하고 배치합니다.
        /// </summary>
        private System.Collections.IEnumerator TriggerStormOfSpaceTimeCardCoroutine()
        {
            GameLogger.LogInfo($"[{GetCharacterName()}] 시공의 폭풍 카드 트리거 시작", GameLogger.LogCategory.Character);

            // SkillCardFactory 찾기
            if (cardFactory == null)
            {
                // Zenject를 통해 resolve 시도
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null)
                {
                    try
                    {
                        cardFactory = projectContext.Container.Resolve<Game.SkillCardSystem.Interface.ISkillCardFactory>();
                    }
                    catch (System.Exception e)
                    {
                        GameLogger.LogWarning($"[{GetCharacterName()}] SkillCardFactory resolve 실패: {e.Message}", GameLogger.LogCategory.Character);
                    }
                }
            }

            if (cardFactory == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SkillCardFactory를 찾을 수 없습니다. 시공의 폭풍 카드를 생성할 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // SlotMovementController 찾기
            if (slotMovementController == null)
            {
                EnsureSlotMovementControllerInjected();
            }

            if (slotMovementController == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] SlotMovementController를 찾을 수 없습니다. 시공의 폭풍 카드를 배치할 수 없습니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // 시공의 폭풍 카드 생성 (효과 기반)
            Game.SkillCardSystem.Interface.ISkillCard stormCard = null;
            try
            {
                // 덱에서 시공의 폭풍 카드 정의 찾기 (효과 기반)
                if (skillDeck != null)
                {
                    var allCards = skillDeck.GetAllCards();
                    foreach (var entry in allCards)
                    {
                        if (entry?.definition != null && entry.definition.IsStormOfSpaceTimeCard())
                        {
                            stormCard = cardFactory.CreateEnemyCard(entry.definition, GetCharacterName());
                            GameLogger.LogInfo($"[{GetCharacterName()}] 시공의 폭풍 카드 생성 완료 (덱에서 찾음)", GameLogger.LogCategory.Character);
                            break;
                        }
                    }
                }
                
                if (stormCard == null)
                {
                    GameLogger.LogError($"[{GetCharacterName()}] 시공의 폭풍 카드를 덱에서 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                    yield break;
                }
            }
            catch (System.Exception e)
            {
                GameLogger.LogError($"[{GetCharacterName()}] 시공의 폭풍 카드 생성 실패: {e.Message}", GameLogger.LogCategory.Character);
                yield break;
            }

            if (stormCard == null)
            {
                GameLogger.LogError($"[{GetCharacterName()}] 시공의 폭풍 카드가 null입니다.", GameLogger.LogCategory.Character);
                yield break;
            }

            // 카드를 WAIT_SLOT_4에 배치
            // SlotMovementController의 PlaceCardInWaitSlot4AndMoveRoutine 사용
            var slotController = slotMovementController as Game.CombatSystem.Manager.SlotMovementController;
            if (slotController != null)
            {
                // 프리팹은 SlotMovementController 내부에서 처리
                yield return slotController.PlaceCardInWaitSlot4AndMoveRoutine(
                    stormCard, 
                    Game.CombatSystem.Data.SlotOwner.ENEMY, 
                    null); // 프리팹은 SlotMovementController에서 자동으로 로드
                
                GameLogger.LogInfo($"[{GetCharacterName()}] 시공의 폭풍 카드 배치 완료", GameLogger.LogCategory.Character);
            }
            else
            {
                GameLogger.LogError($"[{GetCharacterName()}] SlotMovementController를 캐스팅할 수 없습니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 카드 실행 완료 시 호출되는 콜백입니다.
        /// 페이즈 전환을 체크합니다.
        /// </summary>
        private void OnCardExecutionCompleted(Game.CombatSystem.Interface.ExecutionResult result)
        {
            if (isPhaseTransitionPending)
            {
                GameLogger.LogDebug($"[{GetCharacterName()}] 페이즈 전환 대기 중이므로 체크 스킵", GameLogger.LogCategory.Character);
                return;
            }

            if (isDead)
            {
                GameLogger.LogDebug($"[{GetCharacterName()}] 캐릭터가 죽어있으므로 페이즈 전환 체크 스킵", GameLogger.LogCategory.Character);
                return;
            }

            if (CharacterData == null || !CharacterData.HasPhases)
            {
                GameLogger.LogDebug($"[{GetCharacterName()}] 페이즈 시스템이 없으므로 체크 스킵", GameLogger.LogCategory.Character);
                return;
            }

            int currentHP = GetCurrentHP();
            int maxHP = GetMaxHP();
            if (maxHP <= 0)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] MaxHP가 0 이하이므로 페이즈 전환 체크 스킵", GameLogger.LogCategory.Character);
                return;
            }

            GameLogger.LogDebug($"[{GetCharacterName()}] 카드 실행 완료 - 페이즈 전환 체크 시작 (체력: {currentHP}/{maxHP})", GameLogger.LogCategory.Character);
            
            // 페이즈 전환 체크 (이제는 NotifyHealthChanged에서 처리하므로 여기서는 스킵)
            // CheckPhaseTransition(currentHP, maxHP);
        }

        /// <summary>
        /// 체력 변경 후 약간의 지연을 두고 페이즈 전환을 체크합니다.
        /// 카드 실행 완료 후 VFX/데미지 효과가 완료된 후에 체크하도록 합니다.
        /// </summary>
        /// <param name="currentHP">현재 체력</param>
        /// <param name="maxHP">최대 체력</param>
        private System.Collections.IEnumerator CheckPhaseTransitionDelayed(int currentHP, int maxHP)
        {
            // 카드 실행 완료 후 약간의 지연 (VFX/데미지 효과 완료 대기)
            yield return new WaitForSeconds(0.5f);
            
            // 다시 한 번 체크 (이미 전환 중이거나 죽었으면 스킵)
            if (isDead || CharacterData == null || !CharacterData.HasPhases)
            {
                // isPhaseTransitionPending이 false면 플래그를 해제하고 종료
                if (isPhaseTransitionPending)
                {
                    GameLogger.LogWarning($"[{GetCharacterName()}] 페이즈 전환 플래그가 설정되어 있지만 전환 조건이 만족되지 않음 - 플래그 해제", GameLogger.LogCategory.Character);
                    isPhaseTransitionPending = false;
                }
                yield break;
            }
            
            // isPhaseTransitionPending이 true면 이미 NotifyHealthChanged에서 조건을 확인했으므로 실제 전환 시작
            if (isPhaseTransitionPending)
            {
                // 현재 체력이 변경되었을 수 있으므로 다시 가져옴
                int actualCurrentHP = GetCurrentHP();
                int actualMaxHP = GetMaxHP();
                
                GameLogger.LogInfo($"[{GetCharacterName()}] 지연된 페이즈 전환 시작 (체력: {actualCurrentHP}/{actualMaxHP})", GameLogger.LogCategory.Character);
                
                // 페이즈 인덱스 찾기 (CheckPhaseTransition과 동일한 로직)
                int startIndex = currentPhaseIndex < 0 ? 0 : currentPhaseIndex + 1;
                int targetPhaseIndex = -1;
                
                for (int i = startIndex; i < CharacterData.Phases.Count; i++)
                {
                    var phase = CharacterData.Phases[i];
                    if (phase != null && phase.IsThresholdReached(actualCurrentHP, actualMaxHP))
                    {
                        targetPhaseIndex = i;
                        GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환 대상: {phase.phaseName} (인덱스: {i})", GameLogger.LogCategory.Character);
                        break;
                    }
                }
                
                // 페이즈 전환 대상이 있으면 직접 코루틴 시작 (StartPhaseTransition은 플래그 체크 때문에 사용 불가)
                if (targetPhaseIndex >= 0)
                {
                    GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환 코루틴 직접 시작: 페이즈 인덱스 {targetPhaseIndex}", GameLogger.LogCategory.Character);
                    StartCoroutine(TransitionToPhaseCoroutine(targetPhaseIndex));
                }
                else
                {
                    GameLogger.LogWarning($"[{GetCharacterName()}] 페이즈 전환 플래그가 설정되어 있지만 전환 대상 페이즈를 찾을 수 없음 - 플래그 해제", GameLogger.LogCategory.Character);
                    isPhaseTransitionPending = false;
                }
            }
            else
            {
                // 플래그가 설정되지 않았으면 조건 재확인 (안전장치)
                int actualCurrentHP = GetCurrentHP();
                int actualMaxHP = GetMaxHP();
                
                if (ShouldTransitionPhase())
                {
                    GameLogger.LogInfo($"[{GetCharacterName()}] 지연된 페이즈 전환 체크에서 조건 만족 - 전환 시작 (체력: {actualCurrentHP}/{actualMaxHP})", GameLogger.LogCategory.Character);
                    CheckPhaseTransition(actualCurrentHP, actualMaxHP);
                }
            }
        }

        /// <summary>
        /// 페이즈 전환이 진행 중인지 확인합니다 (외부에서 호출 가능).
        /// </summary>
        /// <returns>페이즈 전환이 진행 중이면 true</returns>
        public bool IsPhaseTransitionPending()
        {
            return isPhaseTransitionPending;
        }

        /// <summary>
        /// 페이즈 전환이 필요한지 확인합니다 (외부에서 호출 가능).
        /// </summary>
        /// <returns>페이즈 전환이 필요하면 true</returns>
        public bool ShouldTransitionPhase()
        {
            if (isPhaseTransitionPending || isDead || CharacterData == null || !CharacterData.HasPhases)
                return false;

            int currentHP = GetCurrentHP();
            int maxHP = GetMaxHP();
            if (maxHP <= 0)
                return false;

            // 현재 페이즈 인덱스 확인
            int startIndex = (currentPhaseIndex < 0) ? 0 : (currentPhaseIndex + 1);

            // 다음 페이즈들 체크
            for (int i = startIndex; i < CharacterData.Phases.Count; i++)
            {
                var phase = CharacterData.Phases[i];
                if (phase == null)
                    continue;

                float currentHealthRatio = (float)currentHP / maxHP;
                bool thresholdReached = phase.IsThresholdReached(currentHP, maxHP);

                if (thresholdReached)
                {
                    GameLogger.LogDebug($"[{GetCharacterName()}] 페이즈 전환 필요 확인: {phase.phaseName} (체력: {currentHP}/{maxHP}, 비율: {currentHealthRatio:P2} <= {phase.healthThreshold:P2})", GameLogger.LogCategory.Character);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 체력 변경에 따라 페이즈 전환을 체크합니다.
        /// 기본 정보(1페이즈)에서 Phases[0](2페이즈)로, Phases[0]에서 Phases[1](3페이즈)로 전환됩니다.
        /// </summary>
        /// <param name="currentHP">현재 체력</param>
        /// <param name="maxHP">최대 체력</param>
        private void CheckPhaseTransition(int currentHP, int maxHP)
        {
            if (CharacterData == null || !CharacterData.HasPhases)
            {
                GameLogger.LogDebug($"[{GetCharacterName()}] 페이즈 전환 체크 스킵: 데이터 없음", GameLogger.LogCategory.Character);
                return;
            }

            if (maxHP <= 0)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] 페이즈 전환 체크 스킵: MaxHP가 0 이하", GameLogger.LogCategory.Character);
                return;
            }

            // 시작 인덱스 결정
            // currentPhaseIndex = -1 (기본 정보/1페이즈): Phases[0]부터 체크
            // currentPhaseIndex >= 0 (Phases 리스트의 페이즈): 다음 인덱스부터 체크
            int startIndex = currentPhaseIndex < 0 ? 0 : currentPhaseIndex + 1;
            float currentHealthRatio = (float)currentHP / maxHP;

            GameLogger.LogDebug($"[{GetCharacterName()}] 페이즈 전환 체크: 현재 페이즈 인덱스={currentPhaseIndex}, 체력={currentHP}/{maxHP} ({currentHealthRatio:P2}), 시작 인덱스={startIndex}", GameLogger.LogCategory.Character);

            // 현재 페이즈보다 높은 인덱스의 페이즈만 체크 (리스트 순서대로 진행)
            for (int i = startIndex; i < CharacterData.Phases.Count; i++)
            {
                var phase = CharacterData.Phases[i];
                
                if (phase == null)
                {
                    GameLogger.LogWarning($"[{GetCharacterName()}] 페이즈 {i}가 null입니다", GameLogger.LogCategory.Character);
                    continue;
                }

                bool thresholdReached = phase.IsThresholdReached(currentHP, maxHP);
                GameLogger.LogDebug($"[{GetCharacterName()}] 페이즈 {i} ({phase.phaseName}) 체크: 임계값={phase.healthThreshold:P2}, 현재 비율={currentHealthRatio:P2}, 도달={thresholdReached}", GameLogger.LogCategory.Character);
                
                // 임계값에 도달했으면 다음 페이즈로 전환
                if (thresholdReached)
                {
                    GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환 조건 만족: {phase.phaseName} (체력: {currentHP}/{maxHP}, 비율: {currentHealthRatio:P2} <= {phase.healthThreshold:P2})", GameLogger.LogCategory.Character);
                    StartPhaseTransition(i);
                    break; // 한 번에 하나의 페이즈만 전환
                }
            }
        }

        /// <summary>
        /// 페이즈 전환을 시작합니다 (코루틴으로 처리).
        /// </summary>
        /// <param name="phaseIndex">전환할 페이즈 인덱스</param>
        private void StartPhaseTransition(int phaseIndex)
        {
            // 이미 전환 중이면 중복 시작 방지
            if (isPhaseTransitionPending)
            {
                GameLogger.LogDebug($"[{GetCharacterName()}] 페이즈 전환이 이미 진행 중 - 중복 시작 방지", GameLogger.LogCategory.Character);
                return;
            }

            // 플래그 설정 (NotifyHealthChanged에서 이미 설정했을 수 있지만, 안전을 위해 여기서도 설정)
            isPhaseTransitionPending = true;
            GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환 코루틴 시작: 페이즈 인덱스 {phaseIndex}", GameLogger.LogCategory.Character);
            StartCoroutine(TransitionToPhaseCoroutine(phaseIndex));
        }

        /// <summary>
        /// 슬롯 이동이 완료될 때까지 대기합니다.
        /// </summary>
        private System.Collections.IEnumerator WaitForSlotMovementToComplete()
        {
            if (slotMovementController == null)
            {
                yield break;
            }

            // SlotMovementController의 IsAdvancingQueue가 false가 될 때까지 대기
            int maxWaitFrames = 300; // 최대 5초 (60fps 기준)
            int waitFrames = 0;

            while (slotMovementController.IsAdvancingQueue && waitFrames < maxWaitFrames)
            {
                yield return null;
                waitFrames++;
            }

            if (waitFrames >= maxWaitFrames)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] 슬롯 이동 대기 시간 초과", GameLogger.LogCategory.Character);
            }
            else if (waitFrames > 0)
            {
                GameLogger.LogDebug($"[{GetCharacterName()}] 슬롯 이동 완료 대기 완료 ({waitFrames}프레임)", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 페이즈 전환 코루틴입니다.
        /// </summary>
        private System.Collections.IEnumerator TransitionToPhaseCoroutine(int phaseIndex)
        {
            if (CharacterData == null || !CharacterData.HasPhases)
            {
                isPhaseTransitionPending = false;
                yield break;
            }

            if (phaseIndex < 0 || phaseIndex >= CharacterData.Phases.Count)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] 잘못된 페이즈 인덱스: {phaseIndex}", GameLogger.LogCategory.Character);
                isPhaseTransitionPending = false;
                yield break;
            }

            // 이미 해당 페이즈에 있으면 스킵
            if (phaseIndex == currentPhaseIndex)
            {
                isPhaseTransitionPending = false;
                yield break;
            }

            var newPhase = CharacterData.Phases[phaseIndex];
            int previousPhaseIndex = currentPhaseIndex;
            string previousPhaseName;
            
            // 이전 페이즈 이름 결정
            if (previousPhaseIndex < 0)
            {
                // 기본 정보(1페이즈)
                previousPhaseName = "1페이즈";
            }
            else if (previousPhaseIndex < CharacterData.Phases.Count)
            {
                // Phases 리스트의 페이즈
                previousPhaseName = CharacterData.Phases[previousPhaseIndex].phaseName;
            }
            else
            {
                previousPhaseName = "기본";
            }

            GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환 시작: {previousPhaseName} → {newPhase.phaseName} (체력: {GetCurrentHP()}/{GetMaxHP()})", GameLogger.LogCategory.Character);

            // 페이즈 전환 시작 시 즉시 모든 배치/대기 슬롯 제거
            if (slotRegistry != null)
            {
                slotRegistry.ClearAllSlots();
                GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환: 모든 배치/대기 슬롯 제거 완료", GameLogger.LogCategory.Character);
            }
            else
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] ICardSlotRegistry를 찾을 수 없습니다 - 슬롯 제거 건너뜀", GameLogger.LogCategory.Character);
            }

            // 페이즈 전환 중 자동 보충 억제 (RefillWaitSlot4IfNeededRoutine이 호출되지 않도록)
            EnsureSlotMovementControllerInjected();
            if (slotMovementController != null)
            {
                var slotController = slotMovementController as Game.CombatSystem.Manager.SlotMovementController;
                if (slotController != null)
                {
                    // 리플렉션을 사용하여 _suppressAutoRefill 필드에 접근
                    var suppressField = typeof(Game.CombatSystem.Manager.SlotMovementController).GetField("_suppressAutoRefill",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (suppressField != null)
                    {
                        suppressField.SetValue(slotController, true);
                        GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환: 자동 보충 억제 활성화", GameLogger.LogCategory.Character);
                    }
                }
            }

            // 상태 머신이 안전한 상태가 될 때까지 대기 (가장 중요)
            EnsureCombatStateMachineInjected();
            if (combatStateMachine != null)
            {
                var currentState = combatStateMachine.GetCurrentState();
                GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환: 상태 머신 안전 상태 대기 시작 (현재 상태: {currentState?.StateName ?? "null"})", GameLogger.LogCategory.Character);
                
                // 적 턴 또는 플레이어 턴 중에 페이즈 전환이 발생한 경우, 안전한 상태로 전환
                if (currentState is Game.CombatSystem.State.EnemyTurnState || 
                    currentState is Game.CombatSystem.State.PlayerTurnState)
                {
                    string stateType = currentState is Game.CombatSystem.State.EnemyTurnState ? "적 턴" : "플레이어 턴";
                    GameLogger.LogInfo($"[{GetCharacterName()}] {stateType} 중 페이즈 전환 감지 - 상태 종료 및 안전한 상태로 전환", GameLogger.LogCategory.Character);
                    
                    // 현재 턴을 종료하고 SlotMovingState로 전환 (안전한 상태)
                    // 이렇게 하면 페이즈 전환이 완료될 때까지 안전하게 대기할 수 있음
                    var slotMovingState = new Game.CombatSystem.State.SlotMovingState();
                    combatStateMachine.ChangeState(slotMovingState);
                    
                    // SlotMovingState가 완료될 때까지 대기
                    yield return new WaitForSeconds(0.1f);
                }
                
                // 안전한 상태가 될 때까지 대기
                yield return combatStateMachine.WaitForSafeStateForPhaseTransition();
                GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환: 상태 머신 안전 상태 도달 (현재 상태: {combatStateMachine.GetCurrentState()?.StateName ?? "null"})", GameLogger.LogCategory.Character);
            }
            else
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] CombatStateMachine를 찾을 수 없습니다 - 슬롯 이동 대기로 대체", GameLogger.LogCategory.Character);
                // 상태 머신이 없으면 슬롯 이동 대기로 대체
                yield return StartCoroutine(WaitForSlotMovementToComplete());
            }

            // 슬롯 이동이 완료될 때까지 대기 (타이밍 충돌 방지)
            yield return StartCoroutine(WaitForSlotMovementToComplete());

            // 페이즈 전환 연출 재생 (완료까지 대기)
            yield return StartCoroutine(PlayPhaseTransitionEffectsCoroutine(newPhase));

            // 페이즈 설정 적용 (카드 교체 제외)
            currentPhaseIndex = phaseIndex;
            ApplyPhaseSettings(newPhase, true, skipCardRegeneration: true);

            // 슬롯 재생성 전에 상태 머신이 안전한 상태인지 다시 확인
            EnsureCombatStateMachineInjected();
            if (combatStateMachine != null)
            {
                var stateBeforeSetup = combatStateMachine.GetCurrentState();
                // 플레이어 턴이나 적 턴 상태이면 SlotMovingState로 전환하여 슬롯 생성이 안전하게 진행되도록 함
                if (stateBeforeSetup is Game.CombatSystem.State.PlayerTurnState || 
                    stateBeforeSetup is Game.CombatSystem.State.EnemyTurnState)
                {
                    GameLogger.LogInfo($"[{GetCharacterName()}] 슬롯 재생성 전 상태 전환: {stateBeforeSetup.StateName} → SlotMovingState", GameLogger.LogCategory.Character);
                    var slotMovingState = new Game.CombatSystem.State.SlotMovingState();
                    combatStateMachine.ChangeState(slotMovingState);
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // 새로운 적 생성처럼 자연스럽게 슬롯 재생성 (SetupInitialEnemyQueueRoutine 사용)
            yield return StartCoroutine(SetupSlotsLikeNewEnemyCoroutine());

            // 페이즈 전환 완료 후 자동 보충 억제 해제
            if (slotMovementController != null)
            {
                var slotController = slotMovementController as Game.CombatSystem.Manager.SlotMovementController;
                if (slotController != null)
                {
                    // 리플렉션을 사용하여 _suppressAutoRefill 필드에 접근
                    var suppressField = typeof(Game.CombatSystem.Manager.SlotMovementController).GetField("_suppressAutoRefill",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (suppressField != null)
                    {
                        suppressField.SetValue(slotController, false);
                        GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환: 자동 보충 억제 해제", GameLogger.LogCategory.Character);
                    }
                }
            }

            GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환 완료: {newPhase.phaseName}", GameLogger.LogCategory.Character);

            isPhaseTransitionPending = false;

            // 페이즈 전환 완료 후 플레이어 턴으로 자동 전환 (초기 셋업처럼 플레이어 선턴)
            // CombatInitState와 동일한 방식으로 처리
            EnsureCombatStateMachineInjected();
            if (combatStateMachine != null)
            {
                var currentState = combatStateMachine.GetCurrentState();
                // 안전한 상태이면 플레이어 턴으로 전환
                if (combatStateMachine.IsInSafeStateForPhaseTransition())
                {
                    // 배틀 슬롯에 플레이어 마커가 있는지 확인 (초기 셋업처럼 플레이어 선턴)
                    if (slotRegistry != null)
                    {
                        var battleCard = slotRegistry.GetCardInSlot(Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT);
                        if (battleCard != null && battleCard.IsFromPlayer())
                        {
                            GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환 완료 후 플레이어 턴으로 자동 전환 시작 (플레이어 선턴)", GameLogger.LogCategory.Character);
                            // 플레이어 턴으로 전환 (CombatInitState와 동일한 방식)
                            var playerTurnState = new Game.CombatSystem.State.PlayerTurnState();
                            combatStateMachine.ChangeState(playerTurnState);
                            yield break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 페이즈 설정을 적용합니다.
        /// </summary>
        /// <param name="phase">적용할 페이즈 데이터</param>
        /// <param name="isTransition">전환인지 초기화인지 여부</param>
        /// <param name="skipCardRegeneration">카드 재생성 스킵 여부 (연출 후 별도 처리용)</param>
        private void ApplyPhaseSettings(EnemyPhaseData phase, bool isTransition, bool skipCardRegeneration = false)
        {
            if (phase == null) return;

            // 1. 페이즈별 기본 정보 캐시
            CachePhaseBasicInfo(phase);

            // 2. 버프/디버프 모두 제거 (전환 시에만)
            if (isTransition)
            {
                ClearAllBuffsAndDebuffs();
            }

            // 3. 최대 체력 변경 (phaseMaxHP가 0이 아니면)
            if (phase.phaseMaxHP > 0)
            {
                int oldMaxHP = GetMaxHP();
                SetMaxHP(phase.phaseMaxHP);
                
                // 현재 체력을 새 최대 체력으로 회복
                SetCurrentHP(phase.phaseMaxHP);
                
                // 페이즈 전환 시 체력 히스토리 초기화 (시간 역행 효과가 페이즈 전환 전 체력으로 되돌리지 않도록)
                ResetHPHistoryForPhaseTransition();
                
                GameLogger.LogInfo($"[{GetCharacterName()}] 최대 체력 변경: {oldMaxHP} → {phase.phaseMaxHP}, 현재 체력 회복: {phase.phaseMaxHP}/{phase.phaseMaxHP}", GameLogger.LogCategory.Character);
            }

            // 4. 덱 교체
            if (phase.phaseDeck != null)
            {
                skillDeck = phase.phaseDeck;
                GameLogger.LogInfo($"[{GetCharacterName()}] 덱 교체: {phase.phaseDeck.name}", GameLogger.LogCategory.Character);

                // 덱 스택 초기화
                InitializeEnemyDeckStacks();
            }

            // 5. 페이즈 전용 이펙트 적용 (전환 시에만)
            if (isTransition)
            {
                ApplyPhaseEffects(phase);
            }

            // 6. 페이즈별 Portrait 프리팹 교체 (전환 시에만)
            if (isTransition && phase.phasePortraitPrefab != null)
            {
                ApplyPhasePortraitPrefab(phase.phasePortraitPrefab);
            }

            // 7. 슬롯의 적 카드 제거 후 새 카드 생성 (전환 시에만, skipCardRegeneration이 false일 때만)
            // skipCardRegeneration이 true면 TransitionToPhaseCoroutine에서 별도로 처리
            if (isTransition && !skipCardRegeneration)
            {
                StartCoroutine(ClearEnemyCardsAndRegenerateCoroutine());
            }

            // UI 갱신
            RefreshUI();
        }

        /// <summary>
        /// 페이즈별 기본 정보를 캐시합니다.
        /// </summary>
        /// <param name="phase">페이즈 데이터</param>
        private void CachePhaseBasicInfo(EnemyPhaseData phase)
        {
            if (phase == null) return;

            // DisplayName 캐시 (null이면 기본값 사용)
            cachedPhaseDisplayName = !string.IsNullOrEmpty(phase.phaseDisplayName) 
                ? phase.phaseDisplayName 
                : null;

            // IndexIcon 캐시
            cachedPhaseIndexIcon = phase.phaseIndexIcon;

            // PortraitPrefab 캐시
            cachedPhasePortraitPrefab = phase.phasePortraitPrefab;

            GameLogger.LogDebug($"[{GetCharacterName()}] 페이즈 기본 정보 캐시: DisplayName={cachedPhaseDisplayName ?? "기본"}, IndexIcon={cachedPhaseIndexIcon != null}, PortraitPrefab={cachedPhasePortraitPrefab != null}", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 페이즈별 Portrait 프리팹을 적용합니다.
        /// </summary>
        /// <param name="portraitPrefab">적용할 Portrait 프리팹</param>
        private void ApplyPhasePortraitPrefab(GameObject portraitPrefab)
        {
            if (portraitPrefab == null) return;

            // 기존 Portrait 제거
            var existingPortrait = transform.Find("Portrait");
            if (existingPortrait != null)
            {
                Destroy(existingPortrait.gameObject);
            }

            // Portrait 부모 Transform 찾기
            Transform parent = portraitParent;
            if (parent == null)
            {
                parent = transform;
            }

            // 새 Portrait 프리팹 인스턴스화
            GameObject portraitInstance = Instantiate(portraitPrefab, parent);
            portraitInstance.name = "Portrait";

            // Portrait Image 컴포넌트 찾기
            portraitImage = portraitInstance.GetComponentInChildren<Image>(true);
            if (portraitImage == null)
            {
                GameLogger.LogWarning("[EnemyCharacter] 페이즈 Portrait 프리팹에서 Image 컴포넌트를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
            }

            // HP Text Anchor 찾기 (페이즈 전환 시 항상 다시 찾기)
            var hpAnchor = portraitInstance.transform.Find("HPTectAnchor");
            if (hpAnchor == null)
            {
                hpAnchor = portraitInstance.transform.Find("HPTextAnchor");
            }
            if (hpAnchor != null)
            {
                hpTextAnchor = hpAnchor;
                GameLogger.LogDebug($"[{GetCharacterName()}] HP Text Anchor 재찾기 완료: {hpAnchor.name}", GameLogger.LogCategory.Character);
            }
            else
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] HP Text Anchor를 찾을 수 없습니다 (데미지 텍스트가 표시되지 않을 수 있음)", GameLogger.LogCategory.Character);
            }

            GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 Portrait 프리팹 교체: {portraitPrefab.name}", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 모든 버프/디버프를 제거합니다.
        /// </summary>
        private void ClearAllBuffsAndDebuffs()
        {
            // CharacterBase의 perTurnEffects 제거
            perTurnEffects.Clear();
            NotifyBuffsChanged();

            GameLogger.LogInfo($"[{GetCharacterName()}] 모든 버프/디버프 제거", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 페이즈 전용 이펙트를 적용합니다.
        /// </summary>
        /// <param name="phase">적용할 페이즈 데이터</param>
        private void ApplyPhaseEffects(EnemyPhaseData phase)
        {
            if (phase == null || phase.phaseEffects == null || phase.phaseEffects.Count == 0)
                return;

            // 기존 이펙트 정리
            CleanupEffects();
            characterEffects.Clear();

            foreach (var effectEntry in phase.phaseEffects)
            {
                if (effectEntry.effectSO == null) continue;

                // 새 이펙트 추가
                characterEffects.Add(effectEntry.effectSO);

                // 커스텀 설정 사용 여부에 따라 초기화
                if (effectEntry.useCustomSettings)
                {
                    if (effectEntry.effectSO is Effect.SummonEffectSO summonEffectWithCustom)
                    {
                        summonEffectWithCustom.InitializeWithCustomSettings(this, effectEntry.customSettings);
                        summonEffectWithCustom.OnSummonTriggered += HandleSummonTriggered;
                    }
                    else if (effectEntry.effectSO is Effect.TriggerSkillOnHealthEffectSO skillEffectWithCustom)
                    {
                        skillEffectWithCustom.InitializeWithCustomSettings(this, effectEntry.customSettings);
                        skillEffectWithCustom.OnSkillTriggered += HandleSkillTriggered;
                    }
                    else
                    {
                        effectEntry.effectSO.Initialize(this);
                    }
                }
                else
                {
                    effectEntry.effectSO.Initialize(this);

                    // SummonEffectSO 지원
                    if (effectEntry.effectSO is Effect.SummonEffectSO summonEffectSO)
                    {
                        summonEffectSO.OnSummonTriggered += HandleSummonTriggered;
                    }
                    // TriggerSkillOnHealthEffectSO 지원
                    else if (effectEntry.effectSO is Effect.TriggerSkillOnHealthEffectSO skillEffect)
                    {
                        skillEffect.OnSkillTriggered += HandleSkillTriggered;
                        skillEffect.OnSkillDefinitionTriggered += HandleSkillDefinitionTriggered;
                    }
                }

                GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 이펙트 적용: {effectEntry.effectSO.GetEffectName()}", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 페이즈 전환 연출을 재생합니다 (코루틴).
        /// VFX/SFX가 없어도 전환은 진행됩니다.
        /// </summary>
        private System.Collections.IEnumerator PlayPhaseTransitionEffectsCoroutine(EnemyPhaseData phase)
        {
            if (phase == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] 페이즈 데이터가 null이므로 연출 스킵", GameLogger.LogCategory.Character);
                yield break;
            }

            bool hasVFX = phase.phaseTransitionVFX != null && vfxManager != null;
            bool hasSFX = phase.phaseTransitionSFX != null && audioManager != null;

            GameLogger.LogDebug($"[{GetCharacterName()}] 페이즈 전환 연출 시작: VFX={hasVFX}, SFX={hasSFX}", GameLogger.LogCategory.Character);

            // VFX 재생
            if (hasVFX)
            {
                Transform visualRoot = GetHitVisualRoot();
                if (visualRoot != null)
                {
                    vfxManager.PlayEffect(phase.phaseTransitionVFX, visualRoot.position);
                    GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환 VFX 재생: {phase.phaseTransitionVFX.name}", GameLogger.LogCategory.Character);
                    
                    // VFX 재생 시간 대기 (기본 1초)
                    yield return new WaitForSeconds(1.0f);
                }
                else
                {
                    GameLogger.LogWarning($"[{GetCharacterName()}] VisualRoot를 찾을 수 없어 VFX 스킵", GameLogger.LogCategory.Character);
                }
            }
            else
            {
                GameLogger.LogDebug($"[{GetCharacterName()}] VFX가 없거나 VFXManager가 null - 연출 없이 진행", GameLogger.LogCategory.Character);
            }

            // SFX 재생
            if (hasSFX)
            {
                audioManager.PlaySFX(phase.phaseTransitionSFX);
                GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환 SFX 재생: {phase.phaseTransitionSFX.name}", GameLogger.LogCategory.Character);
            }
            else
            {
                GameLogger.LogDebug($"[{GetCharacterName()}] SFX가 없거나 AudioManager가 null - 연출 없이 진행", GameLogger.LogCategory.Character);
            }

            // 연출이 없어도 최소한의 대기 시간 (0.1초) - 전환 효과를 위한 최소 딜레이
            if (!hasVFX && !hasSFX)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// 슬롯의 적 카드를 제거하고 새로 생성합니다 (코루틴).
        /// </summary>
        private System.Collections.IEnumerator ClearEnemyCardsAndRegenerateCoroutine()
        {
            // SlotMovementController 주입 확인 및 수동 주입 시도
            EnsureSlotMovementControllerInjected();
            
            // SlotMovementController가 없으면 슬롯 초기화 불가
            if (slotMovementController == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] SlotMovementController를 찾을 수 없습니다 - 슬롯 초기화 건너뜀", GameLogger.LogCategory.Character);
                yield break;
            }

            var slotController = slotMovementController as Game.CombatSystem.Manager.SlotMovementController;
            if (slotController == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] SlotMovementController를 구체 클래스로 캐스팅할 수 없습니다", GameLogger.LogCategory.Character);
                yield break;
            }

            if (skillDeck == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] 적 덱이 null입니다 - 새 카드 생성 건너뜀", GameLogger.LogCategory.Character);
                yield break;
            }

            // ICardSlotRegistry 찾기 (여러 방법 시도)
            var registry = slotRegistry;
            if (registry == null)
            {
                // slotMovementController를 통해 접근
                registry = slotController.GetCardSlotRegistry();
            }

            // 모든 카드 제거 (플레이어 마커 포함) - 초기 셋업 방식으로 재생성하기 위해
            if (registry != null)
            {
                // 모든 슬롯의 카드 제거
                var clearedSlots = new List<Game.CombatSystem.Slot.CombatSlotPosition>();
                var allSlots = new[]
                {
                    Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT,
                    Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_1,
                    Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_2,
                    Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_3,
                    Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_4
                };

                foreach (var slot in allSlots)
                {
                    var card = registry.GetCardInSlot(slot);
                    if (card != null)
                {
                    registry.ClearSlot(slot);
                        clearedSlots.Add(slot);
                    GameLogger.LogDebug($"[{GetCharacterName()}] 슬롯 카드 제거: {slot}", GameLogger.LogCategory.Character);
                    }
                }

                GameLogger.LogInfo($"[{GetCharacterName()}] 모든 슬롯 카드 제거 완료 ({clearedSlots.Count}개 슬롯)", GameLogger.LogCategory.Character);
            }
            else
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] ICardSlotRegistry를 찾을 수 없지만 SlotMovementController를 통해 슬롯 초기화 진행", GameLogger.LogCategory.Character);
            }

            // SlotMovementController의 적 덱 캐시 업데이트
            var currentEnemyData = CharacterData as Game.CharacterSystem.Data.EnemyCharacterData;
            var currentEnemyName = GetCharacterName();
            slotMovementController.UpdateEnemyCache(currentEnemyData, currentEnemyName);
            GameLogger.LogDebug($"[{GetCharacterName()}] SlotMovementController 적 덱 캐시 업데이트 완료", GameLogger.LogCategory.Character);

            // SlotMovementController를 통해 모든 전투/대기 슬롯을 새 덱으로 채우기
            // 이 메서드는 내부에서 ICardSlotRegistry를 직접 사용하므로 registry가 null이어도 문제없음
            GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환: 모든 전투/대기 슬롯을 새 덱으로 채우기 시작", GameLogger.LogCategory.Character);
            yield return slotController.RefillAllCombatSlotsWithEnemyDeckCoroutine();
            GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환: 모든 전투/대기 슬롯 재채우기 완료", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 새로운 적 생성처럼 자연스럽게 슬롯을 재생성합니다 (SetupInitialEnemyQueueRoutine 사용).
        /// </summary>
        private System.Collections.IEnumerator SetupSlotsLikeNewEnemyCoroutine()
        {
            // SlotMovementController 주입 확인 및 수동 주입 시도
            EnsureSlotMovementControllerInjected();
            
            // SlotMovementController가 없으면 슬롯 재생성 불가
            if (slotMovementController == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] SlotMovementController를 찾을 수 없습니다 - 슬롯 재생성 건너뜀", GameLogger.LogCategory.Character);
                yield break;
            }

            if (skillDeck == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] 적 덱이 null입니다 - 새 카드 생성 건너뜀", GameLogger.LogCategory.Character);
                yield break;
            }

            // 현재 적 데이터 가져오기 (새 페이즈의 덱 사용)
            var currentEnemyData = CharacterData as Game.CharacterSystem.Data.EnemyCharacterData;
            var currentEnemyName = GetCharacterName();
            
            if (currentEnemyData == null)
            {
                GameLogger.LogWarning($"[{GetCharacterName()}] 적 데이터를 가져올 수 없습니다 - 슬롯 재생성 건너뜀", GameLogger.LogCategory.Character);
                yield break;
            }

            // 새로운 적 생성처럼 SetupInitialEnemyQueueRoutine 사용
            // 이 메서드는 초기 셋업과 동일한 방식으로 슬롯을 생성합니다
            GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환: 새로운 적 생성처럼 슬롯 초기 셋업 시작", GameLogger.LogCategory.Character);
            yield return slotMovementController.SetupInitialEnemyQueueRoutine(currentEnemyData, currentEnemyName);
            GameLogger.LogInfo($"[{GetCharacterName()}] 페이즈 전환: 슬롯 초기 셋업 완료", GameLogger.LogCategory.Character);
        }

        #endregion
    }
}
