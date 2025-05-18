using UnityEngine;
using Game.CombatSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

namespace Game.CombatSystem.State
{
    public class CombatPrepareState : ICombatTurnState
    {
        private readonly ICombatTurnManager controller;
        private readonly IEnemySpawnerManager spawnerManager;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly ICombatStateFactory stateFactory;

        public CombatPrepareState(
            ICombatTurnManager controller,
            IEnemySpawnerManager spawnerManager,
            IEnemyHandManager enemyHandManager,
            ICombatStateFactory stateFactory)
        {
            this.controller = controller;
            this.spawnerManager = spawnerManager;
            this.enemyHandManager = enemyHandManager;
            this.stateFactory = stateFactory;
        }

        public void EnterState()
        {
            if (spawnerManager == null || enemyHandManager == null || controller == null || stateFactory == null)
            {
                Debug.LogError("[CombatPrepareState] 의존성 누락 - 상태 전이를 수행할 수 없습니다.");
                return;
            }

            if (!IsEnemySlotReady())
            {
                Debug.LogWarning("[CombatPrepareState] ENEMY 슬롯이 준비되지 않아 적 스폰을 건너뜁니다.");
            }
            else
            {
                Debug.Log("[CombatPrepareState] 적 스폰 시도");
                spawnerManager.SpawnInitialEnemy();

                Debug.Log("[CombatPrepareState] 적 핸드 초기화 시도");
                enemyHandManager.GenerateInitialHand();

                // EnemyHandManager를 통해 직접 전투 카드 가져오기
                var enemyCard = enemyHandManager.GetCardForCombat();
                if (enemyCard != null)
                {
                    var slot = Random.value < 0.5f
                        ? CombatSlotPosition.FIRST
                        : CombatSlotPosition.SECOND;

                    controller.ReserveEnemySlot(slot);

                    var combatSlot = SlotRegistry.Instance?.GetCombatSlot(slot);
                    if (combatSlot != null)
                        combatSlot.SetCard(enemyCard);
                    else
                        Debug.LogWarning($"[CombatPrepareState] 전투 슬롯 {slot} 찾기 실패");
                }
                else
                {
                    Debug.LogWarning("[CombatPrepareState] 적 핸드에 카드가 없어 전투 슬롯 배치를 건너뜁니다.");
                }
            }

            var nextState = stateFactory.CreatePlayerInputState();
            if (nextState == null)
            {
                Debug.LogError("[CombatPrepareState] PlayerInputState 생성 실패");
                return;
            }

            controller.RequestStateChange(nextState);
            Debug.Log("[CombatPrepareState] PlayerInputState로 전이 요청됨");
        }

        private bool IsEnemySlotReady()
        {
            var registry = SlotRegistry.Instance;
            if (registry == null)
            {
                Debug.LogWarning("[CombatPrepareState] SlotRegistry.Instance가 아직 null입니다.");
                return false;
            }

            var slot = registry.GetCharacterSlot(SlotOwner.ENEMY);
            return slot != null;
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatPrepareState] 전투 준비 상태 종료");
        }
    }
}
