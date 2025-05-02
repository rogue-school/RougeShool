using UnityEngine;
using Game.Managers;

namespace Game.Battle
{
    /// <summary>
    /// 턴 흐름을 제어하는 매니저. 적 → 플레이어 → 전투 실행 → 다음 턴.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        public EnemyCardManager enemyManager;
        public BattleSlotManager battleManager;

        private void Start() => StartNewTurn();

        public void StartNewTurn()
        {
            Debug.Log("==== 턴 시작 ====");
            enemyManager.PrepareEnemyTurn();
        }

        public void TryEndTurn()
        {
            if (battleManager.IsReady())
            {
                battleManager.StartBattle();
                Invoke(nameof(StartNewTurn), 2f);
            }
        }
    }
}
