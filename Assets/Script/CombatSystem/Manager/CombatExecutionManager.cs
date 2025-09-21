using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using Zenject;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 실행을 관리하는 통합 매니저
    /// SlotExecutionSystem, CardManager, CharacterManager의 기능을 통합합니다.
    /// </summary>
    public class CombatExecutionManager : MonoBehaviour, ICombatExecutionManager
    {
        #region 기본 설정

        [Header("매니저 기본 설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] private bool enableDebugLogging = true;

        [Tooltip("자동 초기화 활성화")]
        [SerializeField] private bool autoInitialize = true;

        #endregion

        #region 실행 설정

        [Header("실행 설정")]
        [Tooltip("즉시 실행 활성화")]
        [SerializeField] private bool enableImmediateExecution = true;

        [Tooltip("실행 지연 시간")]
        [SerializeField] private float executionDelay = 0.1f;

        [Tooltip("애니메이션 대기 시간")]
        [SerializeField] private float animationWaitTime = 0.5f;

        #endregion

        #region 의존성 주입

        [Inject] private IPlayerManager playerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private CombatSlotManager slotManager;
        [Inject] private TurnManager turnManager;

        #endregion

        #region 내부 상태

        private bool isExecuting = false;
        private bool isInitialized = false;
        private Queue<ExecutionCommand> executionQueue = new Queue<ExecutionCommand>();

        #endregion

        #region 이벤트

        public System.Action<ISkillCard, ICharacter, ICharacter> OnCardExecuted { get; set; }
        public System.Action<ExecutionResult> OnExecutionCompleted { get; set; }

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CombatExecutionManager 초기화 시작", GameLogger.LogCategory.Combat);
            }
        }

        private void Start()
        {
            if (autoInitialize)
            {
                StartCoroutine(InitializeExecution());
            }
        }

        #endregion

        #region 초기화

        private IEnumerator InitializeExecution()
        {
            if (isInitialized)
            {
                yield break;
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("실행 시스템 초기화 중...", GameLogger.LogCategory.Combat);
            }

            // 의존성 검증
            if (!ValidateDependencies())
            {
                GameLogger.LogError("실행 시스템 의존성 검증 실패", GameLogger.LogCategory.Error);
                yield break;
            }

            // 초기 상태 설정
            isExecuting = false;
            executionQueue.Clear();

            isInitialized = true;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("실행 시스템 초기화 완료", GameLogger.LogCategory.Combat);
            }
        }

        #endregion

        #region 카드 실행

        /// <summary>
        /// 카드 즉시 실행
        /// </summary>
        public void ExecuteCardImmediately(ISkillCard card, CombatSlotPosition slotPosition)
        {
            if (!isInitialized)
            {
                GameLogger.LogWarning("실행 시스템이 초기화되지 않았습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            if (isExecuting)
            {
                GameLogger.LogWarning("이미 실행 중입니다.", GameLogger.LogCategory.Combat);
                return;
            }

            if (!enableImmediateExecution)
            {
                GameLogger.LogWarning("즉시 실행이 비활성화되어 있습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            StartCoroutine(ExecuteCardCoroutine(card, slotPosition));
        }

        private IEnumerator ExecuteCardCoroutine(ISkillCard card, CombatSlotPosition slotPosition)
        {
            isExecuting = true;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"카드 실행 시작: {card.GetCardName()} at {slotPosition}", GameLogger.LogCategory.Combat);
            }

            // 실행 지연
            if (executionDelay > 0)
            {
                yield return new WaitForSeconds(executionDelay);
            }

            // 카드 실행
            ExecutionResult result = ExecuteCard(card, slotPosition);

            // 애니메이션 대기
            if (animationWaitTime > 0)
            {
                yield return new WaitForSeconds(animationWaitTime);
            }

            // 실행 완료 이벤트 발생
            OnExecutionCompleted?.Invoke(result);

            isExecuting = false;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"카드 실행 완료: {card.GetCardName()}", GameLogger.LogCategory.Combat);
            }
        }

        private ExecutionResult ExecuteCard(ISkillCard card, CombatSlotPosition slotPosition)
        {
            // 소스와 타겟 캐릭터 결정
            ICharacter sourceCharacter = GetSourceCharacter(slotPosition);
            ICharacter targetCharacter = GetTargetCharacter(slotPosition);

            if (sourceCharacter == null || targetCharacter == null)
            {
                GameLogger.LogError("소스 또는 타겟 캐릭터를 찾을 수 없습니다.", GameLogger.LogCategory.Error);
                return new ExecutionResult(false, null, "캐릭터를 찾을 수 없음");
            }

            // 카드 실행
            try
            {
                card.ExecuteSkill(sourceCharacter, targetCharacter);

                // 실행 이벤트 발생
                OnCardExecuted?.Invoke(card, sourceCharacter, targetCharacter);

                return new ExecutionResult(true, null, "카드 실행 성공");
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"카드 실행 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
                return new ExecutionResult(false, null, ex.Message);
            }
        }

        #endregion

        #region 슬롯 관리

        /// <summary>
        /// 슬롯 이동 (새로운 5슬롯 시스템)
        /// </summary>
        public void MoveSlotsForwardNew()
        {
            if (slotManager != null)
            {
                slotManager.MoveSlotsForwardNew();
            }
        }

        /// <summary>
        /// 슬롯 이동 (레거시 4슬롯 시스템)
        /// </summary>
        public void MoveSlotsForward()
        {
            if (slotManager != null)
            {
                slotManager.MoveSlotsForward();
            }
        }

        #endregion

        #region 캐릭터 관리

        /// <summary>
        /// 소스 캐릭터 결정
        /// </summary>
        private ICharacter GetSourceCharacter(CombatSlotPosition slotPosition)
        {
            // 슬롯 위치에 따라 소스 캐릭터 결정
            if (IsPlayerSlot(slotPosition))
            {
                return playerManager?.GetPlayer();
            }
            else
            {
                return enemyManager?.GetCurrentEnemy();
            }
        }

        /// <summary>
        /// 타겟 캐릭터 결정
        /// </summary>
        private ICharacter GetTargetCharacter(CombatSlotPosition slotPosition)
        {
            // 슬롯 위치에 따라 타겟 캐릭터 결정
            if (IsPlayerSlot(slotPosition))
            {
                return enemyManager?.GetCurrentEnemy();
            }
            else
            {
                return playerManager?.GetPlayer();
            }
        }

        /// <summary>
        /// 플레이어 슬롯인지 확인
        /// </summary>
        private bool IsPlayerSlot(CombatSlotPosition slotPosition)
        {
            return slotPosition == CombatSlotPosition.BATTLE_SLOT ||
                   slotPosition == CombatSlotPosition.WAIT_SLOT_1 ||
                   slotPosition == CombatSlotPosition.WAIT_SLOT_2 ||
                   slotPosition == CombatSlotPosition.WAIT_SLOT_3 ||
                   slotPosition == CombatSlotPosition.WAIT_SLOT_4;
        }

        #endregion

        #region 실행 큐 관리

        /// <summary>
        /// 실행 명령을 큐에 추가
        /// </summary>
        public void QueueExecution(ExecutionCommand command)
        {
            executionQueue.Enqueue(command);

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"실행 명령 큐에 추가: {command.commandType}", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 큐의 모든 실행 명령 처리
        /// </summary>
        public void ProcessExecutionQueue()
        {
            if (isExecuting)
            {
                return;
            }

            StartCoroutine(ProcessQueueCoroutine());
        }

        private IEnumerator ProcessQueueCoroutine()
        {
            while (executionQueue.Count > 0)
            {
                ExecutionCommand command = executionQueue.Dequeue();
                yield return StartCoroutine(ExecuteCommandCoroutine(command));
            }
        }

        private IEnumerator ExecuteCommandCoroutine(ExecutionCommand command)
        {
            // 명령 실행 로직
            yield return null;
        }

        #endregion

        #region 유틸리티

        private bool ValidateDependencies()
        {
            bool isValid = true;

            if (playerManager == null)
            {
                GameLogger.LogError("PlayerManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            if (enemyManager == null)
            {
                GameLogger.LogError("EnemyManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            if (slotManager == null)
            {
                GameLogger.LogError("CombatSlotManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            if (turnManager == null)
            {
                GameLogger.LogError("CombatTurnManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            return isValid;
        }

        #endregion

        #region 공개 프로퍼티

        public bool IsExecuting => isExecuting;
        public bool IsInitialized => isInitialized;
        public int QueueCount => executionQueue.Count;

        #endregion

        #region 리셋

        public void ResetExecution()
        {
            isExecuting = false;
            executionQueue.Clear();
            isInitialized = false;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CombatExecutionManager 리셋 완료", GameLogger.LogCategory.Combat);
            }
        }

        #endregion
    }

}
