using Game.CombatSystem.Interface;
using Game.IManager;
using UnityEngine;

namespace Game.CombatSystem.Service
{
    public class BattleResetService : IBattleResetService
    {
        private readonly IPlayerManager _playerManager;
        private readonly IEnemyManager _enemyManager;
        private readonly ICombatTurnManager _combatTurnManager;
        private readonly ITurnCardRegistry _turnCardRegistry;

        public BattleResetService(
            IPlayerManager playerManager,
            IEnemyManager enemyManager,
            ICombatTurnManager combatTurnManager,
            ITurnCardRegistry turnCardRegistry)
        {
            _playerManager = playerManager;
            _enemyManager = enemyManager;
            _combatTurnManager = combatTurnManager;
            _turnCardRegistry = turnCardRegistry;
        }

        public void ResetAll()
        {
            _playerManager.Reset();
            _enemyManager.Reset();
            _combatTurnManager.Reset();
            _turnCardRegistry.Reset();

            PlayerPrefs.DeleteKey("SaveGame");
            Debug.Log("[BattleResetService] 전투 상태 초기화 완료");
        }
    }
}
