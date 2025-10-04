using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 소환/복귀 전환 전용 상태
    /// 기존 적 제거 → 새 적 생성 → 슬롯 정리 → 전투 재시작을 순차 처리
    /// </summary>
    public class SummonTransitionState : BaseCombatState
    {
        public override string StateName => "SummonTransition";

        private EnemyCharacterData _targetEnemyData;
        private int _restoredHP;
        private bool _isRestoreMode; // true: 원본 복귀, false: 소환

        /// <summary>
        /// 소환 전환 설정 (새로운 적 소환)
        /// </summary>
        public void SetupSummon(EnemyCharacterData summonTarget)
        {
            _targetEnemyData = summonTarget;
            _restoredHP = 0;
            _isRestoreMode = false;
        }

        /// <summary>
        /// 복귀 전환 설정 (원본 적 복귀)
        /// </summary>
        public void SetupRestore(EnemyCharacterData originalEnemy, int restoredHP)
        {
            _targetEnemyData = originalEnemy;
            _restoredHP = restoredHP;
            _isRestoreMode = true;
        }

        public override void OnEnter(CombatStateContext context)
        {
            base.OnEnter(context);

            if (context == null || !context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패");
                TransitionToPlayerTurn(context);
                return;
            }

            if (_targetEnemyData == null)
            {
                LogError("대상 적 데이터가 null입니다");
                TransitionToPlayerTurn(context);
                return;
            }

            LogStateTransition($"{(_isRestoreMode ? "원본 적 복귀" : "적 소환")} 시작: {_targetEnemyData.DisplayName}");

            // 코루틴으로 전환 처리 시작
            var coroutineRunner = context.StateMachine;
            if (coroutineRunner != null)
            {
                coroutineRunner.StartCoroutine(PerformTransition(context));
            }
            else
            {
                LogError("코루틴 실행을 위한 MonoBehaviour를 찾을 수 없습니다");
                TransitionToPlayerTurn(context);
            }
        }

        private System.Collections.IEnumerator PerformTransition(CombatStateContext context)
        {
            // 1단계: 기존 적 제거 및 슬롯 완전 정리
            LogStateTransition("1단계: 기존 적 제거 및 슬롯 정리");
            yield return CleanupCurrentEnemy(context);

            // 2단계: 새로운 적 생성
            LogStateTransition($"2단계: 새로운 적 생성 - {_targetEnemyData.DisplayName}");
            ICharacter newEnemy = null;
            yield return CreateNewEnemy(context, (enemy) => newEnemy = enemy);

            if (newEnemy == null)
            {
                LogError("새로운 적 생성 실패");
                TransitionToPlayerTurn(context);
                yield break;
            }

            // 3단계: 적 등록
            LogStateTransition("3단계: 적 등록");
            RegisterNewEnemy(context, newEnemy);

            // 3.5단계: 새로운 적의 슬롯 설정
            LogStateTransition("3.5단계: 새로운 적의 슬롯 설정");
            yield return SetupNewEnemySlots(context, newEnemy);

            // 4단계: 전투 재초기화
            LogStateTransition("4단계: 전투 재초기화로 전환");
            yield return new WaitForSeconds(0.2f); // 안정화 대기

            TransitionToCombatInit(context, newEnemy);
        }

        /// <summary>
        /// 1단계: 기존 적 제거 및 슬롯 완전 정리
        /// </summary>
        private System.Collections.IEnumerator CleanupCurrentEnemy(CombatStateContext context)
        {
            var enemyManager = context.EnemyManager;
            var slotRegistry = context.SlotRegistry;
            var slotMovement = context.SlotMovement;

            // 기존 적 제거
            var currentEnemy = enemyManager?.GetCharacter();
            if (currentEnemy != null)
            {
                enemyManager.UnregisterEnemy();

                if (currentEnemy is Game.CharacterSystem.Core.EnemyCharacter enemyChar)
                {
                    Object.Destroy(enemyChar.gameObject);
                }

                LogStateTransition($"기존 적 제거 완료: {currentEnemy.GetCharacterName()}");
            }

            // 플레이어 핸드 카드 제거
            if (context.HandManager != null)
            {
                context.HandManager.ClearAll();
                LogStateTransition("플레이어 핸드 카드 제거 완료");
            }

            // 모든 슬롯 정리 (새 인터페이스 사용)
            if (slotRegistry != null)
            {
                slotRegistry.ClearAllSlots();
                LogStateTransition("전투/대기 슬롯 정리 완료");
            }

            // 적 캐시 정리
            if (slotMovement != null)
            {
                slotMovement.ClearEnemyCache();
                LogStateTransition("적 캐시 정리 완료");
            }

            // 정리 완료 대기
            yield return new WaitForSeconds(0.3f);
        }

        /// <summary>
        /// 2단계: 새로운 적 생성 (콜백 방식)
        /// </summary>
        private System.Collections.IEnumerator CreateNewEnemy(CombatStateContext context, System.Action<ICharacter> onComplete)
        {
            var stageManager = Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
            if (stageManager == null)
            {
                LogError("StageManager를 찾을 수 없습니다");
                onComplete?.Invoke(null);
                yield break;
            }

            // 비동기 적 생성을 코루틴으로 대기
            var createTask = stageManager.CreateEnemyForSummonAsync(_targetEnemyData);
            
            while (!createTask.IsCompleted)
            {
                yield return null;
            }

            var newEnemy = createTask.Result;
            
            if (newEnemy != null)
            {
                // 복귀 모드인 경우 HP 복원
                if (_isRestoreMode && _restoredHP > 0)
                {
                    if (newEnemy is Game.CharacterSystem.Core.CharacterBase characterBase)
                    {
                        characterBase.SetCurrentHP(_restoredHP);
                        LogStateTransition($"HP 복원: {_restoredHP}");
                    }
                }
                
                LogStateTransition($"새로운 적 생성 완료: {newEnemy.GetCharacterName()}");
            }
            else
            {
                LogError("적 생성 실패");
            }

            onComplete?.Invoke(newEnemy);
        }

        /// <summary>
        /// 3.5단계: 새로운 적의 슬롯 설정
        /// </summary>
        private System.Collections.IEnumerator SetupNewEnemySlots(CombatStateContext context, ICharacter newEnemy)
        {
            var slotMovement = context.SlotMovement;
            if (slotMovement == null)
            {
                LogError("SlotMovement를 찾을 수 없습니다");
                yield break;
            }

            LogStateTransition($"새로운 적의 슬롯 설정 시작: {newEnemy.GetCharacterName()}");

            // 새로운 적의 초기 슬롯 설정
            if (newEnemy is Game.CharacterSystem.Core.EnemyCharacter enemyChar)
            {
                var enemyData = enemyChar.CharacterData as Game.CharacterSystem.Data.EnemyCharacterData;
                if (enemyData != null)
                {
                    // 적의 초기 슬롯 설정 (SlotMovementController 사용)
                    yield return context.StateMachine.StartCoroutine(
                        slotMovement.SetupInitialEnemyQueueRoutine(enemyData, newEnemy.GetCharacterName())
                    );
                    LogStateTransition($"적 초기 슬롯 설정 완료: {newEnemy.GetCharacterName()}");
                }
            }

            // 슬롯 설정 완료 대기
            yield return new WaitForSeconds(0.3f);
            LogStateTransition("새로운 적의 슬롯 설정 완료");
        }

        /// <summary>
        /// 3단계: 새로운 적 등록
        /// </summary>
        private void RegisterNewEnemy(CombatStateContext context, ICharacter newEnemy)
        {
            var stageManager = Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
            if (stageManager != null)
            {
                if (_isRestoreMode)
                {
                    stageManager.RegisterEnemy(newEnemy);
                }
                else
                {
                    stageManager.RegisterSummonedEnemy(newEnemy);
                }
                
                LogStateTransition($"적 등록 완료: {newEnemy.GetCharacterName()}");
            }
        }

        /// <summary>
        /// 4단계: 전투 재초기화로 전환
        /// </summary>
        private void TransitionToCombatInit(CombatStateContext context, ICharacter newEnemy)
        {
            var initState = new CombatInitState();

            // 적 데이터 전달
            if (newEnemy is Game.CharacterSystem.Core.EnemyCharacter enemyChar)
            {
                var enemyData = enemyChar.CharacterData as EnemyCharacterData;
                if (enemyData != null)
                {
                    initState.SetEnemyData(enemyData, newEnemy.GetCharacterName());
                }
            }

            // 슬롯 설정을 건너뛰도록 설정 (이미 SummonTransitionState에서 설정했음)
            initState.SkipSlotSetup();

            LogStateTransition($"CombatInitState로 전환 - {newEnemy.GetCharacterName()}");
            RequestTransition(context, initState);
        }

        /// <summary>
        /// 오류 시 플레이어 턴으로 복귀
        /// </summary>
        private void TransitionToPlayerTurn(CombatStateContext context)
        {
            LogStateTransition("오류 발생 - PlayerTurnState로 복귀");
            var playerTurnState = new PlayerTurnState();
            RequestTransition(context, playerTurnState);
        }

        public override void OnExit(CombatStateContext context)
        {
            LogStateTransition($"{(_isRestoreMode ? "복귀" : "소환")} 전환 완료");
            base.OnExit(context);
        }
    }
}

