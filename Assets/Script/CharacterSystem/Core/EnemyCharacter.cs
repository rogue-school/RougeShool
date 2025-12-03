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

        [Inject(Optional = true)] private VFXManager vfxManager;
        private Sequence deathSequence;

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

            // Portrait 프리팹 인스턴스화 (데이터에 설정된 경우)
            InitializePortrait(data);

            SetMaxHP(data.MaxHP);
            ApplyPassiveEffects();
            RefreshUI();
            
            // HP 바 초기화 (적 캐릭터 전용)
            ReinitializeHPBarController();
            
            // 적 캐릭터 덱의 스킬카드 스택 초기화
            InitializeEnemyDeckStacks();
            
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

            // Portrait 프리팹이 설정되어 있으면 인스턴스화
            if (data.PortraitPrefab != null)
            {
                // Portrait 부모 Transform 찾기
                Transform parent = portraitParent;
                if (parent == null)
                {
                    // 기존 Portrait GameObject의 부모를 찾기
                    var existingPortrait = transform.Find("Portrait");
                    if (existingPortrait != null)
                    {
                        parent = existingPortrait.parent;
                        // 기존 Portrait 비활성화 (프리팹으로 교체)
                        existingPortrait.gameObject.SetActive(false);
                    }
                    else
                    {
                        // Portrait 부모를 찾을 수 없으면 캐릭터 Transform 사용
                        parent = transform;
                    }
                }

                // Portrait 프리팹 인스턴스화
                GameObject portraitInstance = Instantiate(data.PortraitPrefab, parent);
                portraitInstance.name = "Portrait";

                // Portrait Image 컴포넌트 찾기
                if (portraitImage == null)
                {
                    portraitImage = portraitInstance.GetComponentInChildren<Image>(true);
                    if (portraitImage == null)
                    {
                        GameLogger.LogWarning("[EnemyCharacter] Portrait 프리팹에서 Image 컴포넌트를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                    }
                }

                // HP Text Anchor 찾기 (Portrait 프리팹 내부에 있을 수 있음)
                if (hpTextAnchor == null)
                {
                    // "HPTectAnchor" 또는 "HPTextAnchor" 이름으로 찾기
                    var hpAnchor = portraitInstance.transform.Find("HPTectAnchor");
                    if (hpAnchor == null)
                    {
                        hpAnchor = portraitInstance.transform.Find("HPTextAnchor");
                    }
                    if (hpAnchor != null)
                    {
                        hpTextAnchor = hpAnchor;
                    }
                }

            }
            else
            {
                // Portrait 프리팹이 없으면 기존 Portrait GameObject 사용
                if (portraitImage == null)
                {
                    var existingPortrait = transform.Find("Portrait");
                    if (existingPortrait != null)
                    {
                        portraitImage = existingPortrait.GetComponent<Image>();
                    }
                }

                if (portraitImage == null)
                {
                    GameLogger.LogWarning("[EnemyCharacter] Portrait Image를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                }
            }
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
            
            GameLogger.LogDebug($"[EnemyCharacter] SetCharacterData 호출: {data.DisplayName}", GameLogger.LogCategory.Character);
            
            CharacterData = data;
            this.gameObject.name = CharacterData.name;
            Initialize(data);
            
            GameLogger.LogDebug($"[EnemyCharacter] SetCharacterData 완료: {data.DisplayName}", GameLogger.LogCategory.Character);
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

            // 피격 애니메이션 재생
            PlayHitAnimation();

            // 피격 시각 효과 재생
            PlayHitVisualEffects(amount);
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
            
            foreach (var effect in characterEffects)
            {
                GameLogger.LogDebug($"[{GetCharacterName()}] 이펙트 체력 변경 처리: {effect.GetEffectName()}", GameLogger.LogCategory.Character);
                effect.OnHealthChanged(this, previousHP, currentHP);
            }
        }

        private void HandleSummonTriggered(EnemyCharacterData summonTarget, int currentHP)
        {
            GameLogger.LogInfo($"[{GetCharacterName()}] 소환 요청 전달: {summonTarget.DisplayName}, 현재 체력: {currentHP}/{GetMaxHP()}", GameLogger.LogCategory.Character);
            
            // StageManager에 소환 요청 전달 (상태 패턴에서 처리)
            var stageManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
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
            // DOTween 시퀀스 정리
            if (deathSequence != null && deathSequence.IsActive())
            {
                deathSequence.Kill();
                deathSequence = null;
            }
            CleanupEffects();
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
    }
}
