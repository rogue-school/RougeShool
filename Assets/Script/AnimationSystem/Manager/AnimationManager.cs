using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Core;
using Game.CharacterSystem.Data;
using AnimationSystem.Controllers;

namespace AnimationSystem.Manager
{
    /// <summary>
    /// 애니메이션 시스템의 중앙 매니저
    /// 스킬카드와 캐릭터 애니메이션을 스크립트 기반으로 제어합니다.
    /// </summary>
    public class AnimationManager : MonoBehaviour
    {
        #region Singleton
        public static AnimationManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion
        
        #region Data Collections
        [Header("스킬카드 데이터")]
        [SerializeField] private List<PlayerSkillCard> playerSkillCards = new();
        [SerializeField] private List<EnemySkillCard> enemySkillCards = new();
        
        [Header("캐릭터 데이터")]
        [SerializeField] private List<PlayerCharacterData> playerCharacters = new();
        [SerializeField] private List<EnemyCharacterData> enemyCharacters = new();
        
        [Header("애니메이션 설정")]
        [SerializeField] private bool autoLoadDataOnStart = true;
        [SerializeField] private bool enableAnimationLogging = true;
        #endregion
        
        #region Animation Controllers
        private Dictionary<string, SkillCardAnimationController> skillCardControllers = new();
        private Dictionary<string, CharacterAnimationController> characterControllers = new();
        #endregion
        
        #region Events
        public static System.Action OnDataLoaded;
        public static System.Action<string> OnAnimationStarted;
        public static System.Action<string> OnAnimationCompleted;
        public static System.Action<string> OnAnimationFailed;
        #endregion
        
        #region Initialization
        private void InitializeManager()
        {
            if (autoLoadDataOnStart)
            {
                LoadAllData();
            }
        }
        
        /// <summary>
        /// 모든 데이터를 로드합니다.
        /// </summary>
        [ContextMenu("모든 데이터 로드")]
        public void LoadAllData()
        {
            LoadSkillCardData();
            LoadCharacterData();
            CreateAnimationControllers();
            
            OnDataLoaded?.Invoke();
            LogMessage("애니메이션 매니저 초기화 완료");
        }
        
        /// <summary>
        /// 스킬카드 데이터를 로드합니다.
        /// </summary>
        public void LoadSkillCardData()
        {
            // 플레이어 스킬카드 로드
            var playerCards = Resources.LoadAll<PlayerSkillCard>("Data/SkillCard/Player/PlayerSkillCardData");
            playerSkillCards.Clear();
            playerSkillCards.AddRange(playerCards);
            
            // 적 스킬카드 로드
            var enemyCards = Resources.LoadAll<EnemySkillCard>("Data/SkillCard/Enemy/EnemySkillCardData");
            enemySkillCards.Clear();
            enemySkillCards.AddRange(enemyCards);
            
            LogMessage($"스킬카드 데이터 로드 완료 - 플레이어: {playerSkillCards.Count}개, 적: {enemySkillCards.Count}개");
        }
        
        /// <summary>
        /// 캐릭터 데이터를 로드합니다.
        /// </summary>
        public void LoadCharacterData()
        {
            // 플레이어 캐릭터 로드
            var playerChars = Resources.LoadAll<PlayerCharacterData>("Data/Character/PlayerCharacters");
            playerCharacters.Clear();
            playerCharacters.AddRange(playerChars);
            
            // 적 캐릭터 로드
            var enemyChars = Resources.LoadAll<EnemyCharacterData>("Data/Character/EnemyCharters");
            enemyCharacters.Clear();
            enemyCharacters.AddRange(enemyChars);
            
            LogMessage($"캐릭터 데이터 로드 완료 - 플레이어: {playerCharacters.Count}개, 적: {enemyCharacters.Count}개");
        }
        
        /// <summary>
        /// 각 데이터에 대한 애니메이션 컨트롤러를 생성합니다.
        /// </summary>
        private void CreateAnimationControllers()
        {
            skillCardControllers.Clear();
            characterControllers.Clear();
            
            // 스킬카드 컨트롤러 생성
            foreach (var card in playerSkillCards)
            {
                if (card != null)
                {
                    var controller = new SkillCardAnimationController(card);
                    skillCardControllers[card.name] = controller;
                }
            }
            
            foreach (var card in enemySkillCards)
            {
                if (card != null)
                {
                    var controller = new SkillCardAnimationController(card);
                    skillCardControllers[card.name] = controller;
                }
            }
            
            // 캐릭터 컨트롤러 생성
            foreach (var character in playerCharacters)
            {
                if (character != null)
                {
                    var controller = new CharacterAnimationController(character);
                    characterControllers[character.name] = controller;
                }
            }
            
            foreach (var character in enemyCharacters)
            {
                if (character != null)
                {
                    var controller = new CharacterAnimationController(character);
                    characterControllers[character.name] = controller;
                }
            }
            
            LogMessage($"애니메이션 컨트롤러 생성 완료 - 스킬카드: {skillCardControllers.Count}개, 캐릭터: {characterControllers.Count}개");
        }
        #endregion
        
        #region Public API - Data Access
        /// <summary>
        /// 모든 플레이어 스킬카드를 반환합니다.
        /// </summary>
        public List<PlayerSkillCard> GetAllPlayerSkillCards() => playerSkillCards;
        
        /// <summary>
        /// 모든 적 스킬카드를 반환합니다.
        /// </summary>
        public List<EnemySkillCard> GetAllEnemySkillCards() => enemySkillCards;
        
        /// <summary>
        /// 모든 플레이어 캐릭터를 반환합니다.
        /// </summary>
        public List<PlayerCharacterData> GetAllPlayerCharacters() => playerCharacters;
        
        /// <summary>
        /// 모든 적 캐릭터를 반환합니다.
        /// </summary>
        public List<EnemyCharacterData> GetAllEnemyCharacters() => enemyCharacters;
        
        /// <summary>
        /// 특정 스킬카드를 찾습니다.
        /// </summary>
        public PlayerSkillCard GetPlayerSkillCard(string cardName)
        {
            return playerSkillCards.FirstOrDefault(card => card.name == cardName);
        }
        
        /// <summary>
        /// 특정 적 스킬카드를 찾습니다.
        /// </summary>
        public EnemySkillCard GetEnemySkillCard(string cardName)
        {
            return enemySkillCards.FirstOrDefault(card => card.name == cardName);
        }
        
        /// <summary>
        /// 특정 플레이어 캐릭터를 찾습니다.
        /// </summary>
        public PlayerCharacterData GetPlayerCharacter(string characterName)
        {
            return playerCharacters.FirstOrDefault(character => character.name == characterName);
        }
        
        /// <summary>
        /// 특정 적 캐릭터를 찾습니다.
        /// </summary>
        public EnemyCharacterData GetEnemyCharacter(string characterName)
        {
            return enemyCharacters.FirstOrDefault(character => character.name == characterName);
        }
        #endregion
        
        #region Public API - Animation Control
        /// <summary>
        /// 스킬카드 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="cardName">카드 이름</param>
        /// <param name="animationType">애니메이션 타입</param>
        /// <param name="target">애니메이션 대상 오브젝트</param>
        public void PlaySkillCardAnimation(string cardName, string animationType, GameObject target)
        {
            if (skillCardControllers.TryGetValue(cardName, out var controller))
            {
                controller.PlayAnimation(animationType, target);
                OnAnimationStarted?.Invoke($"{cardName}_{animationType}");
            }
            else
            {
                LogError($"스킬카드 컨트롤러를 찾을 수 없습니다: {cardName}");
                OnAnimationFailed?.Invoke($"{cardName}_{animationType}");
            }
        }
        
        /// <summary>
        /// 캐릭터 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="characterName">캐릭터 이름</param>
        /// <param name="animationType">애니메이션 타입</param>
        /// <param name="target">애니메이션 대상 오브젝트</param>
        public void PlayCharacterAnimation(string characterName, string animationType, GameObject target)
        {
            if (characterControllers.TryGetValue(characterName, out var controller))
            {
                controller.PlayAnimation(animationType, target);
                OnAnimationStarted?.Invoke($"{characterName}_{animationType}");
            }
            else
            {
                LogError($"캐릭터 컨트롤러를 찾을 수 없습니다: {characterName}");
                OnAnimationFailed?.Invoke($"{characterName}_{animationType}");
            }
        }
        
        /// <summary>
        /// 스킬카드 애니메이션 설정을 가져옵니다.
        /// </summary>
        public SkillCardAnimationController GetSkillCardController(string cardName)
        {
            return skillCardControllers.TryGetValue(cardName, out var controller) ? controller : null;
        }
        
        /// <summary>
        /// 캐릭터 애니메이션 설정을 가져옵니다.
        /// </summary>
        public CharacterAnimationController GetCharacterController(string characterName)
        {
            return characterControllers.TryGetValue(characterName, out var controller) ? controller : null;
        }
        #endregion
        
        #region Utility Methods
        private void LogMessage(string message)
        {
            if (enableAnimationLogging)
            {
                Debug.Log($"[AnimationManager] {message}");
            }
        }
        
        private void LogError(string message)
        {
            if (enableAnimationLogging)
            {
                Debug.LogError($"[AnimationManager] {message}");
            }
        }
        
        /// <summary>
        /// 매니저 상태를 출력합니다.
        /// </summary>
        [ContextMenu("상태 출력")]
        public void PrintStatus()
        {
            LogMessage($"=== 애니메이션 매니저 상태 ===");
            LogMessage($"플레이어 스킬카드: {playerSkillCards.Count}개");
            LogMessage($"적 스킬카드: {enemySkillCards.Count}개");
            LogMessage($"플레이어 캐릭터: {playerCharacters.Count}개");
            LogMessage($"적 캐릭터: {enemyCharacters.Count}개");
            LogMessage($"스킬카드 컨트롤러: {skillCardControllers.Count}개");
            LogMessage($"캐릭터 컨트롤러: {characterControllers.Count}개");
        }
        #endregion
    }
} 