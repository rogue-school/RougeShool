using UnityEngine;
using Game.Characters;
using Game.UI;

namespace Game.Battle
{
    /// <summary>
    /// 전투에 등장할 플레이어와 적 캐릭터의 UI를 초기화합니다.
    /// 외부에서 캐릭터 설정을 위해 Player, Enemy 프로퍼티를 제공합니다.
    /// </summary>
    public class BattleInitializer : MonoBehaviour
    {
        private PlayerCharacter player;
        private EnemyCharacter enemy;

        /// <summary>
        /// 외부에서 플레이어 캐릭터 설정
        /// </summary>
        public PlayerCharacter Player
        {
            get => player;
            set => player = value;
        }

        /// <summary>
        /// 외부에서 적 캐릭터 설정
        /// </summary>
        public EnemyCharacter Enemy
        {
            get => enemy;
            set => enemy = value;
        }

        [SerializeField] public CharacterCardUI playerCardUI;
        [SerializeField] public CharacterCardUI enemyCardUI;

        private void Start()
        {
            if (player == null)
                player = GameObject.FindWithTag("Player")?.GetComponent<PlayerCharacter>();

            if (player != null)
                playerCardUI.Initialize(player.characterData, player);

            if (enemy != null)
                enemyCardUI.Initialize(enemy.characterData, enemy);
        }
    }
}
