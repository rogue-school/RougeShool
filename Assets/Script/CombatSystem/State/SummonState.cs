using System.Collections;
using UnityEngine;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 적 소환 처리 상태
    /// </summary>
    public class SummonState : BaseCombatState
    {
        private EnemyCharacterData summonTarget;
        private int originalHP;
        private bool isSummonCompleted = false;

        public override string StateName => "Summon";

        public SummonState(EnemyCharacterData target, int hp)
        {
            summonTarget = target;
            originalHP = hp;
        }

        public override void OnEnter(CombatStateContext context)
        {
            LogStateTransition($"소환 상태 진입: {summonTarget.DisplayName} (원본 HP: {originalHP})");

            // 즉시 소환 플래그 해제 (중복 소환 방지)
            var stageManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
            if (stageManager != null)
            {
                stageManager.SetSummonedEnemyActive(false);
            }

            StartSummonProcess(context);
        }

        public override void OnUpdate(CombatStateContext context)
        {
            // 소환 완료 대기
            if (isSummonCompleted)
            {
                LogStateTransition($"소환 완료 - CombatInitState로 전환 (적 데이터: {summonTarget.DisplayName})");

                // CombatInitState에 소환된 적 데이터 전달
                var combatInitState = new CombatInitState();
                combatInitState.SetEnemyData(summonTarget, summonTarget.DisplayName);
                combatInitState.SetSummonMode(true);

                RequestTransition(context, combatInitState);
            }
        }

        public override void OnExit(CombatStateContext context)
        {
            LogStateTransition("소환 상태 종료");
            
            // 소환이 완료되지 않은 상태에서 종료되면 경고
            if (!isSummonCompleted)
            {
                LogError("소환 프로세스가 완료되지 않은 상태에서 상태 종료됨!");
            }
        }

        private void StartSummonProcess(CombatStateContext context)
        {
            context.StateMachine.StartCoroutine(SummonProcessCoroutine(context));
        }

        private IEnumerator SummonProcessCoroutine(CombatStateContext context)
        {
            // 1단계: StageManager에서 원본 적 데이터 가져오기
            var stageManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
            if (stageManager == null)
            {
                LogError("소환 실패 - StageManager 없음");
                yield break;
            }

            var originalEnemyData = stageManager.GetOriginalEnemyData();
            if (originalEnemyData == null)
            {
                LogError("소환 실패 - StageManager에서 원본 적 데이터 없음");
                yield break;
            }

            // 2단계: 기존 적(원본) 비활성화 (태그 매치 방식 - 나중에 복귀)
            var currentEnemy = context.EnemyManager?.GetEnemy();
            if (currentEnemy != null)
            {
                if (currentEnemy is EnemyCharacter enemyChar)
                {
                    // 비활성화 직전의 실제 현재 HP를 저장 (소환 트리거 시점 이후 체력 변화 반영)
                    int currentActualHP = currentEnemy.GetCurrentHP();
                    // 상위 스코프에서 이미 선언된 stageManager 사용
                    if (stageManager != null)
                    {
                        stageManager.SetOriginalEnemyHP(currentActualHP);
                    }
                    
                    // 데미지 텍스트 정리 (소환 전에 남아있는 텍스트 제거)
                    enemyChar.ClearDamageTexts();
                }

                // EnemyManager에서 등록 해제 (하지만 GameObject는 유지)
                context.EnemyManager?.UnregisterCharacter();

                // GameObject 비활성화 (나중에 복귀를 위해 보관)
                if (currentEnemy is EnemyCharacter enemyChar2)
                {
                    enemyChar2.gameObject.SetActive(false);
                    LogStateTransition($"원본 적 GameObject 비활성화 완료: {enemyChar2.GetCharacterName()}");
                }
            }
            yield return null;

            // 2.5단계: 원본 적의 슬롯/핸드 정리 (원본 적의 스킬 카드 제거)
            LogStateTransition("원본 적 비활성화 후 슬롯/핸드 초기화");

            LogStateTransition("플레이어 핸드 카드 제거");
            context.HandManager?.ClearAll();

            LogStateTransition("전투/대기 슬롯 정리");
            context.SlotRegistry?.ClearAllSlots();

            LogStateTransition("적 캐시 초기화");
            context.SlotMovement?.ClearEnemyCache();

            LogStateTransition("슬롯 상태 리셋");
            context.SlotMovement?.ResetSlotStates();

            yield return null;

            // 3단계: 소환된 적 생성
            LogStateTransition($"소환된 적 생성 시작: {summonTarget.DisplayName}");
            ICharacter newEnemy = null;
            yield return context.StateMachine.StartCoroutine(CreateSummonedEnemy(context, summonTarget, (enemy) => newEnemy = enemy));

            if (newEnemy == null)
            {
                LogError("소환된 적 생성 실패");
                yield break;
            }

            // 4단계: 소환된 적 등록
            LogStateTransition($"소환된 적 등록: {newEnemy.GetCharacterName()}");
            RegisterSummonedEnemy(context, newEnemy, originalEnemyData, originalHP);

            // 5단계: 소환 완료
            LogStateTransition("소환 프로세스 완료");
            isSummonCompleted = true;
        }


        private IEnumerator CreateSummonedEnemy(CombatStateContext context, EnemyCharacterData targetData, System.Action<ICharacter> onComplete)
        {
            // StageManager에서 적 생성 로직을 가져와서 사용
            var stageManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
            if (stageManager == null)
            {
                LogError("StageManager를 찾을 수 없습니다");
                onComplete?.Invoke(null);
                yield break;
            }

            // 비동기 적 생성
            var createTask = stageManager.CreateEnemyForSummonAsync(targetData);
            yield return new UnityEngine.WaitUntil(() => createTask.IsCompleted);

            if (createTask.IsFaulted)
            {
                LogError($"적 생성 실패: {createTask.Exception?.GetBaseException().Message}");
                onComplete?.Invoke(null);
                yield break;
            }

            var newEnemy = createTask.Result;
            if (newEnemy != null)
            {
                LogStateTransition($"소환된 적 생성 완료: {newEnemy.GetCharacterName()}");
                
                // 적 등장 애니메이션이 완료될 때까지 대기
                LogStateTransition("적 등장 애니메이션 완료 대기 중...");
                yield return new UnityEngine.WaitForSeconds(1.0f); // 애니메이션 시간만큼 대기
                
                onComplete?.Invoke(newEnemy);
            }
            else
            {
                LogError("소환된 적 생성 결과가 null입니다");
                onComplete?.Invoke(null);
            }
        }

        private void RegisterSummonedEnemy(CombatStateContext context, ICharacter newEnemy, EnemyCharacterData originalData, int originalHP)
        {
            // StageManager를 통해 소환된 적 등록
            var stageManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
            if (stageManager != null)
            {
                stageManager.RegisterSummonedEnemy(newEnemy);
                LogStateTransition($"소환된 적 StageManager 등록: {newEnemy.GetCharacterName()}");
                
                // EnemyManager에도 직접 등록하여 즉시 접근 가능하도록 함
                context.EnemyManager?.RegisterEnemy(newEnemy);
                LogStateTransition($"소환된 적 EnemyManager 직접 등록: {newEnemy.GetCharacterName()}");
            }
            else
            {
                // StageManager가 없으면 직접 EnemyManager에 등록
                context.EnemyManager?.RegisterEnemy(newEnemy);
                LogStateTransition($"소환된 적 EnemyManager 직접 등록: {newEnemy.GetCharacterName()}");
            }

            // 소환된 적으로 설정 (사망 시 원본 복귀를 위해)
            if (newEnemy is EnemyCharacter concreteEnemy)
            {
                // 소환된 적 사망 콜백 설정
                concreteEnemy.SetDeathCallback((enemy) => OnSummonedEnemyDeath(context, enemy, originalData, originalHP));
                LogStateTransition($"소환된 적 사망 콜백 설정: {concreteEnemy.GetCharacterName()}");
            }
            
            // 등록 완료 확인
            var registeredEnemy = context.EnemyManager?.GetEnemy();
            if (registeredEnemy != null)
            {
                LogStateTransition($"소환된 적 등록 확인 완료: {registeredEnemy.GetCharacterName()}");
            }
            else
            {
                LogError("소환된 적 등록 확인 실패 - EnemyManager에서 찾을 수 없음");
            }
        }

        private void OnSummonedEnemyDeath(CombatStateContext context, ICharacter deadEnemy, EnemyCharacterData originalData, int originalHP)
        {
            LogStateTransition($"소환된 적 사망: {deadEnemy.GetCharacterName()} - 원본 적 복귀 처리");
            LogStateTransition($"[소환 디버그] 복귀할 원본 데이터: {originalData?.DisplayName ?? "null"}, 파라미터 HP: {originalHP}");

            // StageManager에서 최신 원본 적 HP 가져오기 (비활성화 직전 업데이트된 값 사용)
            var stageManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
            int latestOriginalHP = originalHP;
            if (stageManager != null)
            {
                latestOriginalHP = stageManager.GetOriginalEnemyHP();
                LogStateTransition($"[소환 디버그] StageManager에서 최신 원본 HP 가져옴: {latestOriginalHP}");
            }

            // 원본 적 복귀 상태로 전환 (최신 HP 사용)
            var returnState = new SummonReturnState(originalData, latestOriginalHP);
            RequestTransition(context, returnState);
        }

        private IEnumerator RestoreOriginalEnemy(CombatStateContext context)
        {
            LogStateTransition("오류 발생 - 원본 적 복귀 시도");
            // 원본 적 복귀 로직 (필요시 구현)
            yield return null;
        }
    }
}
