using UnityEngine;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// 플레이어 캐릭터 선택을 관리하는 코어 매니저입니다. (Zenject DI 기반)
    /// 선택된 캐릭터 데이터를 저장하고 전달합니다.
    /// </summary>
    public class PlayerCharacterSelectionManager : MonoBehaviour, IPlayerCharacterSelectionManager, ICoreSystemInitializable
    {
        
        [Header("선택된 캐릭터")]
        [SerializeField] private PlayerCharacterData selectedCharacterData;
        
        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        public bool IsInitialized { get; private set; } = false;
        
        /// <summary>
        /// 캐릭터가 선택되었을 때 발생하는 이벤트 (선택된 캐릭터 데이터)
        /// </summary>
        public System.Action<PlayerCharacterData> OnCharacterSelected { get; set; }
        
        // 의존성 주입
        private IGameStateManager gameStateManager;
        
        [Inject]
        public void Construct(IGameStateManager gameStateManager)
        {
            this.gameStateManager = gameStateManager;
        }
        
        /// <summary>
        /// 선택된 캐릭터 데이터를 반환합니다
        /// </summary>
        public PlayerCharacterData SelectedCharacter => selectedCharacterData;
        
        private void Awake()
        {
            GameLogger.LogInfo("PlayerCharacterSelectionManager 초기화 완료", GameLogger.LogCategory.UI);
        }
        
        /// <summary>
        /// 캐릭터 선택
        /// </summary>
        /// <param name="characterData">선택된 캐릭터 데이터</param>
        public void SelectCharacter(PlayerCharacterData characterData)
        {
            if (characterData == null)
            {
                GameLogger.LogError("캐릭터 데이터가 null입니다.", GameLogger.LogCategory.Error);
                return;
            }
            
            // DisplayName이 null이거나 빈 값인지 확인
            if (string.IsNullOrEmpty(characterData.DisplayName))
            {
                GameLogger.LogWarning($"캐릭터 데이터의 DisplayName이 설정되지 않았습니다. 캐릭터 이름: {characterData.name}. 기본값으로 설정합니다.", GameLogger.LogCategory.UI);
                // DisplayName이 없으면 ScriptableObject의 name을 사용
                characterData = CreateCharacterDataWithDisplayName(characterData);
            }
            
            selectedCharacterData = characterData;
            OnCharacterSelected?.Invoke(characterData);
            
            // GameStateManager에도 캐릭터 선택 정보 전달
            if (gameStateManager != null)
            {
                gameStateManager.SelectCharacter(characterData);
                GameLogger.LogInfo($"GameStateManager에 캐릭터 선택 정보 전달: {characterData.DisplayName}", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning("GameStateManager가 null입니다. 캐릭터 선택 정보를 전달할 수 없습니다.", GameLogger.LogCategory.UI);
            }
            
            GameLogger.LogInfo($"캐릭터 선택됨: {characterData.DisplayName}", GameLogger.LogCategory.UI);
        }
        
        /// <summary>
        /// 선택된 캐릭터 데이터 반환
        /// </summary>
        /// <returns>선택된 캐릭터 데이터</returns>
        public PlayerCharacterData GetSelectedCharacter()
        {
            return selectedCharacterData;
        }
        
        /// <summary>
        /// 캐릭터가 선택되었는지 확인
        /// </summary>
        /// <returns>선택 여부</returns>
        public bool HasSelectedCharacter()
        {
            return selectedCharacterData != null;
        }
        
        /// <summary>
        /// DisplayName이 없는 캐릭터 데이터에 기본 DisplayName을 설정합니다.
        /// </summary>
        /// <param name="originalData">원본 캐릭터 데이터</param>
        /// <returns>DisplayName이 설정된 캐릭터 데이터</returns>
        private PlayerCharacterData CreateCharacterDataWithDisplayName(PlayerCharacterData originalData)
        {
            // ScriptableObject의 name을 DisplayName으로 사용
            string displayName = originalData.name;
            
            // 새로운 캐릭터 데이터 생성 (런타임에서만 사용)
            var newData = ScriptableObject.CreateInstance<PlayerCharacterData>();
            
            // 원본 데이터 복사
            newData.name = originalData.name;
            
            // DisplayName 설정
            var displayNameField = typeof(PlayerCharacterData).GetField("<DisplayName>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (displayNameField != null)
            {
                displayNameField.SetValue(newData, displayName);
            }
            
            // 다른 필드들도 복사
            var maxHPField = typeof(PlayerCharacterData).GetField("<MaxHP>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (maxHPField != null)
            {
                maxHPField.SetValue(newData, originalData.MaxHP);
            }
            
            // Portrait는 프리팹으로 관리하므로 필드 복사 제거
            
            var skillDeckField = typeof(PlayerCharacterData).GetField("<SkillDeck>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (skillDeckField != null)
            {
                skillDeckField.SetValue(newData, originalData.SkillDeck);
            }
            
            return newData;
        }
        
        /// <summary>
        /// 선택을 초기화합니다
        /// </summary>
        public void ClearSelection()
        {
            selectedCharacterData = null;
            GameLogger.LogInfo("캐릭터 선택 초기화됨", GameLogger.LogCategory.UI);
        }
        
        /// <summary>
        /// 캐릭터 선택 가능 여부를 확인합니다
        /// </summary>
        /// <param name="characterData">선택할 캐릭터 데이터</param>
        /// <returns>선택 가능 여부</returns>
        public bool CanSelectCharacter(PlayerCharacterData characterData)
        {
            return characterData != null && !string.IsNullOrEmpty(characterData.name);
        }
        
        #region ICoreSystemInitializable 구현
        
        /// <summary>
        /// 코어 시스템을 초기화합니다
        /// </summary>
        /// <returns>초기화 코루틴</returns>
        public System.Collections.IEnumerator Initialize()
        {
            GameLogger.LogInfo("PlayerCharacterSelectionManager 초기화 시작", GameLogger.LogCategory.UI);
            
            // 초기화 완료
            IsInitialized = true;
            
            GameLogger.LogInfo("PlayerCharacterSelectionManager 초기화 완료", GameLogger.LogCategory.UI);
            yield return null;
        }
        
        /// <summary>
        /// 초기화 실패 시 호출
        /// </summary>
        public void OnInitializationFailed()
        {
            GameLogger.LogError("PlayerCharacterSelectionManager 초기화 실패", GameLogger.LogCategory.Error);
            IsInitialized = false;
        }
        
        #endregion
    }
}
