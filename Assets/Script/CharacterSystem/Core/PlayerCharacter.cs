using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.IManager;
using Game.CombatSystem.UI;
using Game.CombatSystem;

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

        [Header("Damage UI")]
        [SerializeField] private Transform hpTextAnchor;
        [SerializeField] private GameObject damageTextPrefab;


        #endregion

        #region Private Fields

        private ISkillCard lastUsedCard;
        private IPlayerHandManager handManager;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 에디터에서 설정된 데이터로 초기화합니다.
        /// </summary>
        protected override void Awake()
        {
            base.Awake(); // 부모 클래스의 Awake 호출
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
            if (Data == null) return;

            nameText.text = Data.DisplayName;
            portraitImage.sprite = Data.Portrait;

            // 체력 숫자 및 색상 설정
            if (currentHP >= Data.MaxHP)
            {
                hpText.text = Data.MaxHP.ToString(); // 최대 체력 표시
                hpText.color = Color.white;          // 회색 또는 흰색으로 보임
            }
            else
            {
                hpText.text = currentHP.ToString();  // 현재 체력만 표시
                hpText.color = Color.red;            // 붉은색으로 표시
            }
        }

        /// <summary>
        /// 데미지 처리 후 UI 갱신
        /// </summary>
        /// <param name="amount">피해량</param>
        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            UpdateUI();

            if (damageTextPrefab != null && hpTextAnchor != null)
            {
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

            if (damageTextPrefab != null && hpTextAnchor != null)
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

        /// <summary>
        /// 사망 처리 (이벤트 발행)
        /// </summary>
        public override void Die()
        {
            // 기본 사망 처리
            base.Die();
            Debug.Log($"[PlayerCharacter] '{GetCharacterName()}' 사망");
        }

        #endregion
    }
}
