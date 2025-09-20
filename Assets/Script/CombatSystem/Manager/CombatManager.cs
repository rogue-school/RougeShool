using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.SkillCardSystem.Manager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 통합 전투 매니저 - 모든 전투 관련 시스템을 중앙 집중식으로 관리
    /// 기존의 여러 매니저들을 통합하여 단순화된 아키텍처 제공
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        #region 싱글톤 패턴
        
        public static CombatManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        #endregion
        
        #region 통합된 매니저들
        
        // 슬롯 관리 (기존 CombatSlotManager 통합)
        private CombatSlotManager slotManager;
        
        // 턴 관리 (기존 TurnManager 통합)
        private TurnManager turnManager;
        
        // 슬롯 실행 (기존 SlotExecutionSystem 통합)
        private SlotExecutionSystem executionSystem;
        
        // 캐릭터 관리 (기존 PlayerManager, EnemyManager 통합)
        private CharacterManager characterManager;
        
        // 카드 관리 (기존 PlayerHandManager, EnemyHandManager 통합)
        private CardManager cardManager;
        
        #endregion
        
        #region 초기화
        
        /// <summary>
        /// 통합 매니저 초기화
        /// </summary>
        private void Initialize()
        {
            GameLogger.LogInfo("CombatManager 초기화 시작", GameLogger.LogCategory.Combat);
            
            // 기존 매니저들 초기화
            InitializeSlotManager();
            InitializeTurnManager();
            InitializeExecutionSystem();
            InitializeCharacterManager();
            InitializeCardManager();
            
            GameLogger.LogInfo("CombatManager 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 슬롯 매니저 초기화
        /// </summary>
        private void InitializeSlotManager()
        {
            slotManager = FindFirstObjectByType<CombatSlotManager>();
            if (slotManager == null)
            {
                var slotManagerObj = new GameObject("CombatSlotManager");
                slotManager = slotManagerObj.AddComponent<CombatSlotManager>();
            }
            GameLogger.LogInfo("슬롯 매니저 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 턴 매니저 초기화
        /// </summary>
        private void InitializeTurnManager()
        {
            turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager == null)
            {
                var turnManagerObj = new GameObject("TurnManager");
                turnManager = turnManagerObj.AddComponent<TurnManager>();
            }
            GameLogger.LogInfo("턴 매니저 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 실행 시스템 초기화
        /// </summary>
        private void InitializeExecutionSystem()
        {
            executionSystem = FindFirstObjectByType<SlotExecutionSystem>();
            if (executionSystem == null)
            {
                var executionSystemObj = new GameObject("SlotExecutionSystem");
                executionSystem = executionSystemObj.AddComponent<SlotExecutionSystem>();
            }
            GameLogger.LogInfo("실행 시스템 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 캐릭터 매니저 초기화
        /// </summary>
        private void InitializeCharacterManager()
        {
            characterManager = FindFirstObjectByType<CharacterManager>();
            if (characterManager == null)
            {
                var characterManagerObj = new GameObject("CharacterManager");
                characterManager = characterManagerObj.AddComponent<CharacterManager>();
            }
            GameLogger.LogInfo("캐릭터 매니저 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 카드 매니저 초기화
        /// </summary>
        private void InitializeCardManager()
        {
            cardManager = FindFirstObjectByType<CardManager>();
            if (cardManager == null)
            {
                var cardManagerObj = new GameObject("CardManager");
                cardManager = cardManagerObj.AddComponent<CardManager>();
            }
            GameLogger.LogInfo("카드 매니저 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        #endregion
        
        #region 공개 API
        
        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartCombat()
        {
            GameLogger.LogInfo("전투 시작", GameLogger.LogCategory.Combat);
            StartCoroutine(ExecuteCombatSequence());
        }
        
        /// <summary>
        /// 전투 시퀀스 실행
        /// </summary>
        private IEnumerator ExecuteCombatSequence()
        {
            // 1. 캐릭터 스폰
            yield return StartCoroutine(SpawnCharacters());
            
            // 2. 카드 생성
            yield return StartCoroutine(GenerateCards());
            
            // 3. 전투 준비 완료
            CompleteCombatPreparation();
            
            // 4. 플레이어 입력 활성화
            EnablePlayerInput();
        }
        
        /// <summary>
        /// 캐릭터 스폰
        /// </summary>
        private IEnumerator SpawnCharacters()
        {
            GameLogger.LogInfo("캐릭터 스폰 시작", GameLogger.LogCategory.Combat);
            
            // 플레이어 스폰
            characterManager.SpawnPlayer();
            
            // 적 스폰
            yield return StartCoroutine(characterManager.SpawnEnemy());
            
            GameLogger.LogInfo("캐릭터 스폰 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 카드 생성
        /// </summary>
        private IEnumerator GenerateCards()
        {
            GameLogger.LogInfo("카드 생성 시작", GameLogger.LogCategory.Combat);
            
            // 플레이어 카드 생성
            cardManager.GeneratePlayerCards();
            
            // 적 카드 생성
            cardManager.GenerateEnemyCards();
            
            GameLogger.LogInfo("카드 생성 완료", GameLogger.LogCategory.Combat);
            yield return null;
        }
        
        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        private void CompleteCombatPreparation()
        {
            GameLogger.LogInfo("전투 준비 완료", GameLogger.LogCategory.Combat);
            
            // 슬롯 상태 로깅
            slotManager?.LogSlotStates();
            
            // 턴 정보 로깅
            turnManager?.LogTurnInfo();
        }
        
        /// <summary>
        /// 플레이어 입력 활성화
        /// </summary>
        private void EnablePlayerInput()
        {
            GameLogger.LogInfo("플레이어 입력 활성화", GameLogger.LogCategory.Combat);
            
            // 플레이어 턴으로 설정
            turnManager?.SetTurn(TurnManager.TurnType.Player);
            
            // 드래그 앤 드롭 활성화
            cardManager?.EnablePlayerCardDragDrop();
        }
        
        #endregion
        
        #region 매니저 접근자
        
        /// <summary>
        /// 슬롯 매니저 접근
        /// </summary>
        public CombatSlotManager GetSlotManager() => slotManager;
        
        /// <summary>
        /// 턴 매니저 접근
        /// </summary>
        public TurnManager GetTurnManager() => turnManager;
        
        /// <summary>
        /// 실행 시스템 접근
        /// </summary>
        public SlotExecutionSystem GetExecutionSystem() => executionSystem;
        
        /// <summary>
        /// 캐릭터 매니저 접근
        /// </summary>
        public CharacterManager GetCharacterManager() => characterManager;
        
        /// <summary>
        /// 카드 매니저 접근
        /// </summary>
        public CardManager GetCardManager() => cardManager;
        
        #endregion
    }
}
