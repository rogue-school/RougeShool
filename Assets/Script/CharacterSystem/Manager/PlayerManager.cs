using UnityEngine;
using Zenject;
using Game.CharacterSystem.Interface;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Interface;
using Game.CharacterSystem.UI;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 플레이어 캐릭터 및 핸드 매니저를 관리하는 클래스입니다.
    /// 캐릭터 생성, 선택 데이터 로드, 핸드 초기화 등을 처리합니다.
    /// </summary>
    public class PlayerManager : MonoBehaviour, IPlayerManager
    {
        #region Serialized Fields

        [Header("프리팹 및 슬롯")]
        [Tooltip("플레이어 캐릭터 프리팹")]
        [SerializeField] private GameObject playerPrefab;

        [Tooltip("플레이어 캐릭터가 배치될 슬롯 위치")]
        [SerializeField] private Transform playerSlot;

        [Header("플레이어 UI")]
        [Tooltip("플레이어 HUD UI 컨트롤러(씬 상 존재)")]
        [SerializeField] private PlayerCharacterUIController playerUI;

        [Header("기본 캐릭터 데이터")]
        [Tooltip("캐릭터 선택이 없을 경우 사용할 기본 캐릭터 데이터")]
        [SerializeField] private PlayerCharacterData defaultCharacterData;

        #endregion

        #region Private Fields

        private IPlayerCharacter playerCharacter;
        private IPlayerHandManager handManager;
        private IGameStateManager gameStateManager;

        #endregion

        #region DI

        /// <summary>
        /// Zenject 의존성 주입.
        /// </summary>
        [Inject]
        public void Construct(
            IPlayerHandManager handManager,
            IGameStateManager gameStateManager)
        {
            this.handManager = handManager;
            this.gameStateManager = gameStateManager;
        }

        #endregion

        #region 캐릭터 생성 및 설정

        /// <summary>
        /// 선택된 캐릭터 데이터를 기반으로 플레이어 캐릭터를 생성 및 등록합니다.
        /// 이미 생성된 경우 핸드 초기화만 수행합니다.
        /// </summary>
        public void CreateAndRegisterPlayer()
        {
            if (playerCharacter != null)
            {
                InitializeHandManager();
                return;
            }

            // 캐릭터 선택은 DI로 주입된 GameStateManager 사용
            var selectedData = gameStateManager?.SelectedCharacter ?? defaultCharacterData;
            if (selectedData == null)
            {
                Debug.LogError("[PlayerManager] 선택된 캐릭터 데이터가 없습니다.");
                return;
            }

            if (playerPrefab == null || playerSlot == null)
            {
                Debug.LogError("[PlayerManager] 프리팹 또는 슬롯 참조가 누락되었습니다.");
                return;
            }

            var instance = Instantiate(playerPrefab, playerSlot);

            if (!instance.TryGetComponent(out IPlayerCharacter character))
            {
                Debug.LogError("[PlayerManager] IPlayerCharacter 컴포넌트 누락.");
                Destroy(instance);
                return;
            }

            character.SetCharacterData(selectedData);
            SetPlayer(character);
            InitializeHandManager();

            // UI 대상 연결(있을 때만)
            if (playerUI != null && character is ICharacter ic)
            {
                playerUI.SetTarget(ic);
            }
        }

        /// <summary>
        /// 핸드 매니저를 초기화하고 플레이어 캐릭터에 주입합니다.
        /// </summary>
        private void InitializeHandManager()
        {
            handManager.GenerateInitialHand();
            handManager.LogPlayerHandSlotStates();
            playerCharacter.InjectHandManager(handManager);
        }

        /// <summary>
        /// 플레이어 캐릭터를 설정합니다.
        /// </summary>
        public void SetPlayer(IPlayerCharacter player)
        {
            playerCharacter = player;
        }

        #endregion

        #region 캐릭터/핸드 조회

        /// <summary>
        /// 현재 플레이어 캐릭터를 반환합니다.
        /// </summary>
        public IPlayerCharacter GetPlayer() => playerCharacter;

        /// <summary>
        /// 플레이어 핸드 매니저를 반환합니다.
        /// </summary>
        public IPlayerHandManager GetPlayerHandManager() => handManager;

        /// <summary>
        /// 지정 슬롯의 스킬 카드를 반환합니다.
        /// </summary>
        public ISkillCard GetCardInSlot(SkillCardSlotPosition pos) =>
            handManager?.GetCardInSlot(pos);

        /// <summary>
        /// 지정 슬롯의 카드 UI를 반환합니다.
        /// </summary>
        public ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos) =>
            handManager?.GetCardUIInSlot(pos);

        #endregion

        #region 초기화

        /// <summary>
        /// 매니저 상태를 초기화합니다. (확장용)
        /// </summary>
        public void Reset()
        {
            Debug.Log("[PlayerManager] Reset 호출됨.");
        }

        #endregion
    }
}
