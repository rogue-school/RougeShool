using Game.CombatSystem.Interface;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.CombatSystem.Initializer
{
    /// <summary>
    /// 플레이어와 적의 핸드를 초기화하는 클래스입니다.
    /// 전투 시작 시 핸드 클리어 및 적 카드 생성 등의 초기 세팅을 수행합니다.
    /// </summary>
    public class HandInitializer : IHandInitializer
    {
        private readonly IPlayerHandManager playerHand;
        private readonly IEnemyHandManager enemyHand;

        /// <summary>
        /// 핸드 초기화기 생성자입니다.
        /// </summary>
        /// <param name="playerHand">플레이어 핸드 매니저</param>
        /// <param name="enemyHand">적 핸드 매니저</param>
        public HandInitializer(IPlayerHandManager playerHand, IEnemyHandManager enemyHand)
        {
            this.playerHand = playerHand;
            this.enemyHand = enemyHand;
        }

        /// <summary>
        /// 플레이어 핸드를 클리어하고, 적 핸드를 생성합니다.
        /// </summary>
        public void SetupHands()
        {
            playerHand.ClearAll();
            enemyHand.GenerateInitialHand();
            Debug.Log("[HandInitializer] 핸드 초기화 완료");
        }
    }
}
