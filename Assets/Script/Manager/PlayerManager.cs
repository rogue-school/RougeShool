using UnityEngine;
using Game.Characters;
using Game.Player;

namespace Game.Managers
{
    /// <summary>
    /// 플레이어 캐릭터를 관리하는 매니저입니다.
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        [SerializeField] private PlayerCharacter player;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// 플레이어 캐릭터를 반환합니다.
        /// </summary>
        public PlayerCharacter GetPlayer() => player;

        /// <summary>
        /// 플레이어 캐릭터를 런타임 중 설정합니다.
        /// </summary>
        public void SetPlayer(PlayerCharacter player)
        {
            this.player = player;
        }
    }
}
