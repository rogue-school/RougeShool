using UnityEngine;
using Game.Managers;

namespace Game.Battle.Initialization
{
    /// <summary>
    /// 플레이어 및 적 핸드의 카드를 초기화합니다.
    /// </summary>
    public static class HandInitializer
    {
        public static void SetupHands()
        {
            var playerHand = Object.FindObjectOfType<PlayerHandManager>();
            var enemyHand = Object.FindObjectOfType<EnemyHandManager>();

            playerHand?.ClearAll();
            enemyHand?.GenerateInitialHand();

            Debug.Log("[HandInitializer] 플레이어/적 핸드 초기화 완료");
        }
    }
}
