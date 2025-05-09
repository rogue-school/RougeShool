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
            var playerHand = Object.FindFirstObjectByType<PlayerHandManager>();
            var enemyHand = Object.FindFirstObjectByType<EnemyHandManager>();

            if (playerHand == null)
            {
                Debug.LogError("[HandInitializer] PlayerHandManager를 찾을 수 없습니다.");
            }
            else
            {
                playerHand.ClearAll();
            }

            if (enemyHand == null)
            {
                Debug.LogError("[HandInitializer] EnemyHandManager를 찾을 수 없습니다.");
            }
            else
            {
                enemyHand.GenerateInitialHand();
            }

            Debug.Log("[HandInitializer] 플레이어/적 핸드 초기화 완료");
        }
    }
}
