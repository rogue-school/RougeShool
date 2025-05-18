using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.IManager;

namespace Game.Manager
{
    /// <summary>
    /// 플레이어 캐릭터와 핸드 매니저를 관리하는 클래스입니다.
    /// </summary>
    public class PlayerManager : MonoBehaviour, IPlayerManager
    {
        private IPlayerCharacter player;
        private IPlayerHandManager handManager;

        public void SetPlayer(IPlayerCharacter player)
        {
            this.player = player;
            Debug.Log("[PlayerManager] 플레이어 캐릭터가 등록되었습니다.");
        }

        public IPlayerCharacter GetPlayer()
        {
            if (player == null)
            {
                Debug.LogWarning("[PlayerManager] 플레이어 캐릭터가 아직 등록되지 않았습니다.");
            }
            return player;
        }

        public void SetPlayerHandManager(IPlayerHandManager manager)
        {
            handManager = manager;
            Debug.Log("[PlayerManager] 핸드 매니저가 등록되었습니다.");
        }

        public IPlayerHandManager GetPlayerHandManager()
        {
            if (handManager == null)
            {
                Debug.LogWarning("[PlayerManager] 핸드 매니저가 아직 등록되지 않았습니다.");
            }
            return handManager;
        }
    }
}
