using UnityEngine;
using Game.Characters;
using Game.Player;

namespace Game.Managers
{
    /// <summary>
    /// 플레이어 캐릭터 및 핸드 시스템을 관리하는 매니저입니다.
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        [SerializeField] private PlayerCharacter player;
        [SerializeField] private PlayerHandManager playerHandManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public PlayerCharacter GetPlayer()
        {
            if (player == null)
                Debug.LogWarning("[PlayerManager] 플레이어 캐릭터가 설정되지 않았습니다.");
            return player;
        }

        public void SetPlayer(PlayerCharacter player)
        {
            this.player = player;
        }

        public PlayerHandManager GetPlayerHandManager()
        {
            if (playerHandManager == null)
                Debug.LogWarning("[PlayerManager] PlayerHandManager가 설정되지 않았습니다.");
            return playerHandManager;
        }

        public void SetPlayerHandManager(PlayerHandManager manager)
        {
            playerHandManager = manager;
        }

        public bool HasPlayer() => player != null;
        public bool HasHandManager() => playerHandManager != null;
    }
}
