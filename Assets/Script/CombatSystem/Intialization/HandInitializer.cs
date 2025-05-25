using Game.CombatSystem.Interface;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.CombatSystem.Initializer
{
    public class HandInitializer : IHandInitializer
    {
        private readonly IPlayerHandManager playerHand;
        private readonly IEnemyHandManager enemyHand;

        public HandInitializer(IPlayerHandManager playerHand, IEnemyHandManager enemyHand)
        {
            this.playerHand = playerHand;
            this.enemyHand = enemyHand;
        }

        public void SetupHands()
        {
            playerHand.ClearAll();
            enemyHand.GenerateInitialHand();
            Debug.Log("[HandInitializer] 핸드 초기화 완료");
        }
    }
}
