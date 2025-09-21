using UnityEngine;
using Zenject;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Utility;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 플레이어 캐릭터 전용 관리자입니다.
    /// 캐릭터 생성, 데이터 로드, 상태 관리만 담당합니다.
    /// 핸드 관리나 UI 관리는 다른 매니저에 위임합니다.
    /// </summary>
    public class PlayerManager : BaseCharacterManager<ICharacter>
    {
        #region 플레이어 캐릭터 전용 설정

        [Header("플레이어 UI 연결 (씬 UI 통합)")]
        [Tooltip("플레이어 캐릭터 UI 컨트롤러 - 씬에 있는 PlayerCharacterUIController를 자동으로 찾아 연결합니다")]
        [SerializeField] private MonoBehaviour playerUI;

        [Header("자동 UI 연결 설정")]
        [Tooltip("씬에서 PlayerCharacterUIController를 자동으로 찾아 연결할지 여부")]
        [SerializeField] private bool autoConnectUI = true;

        #endregion

        #region Private Fields

        private IPlayerHandManager handManager;
        private IGameStateManager gameStateManager;
        private PlayerCharacterData cachedSelectedCharacter;

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
        /// 베이스 클래스의 characterPrefab을 사용합니다.
        /// </summary>
        protected override GameObject GetRelatedPrefab() => characterPrefab;

        /// <summary>
        /// 플레이어 UI 컨트롤러를 반환합니다.
        /// 씬에 통합된 UI를 자동으로 찾아 연결합니다.
        /// </summary>
        protected override MonoBehaviour GetUIController()
        {
            // 수동으로 설정된 UI가 있으면 사용
            if (playerUI != null)
                return playerUI;

            // 자동 연결이 활성화되어 있으면 씬에서 찾기
            if (autoConnectUI)
            {
                var foundUI = FindFirstObjectByType<Game.CharacterSystem.UI.PlayerCharacterUIController>();
                if (foundUI != null)
                {
                    playerUI = foundUI;
                    GameLogger.LogInfo("씬에서 PlayerCharacterUIController를 자동으로 찾아 연결했습니다.", GameLogger.LogCategory.Character);
                    return playerUI;
                }
            }

            GameLogger.LogWarning("PlayerCharacterUIController를 찾을 수 없습니다. 씬에 UI가 있는지 확인해주세요.", GameLogger.LogCategory.Character);
            return null;
        }

        #endregion

        #region BaseCharacterManager 오버라이드

        /// <summary>
        /// 플레이어 매니저 특화 초기화 로직
        /// </summary>
        protected override System.Collections.IEnumerator InitializeCharacterManager()
        {
            // 초기화 완료 후 자동으로 캐릭터 생성
            yield return new WaitForEndOfFrame();
            
            // 캐릭터 데이터를 안전하게 가져오기
            var selectedData = GetSelectedCharacterData();
            
            if (selectedData != null)
            {
                GameLogger.LogInfo($"선택된 캐릭터 발견: {selectedData.DisplayName}", GameLogger.LogCategory.Character);
                CreateAndRegisterCharacter();
            }
            else
            {
                GameLogger.LogWarning("선택된 캐릭터 데이터가 없습니다. 수동으로 캐릭터를 생성해주세요.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// GameStateManager를 찾는 안전한 방법
        /// </summary>
        private IGameStateManager FindGameStateManager()
        {
            // 1. DI로 주입된 매니저 사용
            if (gameStateManager != null)
            {
                return gameStateManager;
            }

            // 2. 씬에서 직접 찾기
            var foundManager = FindFirstObjectByType<GameStateManager>();
            if (foundManager != null)
            {
                return foundManager;
            }

            // 3. 모든 GameObject에서 찾기
            var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                var manager = obj.GetComponent<GameStateManager>();
                if (manager != null)
                {
                    return manager;
                }
            }

            GameLogger.LogWarning("GameStateManager를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
            return null;
        }

        /// <summary>
        /// 선택된 캐릭터 데이터를 가져오는 안전한 방법
        /// </summary>
        private PlayerCharacterData GetSelectedCharacterData()
        {
            // 1. 캐시된 데이터 사용
            if (cachedSelectedCharacter != null)
            {
                return cachedSelectedCharacter;
            }

            // 2. GameStateManager에서 가져오기
            var gameStateManager = FindGameStateManager();
            if (gameStateManager?.SelectedCharacter != null)
            {
                cachedSelectedCharacter = gameStateManager.SelectedCharacter;
                return cachedSelectedCharacter;
            }


            return null;
        }

        /// <summary>
        /// 캐릭터 데이터를 캐시에 저장
        /// </summary>
        public void CacheSelectedCharacter(PlayerCharacterData characterData)
        {
            cachedSelectedCharacter = characterData;
            GameLogger.LogInfo($"캐릭터 데이터 캐시됨: {characterData?.DisplayName}", GameLogger.LogCategory.Character);
        }

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

            // 캐릭터 선택은 안전한 방법으로 가져오기
            var selectedData = GetSelectedCharacterData();
            if (selectedData == null)
            {
                GameLogger.LogError("선택된 캐릭터 데이터가 없습니다. GameStateManager에서 캐릭터를 선택해주세요.", GameLogger.LogCategory.Character);
                return;
            }

            if (!ValidateReferences())
            {
                return;
            }

            // 캐릭터 인스턴스 생성
            var instance = CreateCharacterInstance();

            // 캐릭터 발가락 위치 설정
            SetCharacterFootPosition(instance);

            if (!instance.TryGetComponent(out ICharacter character))
            {
                GameLogger.LogError("ICharacter 컴포넌트가 누락되었습니다.", GameLogger.LogCategory.Character);
                Destroy(instance);
                return;
            }

            // 데이터 설정 (Awake 이후에 설정하여 경고 방지)
            character.SetCharacterData(selectedData);
            
            // 리소스 초기화
            InitializeResource(selectedData);
            
            // 캐릭터 등록
            SetCharacter(character);
            
            // UI 연결 (씬에 통합된 UI)
            ConnectPlayerUI(character);
            
            // 핸드 매니저 초기화
            InitializeHandManager();
            
            GameLogger.LogInfo($"플레이어 캐릭터 생성 완료: {character.GetCharacterName()}", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 플레이어 UI를 연결합니다. (씬에 통합된 UI)
        /// </summary>
        /// <param name="character">연결할 플레이어 캐릭터</param>
        private void ConnectPlayerUI(ICharacter character)
        {
            var uiController = GetUIController();
            if (uiController == null)
            {
                GameLogger.LogWarning("플레이어 UI 컨트롤러를 찾을 수 없습니다. UI 연결을 건너뜁니다.", GameLogger.LogCategory.Character);
                return;
            }

            // PlayerCharacterUIController인지 확인
            if (uiController is Game.CharacterSystem.UI.PlayerCharacterUIController playerUIController)
            {
                // PlayerCharacter로 캐스팅하여 연결
                if (character is PlayerCharacter playerCharacter)
                {
                    playerUIController.Initialize(playerCharacter);
                    GameLogger.LogInfo($"플레이어 UI 연결 완료: {character.GetCharacterName()}", GameLogger.LogCategory.Character);
                }
                else
                {
                    GameLogger.LogWarning("캐릭터가 PlayerCharacter 타입이 아닙니다. UI 연결을 건너뜁니다.", GameLogger.LogCategory.Character);
                }
            }
            else
            {
                GameLogger.LogWarning("UI 컨트롤러가 PlayerCharacterUIController 타입이 아닙니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 핸드 매니저를 초기화하고 플레이어 캐릭터에 주입합니다.
        /// </summary>
        private void InitializeHandManager()
        {
            if (handManager == null)
            {
                GameLogger.LogWarning("handManager가 null입니다. 핸드 매니저 초기화를 건너뜁니다.", GameLogger.LogCategory.Character);
                return;
            }

            if (currentCharacter == null)
            {
                GameLogger.LogWarning("currentCharacter가 null입니다. 핸드 매니저 초기화를 건너뜁니다.", GameLogger.LogCategory.Character);
                return;
            }

            // 먼저 핸드 소유자를 지정해야 덱 조회가 정상 동작합니다.
            handManager.SetPlayer(currentCharacter);

            // 플레이어 스킬카드 생성은 CombatStartupManager에서 처리
            // handManager.GenerateInitialHand(); // 기존 시스템 비활성화
            // 디버깅 로그 제거됨 (단순화)
            currentCharacter.InjectHandManager(handManager);
            
            GameLogger.LogInfo($"핸드 매니저 초기화 완료: {currentCharacter.GetCharacterName()}", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 플레이어 캐릭터를 설정합니다.
        /// </summary>
        /// <param name="player">설정할 플레이어 캐릭터</param>
        public void SetPlayer(ICharacter player)
        {
            SetCharacter(player);
        }
        
        /// <summary>
        /// 플레이어 캐릭터를 설정합니다.
        /// </summary>
        public override void SetCharacter(ICharacter character)
        {
            currentCharacter = character;
        }

        #endregion

        #region 캐릭터/핸드 조회

        /// <summary>
        /// 현재 플레이어 캐릭터를 반환합니다.
        /// </summary>
        public override ICharacter GetCharacter() => currentCharacter;

        /// <summary>
        /// 현재 플레이어 캐릭터를 반환합니다. (호환성 유지)
        /// </summary>
        public ICharacter GetPlayer() => currentCharacter;

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
        
        #region 리소스 관리 (PlayerResourceManager 통합)
        
        private int currentResource;
        private int maxResource;
        private string resourceName;
        
        /// <summary>현재 리소스</summary>
        public int CurrentResource => currentResource;
        
        /// <summary>최대 리소스</summary>
        public int MaxResource => maxResource;
        
        /// <summary>리소스 이름</summary>
        public string ResourceName => resourceName;
        
        /// <summary>
        /// 리소스를 소모합니다.
        /// </summary>
        /// <param name="amount">소모할 양</param>
        /// <returns>소모 성공 여부</returns>
        public bool ConsumeResource(int amount)
        {
            if (amount < 0)
            {
                GameLogger.LogWarning("음수 리소스 소모는 불가능합니다.", GameLogger.LogCategory.Character);
                return false;
            }

            if (currentResource < amount)
            {
                GameLogger.LogWarning($"리소스 부족: {currentResource}/{maxResource} (필요: {amount})", GameLogger.LogCategory.Character);
                return false;
            }

            currentResource -= amount;
            GameLogger.LogInfo($"리소스 소모: {amount} (남은 양: {currentResource}/{maxResource})", GameLogger.LogCategory.Character);
            return true;
        }
        
        /// <summary>
        /// 리소스를 회복합니다.
        /// </summary>
        /// <param name="amount">회복할 양</param>
        public void RestoreResource(int amount)
        {
            if (amount < 0)
            {
                GameLogger.LogWarning("음수 리소스 회복은 불가능합니다.", GameLogger.LogCategory.Character);
                return;
            }

            currentResource = Mathf.Min(currentResource + amount, maxResource);
            GameLogger.LogInfo($"리소스 회복: {amount} (현재 양: {currentResource}/{maxResource})", GameLogger.LogCategory.Character);
        }
        
        /// <summary>
        /// 리소스를 최대치로 회복합니다.
        /// </summary>
        public void RestoreToMax()
        {
            currentResource = maxResource;
            GameLogger.LogInfo($"리소스 최대치 회복: {currentResource}/{maxResource}", GameLogger.LogCategory.Character);
        }
        
        /// <summary>
        /// 충분한 리소스가 있는지 확인합니다.
        /// </summary>
        /// <param name="amount">확인할 양</param>
        /// <returns>충분한 리소스 여부</returns>
        public bool HasEnoughResource(int amount)
        {
            return currentResource >= amount;
        }
        
        /// <summary>
        /// 리소스를 초기화합니다.
        /// </summary>
        /// <param name="characterData">캐릭터 데이터</param>
        private void InitializeResource(PlayerCharacterData characterData)
        {
            if (characterData == null)
            {
                GameLogger.LogError("캐릭터 데이터가 null입니다.", GameLogger.LogCategory.Character);
                return;
            }

            maxResource = characterData.MaxResource;
            resourceName = characterData.ResourceName;
            currentResource = maxResource; // 시작 시 최대치로 설정

            GameLogger.LogInfo($"{characterData.DisplayName} 리소스 초기화: {resourceName} {currentResource}/{maxResource}", GameLogger.LogCategory.Character);
        }
        
        #endregion
        
        #region 캐릭터 초기화 (PlayerCharacterInitializer 통합)
        
        /// <summary>
        /// 플레이어 캐릭터를 생성하고 슬롯에 배치합니다.
        /// </summary>
        public void SetupPlayerCharacter()
        {
            if (!ValidateData())
            {
                GameLogger.LogError("유효하지 않은 초기화 데이터입니다.", GameLogger.LogCategory.Character);
                return;
            }

            var slot = GetPlayerSlot();
            if (slot == null) return;

            Transform slotTransform = ((MonoBehaviour)slot).transform;

            // 기존 자식 제거
            foreach (Transform child in slotTransform)
                Destroy(child.gameObject);

            // 플레이어 캐릭터 생성 및 부모 설정
            var playerCharacterInstance = Instantiate(characterPrefab);
            playerCharacterInstance.name = "PlayerCharacter";
            playerCharacterInstance.transform.SetParent(slotTransform, false);

            // 캐릭터 발가락 위치 설정
            SetCharacterFootPosition(playerCharacterInstance);

            // PlayerCharacter 컴포넌트 확인
            if (!playerCharacterInstance.TryGetComponent(out PlayerCharacter playerCharacterComponent))
            {
                GameLogger.LogError("PlayerCharacter 컴포넌트를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                Destroy(playerCharacterInstance.gameObject);
                return;
            }

            // 데이터 설정
            var data = ResolvePlayerData();
            if (data == null)
            {
                GameLogger.LogError("캐릭터 데이터가 없습니다.", GameLogger.LogCategory.Character);
                return;
            }

            playerCharacterComponent.SetCharacterData(data);
            InitializeResource(data); // 리소스 초기화
            slot.SetCharacter(playerCharacterComponent);
            SetPlayer(playerCharacterComponent);
            
            GameLogger.LogInfo("플레이어 캐릭터 초기화 완료", GameLogger.LogCategory.Character);
        }
        
        /// <summary>
        /// 플레이어 캐릭터 프리팹의 유효성을 검사합니다.
        /// </summary>
        private bool ValidateData()
        {
            return characterPrefab != null;
        }
        
        /// <summary>
        /// 플레이어 캐릭터를 배치할 슬롯을 조회합니다.
        /// </summary>
        private ICharacterSlot GetPlayerSlot()
        {
            // 슬롯 레지스트리에서 플레이어 슬롯 조회
            // 실제 구현은 슬롯 시스템에 따라 달라질 수 있음
            return null; // TODO: 실제 슬롯 조회 로직 구현
        }
        
        /// <summary>
        /// 플레이어 캐릭터 데이터 선택 로직입니다.
        /// GetSelectedCharacterData()를 사용합니다.
        /// </summary>
        private PlayerCharacterData ResolvePlayerData()
        {
            return GetSelectedCharacterData();
        }
        
        #endregion

        #region 캐릭터 위치 설정 유틸리티

        /// <summary>
        /// 캐릭터의 하단 외각(발바닥 라인)이 트랜스폼 위치에 오도록 RectTransform을 설정합니다.
        /// </summary>
        /// <param name="characterInstance">설정할 캐릭터 인스턴스</param>
        /// <param name="footOffset">발바닥 오프셋 (기본값: 0f, 음수값으로 더 아래쪽 설정 가능)</param>
        public static void SetCharacterFootPosition(GameObject characterInstance, float footOffset = 0f)
        {
            if (characterInstance.TryGetComponent(out RectTransform rt))
            {
                rt.anchorMin = new Vector2(0.5f, 0f);      // 하단 중앙 앵커
                rt.anchorMax = new Vector2(0.5f, 0f);      // 하단 중앙 앵커
                rt.pivot = new Vector2(0.5f, footOffset);   // 하단 외각 피벗 (발바닥 라인 + 오프셋)
                rt.anchoredPosition = Vector2.zero;
                rt.localPosition = Vector3.zero;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
                
                GameLogger.LogInfo($"캐릭터 하단 외각 위치 설정 완료: {characterInstance.name} (오프셋: {footOffset})", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 캐릭터의 하단 외각(발바닥 라인)이 트랜스폼 위치에 오도록 RectTransform을 설정합니다. (기본 오프셋 사용)
        /// </summary>
        /// <param name="characterInstance">설정할 캐릭터 인스턴스</param>
        public static void SetCharacterFootPosition(GameObject characterInstance)
        {
            SetCharacterFootPosition(characterInstance, 0f); // 기본값: 프리팹 하단 외각
        }

        #endregion
    }
}
