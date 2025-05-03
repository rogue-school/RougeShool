using UnityEngine;
using Game.UI;
using Game.Units;

namespace Game.Battle
{
    /// <summary>
    /// 전투 씬 진입 시, 캐릭터 UI를 초기화하는 매니저
    /// </summary>
    public class BattleInitializer : MonoBehaviour
    {
        public PlayerUnit player;
        public CharacterCardUI playerCardUI;

        public EnemyUnit enemy;
        public CharacterCardUI enemyCardUI;

        private void Start()
        {
            // 플레이어 초기화
            var playerData = player.characterData;
            playerCardUI.Initialize(playerData, player);

            // 적 초기화
            var enemyData = enemy.characterData;
            enemyCardUI.Initialize(enemyData, enemy);
        }
    }
}
