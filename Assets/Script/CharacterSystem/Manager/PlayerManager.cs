using UnityEngine;
using System;
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
using DG.Tweening;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 플레이어 캐릭터 전용 관리자입니다.
    /// 캐릭터 생성, 데이터 로드, 상태 관리만 담당합니다.
    /// 핸드 관리나 UI 관리는 다른 매니저에 위임합니다.
    /// </summary>
    public class PlayerManager : BaseCharacterManager<ICharacter>
    {
        public static PlayerManager Instance { get; private set; }
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
        [Zenject.Inject(Optional = true)] private Game.SkillCardSystem.Service.SkillCardRegistry _skillCardRegistry;
        private PlayerCharacterData cachedSelectedCharacter;

        // UI 컨트롤러 (선택적 의존성)
        private Game.CharacterSystem.UI.PlayerCharacterUIController playerCharacterUIController;

        #endregion

        #region Unity 생명주기 / 전역 접근 지원

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLogger.LogWarning("[PlayerManager] 중복 인스턴스가 감지되었습니다. 기존 인스턴스를 유지하고 새 인스턴스를 제거합니다.", GameLogger.LogCategory.Character);
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// 플레이어 캐릭터가 생성되고 초기화가 완료되었을 때 발생하는 이벤트
        /// </summary>
        public event Action<ICharacter> OnPlayerCharacterReady;

        /// <summary>
        /// 리소스를 소모했을 때 발생하는 이벤트 (amount)
        /// </summary>
        public event Action<int> OnResourceConsumed;

        /// <summary>
        /// 리소스를 획득/회복했을 때 발생하는 이벤트 (amount)
        /// </summary>
        public event Action<int> OnResourceRestored;

        #endregion

        #region DI

        /// <summary>
        /// Zenject 의존성 주입.
        /// </summary>
        [Inject]
        public void Construct(
            IPlayerHandManager handManager = null,
            IGameStateManager gameStateManager = null,
            [InjectOptional] Game.CharacterSystem.UI.PlayerCharacterUIController playerUI = null)
        {
            this.handManager = handManager;
            this.gameStateManager = gameStateManager;
            this.playerCharacterUIController = playerUI;
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
        /// DI로 주입된 UI를 사용합니다.
        /// </summary>
        protected override MonoBehaviour GetUIController()
        {
            // 수동으로 설정된 UI가 있으면 사용
            if (playerUI != null)
                return playerUI;

            // DI로 주입된 UI 사용
            if (autoConnectUI && playerCharacterUIController != null)
            {
                playerUI = playerCharacterUIController;
                GameLogger.LogInfo("DI로 주입된 PlayerCharacterUIController를 사용합니다.", GameLogger.LogCategory.Character);
                return playerUI;
            }

            GameLogger.LogWarning("PlayerCharacterUIController를 찾을 수 없습니다. DI 설정을 확인해주세요.", GameLogger.LogCategory.Character);
            return null;
        }

        #endregion

        #region BaseCharacterManager 오버라이드

        /// <summary>
        /// 플레이어 매니저 특화 초기화 로직
        /// </summary>
        protected override System.Collections.IEnumerator InitializeCharacterManager()
        {
            yield return new WaitForEndOfFrame();

            // 캐릭터 데이터 확인
            var selectedData = GetSelectedCharacterData();

            if (selectedData != null)
            {
                GameLogger.LogInfo($"선택된 캐릭터: {selectedData.DisplayName} - 플레이어 캐릭터 생성 시작", GameLogger.LogCategory.Character);

                // 플레이어 캐릭터 자동 생성
                CreateAndRegisterCharacter();
            }
            else
            {
                GameLogger.LogWarning("선택된 캐릭터 데이터가 없습니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// GameStateManager를 찾는 안전한 방법
        /// </summary>
        private IGameStateManager FindGameStateManager()
        {
            // DI로 주입된 매니저만 사용
            if (gameStateManager != null)
            {
                return gameStateManager;
            }

            GameLogger.LogWarning("GameStateManager가 주입되지 않았습니다. 선택된 캐릭터를 가져올 수 없습니다.", GameLogger.LogCategory.Character);
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

            // 3. PlayerPrefs 폴백 (선택 타입만 저장되어 있는 경우)
            try
            {
                string typeStr = PlayerPrefs.GetString("SELECTED_CHARACTER_TYPE", string.Empty);
                if (!string.IsNullOrEmpty(typeStr) && System.Enum.TryParse<PlayerCharacterType>(typeStr, out var ct))
                {
                    // 가능한 모든 PlayerCharacterData를 탐색하여 타입이 일치하는 첫 번째 자산을 선택
                    var allDatas = Resources.LoadAll<PlayerCharacterData>(string.Empty);
                    foreach (var d in allDatas)
                    {
                        if (d != null && d.CharacterType == ct)
                        {
                            cachedSelectedCharacter = d;
                            GameLogger.LogInfo($"[PlayerManager] PlayerPrefs 폴백으로 캐릭터 복구: {d.DisplayName}", GameLogger.LogCategory.Character);
                            return cachedSelectedCharacter;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogWarning($"[PlayerManager] PlayerPrefs 폴백 중 예외: {ex.Message}", GameLogger.LogCategory.Character);
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
                // 캐릭터가 이미 존재해도 스택 초기화는 실행 (새게임 시 필요)
                InitializeSkillCardStacks();
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
			// 등장 연출 (왼쪽 바깥에서 자리로) 완료 후 Ready 이벤트 발생
			var entrance = TryPlayEntranceAnimation(instance.transform, fromLeft: true);

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
            
            // 스킬카드 스택 초기화 (새 캐릭터 생성 시)
            InitializeSkillCardStacks();
            
            // 캐릭터 등록
            SetCharacter(character);
            
            // UI 연결 (씬에 통합된 UI)
            ConnectPlayerUI(character);
            
            // 핸드 매니저 초기화
            InitializeHandManager();
            
			// 플레이어 캐릭터 준비 완료 이벤트를 입장 연출 종료 시점에 발생
			if (entrance != null)
			{
				entrance.OnComplete(() =>
				{
					// 입장 연출 완료 후 대기 애니메이션 재생
					if (character is PlayerCharacter playerCharacter)
					{
						playerCharacter.PlayIdleAnimation();
					}
					
					OnPlayerCharacterReady?.Invoke(character);
					GameLogger.LogInfo($"플레이어 캐릭터 생성/입장 완료: {character.GetCharacterName()}", GameLogger.LogCategory.Character);
				});
			}
			else
			{
				// 연출 없이 생성된 경우 즉시 대기 애니메이션 재생
				if (character is PlayerCharacter playerCharacter)
				{
					playerCharacter.PlayIdleAnimation();
				}
				
				OnPlayerCharacterReady?.Invoke(character);
				GameLogger.LogInfo($"플레이어 캐릭터 생성 완료(연출 없음): {character.GetCharacterName()}", GameLogger.LogCategory.Character);
			}
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

		#region 연출 유틸리티 (입장 애니메이션)

		/// <summary>
		/// 캐릭터가 화면 밖에서 슬라이드 인 되는 연출을 수행합니다.
		/// RectTransform이 있으면 DOAnchorPos, 아니면 DOMove를 사용합니다.
		/// </summary>
		private Tween TryPlayEntranceAnimation(Transform target, bool fromLeft)
		{
			if (target == null) return null;
			const float duration = 1.5f;
			var ease = Ease.InOutCubic; // 처음 느림 → 빠름 → 마지막 살짝 느림

			if (target is RectTransform rt)
			{
				Vector2 end = rt.anchoredPosition;
				Vector2 start = new Vector2(fromLeft ? -1100f : 1100f, end.y);
				rt.anchoredPosition = start;
				return rt.DOAnchorPos(end, duration).SetEase(ease);
			}
			else
			{
				Vector3 end = target.position;
				Vector3 start = new Vector3(fromLeft ? -1100f : 1100f, end.y, end.z);
				target.position = start;
				return target.DOMove(end, duration).SetEase(ease);
			}
		}

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
        
        #region 리소스 관리 (PlayerResourceManager로 위임)

        private PlayerResourceManager resourceManager = new PlayerResourceManager();

        /// <summary>현재 리소스</summary>
        public int CurrentResource => resourceManager.CurrentResource;

        /// <summary>최대 리소스</summary>
        public int MaxResource => resourceManager.MaxResource;

        /// <summary>리소스 이름</summary>
        public string ResourceName => resourceManager.ResourceName;

        /// <summary>리소스 변경 이벤트</summary>
        public event System.Action<int, int> OnResourceChanged
        {
            add => resourceManager.OnResourceChanged += value;
            remove => resourceManager.OnResourceChanged -= value;
        }

        /// <summary>
        /// 리소스를 소모합니다.
        /// </summary>
        /// <param name="amount">소모할 양</param>
        /// <returns>소모 성공 여부</returns>
        public bool ConsumeResource(int amount)
        {
            bool consumed = resourceManager.ConsumeResource(amount);
            if (consumed)
            {
                try { OnResourceConsumed?.Invoke(amount); }
                catch (Exception ex)
                {
                    GameLogger.LogWarning($"[PlayerManager] OnResourceConsumed 핸들러 예외: {ex.Message}", GameLogger.LogCategory.Character);
                }
            }
            return consumed;
        }

        /// <summary>
        /// 리소스를 회복합니다.
        /// </summary>
        /// <param name="amount">회복할 양</param>
        public void RestoreResource(int amount)
        {
            resourceManager.RestoreResource(amount);
            try { OnResourceRestored?.Invoke(amount); }
            catch (Exception ex)
            {
                GameLogger.LogWarning($"[PlayerManager] OnResourceRestored 핸들러 예외: {ex.Message}", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 리소스를 최대치로 회복합니다.
        /// </summary>
        public void RestoreToMax() => resourceManager.RestoreResourceToMax();

        /// <summary>
        /// 충분한 리소스가 있는지 확인합니다.
        /// </summary>
        /// <param name="amount">확인할 양</param>
        /// <returns>충분한 리소스 여부</returns>
        public bool HasEnoughResource(int amount) => resourceManager.HasEnoughResource(amount);

        /// <summary>
        /// 스킬카드 스택을 초기화합니다.
        /// </summary>
        private void InitializeSkillCardStacks()
        {
            try
            {
                if (_skillCardRegistry != null)
                {
                    _skillCardRegistry.ResetAllSkillCardStacks();
                    GameLogger.LogInfo("[PlayerManager] 스킬카드 스택 초기화 완료", GameLogger.LogCategory.Character);
                }
                else
                {
                    GameLogger.LogWarning("[PlayerManager] SkillCardRegistry가 주입되지 않았습니다 - 스택 초기화 건너뜁니다", GameLogger.LogCategory.Character);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[PlayerManager] 스킬카드 스택 초기화 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 리소스를 초기화합니다.
        /// </summary>
        /// <param name="characterData">캐릭터 데이터</param>
        private void InitializeResource(PlayerCharacterData characterData)
        {
            resourceManager.Initialize(characterData);
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
