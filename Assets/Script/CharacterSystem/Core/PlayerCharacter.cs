using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.IManager;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 플레이어 캐릭터의 구체 클래스입니다.
    /// UI, 카드 핸들링, 플레이어 데이터 연동 등의 기능을 담당합니다.
    /// </summary>
    public class PlayerCharacter : CharacterBase, IPlayerCharacter
    {
        #region Serialized Fields

        /// <summary>
        /// 플레이어 데이터 스크립터블 오브젝트
        /// </summary>
        [field: SerializeField]
        public PlayerCharacterData Data { get; private set; }

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI descriptionText;

        #endregion

        #region Private Fields

        private ISkillCard lastUsedCard;
        private IPlayerHandManager handManager;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 에디터에서 설정된 데이터로 초기화합니다.
        /// </summary>
        private void Awake()
        {
            if (Data != null)
                InitializeCharacter(Data);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 외부에서 캐릭터 데이터를 설정하고 초기화합니다.
        /// </summary>
        /// <param name="data">플레이어 캐릭터 데이터</param>
        public void SetCharacterData(PlayerCharacterData data)
        {
            Data = data;
            InitializeCharacter(data);
        }

        /// <summary>
        /// 체력 초기화 및 UI 갱신
        /// </summary>
        /// <param name="data">플레이어 데이터</param>
        private void InitializeCharacter(PlayerCharacterData data)
        {
            SetMaxHP(data.MaxHP);
            UpdateUI();
        }

        #endregion

        #region UI Handling

        /// <summary>
        /// 이름, 체력, 초상화 등 UI 갱신
        /// </summary>
        private void UpdateUI()
        {
            nameText.text = Data?.DisplayName ?? "???";
            hpText.text = $"{currentHP} / {Data?.MaxHP ?? 0}";
            portraitImage.sprite = Data?.Portrait;
        }

        /// <summary>
        /// 데미지 처리 후 UI 갱신
        /// </summary>
        /// <param name="amount">피해량</param>
        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            UpdateUI();
        }

        /// <summary>
        /// 회복 처리 후 UI 갱신
        /// </summary>
        /// <param name="amount">회복량</param>
        public override void Heal(int amount)
        {
            base.Heal(amount);
            UpdateUI();
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
            Debug.Log($"[PlayerCharacter] 카드 복귀: {card?.CardData?.Name}");
            // 실제 핸드 복원 로직은 handManager 내부에 구현되어야 함
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
        public override string GetCharacterName() => Data?.DisplayName ?? "Unnamed Player";

        /// <summary>
        /// 생존 여부 반환 (명시적 override)
        /// </summary>
        public override bool IsAlive() => base.IsAlive();

        #endregion
        public override void PlayHitEffect()
        {
            Debug.Log($"[PlayHitEffect] {GetCharacterName()} - 이펙트 실행 시도");

            if (hitEffectPrefab == null)
            {
                Debug.LogWarning("[PlayHitEffect] hitEffectPrefab이 연결되지 않았습니다.");
                return;
            }

            // 월드 공간 기준 위치 (캐릭터 머리 위)
            Vector3 spawnPosition = transform.position + new Vector3(0f, 0f, 0f);
            GameObject effectInstance = Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);

            if (effectInstance == null)
            {
                Debug.LogError("[PlayHitEffect] 이펙트 인스턴스 생성 실패");
                return;
            }

            Debug.Log("[PlayHitEffect] 이펙트 인스턴스 생성 완료");

            // 캔버스에 넣지 않고 월드에 그대로 둠
            effectInstance.transform.SetParent(null); // 또는 다른 월드 루트 오브젝트

            // 레이어 설정: 월드 공간에 있지만 "UI"로 렌더되도록
            var psRenderer = effectInstance.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.sortingLayerName = "UI";     // 반드시 존재하는 Sorting Layer
                psRenderer.sortingOrder = 200;          // UI 위로 올라오도록 높은 값

                Debug.Log($"[PlayHitEffect] Sorting Layer: {psRenderer.sortingLayerName}, Order: {psRenderer.sortingOrder}");
            }
            else
            {
                Debug.LogWarning("[PlayHitEffect] ParticleSystemRenderer를 찾을 수 없습니다.");
            }

            Destroy(effectInstance, 2f);
        }
    }
}
