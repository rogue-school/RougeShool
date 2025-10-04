using System.Collections;
using UnityEngine;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 소환된 적 사망 시 원본 적 복귀 처리 상태
    /// </summary>
    public class SummonReturnState : BaseCombatState
    {
        private EnemyCharacterData originalEnemyData;
        private int originalHP;
        private bool isReturnCompleted = false;

        public override string StateName => "SummonReturn";

        public SummonReturnState(EnemyCharacterData originalData, int hp)
        {
            originalEnemyData = originalData;
            originalHP = hp;
        }

        public override void OnEnter(CombatStateContext context)
        {
            LogStateTransition($"원본 적 복귀 상태 진입: {originalEnemyData.DisplayName} (HP: {originalHP})");
            StartReturnProcess(context);
        }

        public override void OnUpdate(CombatStateContext context)
        {
            // 복귀 완료 대기
            if (isReturnCompleted)
            {
                LogStateTransition($"원본 적 복귀 완료 - CombatInitState로 전환 (적 데이터: {originalEnemyData.DisplayName})");

                // CombatInitState에 복귀된 적 데이터 전달
                var combatInitState = new CombatInitState();
                combatInitState.SetEnemyData(originalEnemyData, originalEnemyData.DisplayName);
                combatInitState.SetSummonMode(true);

                RequestTransition(context, combatInitState);
            }
        }

        public override void OnExit(CombatStateContext context)
        {
            LogStateTransition("원본 적 복귀 상태 종료");
        }

        private void StartReturnProcess(CombatStateContext context)
        {
            LogStateTransition("원본 적 복귀 프로세스 시작");
            context.StateMachine.StartCoroutine(ReturnProcessCoroutine(context));
        }

        private IEnumerator ReturnProcessCoroutine(CombatStateContext context)
        {
            // 1단계: 소환된 적 비활성화 (파괴하지 않고 보관)
            LogStateTransition("소환된 적 비활성화 시작");
            var summonedEnemy = context.EnemyManager?.GetEnemy();
            if (summonedEnemy != null)
            {
                LogStateTransition($"소환된 적 비활성화: {summonedEnemy.GetCharacterName()}");

                // EnemyManager에서 등록 해제
                context.EnemyManager?.UnregisterCharacter();

                // GameObject 비활성화
                if (summonedEnemy is EnemyCharacter enemyChar)
                {
                    enemyChar.gameObject.SetActive(false);
                    LogStateTransition($"소환된 적 GameObject 비활성화 완료: {enemyChar.GetCharacterName()}");
                }
            }

            // 비활성화가 완전히 적용되도록 1프레임 대기
            yield return null;
            LogStateTransition("소환된 적 비활성화 완료 - 슬롯/핸드 초기화 시작");

            // 1.5단계: 슬롯 및 핸드 초기화 (소환된 적의 카드 제거)
            LogStateTransition("플레이어 핸드 카드 제거");
            context.HandManager?.ClearAll();

            LogStateTransition("전투/대기 슬롯 정리");
            context.SlotRegistry?.ClearAllSlots();

            LogStateTransition("적 캐시 초기화");
            context.SlotMovement?.ClearEnemyCache();

            LogStateTransition("슬롯 상태 리셋");
            context.SlotMovement?.ResetSlotStates();

            yield return null;

            // 2단계: 원본 적 재활성화 (새로 생성하지 않고 기존 GameObject 사용)
            LogStateTransition($"원본 적 재활성화 시작: {originalEnemyData.DisplayName}");
            ICharacter restoredEnemy = FindAndReactivateOriginalEnemy(context, originalEnemyData);

            if (restoredEnemy == null)
            {
                LogError("원본 적 재활성화 실패 - 비활성화된 GameObject를 찾을 수 없음");
                yield break;
            }

            // 3단계: 복귀된 적 등록
            LogStateTransition($"복귀된 적 등록: {restoredEnemy.GetCharacterName()}");
            RegisterRestoredEnemy(context, restoredEnemy);

            // 4단계: 복귀 완료
            LogStateTransition("원본 적 복귀 프로세스 완료");
            isReturnCompleted = true;
        }

        /// <summary>
        /// 비활성화된 원본 적을 찾아서 재활성화합니다
        /// </summary>
        private ICharacter FindAndReactivateOriginalEnemy(CombatStateContext context, EnemyCharacterData targetData)
        {
            LogStateTransition($"[복귀 디버그] 원본 적 찾기 시작 - 찾는 대상: {targetData?.DisplayName ?? "null"}");

            // EnemyManager의 캐릭터 슬롯에서 비활성화된 원본 적 찾기
            var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
            if (enemyManager == null)
            {
                LogError("[복귀 디버그] EnemyManager를 찾을 수 없습니다");
                return null;
            }
            LogStateTransition("[복귀 디버그] EnemyManager 찾기 성공");

            var characterSlot = enemyManager.GetCharacterSlot();
            if (characterSlot == null)
            {
                LogError("[복귀 디버그] CharacterSlot을 찾을 수 없습니다");
                return null;
            }
            LogStateTransition($"[복귀 디버그] CharacterSlot 찾기 성공 - 자식 개수: {characterSlot.childCount}");

            // CharacterSlot의 모든 자식 중 비활성화된 적 찾기
            int childIndex = 0;
            foreach (Transform child in characterSlot)
            {
                LogStateTransition($"[복귀 디버그] 자식 {childIndex} 검사 - 이름: {child.name}, 활성화 상태: {child.gameObject.activeSelf}");

                if (!child.gameObject.activeSelf)
                {
                    LogStateTransition($"[복귀 디버그] 비활성화된 자식 발견: {child.name}");

                    if (child.TryGetComponent<EnemyCharacter>(out var enemyChar))
                    {
                        LogStateTransition($"[복귀 디버그] EnemyCharacter 컴포넌트 있음 - 캐릭터 이름: {enemyChar.GetCharacterName()}");
                        LogStateTransition($"[복귀 디버그] CharacterData 비교 - 타겟: {targetData?.DisplayName ?? "null"}, 현재: {enemyChar.CharacterData?.DisplayName ?? "null"}");
                        LogStateTransition($"[복귀 디버그] CharacterData 참조 비교 - 같은 객체: {enemyChar.CharacterData == targetData}");

                        // 데이터 일치 확인
                        if (enemyChar.CharacterData == targetData)
                        {
                            LogStateTransition($"[복귀 디버그] ✓ 원본 적 발견: {enemyChar.GetCharacterName()}");

                            // GameObject 재활성화
                            child.gameObject.SetActive(true);
                            LogStateTransition($"[복귀 디버그] ✓ 원본 적 재활성화 완료: {enemyChar.GetCharacterName()}");

                            // HP 복원
                            if (originalHP > 0)
                            {
                                enemyChar.SetCurrentHP(originalHP);
                                LogStateTransition($"[복귀 디버그] ✓ 원본 적 HP 복원: {originalHP}/{enemyChar.GetMaxHP()}");
                            }

                            return enemyChar;
                        }
                        else
                        {
                            LogWarning($"[복귀 디버그] CharacterData 불일치 - 계속 탐색");
                        }
                    }
                    else
                    {
                        LogWarning($"[복귀 디버그] EnemyCharacter 컴포넌트 없음: {child.name}");
                    }
                }
                childIndex++;
            }

            LogError($"[복귀 디버그] ✗ 비활성화된 원본 적을 찾을 수 없습니다: {targetData?.DisplayName ?? "null"}");
            LogError($"[복귀 디버그] ✗ 검사한 자식 개수: {childIndex}");
            return null;
        }

        private void RegisterRestoredEnemy(CombatStateContext context, ICharacter restoredEnemy)
        {
            // StageManager를 통해 복귀된 적 등록
            var stageManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
            if (stageManager != null)
            {
                stageManager.RegisterEnemy(restoredEnemy);
                LogStateTransition($"복귀된 적 StageManager 등록: {restoredEnemy.GetCharacterName()}");

                // StageManager의 소환 관련 데이터 초기화
                stageManager.ClearSummonData();
                LogStateTransition("StageManager 소환 데이터 초기화");
            }
            else
            {
                // StageManager가 없으면 직접 EnemyManager에 등록
                context.EnemyManager?.RegisterEnemy(restoredEnemy);
                LogStateTransition($"복귀된 적 EnemyManager 직접 등록: {restoredEnemy.GetCharacterName()}");
            }

            // 일반 적으로 설정 (사망 시 일반 처치 로직)
            if (restoredEnemy is EnemyCharacter concreteEnemy)
            {
                // 일반 사망 콜백 설정
                concreteEnemy.SetDeathCallback((enemy) => OnRestoredEnemyDeath(context, enemy));
                LogStateTransition($"복귀된 적 일반 사망 콜백 설정: {concreteEnemy.GetCharacterName()}");
            }
        }

        private void OnRestoredEnemyDeath(CombatStateContext context, ICharacter deadEnemy)
        {
            LogStateTransition($"복귀된 적 사망: {deadEnemy.GetCharacterName()} - 일반 처치 로직으로 전환");
            
            // 일반적인 적 처치 상태로 전환
            var enemyDefeatedState = new EnemyDefeatedState();
            RequestTransition(context, enemyDefeatedState);
        }
    }
}
