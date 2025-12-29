using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.UI;
using Game.CombatSystem;
using Game.CombatSystem.Core;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.UI;
using Game.CoreSystem.Utility;
using Game.VFXSystem.Manager;
using Zenject;
using DG.Tweening;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 플레이어 캐릭터의 구체 클래스입니다.
    /// UI, 카드 핸들링, 플레이어 데이터 연동 등의 기능을 담당합니다.
    /// </summary>
    public class PlayerCharacter : CharacterBase, ICharacter
    {
        #region Serialized Fields

        /// <summary>
        /// 플레이어 데이터 스크립터블 오브젝트
        /// </summary>
        [field: SerializeField]
        public PlayerCharacterData PlayerCharacterData { get; private set; }
        
        /// <summary>
        /// ICharacter 인터페이스의 CharacterData 프로퍼티 구현
        /// </summary>
        public override object CharacterData => PlayerCharacterData;

        [Header("UI Components")]
        [Tooltip("Portrait 이미지 (자동으로 찾거나 설정됨)")]
        [SerializeField] private Image portraitImage;

        [Tooltip("Portrait가 배치될 부모 Transform (기본 Portrait GameObject의 부모)")]
        [SerializeField] private Transform portraitParent;

        [Header("Damage UI")]
        [SerializeField] private Transform hpTextAnchor;
        [SerializeField] private GameObject damageTextPrefab;

        [Header("HP Bar UI")]
        [SerializeField] private HPBarController hpBarController;
        
        [Header("새로운 통합 UI")]
        [SerializeField] private PlayerCharacterUIController playerCharacterUIController;

        [Header("애니메이션 설정")]
        [Tooltip("플레이어 캐릭터 애니메이터")]
        [SerializeField] private Animator playerAnimator;


        #endregion

        #region Private Fields

        private ISkillCard lastUsedCard;
        private IPlayerHandManager handManager;

        [Inject(Optional = true)] private VFXManager vfxManager;
        [Inject(Optional = true)] private Game.CombatSystem.Interface.ISlotMovementController slotMovementController;
        [Inject(Optional = true)] private Game.CombatSystem.Interface.ICardSlotRegistry slotRegistry;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 에디터에서 설정된 데이터로 초기화합니다.
        /// </summary>
        private void Awake()
        {
            // 애니메이터 자동 검색 (수동 설정이 없는 경우)
            if (playerAnimator == null)
            {
                playerAnimator = GetComponent<Animator>();
            }
            
            if (PlayerCharacterData != null)
            {
                this.gameObject.name = PlayerCharacterData.name;
                InitializeCharacter(PlayerCharacterData);
            }
            else
            {
                GameLogger.LogWarning($"[PlayerCharacter] {gameObject.name}의 CharacterData가 설정되지 않았습니다. 런타임에 초기화됩니다.", GameLogger.LogCategory.Character);
            }
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (PlayerCharacterData != null)
                this.gameObject.name = PlayerCharacterData.name;
        }
        #endif

        #endregion

        #region Initialization

        /// <summary>
        /// 외부에서 캐릭터 데이터를 설정하고 초기화합니다.
        /// </summary>
        /// <param name="data">플레이어 캐릭터 데이터</param>
        public void SetCharacterData(PlayerCharacterData data)
        {
            if (data == null)
            {
                GameLogger.LogError("[PlayerCharacter] 플레이어 캐릭터 데이터가 null입니다.", GameLogger.LogCategory.Character);
                throw new ArgumentNullException(nameof(data), "플레이어 캐릭터 데이터는 null일 수 없습니다.");
            }
            
            PlayerCharacterData = data;
            this.gameObject.name = PlayerCharacterData.name;
            InitializeCharacter(data);
        }

        /// <summary>
        /// ICharacter 인터페이스의 SetCharacterData 구현
        /// </summary>
        /// <param name="data">캐릭터 데이터 (object 타입)</param>
        public override void SetCharacterData(object data)
        {
            if (data is PlayerCharacterData playerData)
            {
                SetCharacterData(playerData);
            }
            else
            {
                GameLogger.LogError($"[PlayerCharacter] 잘못된 데이터 타입입니다. 예상: PlayerCharacterData, 실제: {data?.GetType().Name ?? "null"}", GameLogger.LogCategory.Character);
                throw new ArgumentException("PlayerCharacterData 타입이 필요합니다.", nameof(data));
            }
        }

        /// <summary>
        /// 체력 초기화 및 UI 갱신
        /// </summary>
        /// <param name="data">플레이어 데이터</param>
        private void InitializeCharacter(PlayerCharacterData data)
        {
            // Portrait 프리팹 인스턴스화 (데이터에 설정된 경우)
            InitializePortrait(data);
            
            SetMaxHP(data.MaxHP);
            UpdateUI();
            
            // HP 바 초기화
            if (hpBarController != null)
            {
                hpBarController.Initialize(this);
            }
            
            // 새로운 통합 UI 초기화
            if (playerCharacterUIController != null)
            {
                playerCharacterUIController.Initialize(this);
            }

            // 기본 Idle 시각 효과 시작 (부드러운 호흡)
            StartIdleVisualLoop();
        }

        /// <summary>
        /// Portrait 프리팹을 인스턴스화하고 설정합니다.
        /// </summary>
        /// <param name="data">플레이어 캐릭터 데이터</param>
        private void InitializePortrait(PlayerCharacterData data)
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

        #endregion

        #region UI Handling

        /// <summary>
        /// 포트레이트 이미지 업데이트 (나머지 UI는 씬 통합 UI에서 관리)
        /// </summary>
        private void UpdateUI()
        {
            if (PlayerCharacterData == null) return;

            // Portrait는 프리팹이 자체적으로 관리하므로 여기서 직접 설정하지 않음
            
            // HP 바 업데이트 (HP 바 컨트롤러가 있는 경우)
            hpBarController?.OnHealthChanged();
        }

        /// <summary>
        /// 데미지 처리 후 UI 갱신
        /// </summary>
        /// <param name="amount">피해량</param>
        public override void TakeDamage(int amount)
        {
            // 가드 상태 확인 (base.TakeDamage 호출 전에 미리 확인)
            bool wasGuarded = IsGuarded();
            
            base.TakeDamage(amount);
            UpdateUI();

            // 가드로 차단된 경우 데미지 텍스트 표시하지 않음
            if (wasGuarded)
            {
                return;
            }

            // 실제 데미지를 받은 경우에만 데미지 텍스트 표시
            ShowDamageText(amount);
        }

        /// <summary>
        /// 가드 무시 데미지 처리 후 UI 갱신
        /// </summary>
        /// <param name="amount">피해량</param>
        public override void TakeDamageIgnoreGuard(int amount)
        {
            base.TakeDamageIgnoreGuard(amount);
            UpdateUI();

            // 가드 무시 데미지는 항상 데미지 텍스트 표시
            ShowDamageText(amount);
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

            // 새로운 통합 UI 업데이트
            if (playerCharacterUIController != null)
            {
                playerCharacterUIController.OnTakeDamage(amount);
            }

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
            UpdateUI();

            // 새로운 통합 UI 업데이트
            if (playerCharacterUIController != null)
            {
                playerCharacterUIController.OnHeal(amount);
            }

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

        #endregion

        #region Card & Hand

        /// <summary>
        /// 핸드 매니저 의존성 주입
        /// </summary>
        /// <param name="manager">핸드 매니저 인스턴스</param>
        public void InjectHandManager(IPlayerHandManager manager) => handManager = manager;

        /// <summary>
        /// 지정 슬롯 위치의 스킬 카드 반환
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        public ISkillCard GetCardInHandSlot(SkillCardSlotPosition pos) => handManager?.GetCardInSlot(pos);

        /// <summary>
        /// 지정 슬롯 위치의 카드 UI 반환
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        public ISkillCardUI GetCardUIInHandSlot(SkillCardSlotPosition pos) => handManager?.GetCardUIInSlot(pos);

        /// <summary>
        /// 마지막 사용한 카드 설정
        /// </summary>
        /// <param name="card">스킬 카드</param>
        public void SetLastUsedCard(ISkillCard card) => lastUsedCard = card;

        /// <summary>
        /// 마지막 사용한 카드 반환
        /// </summary>
        public ISkillCard GetLastUsedCard() => lastUsedCard;

        /// <summary>
        /// 지정된 카드를 핸드로 복원 (기능은 외부 구현 예정)
        /// </summary>
        /// <param name="card">복원할 카드</param>
        public void RestoreCardToHand(ISkillCard card)
        {
            // 실제 핸드 복원 로직은 handManager 내부에 구현되어야 함
        }

        #endregion

        #region 버프/디버프 시스템

        /// <summary>
        /// 버프/디버프 아이콘을 추가합니다.
        /// </summary>
        /// <param name="effectId">효과 ID</param>
        /// <param name="iconSprite">아이콘 스프라이트</param>
        /// <param name="isBuff">버프 여부</param>
        /// <param name="duration">지속 시간 (초, -1이면 영구)</param>
        public void AddBuffDebuffIcon(string effectId, Sprite iconSprite, bool isBuff, float duration = -1f)
        {
            if (playerCharacterUIController != null)
            {
                playerCharacterUIController.AddBuffDebuffIcon(effectId, iconSprite, isBuff, duration);
            }
        }

        /// <summary>
        /// 버프/디버프 아이콘을 제거합니다.
        /// </summary>
        /// <param name="effectId">효과 ID</param>
        public void RemoveBuffDebuffIcon(string effectId)
        {
            if (playerCharacterUIController != null)
            {
                playerCharacterUIController.RemoveBuffDebuffIcon(effectId);
            }
        }

        /// <summary>
        /// 모든 버프/디버프 아이콘을 제거합니다.
        /// </summary>
        public void ClearAllBuffDebuffIcons()
        {
            if (playerCharacterUIController != null)
            {
                playerCharacterUIController.ClearAllBuffDebuffIcons();
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// 플레이어 캐릭터임을 나타냅니다.
        /// </summary>
        public override bool IsPlayerControlled() => true;

        /// <summary>
        /// 캐릭터 이름 반환
        /// </summary>
        public override string GetCharacterName() 
        {
            if (PlayerCharacterData != null)
            {
                return PlayerCharacterData.DisplayName;
            }
            
            // CharacterData가 아직 설정되지 않은 경우 GameObject 이름 사용
            return gameObject.name.Replace("(Clone)", "").Trim();
        }

        /// <summary>
        /// 생존 여부 반환 (명시적 override)
        /// </summary>
        public override bool IsAlive() => base.IsAlive();

        /// <summary>
        /// 사망 처리 (이벤트 발행)
        /// </summary>
        public override void Die()
        {
            // 1. 소멸 애니메이션 이벤트 즉시 발행 (플레이어)
            Game.CombatSystem.CombatEvents.RaiseHandSkillCardsVanishOnCharacterDeath(true);

            base.Die();
            Game.CombatSystem.CombatEvents.RaisePlayerCharacterDeath(PlayerCharacterData, this.gameObject);
        }

        // CharacterBase에서 사용할 이름 반환
        protected override string GetCharacterDataName()
        {
            return PlayerCharacterData?.name ?? "Unknown";
        }

        #endregion

        #region 이벤트 처리 오버라이드

        /// <summary>가드 획득 시 이벤트 발행</summary>
        protected override void OnGuarded(int amount)
        {
            CombatEvents.RaisePlayerCharacterGuarded(PlayerCharacterData, this.gameObject, amount);
        }

        /// <summary>회복 시 이벤트 발행</summary>
        protected override void OnHealed(int amount)
        {
            CombatEvents.RaisePlayerCharacterHealed(PlayerCharacterData, this.gameObject, amount);
        }

        /// <summary>피해 시 이벤트 발행</summary>
        protected override void OnDamaged(int amount, bool skipVisualEffects = false)
        {
            CombatEvents.RaisePlayerCharacterDamaged(PlayerCharacterData, this.gameObject, amount);
            
            // 시각 효과를 건너뛰지 않는 경우에만 피격 효과 재생
            if (!skipVisualEffects)
            {
            // 피격 애니메이션 재생
            PlayHitAnimation();

            // 피격 시각 효과 재생
            PlayHitVisualEffects(amount);

            // 카메라 쉐이크 (플레이어만)
            PlayCameraShake(amount);
            }
        }

        /// <summary>
        /// 플레이어가 데미지를 받을 때 카메라 쉐이크를 재생합니다
        /// </summary>
        /// <param name="damageAmount">피해량 (쉐이크 강도 조절용)</param>
        private void PlayCameraShake(int damageAmount)
        {
            if (damageAmount <= 0) return;

            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                GameLogger.LogWarning("[PlayerCharacter] 카메라를 찾을 수 없어 쉐이크를 재생할 수 없습니다.", GameLogger.LogCategory.Character);
                return;
            }

            // 데미지에 비례한 쉐이크 강도 (최소 0.1, 최대 0.3)
            float shakeStrength = Mathf.Clamp(damageAmount * 0.01f, 0.1f, 0.3f);
            float shakeDuration = Mathf.Clamp(damageAmount * 0.01f, 0.15f, 0.25f);

            // 카메라 위치 쉐이크
            mainCamera.transform.DOShakePosition(shakeDuration, shakeStrength, 10, 90f, false, true)
                .SetEase(Ease.OutQuad);
        }

        #endregion

        #region 애니메이션 제어

        /// <summary>
        /// 대기 애니메이션을 재생합니다.
        /// </summary>
        public void PlayIdleAnimation()
        {
            if (playerAnimator != null)
            {
                // Animator 상태 확인
                var currentState = playerAnimator.GetCurrentAnimatorStateInfo(0);
                // 방법 1: Trigger 사용
                try
                {
                    playerAnimator.SetTrigger("Idle");
                    
                    // 강제로 애니메이션 재시작 (포트레이트 애니메이션용)
                    playerAnimator.Play(currentState.shortNameHash, 0, 0f);
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogWarning($"[PlayIdleAnimation] Trigger 실패: {ex.Message}", GameLogger.LogCategory.Character);
                    
                    // 방법 2: 직접 애니메이션 재생 (대안)
                    try
                    {
                        playerAnimator.Play("Player_idle_1", 0, 0f);
                    }
                    catch (System.Exception ex2)
                    {
                        GameLogger.LogError($"[PlayIdleAnimation] 직접 재생도 실패: {ex2.Message}", GameLogger.LogCategory.Character);
                    }
                }
            }
            else
            {
                GameLogger.LogWarning("[PlayIdleAnimation] 플레이어 애니메이터가 설정되지 않았습니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 피격 애니메이션을 재생합니다.
        /// </summary>
        public void PlayHitAnimation()
        {
            if (playerAnimator != null)
            {
                // Hit 트리거 활성화
                playerAnimator.SetTrigger("Hit");
            }
            else
            {
                GameLogger.LogWarning("플레이어 애니메이터가 설정되지 않았습니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 공격 애니메이션을 재생합니다.
        /// </summary>
        public void PlayAttackAnimation()
        {
            if (playerAnimator != null)
            {
                // Attack 트리거 활성화
                playerAnimator.SetTrigger("Attack");
            }
            else
            {
                GameLogger.LogWarning("플레이어 애니메이터가 설정되지 않았습니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 등장 애니메이션을 재생합니다.
        /// </summary>
        public void PlayAppearedAnimation()
        {
            if (playerAnimator != null)
            {
                // Appeared 트리거 활성화
                playerAnimator.SetTrigger("Appeared");
            }
            else
            {
                GameLogger.LogWarning("플레이어 애니메이터가 설정되지 않았습니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 애니메이션 상태를 확인합니다.
        /// </summary>
        /// <param name="stateName">확인할 애니메이션 상태 이름</param>
        /// <returns>해당 상태가 재생 중이면 true</returns>
        public bool IsAnimationPlaying(string stateName)
        {
            if (playerAnimator == null) return false;
            
            return playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
        }

        /// <summary>
        /// 현재 애니메이션의 진행률을 반환합니다.
        /// </summary>
        /// <returns>0.0 ~ 1.0 사이의 진행률</returns>
        public float GetCurrentAnimationProgress()
        {
            if (playerAnimator == null) return 0f;
            
            return playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }

        /// <summary>
        /// 애니메이션 강제 재시작 (테스트용)
        /// </summary>
        public void ForceRestartAnimation()
        {
            if (playerAnimator != null)
            {
                var currentState = playerAnimator.GetCurrentAnimatorStateInfo(0);
                playerAnimator.Play(currentState.shortNameHash, 0, 0f);
            }
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

        #endregion

        #region Turn Effects Processing

        /// <summary>
        /// 등록된 턴 효과를 처리하고 출혈 이펙트 및 운명의 실 효과 완료를 기다립니다 (코루틴)
        /// </summary>
        public override System.Collections.IEnumerator ProcessTurnEffectsCoroutine()
        {
            // GameObject가 비활성화 상태면 턴 효과 처리 안 함
            if (!gameObject.activeInHierarchy)
            {
                yield break;
            }

            // 출혈 효과 개수 카운트 (처리 전에 카운트)
            int bleedEffectCount = 0;
            bool hasThreadOfFate = false;
            foreach (var effect in perTurnEffects)
            {
                if (effect is Game.SkillCardSystem.Effect.BleedEffect)
                {
                    bleedEffectCount++;
                }
                if (effect is Game.SkillCardSystem.Effect.ThreadOfFateDebuff)
                {
                    hasThreadOfFate = true;
                }
            }

            // 출혈 효과가 없으면 즉시 처리
            if (bleedEffectCount == 0 && !hasThreadOfFate)
            {
                ProcessTurnEffects();
                yield break;
            }

            // 출혈 효과가 있으면 완료 이벤트 대기
            int completedBleedEffects = 0;
            System.Action onBleedComplete = () => completedBleedEffects++;

            // 이벤트 구독 (ProcessTurnEffects 호출 전에 구독해야 함)
            if (bleedEffectCount > 0)
            {
                Game.CombatSystem.CombatEvents.OnBleedTurnStartEffectComplete += onBleedComplete;
            }

            // 턴 효과 처리 (출혈 이펙트 재생 시작)
            ProcessTurnEffects();

            // 운명의 실 효과는 핸드 생성 후에 처리되므로 여기서는 처리하지 않음
            // (PlayerTurnState.ContinueAfterTurnEffects에서 핸드 생성 후 처리)

            // 모든 출혈 이펙트 완료 대기 (타임아웃: 최대 1.5초)
            if (bleedEffectCount > 0)
            {
                float timeout = 1.5f;
                float elapsed = 0f;
                
                while (completedBleedEffects < bleedEffectCount && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // 이벤트 구독 해제
                Game.CombatSystem.CombatEvents.OnBleedTurnStartEffectComplete -= onBleedComplete;

                if (completedBleedEffects >= bleedEffectCount)
                {
                    GameLogger.LogInfo($"[{GetCharacterDataName()}] 모든 출혈 이펙트 완료 ({completedBleedEffects}/{bleedEffectCount})", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogWarning($"[{GetCharacterDataName()}] 출혈 이펙트 타임아웃 ({completedBleedEffects}/{bleedEffectCount})", GameLogger.LogCategory.Combat);
                }
            }
        }

        /// <summary>
        /// 운명의 실 효과를 처리하는 코루틴입니다.
        /// 플레이어 턴 시작 시 핸드에서 3장을 뽑고 2개를 제거한 후 나머지 1개를 전투 슬롯으로 이동시킵니다.
        /// </summary>
        /// <param name="handManager">핸드 매니저 (null이면 자동으로 찾음)</param>
        /// <param name="stateMachine">상태 머신 (카드 실행을 위해 필요)</param>
        public System.Collections.IEnumerator ProcessThreadOfFateEffectCoroutine(
            Game.SkillCardSystem.Manager.PlayerHandManager handManagerParam = null,
            Game.CombatSystem.State.CombatStateMachine stateMachine = null)
        {
            GameLogger.LogInfo("[PlayerCharacter] 운명의 실 효과 처리 시작", GameLogger.LogCategory.SkillCard);
            
            // 핸드 생성 후 1초 대기
            yield return new WaitForSeconds(1f);
            
            // handManager 파라미터가 없으면 인스턴스 필드 사용, 그것도 null이면 찾기
            var targetHandManager = handManagerParam ?? (handManager as Game.SkillCardSystem.Manager.PlayerHandManager);
            if (targetHandManager == null)
            {
                targetHandManager = UnityEngine.Object.FindFirstObjectByType<Game.SkillCardSystem.Manager.PlayerHandManager>();
                if (targetHandManager == null)
                {
                    GameLogger.LogWarning("[PlayerCharacter] 운명의 실 효과 처리 실패: HandManager를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                    yield break;
                }
                GameLogger.LogInfo("[PlayerCharacter] HandManager를 FindFirstObjectByType으로 찾았습니다.", GameLogger.LogCategory.SkillCard);
            }

            // HandSlotRegistry를 통해 현재 핸드의 카드 가져오기
            var handManagerType = typeof(Game.SkillCardSystem.Manager.PlayerHandManager);
            var handSlotRegistryField = handManagerType.GetField("handSlotRegistry",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (handSlotRegistryField == null)
            {
                GameLogger.LogWarning("[PlayerCharacter] handSlotRegistry 필드를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                yield break;
            }

            var handSlotRegistry = handSlotRegistryField.GetValue(targetHandManager) as Game.CombatSystem.Slot.HandSlotRegistry;
            if (handSlotRegistry == null)
            {
                GameLogger.LogWarning("[PlayerCharacter] HandSlotRegistry를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                yield break;
            }

            // SlotRegistry 찾기 (Zenject를 통해)
            ICardSlotRegistry slotRegistry = null;
            
            // Zenject ProjectContext를 통해 resolve 시도
            var projectContext = Zenject.ProjectContext.Instance;
            if (projectContext != null && projectContext.Container != null)
            {
                try
                {
                    slotRegistry = projectContext.Container.Resolve<ICardSlotRegistry>();
                    GameLogger.LogInfo("[PlayerCharacter] 운명의 실: SlotRegistry를 Zenject에서 찾았습니다.", GameLogger.LogCategory.SkillCard);
                }
                catch
                {
                    // ProjectContext에서 찾지 못한 경우 SceneContext 시도
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        var sceneContainer = sceneContextRegistry.TryGetContainerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                        if (sceneContainer != null)
                        {
                            slotRegistry = sceneContainer.Resolve<ICardSlotRegistry>();
                            GameLogger.LogInfo("[PlayerCharacter] 운명의 실: SlotRegistry를 SceneContext에서 찾았습니다.", GameLogger.LogCategory.SkillCard);
                        }
                    }
                    catch
                    {
                        // SceneContext에서도 찾지 못함
                    }
                }
            }
            
            if (slotRegistry == null)
            {
                GameLogger.LogWarning("[PlayerCharacter] 운명의 실: SlotRegistry를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                yield break;
            }

            // 현재 핸드 슬롯에서 카드 가져오기
            var handSlots = handSlotRegistry.GetPlayerHandSlot().ToList();
            var currentHandCards = new System.Collections.Generic.List<ISkillCard>();
            
            foreach (var slot in handSlots)
            {
                var card = slot?.GetCard();
                if (card != null)
                {
                    currentHandCards.Add(card);
                }
            }

            GameLogger.LogInfo($"[PlayerCharacter] 운명의 실: 현재 핸드 카드 수 = {currentHandCards.Count}", GameLogger.LogCategory.SkillCard);
            
            if (currentHandCards.Count < 3)
            {
                GameLogger.LogWarning($"[PlayerCharacter] 운명의 실: 핸드에 카드가 3장 미만입니다. (현재: {currentHandCards.Count}장)", GameLogger.LogCategory.SkillCard);
                yield break;
            }

            // 핸드에서 3장을 랜덤으로 선택
            var selectedThreeCards = new System.Collections.Generic.List<ISkillCard>();
            var availableCards = new System.Collections.Generic.List<ISkillCard>(currentHandCards);
            
            for (int i = 0; i < 3 && availableCards.Count > 0; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableCards.Count);
                selectedThreeCards.Add(availableCards[randomIndex]);
                availableCards.RemoveAt(randomIndex);
            }

            GameLogger.LogInfo($"[PlayerCharacter] 운명의 실: 선택된 3장 - {string.Join(", ", selectedThreeCards.Select(c => c.GetCardName()))}", GameLogger.LogCategory.SkillCard);

            // 선택된 3장 중 1장을 랜덤으로 선택하여 전투 슬롯으로 이동
            var selectedCard = selectedThreeCards[UnityEngine.Random.Range(0, selectedThreeCards.Count)];
            var cardsToRemove = selectedThreeCards.Where(c => c != selectedCard).ToList();

            GameLogger.LogInfo($"[PlayerCharacter] 운명의 실: 전투 슬롯으로 이동할 카드 = {selectedCard.GetCardName()}", GameLogger.LogCategory.SkillCard);
            GameLogger.LogInfo($"[PlayerCharacter] 운명의 실: 제거할 카드 - {string.Join(", ", cardsToRemove.Select(c => c.GetCardName()))}", GameLogger.LogCategory.SkillCard);

            // 나머지 2장 제거 (애니메이션 포함)
            foreach (var card in cardsToRemove)
            {
                // 카드 UI 찾기
                var cardUI = handSlotRegistry.GetPlayerHandSlot()
                    .FirstOrDefault(s => s?.GetCard() == card)?.GetCardUI() as Game.SkillCardSystem.UI.SkillCardUI;
                
                if (cardUI != null)
                {
                    // 페이드 아웃 애니메이션
                    var canvasGroup = cardUI.GetComponent<UnityEngine.CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.DOFade(0f, 0.3f)
                            .SetEase(DG.Tweening.Ease.OutQuad)
                            .SetAutoKill(true);
                    }
                    
                    // 스케일 다운 애니메이션
                    cardUI.transform.DOScale(0f, 0.3f)
                        .SetEase(DG.Tweening.Ease.InBack)
                        .SetAutoKill(true);
                }
                
                targetHandManager.RemoveCard(card);
                GameLogger.LogInfo($"[PlayerCharacter] 운명의 실: 카드 제거 - {card.GetCardName()}", GameLogger.LogCategory.SkillCard);
            }

            // 카드 제거 애니메이션 완료 대기
            yield return new WaitForSeconds(0.5f);

            // 선택된 카드를 전투 슬롯으로 직접 이동 (드래그/드랍처럼 자연스러운 애니메이션)
            GameLogger.LogInfo($"[PlayerCharacter] 운명의 실: 선택된 카드 = {selectedCard?.GetCardName()}, slotRegistry = {slotRegistry != null}, stateMachine = {stateMachine != null}", GameLogger.LogCategory.SkillCard);
            
            if (selectedCard != null && slotRegistry != null)
            {
                // 현재 카드 UI 찾기
                var currentCardSlot = handSlotRegistry.GetPlayerHandSlot()
                    .FirstOrDefault(s => s?.GetCard() == selectedCard);
                var currentCardUI = currentCardSlot?.GetCardUI() as Game.SkillCardSystem.UI.SkillCardUI;
                
                if (currentCardUI == null)
                {
                    GameLogger.LogWarning("[PlayerCharacter] 운명의 실: 선택된 카드의 UI를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                    yield break;
                }

                // 전투 슬롯 GameObject 찾기 (SlotMovementController와 동일한 방식)
                string battleSlotName = CombatConstants.SlotNames.BATTLE_SLOT;
                var battleSlotGo = UnityEngine.GameObject.Find(battleSlotName);
                if (battleSlotGo == null)
                {
                    GameLogger.LogWarning("[PlayerCharacter] 운명의 실: 전투 슬롯 GameObject를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                    yield break;
                }

                var battleSlotTransform = battleSlotGo.transform as RectTransform;
                if (battleSlotTransform == null)
                {
                    GameLogger.LogWarning("[PlayerCharacter] 운명의 실: 전투 슬롯이 RectTransform이 아닙니다.", GameLogger.LogCategory.SkillCard);
                    yield break;
                }

                // 카드 UI를 전투 슬롯으로 이동하는 애니메이션 (드래그/드랍처럼)
                // 주의: RemoveCard는 애니메이션 완료 후에 호출해야 카드가 사라지지 않음
                var cardRectTransform = currentCardUI.transform as RectTransform;
                if (cardRectTransform != null && battleSlotTransform != null)
                {
                    // 목적지 위치를 미리 계산 (애니메이션 중에 Transform이 파괴될 수 있으므로)
                    UnityEngine.Vector3 targetWorldPos = battleSlotTransform.TransformPoint(new Vector3(0f, 4f, 0f));
                    
                    // 카드를 상위 레이어로 이동 (애니메이션 중 다른 UI 위에 표시)
                    var rootCanvas = cardRectTransform.root;
                    if (rootCanvas != null)
                    {
                        cardRectTransform.SetParent(rootCanvas.transform, true);
                        cardRectTransform.SetAsLastSibling();
                    }
                    
                    GameLogger.LogInfo($"[PlayerCharacter] 운명의 실: 카드 애니메이션 시작 - {selectedCard.GetCardName()}", GameLogger.LogCategory.SkillCard);

                    // 드래그/드랍처럼 자연스러운 이동 애니메이션 (Sequence로 통합)
                    Sequence moveSequence = DOTween.Sequence();
                    
                    // 이동 애니메이션
                    moveSequence.Append(cardRectTransform.DOMove(targetWorldPos, 0.4f)
                        .SetEase(Ease.OutQuad));
                    
                    // 스케일 효과 (이동과 동시에)
                    moveSequence.Join(cardRectTransform.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad));
                    moveSequence.Append(cardRectTransform.DOScale(1.0f, 0.2f).SetEase(Ease.InQuad));
                    
                    moveSequence.SetAutoKill(true);
                    
                    // 애니메이션 완료 콜백에서 처리 (CardDropService와 동일한 패턴)
                    moveSequence.OnComplete(() =>
                    {
                        // 애니메이션 완료 후 Transform이 여전히 유효한지 확인
                        if (cardRectTransform == null || cardRectTransform.gameObject == null)
                        {
                            GameLogger.LogWarning("[PlayerCharacter] 운명의 실: 카드 UI가 파괴되었습니다.", GameLogger.LogCategory.SkillCard);
                            return;
                        }

                        // 전투 슬롯을 다시 찾기 (파괴되었을 수 있으므로)
                        var battleSlotGoAfterAnimation = UnityEngine.GameObject.Find(battleSlotName);
                        if (battleSlotGoAfterAnimation == null)
                        {
                            GameLogger.LogWarning("[PlayerCharacter] 운명의 실: 애니메이션 완료 후 전투 슬롯을 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                            return;
                        }

                        var battleSlotTransformAfterAnimation = battleSlotGoAfterAnimation.transform as RectTransform;
                        if (battleSlotTransformAfterAnimation == null)
                        {
                            GameLogger.LogWarning("[PlayerCharacter] 운명의 실: 전투 슬롯 Transform이 유효하지 않습니다.", GameLogger.LogCategory.SkillCard);
                            return;
                        }

                        // 카드 UI를 전투 슬롯의 자식으로 설정
                        cardRectTransform.SetParent(battleSlotTransformAfterAnimation, false);
                        cardRectTransform.anchoredPosition = new Vector2(0f, 4f);
                        cardRectTransform.localScale = Vector3.one;

                        // 전투 슬롯에 카드 등록
                        slotRegistry.RegisterCard(
                            Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT, 
                            selectedCard, 
                            currentCardUI, 
                            Game.CombatSystem.Data.SlotOwner.PLAYER);

                        // 핸드에서 카드 제거 (애니메이션 완료 후에 호출)
                        targetHandManager.RemoveCard(selectedCard);

                        GameLogger.LogInfo($"[PlayerCharacter] 운명의 실: 카드를 전투 슬롯으로 이동 완료 - {selectedCard.GetCardName()}", GameLogger.LogCategory.SkillCard);

                        // 애니메이션 완료 후 카드 실행
                        if (stateMachine != null)
                        {
                            stateMachine.OnPlayerCardPlaced(selectedCard, Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT);
                            GameLogger.LogInfo($"[PlayerCharacter] 운명의 실: 카드 실행 요청 - {selectedCard.GetCardName()}", GameLogger.LogCategory.SkillCard);
                        }
                        else
                        {
                            GameLogger.LogWarning("[PlayerCharacter] 운명의 실: 상태 머신이 null입니다. 카드를 실행할 수 없습니다.", GameLogger.LogCategory.SkillCard);
                        }
                    });
                    
                    GameLogger.LogInfo("[PlayerCharacter] 운명의 실: 애니메이션 시작", GameLogger.LogCategory.SkillCard);

                    // 애니메이션 완료 대기
                    yield return moveSequence.WaitForCompletion();
                    
                    GameLogger.LogInfo("[PlayerCharacter] 운명의 실: 애니메이션 완료", GameLogger.LogCategory.SkillCard);
                }
                else
                {
                    GameLogger.LogWarning("[PlayerCharacter] 운명의 실: 카드 UI가 RectTransform이 아닙니다.", GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                GameLogger.LogWarning("[PlayerCharacter] 운명의 실 효과 처리 실패: 필요한 매니저가 null입니다.", GameLogger.LogCategory.SkillCard);
            }
        }

        #endregion
    }
}
