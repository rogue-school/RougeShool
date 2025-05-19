using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.IManager;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Manager
{
    public class CombatFlowCoordinator : MonoBehaviour, ICombatFlowCoordinator
    {
        private ISlotRegistry slotRegistry;
        private IEnemyHandManager enemyHandManager;
        private IPlayerHandManager playerHandManager;
        private IEnemySpawnerManager spawnerManager;
        private IPlayerManager playerManager;
        private IStageManager stageManager;
        private IEnemyManager enemyManager;

        public void InjectDependencies(
            ISlotRegistry slotRegistry,
            IEnemyHandManager enemyHandManager,
            IPlayerHandManager playerHandManager,
            IEnemySpawnerManager spawnerManager,
            IPlayerManager playerManager,
            IStageManager stageManager,
            IEnemyManager enemyManager)
        {
            this.slotRegistry = slotRegistry;
            this.enemyHandManager = enemyHandManager;
            this.playerHandManager = playerHandManager;
            this.spawnerManager = spawnerManager;
            this.playerManager = playerManager;
            this.stageManager = stageManager;
            this.enemyManager = enemyManager;

            if (slotRegistry == null || enemyHandManager == null || playerHandManager == null ||
                spawnerManager == null || playerManager == null || stageManager == null || enemyManager == null)
            {
                Debug.LogError("[CombatFlowCoordinator] 의존성 주입 실패: 일부 구성 요소가 null입니다.");
            }
        }

        public IEnumerator PerformCombatPreparation()
        {
            Debug.Log("[Flow] 전투 준비: 적 소환 + 핸드 생성 + 슬롯 배치");

            spawnerManager.SpawnInitialEnemy();
            yield return null;

            enemyHandManager.GenerateInitialHand();
            yield return null;

            var card = enemyHandManager.GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (card != null)
            {
                var slot = Random.value < 0.5f ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND;
                var combatSlot = slotRegistry?.GetCombatSlot(slot);

                if (combatSlot != null && combatSlot.IsEmpty())
                {
                    combatSlot.SetCard(card);
                    Debug.Log($"[Flow] 적 카드 '{card.GetCardName()}' → 전투 슬롯 {slot} 배치 완료");
                }
                else
                {
                    Debug.LogWarning($"[Flow] 전투 슬롯 {slot}이 이미 사용 중이거나 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[Flow] 적 슬롯 1에 카드 없음");
            }

            yield return null;
        }

        public IEnumerator EnablePlayerInput()
        {
            Debug.Log("[Flow] 플레이어 입력 활성화");
            playerHandManager.EnableInput(true);
            yield return null;
        }

        public IEnumerator DisablePlayerInput()
        {
            Debug.Log("[Flow] 플레이어 입력 비활성화");
            playerHandManager.EnableInput(false);
            yield return null;
        }

        public IEnumerator PerformFirstAttack()
        {
            Debug.Log("[Flow] 선공 처리 시작");

            var firstSlot = slotRegistry?.GetCombatSlot(CombatSlotPosition.FIRST);
            if (firstSlot == null || firstSlot.IsEmpty())
            {
                Debug.LogWarning("[Flow] 선공 슬롯 비어 있음 - 스킵");
                yield break;
            }

            var card = firstSlot.GetCard();
            if (card == null)
            {
                Debug.LogWarning("[Flow] 선공 슬롯에 카드 없음");
                yield break;
            }

            card.ExecuteSkill(); // 실제 공격 로직 수행 (내부에서 대상 판단 및 실행)
            yield return new WaitForSeconds(0.5f);

            firstSlot.Clear();
            Debug.Log("[Flow] 선공 슬롯 정리 완료");

            yield return null;
        }

        public IEnumerator PerformSecondAttack()
        {
            Debug.Log("[Flow] 후공 처리 시작");

            var secondSlot = slotRegistry?.GetCombatSlot(CombatSlotPosition.SECOND);
            if (secondSlot == null || secondSlot.IsEmpty())
            {
                Debug.LogWarning("[Flow] 후공 슬롯 비어 있음 - 스킵");
                yield break;
            }

            var card = secondSlot.GetCard();
            if (card == null)
            {
                Debug.LogWarning("[Flow] 후공 슬롯에 카드 없음");
                yield break;
            }

            card.ExecuteSkill(); // 실제 공격 로직 수행
            yield return new WaitForSeconds(0.5f);

            secondSlot.Clear();
            Debug.Log("[Flow] 후공 슬롯 정리 완료");

            yield return null;
        }

        public IEnumerator PerformResultPhase()
        {
            Debug.Log("[Flow] 결과 처리 단계");

            // 캐릭터 상태 체크
            var player = playerManager?.GetPlayer();
            var enemy = enemyManager?.GetEnemy();

            if (player != null && player.IsDead())
            {
                Debug.Log("[Flow] 플레이어 사망 - 게임 오버 상태로 전이");
                var gameOverState = stateFactory.CreateGameOverState();
                turnManager.RequestStateChange(gameOverState);
                yield break;
            }

            if (enemy != null && enemy.IsDead())
            {
                Debug.Log("[Flow] 적 사망 - 승리 상태로 전이");
                var victoryState = stateFactory.CreateVictoryState();
                turnManager.RequestStateChange(victoryState);
                yield break;
            }

            Debug.Log("[Flow] 전투 지속 - 다시 준비 상태로 전이");
            var prepareState = stateFactory.CreatePrepareState();
            turnManager.RequestStateChange(prepareState);
            yield return null;
        }

        public IEnumerator PerformVictoryPhase()
        {
            Debug.Log("[Flow] 승리 처리 - 보상 및 다음 적 준비");

            enemyHandManager.ClearHand();
            enemyManager.ClearEnemy();

            if (stageManager.HasNextEnemy())
            {
                stageManager.SpawnNextEnemy();
                yield return new WaitForSeconds(0.5f);

                var prepareState = stateFactory.CreatePrepareState();
                turnManager.RequestStateChange(prepareState);
            }
            else
            {
                Debug.Log("[Flow] 스테이지 클리어 - 다음 스테이지 로딩");
                // TODO: 다음 스테이지로 넘어가는 로직 작성
            }

            yield return null;
        }

        public IEnumerator PerformGameOverPhase()
        {
            Debug.Log("[Flow] 게임 오버 처리");

            // UI나 씬 전환
            // TODO: GameOver 화면 표시 또는 로비 씬 로딩
            yield return new WaitForSeconds(1.0f);
        }
    }
}
