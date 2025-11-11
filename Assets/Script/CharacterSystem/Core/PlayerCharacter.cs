using System;
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
                        GameLogger.LogWarning("[PlayerCharacter] Portrait 프리팹에서 Image 컴포넌트를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
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

                GameLogger.LogInfo($"[PlayerCharacter] Portrait 프리팹 인스턴스화 완료: {data.PortraitPrefab.name}", GameLogger.LogCategory.Character);
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
                    GameLogger.LogWarning("[PlayerCharacter] Portrait Image를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                }
            }
        }

        #endregion

        #region UI Handling

        /// <summary>
        /// 포트레이트 이미지 업데이트 (나머지 UI는 씬 통합 UI에서 관리)
        /// </summary>
        private void UpdateUI()
        {
            if (PlayerCharacterData == null) return;

            // 포트레이트 이미지만 설정
            if (portraitImage != null)
                portraitImage.sprite = PlayerCharacterData.Portrait;
            
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
                GameLogger.LogInfo($"[{GetCharacterName()}] 가드로 데미지 차단됨 - 데미지 텍스트 표시 안함", GameLogger.LogCategory.Character);
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

            // 새로운 통합 UI 업데이트
            if (playerCharacterUIController != null)
            {
                playerCharacterUIController.OnTakeDamage(amount);
            }

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
            UpdateUI();

            // 새로운 통합 UI 업데이트
            if (playerCharacterUIController != null)
            {
                playerCharacterUIController.OnHeal(amount);
            }

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
            GameLogger.LogInfo($"[PlayerCharacter] 카드 복귀: {card?.CardDefinition?.displayName}", GameLogger.LogCategory.Character);
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
        protected override void OnDamaged(int amount)
        {
            CombatEvents.RaisePlayerCharacterDamaged(PlayerCharacterData, this.gameObject, amount);
            
            // 피격 애니메이션 재생
            PlayHitAnimation();

            // 피격 시각 효과 재생
            PlayHitVisualEffects(amount);

            // 카메라 쉐이크 (플레이어만)
            PlayCameraShake(amount);
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
                mainCamera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            }

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
            GameLogger.LogInfo($"[PlayIdleAnimation] 호출됨 - playerAnimator: {(playerAnimator != null ? "존재" : "null")}", GameLogger.LogCategory.Character);
            
            if (playerAnimator != null)
            {
                // Animator 상태 확인
                var currentState = playerAnimator.GetCurrentAnimatorStateInfo(0);
                GameLogger.LogInfo($"[PlayIdleAnimation] 현재 상태: {currentState.shortNameHash}, 정규화 시간: {currentState.normalizedTime}", GameLogger.LogCategory.Character);
                
                // 방법 1: Trigger 사용
                try
                {
                    playerAnimator.SetTrigger("Idle");
                    GameLogger.LogInfo("[PlayIdleAnimation] Idle 트리거 설정 완료", GameLogger.LogCategory.Character);
                    
                    // 강제로 애니메이션 재시작 (포트레이트 애니메이션용)
                    playerAnimator.Play(currentState.shortNameHash, 0, 0f);
                    GameLogger.LogInfo("[PlayIdleAnimation] 애니메이션 강제 재시작 완료", GameLogger.LogCategory.Character);
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogWarning($"[PlayIdleAnimation] Trigger 실패: {ex.Message}", GameLogger.LogCategory.Character);
                    
                    // 방법 2: 직접 애니메이션 재생 (대안)
                    try
                    {
                        playerAnimator.Play("Player_idle_1", 0, 0f);
                        GameLogger.LogInfo("[PlayIdleAnimation] 직접 애니메이션 재생 완료", GameLogger.LogCategory.Character);
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
                GameLogger.LogInfo("플레 gameObject.ani이어 피격 애니메이션 재생", GameLogger.LogCategory.Character);
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
                GameLogger.LogInfo("플레이어 공격 애니메이션 재생", GameLogger.LogCategory.Character);
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
                GameLogger.LogInfo("플레이어 등장 애니메이션 재생", GameLogger.LogCategory.Character);
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
                GameLogger.LogInfo($"[ForceRestartAnimation] 애니메이션 강제 재시작: {currentState.shortNameHash}", GameLogger.LogCategory.Character);
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
    }
}
