using UnityEngine;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 게임 오버 UI를 제어하는 매니저입니다.
    /// </summary>
    public class GameOverManager : MonoBehaviour, IGameOverManager
    {
        public static GameOverManager Instance { get; private set; }

        [SerializeField] private GameObject gameOverUI;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 인터페이스 구현: 게임 오버 UI를 표시
        /// </summary>
        public void ShowGameOverUI()
        {
            Debug.Log("[GameOverManager] 게임 오버 UI 호출됨 (실제 UI 연결 필요)");

            if (gameOverUI != null)
                gameOverUI.SetActive(true);
        }
    }
}
