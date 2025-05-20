using UnityEngine;
using System;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.IManager;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Manager
{
    public class CombatFlowCoordinator : MonoBehaviour, ICombatFlowCoordinator
    {

        [Header("UI 프리팹")]
        [SerializeField] private SkillCardUI skillCardUIPrefab;

        private ISlotRegistry slotRegistry;
        private IEnemyHandManager enemyHandManager;
        private IPlayerHandManager playerHandManager;
        private IEnemySpawnerManager spawnerManager;
        private IPlayerManager playerManager;
        private IStageManager stageManager;
        private IEnemyManager enemyManager;
        private ICombatTurnManager turnManager;
        private ICombatStateFactory stateFactory;

        public void Inject(
            ISlotRegistry slotRegistry,
            IEnemyHandManager enemyHandManager,
            IPlayerHandManager playerHandManager,
            IEnemySpawnerManager spawnerManager,
            IPlayerManager playerManager,
            IStageManager stageManager,
            IEnemyManager enemyManager,
            ICombatTurnManager turnManager,
            ICombatStateFactory stateFactory)
        {
            this.slotRegistry = slotRegistry;
            this.enemyHandManager = enemyHandManager;
            this.playerHandManager = playerHandManager;
            this.spawnerManager = spawnerManager;
            this.playerManager = playerManager;
            this.stageManager = stageManager;
            this.enemyManager = enemyManager;
            this.turnManager = turnManager;
            this.stateFactory = stateFactory;
        }

        public void InjectUI(SkillCardUI prefab)
        {
            this.skillCardUIPrefab = prefab;
        }

        public void InjectTurnStateDependencies(ICombatTurnManager turnManager, ICombatStateFactory stateFactory)
        {
            this.turnManager = turnManager;
            this.stateFactory = stateFactory;
        }

        public void StartCombatFlow()
        {
            turnManager.RequestStateChange(stateFactory.CreatePrepareState());
        }

        public IEnumerator PerformCombatPreparation() => PerformCombatPreparation(null);

        public IEnumerator PerformCombatPreparation(Action<bool> onComplete = null)
        {
            Debug.Log("[Flow] 전투 준비: 플레이어 생성 → 적 소환 → 핸드 준비 → 슬롯 배치");

            // 1. 플레이어 생성
            playerManager.CreateAndRegisterPlayer();  // 플레이어 오브젝트 생성 및 핸드 매니저 초기화 포함
            yield return null;

            // 2. 적 생성
            spawnerManager.SpawnInitialEnemy();
            yield return null;

            // 3. 적 카드 확인
            var enemy = enemyManager.GetEnemy();
            if (enemy == null)
            {
                Debug.LogError("[Flow] 적 생성 실패: EnemyManager에 없음");
                onComplete?.Invoke(false);
                yield break;
            }

            var card = enemyHandManager.GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
            var ui = enemyHandManager.GetCardUI(0) as SkillCardUI;

            if (card == null || ui == null)
            {
                Debug.LogError("[Flow] EnemyHandManager에서 카드 또는 UI 가져오기 실패");
                LogEnemyHandSlots();
                onComplete?.Invoke(false);
                yield break;
            }

            // 4. 적 카드 슬롯 배치
            CombatSlotPosition targetSlot = CombatSlotPosition.FIRST;
            var combatSlot = slotRegistry.GetCombatSlot(targetSlot);

            if (combatSlot == null || !combatSlot.IsEmpty())
            {
                targetSlot = CombatSlotPosition.SECOND;
                combatSlot = slotRegistry.GetCombatSlot(targetSlot);
            }

            if (combatSlot == null || !combatSlot.IsEmpty())
            {
                Debug.LogError("[Flow] 사용 가능한 전투 슬롯 없음");
                onComplete?.Invoke(false);
                yield break;
            }

            combatSlot.SetCard(card);
            combatSlot.SetCardUI(ui);

            ui.transform.SetParent(((MonoBehaviour)combatSlot).transform);
            ui.transform.localPosition = Vector3.zero;
            ui.transform.localScale = Vector3.one;

            Debug.Log($"[Flow] 카드 '{card.GetCardName()}' → 전투 슬롯 {targetSlot} 배치 및 UI 이동 완료");

            onComplete?.Invoke(true);
        }

        private void LogEnemyHandSlots()
        {
            Debug.Log("[Debug] EnemyHand 슬롯 상태 출력:");
            foreach (SkillCardSlotPosition pos in Enum.GetValues(typeof(SkillCardSlotPosition)))
            {
                var slotCard = enemyHandManager.GetSlotCard(pos);
                Debug.Log($" → {pos}: {(slotCard != null ? slotCard.CardData.Name : "비어 있음")}");
            }
        }

        public IEnumerator EnablePlayerInput()
        {
            playerHandManager.EnableInput(true);
            yield return null;
        }

        public IEnumerator DisablePlayerInput()
        {
            playerHandManager.EnableInput(false);
            yield return null;
        }

        public IEnumerator PerformFirstAttack()
        {
            var slot = slotRegistry.GetCombatSlot(CombatSlotPosition.FIRST);
            var card = slot?.GetCard();
            card?.ExecuteSkill();
            yield return new WaitForSeconds(0.5f);
            slot?.Clear();
        }

        public IEnumerator PerformSecondAttack()
        {
            var slot = slotRegistry.GetCombatSlot(CombatSlotPosition.SECOND);
            var card = slot?.GetCard();
            card?.ExecuteSkill();
            yield return new WaitForSeconds(0.5f);
            slot?.Clear();
        }

        public IEnumerator PerformResultPhase()
        {
            if (IsPlayerDead())
            {
                turnManager.RequestStateChange(stateFactory.CreateGameOverState());
                yield break;
            }

            if (IsEnemyDead())
            {
                if (CheckHasNextEnemy())
                    turnManager.RequestStateChange(stateFactory.CreatePrepareState());
                else
                    turnManager.RequestStateChange(stateFactory.CreateVictoryState());

                yield break;
            }

            turnManager.RequestStateChange(stateFactory.CreatePlayerInputState());
        }

        public IEnumerator PerformVictoryPhase()
        {
            enemyHandManager.ClearHand();
            enemyManager.ClearEnemy();

            if (stageManager.HasNextEnemy())
            {
                stageManager.SpawnNextEnemy();
                yield return new WaitForSeconds(0.5f);
                turnManager.RequestStateChange(stateFactory.CreatePrepareState());
            }
            else
            {
                Debug.Log("[Flow] 스테이지 클리어 완료");
            }

            yield return null;
        }

        public IEnumerator PerformGameOverPhase()
        {
            Debug.Log("[Flow] 게임 오버 처리");
            yield return new WaitForSeconds(1.0f);
        }

        public bool IsPlayerDead()
        {
            var player = playerManager?.GetPlayer();
            return player == null || player.IsDead();
        }

        public bool IsEnemyDead()
        {
            var enemy = enemyManager?.GetEnemy();
            return enemy == null || enemy.IsDead();
        }

        public bool CheckHasNextEnemy()
        {
            return stageManager?.HasNextEnemy() ?? false;
        }
    }
}
