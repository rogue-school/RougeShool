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
            context.StateMachine.StartCoroutine(ReturnProcessCoroutine(context));
        }

        private IEnumerator ReturnProcessCoroutine(CombatStateContext context)
        {
            var summonedEnemy = context.EnemyManager?.GetEnemy();
            if (summonedEnemy != null)
            {
                // EnemyManager에서 등록 해제
                context.EnemyManager?.UnregisterCharacter();

                // GameObject 비활성화
                if (summonedEnemy is EnemyCharacter enemyChar)
                {
                    enemyChar.ClearDamageTexts();
                    enemyChar.gameObject.SetActive(false);
                }
            }

            // 비활성화가 완전히 적용되도록 1프레임 대기
            yield return null;

            // 1.5단계: 슬롯 및 핸드 초기화 (소환된 적의 카드 제거)
            context.HandManager?.ClearAll();

            context.SlotRegistry?.ClearAllSlots();

            context.SlotMovement?.ClearEnemyCache();

            context.SlotMovement?.ResetSlotStates();

            yield return null;

            // 2단계: 원본 적 재활성화 (새로 생성하지 않고 기존 GameObject 사용)
            ICharacter restoredEnemy = FindAndReactivateOriginalEnemy(context, originalEnemyData);

            if (restoredEnemy == null)
            {
                LogError("원본 적 재활성화 실패 - 비활성화된 GameObject를 찾을 수 없음");
                yield break;
            }

            // 3단계: 복귀된 적 등록
            RegisterRestoredEnemy(context, restoredEnemy);

            // 4단계: 복귀 완료
            isReturnCompleted = true;
        }

        /// <summary>
        /// 비활성화된 원본 적을 찾아서 재활성화합니다
        /// </summary>
        private ICharacter FindAndReactivateOriginalEnemy(CombatStateContext context, EnemyCharacterData targetData)
        {
            // EnemyManager의 캐릭터 슬롯에서 비활성화된 원본 적 찾기
            var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
            if (enemyManager == null)
            {
                LogError("[복귀 디버그] EnemyManager를 찾을 수 없습니다");
                return null;
            }

            var characterSlot = enemyManager.GetCharacterSlot();
            if (characterSlot == null)
            {
                LogError("[복귀 디버그] CharacterSlot을 찾을 수 없습니다");
                return null;
            }

            // CharacterSlot의 모든 자식 중 비활성화된 적 찾기
            int childIndex = 0;
            foreach (Transform child in characterSlot)
            {
                if (!child.gameObject.activeSelf)
                {
                    if (child.TryGetComponent<EnemyCharacter>(out var enemyChar))
                    {
                        // 데이터 일치 확인
                        if (enemyChar.CharacterData == targetData)
                        {
                            LogStateTransition($"[복귀 디버그] ✓ 원본 적 발견: {enemyChar.GetCharacterName()}");

                            // GameObject 재활성화
                            child.gameObject.SetActive(true);
                            LogStateTransition($"[복귀 디버그] ✓ 원본 적 재활성화 완료: {enemyChar.GetCharacterName()}");
                            
                            // 데미지 텍스트 정리 (원본 적 복귀 시 남아있는 텍스트 제거)
                            enemyChar.ClearDamageTexts();
                            LogStateTransition($"[복귀 디버그] ✓ 원본 적의 데미지 텍스트 정리 완료");

                            // HP 복원
                            if (originalHP > 0)
                            {
                                enemyChar.SetCurrentHP(originalHP);
                                LogStateTransition($"[복귀 디버그] ✓ 원본 적 HP 복원: {originalHP}/{enemyChar.GetMaxHP()}");
                            }

                            // HP 바 컨트롤러 재초기화 (체력바 색상 업데이트를 위해 필요)
                            enemyChar.ReinitializeHPBarController();
                            LogStateTransition($"[복귀 디버그] ✓ HP 바 컨트롤러 재초기화 완료");

                            // EnemyCharacterUIController 재연결 (있다면)
                            var uiController = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.UI.EnemyCharacterUIController>();
                            if (uiController != null)
                            {
                                uiController.SetTarget(enemyChar);
                                LogStateTransition($"[복귀 디버그] ✓ EnemyCharacterUIController 재연결 완료");
                            }

                            // UI 업데이트 (체력바 색상 및 버프/이펙트 복원)
                            enemyChar.RefreshUI();
                            LogStateTransition($"[복귀 디버그] ✓ 원본 적 UI 업데이트 완료");

                            // 버프/이펙트 UI 업데이트를 위한 이벤트 트리거
                            // perTurnEffects 리스트는 GameObject 비활성화 시에도 유지되므로,
                            // 재활성화 후 OnBuffsChanged 이벤트를 수동으로 트리거하여 UI 동기화
                            enemyChar.NotifyBuffsChanged();
                            var buffs = enemyChar.GetBuffs();
                            if (buffs != null && buffs.Count > 0)
                            {
                                LogStateTransition($"[복귀 디버그] ✓ 원본 적 버프/이펙트 복원: {buffs.Count}개");
                            }
                            else
                            {
                                LogStateTransition($"[복귀 디버그] 원본 적 버프/이펙트 없음");
                            }

                            // Idle 시각 효과 재시작 (복귀 시 idle 루프가 적용되도록)
                            enemyChar.StartIdleVisualLoop();
                            LogStateTransition($"[복귀 디버그] ✓ 원본 적 Idle 시각 효과 재시작 완료");

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
        }
    }
}
