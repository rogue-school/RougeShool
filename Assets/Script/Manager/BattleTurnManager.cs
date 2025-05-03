using UnityEngine;
using Game.Managers;
using Game.Enemy;

namespace Game.Battle
{
    /// <summary>
    /// 전투의 턴 흐름을 제어합니다.
    /// </summary>
    public class BattleTurnManager : MonoBehaviour
    {
        [SerializeField] private EnemyCombatCardManager enemyCardManager;
        [SerializeField] private PlayerHandManager playerHandManager;
        [SerializeField] private BattleSlotManager slotManager;

        private void Start()
        {
            StartNewTurn();
        }

        public void StartNewTurn()
        {
            enemyCardManager.PrepareEnemyTurn();
        }

        public void TryEndTurn()
        {
            if (slotManager.IsReady())
            {
                slotManager.StartBattle();
                enemyCardManager.AdvanceEnemyHand();
                StartNewTurn();
            }
        }
    }
}
