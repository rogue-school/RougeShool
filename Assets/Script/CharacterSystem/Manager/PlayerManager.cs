using UnityEngine;
using Zenject;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Interface;
using Game.CharacterSystem.UI;
using Game.CoreSystem.Utility;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 플레이어 캐릭터 전용 관리자입니다.
    /// 캐릭터 생성, 데이터 로드, 상태 관리만 담당합니다.
    /// 핸드 관리나 UI 관리는 다른 매니저에 위임합니다.
    /// </summary>
    public class PlayerManager : BaseCharacterManager<IPlayerCharacter>, IPlayerManager
    {
        #region 플레이어 캐릭터 전용 설정

        [Header("플레이어 캐릭터 설정")]
        [Tooltip("플레이어 캐릭터 프리팹")]
        [SerializeField] private PlayerCharacter playerPrefab;

        [Tooltip("기본 플레이어 데이터")]
        [SerializeField] private PlayerCharacterData defaultPlayerData;

        [Header("플레이어 UI 연결")]
        [Tooltip("플레이어 HUD UI 컨트롤러 (선택사항)")]
        [SerializeField] private PlayerCharacterUIController playerUI;

        #endregion

        #region Private Fields

        private IPlayerHandManager handManager;
        private IGameStateManager gameStateManager;

        #endregion

        #region DI

        /// <summary>
        /// Zenject 의존성 주입.
        /// </summary>
        [Inject]
        public void Construct(
            IPlayerHandManager handManager = null,
            IGameStateManager gameStateManager = null)
        {
            this.handManager = handManager;
            this.gameStateManager = gameStateManager;
        }

        #endregion

        #region BaseCoreManager 오버라이드

        /// <summary>
        /// 플레이어 매니저는 캐릭터 프리팹이 필요합니다.
        /// </summary>
        protected override bool RequiresRelatedPrefab() => true;

        /// <summary>
        /// 플레이어 매니저는 UI 컨트롤러가 선택사항입니다.
        /// </summary>
        protected override bool RequiresUIController() => false;

        /// <summary>
        /// 플레이어 캐릭터 프리팹을 반환합니다.
        /// </summary>
        protected override GameObject GetRelatedPrefab() => playerPrefab?.gameObject;

        /// <summary>
        /// 플레이어 UI 컨트롤러를 반환합니다.
        /// </summary>
        protected override MonoBehaviour GetUIController() => playerUI;

        #endregion

        #region 캐릭터 생성 및 설정

        /// <summary>
        /// 플레이어 캐릭터를 생성하고 등록합니다.
        /// </summary>
        public void CreateAndRegisterPlayer()
        {
            CreateAndRegisterCharacter();
        }
        
        /// <summary>
        /// 선택된 캐릭터 데이터를 기반으로 플레이어 캐릭터를 생성 및 등록합니다.
        /// 이미 생성된 경우 핸드 초기화만 수행합니다.
        /// </summary>
        public override void CreateAndRegisterCharacter()
        {
            if (currentCharacter != null)
            {
                InitializeHandManager();
                return;
            }

            // 캐릭터 선택은 DI로 주입된 GameStateManager 사용
            var selectedData = gameStateManager?.SelectedCharacter;
            if (selectedData == null)
            {
                GameLogger.LogError("선택된 캐릭터 데이터가 없습니다. GameStateManager에서 캐릭터를 선택해주세요.", GameLogger.LogCategory.Character);
                return;
            }

            if (!ValidateReferences())
            {
                return;
            }

            var instance = CreateCharacterInstance();

            if (!instance.TryGetComponent(out IPlayerCharacter character))
            {
                GameLogger.LogError("IPlayerCharacter 컴포넌트가 누락되었습니다.", GameLogger.LogCategory.Character);
                Destroy(instance);
                return;
            }

            character.SetCharacterData(selectedData);
            SetCharacter(character);
            InitializeHandManager();

            // UI 대상 연결(있을 때만)
            ConnectCharacterUI(character);
            
            // 플레이어 전용 UI 연결
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
            // 먼저 핸드 소유자를 지정해야 덱 조회가 정상 동작합니다.
            if (currentCharacter != null)
            {
                handManager.SetPlayer(currentCharacter);
            }

            // 플레이어 스킬카드 생성은 CombatStartupManager에서 처리
            // handManager.GenerateInitialHand(); // 기존 시스템 비활성화
            // 디버깅 로그 제거됨 (단순화)
            currentCharacter.InjectHandManager(handManager);
        }

        /// <summary>
        /// 플레이어 캐릭터를 설정합니다.
        /// </summary>
        /// <param name="player">설정할 플레이어 캐릭터</param>
        public void SetPlayer(IPlayerCharacter player)
        {
            SetCharacter(player);
        }
        
        /// <summary>
        /// 플레이어 캐릭터를 설정합니다.
        /// </summary>
        public override void SetCharacter(IPlayerCharacter character)
        {
            currentCharacter = character;
        }

        #endregion

        #region 캐릭터/핸드 조회

        /// <summary>
        /// 현재 플레이어 캐릭터를 반환합니다.
        /// </summary>
        public override IPlayerCharacter GetCharacter() => currentCharacter;

        /// <summary>
        /// 현재 플레이어 캐릭터를 반환합니다. (호환성 유지)
        /// </summary>
        public IPlayerCharacter GetPlayer() => currentCharacter;

        /// <summary>
        /// 플레이어 핸드 매니저를 반환합니다.
        /// </summary>
        public IPlayerHandManager GetPlayerHandManager()
        {
            if (handManager == null)
            {
                GameLogger.LogWarning("PlayerHandManager가 주입되지 않았습니다.", GameLogger.LogCategory.Character);
            }
            return handManager;
        }

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
        /// 캐릭터 등록을 해제합니다.
        /// </summary>
        public override void UnregisterCharacter()
        {
            currentCharacter = null;
            GameLogger.LogInfo("플레이어 캐릭터 등록 해제", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 매니저 상태를 초기화합니다.
        /// </summary>
        public override void ResetCharacter()
        {
            UnregisterCharacter();
            GameLogger.LogInfo("PlayerManager 초기화 완료", GameLogger.LogCategory.Character);
        }

        #endregion
    }
}
