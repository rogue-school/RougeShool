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
        [SerializeField] private Image portraitImage;

        [Header("Damage UI")]
        [SerializeField] private Transform hpTextAnchor;
        [SerializeField] private GameObject damageTextPrefab;

        [Header("HP Bar UI")]
        [SerializeField] private HPBarController hpBarController;
        
        [Header("새로운 통합 UI")]
        [SerializeField] private PlayerCharacterUIController playerCharacterUIController;


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
        }

        #endregion
    }
}
