using UnityEngine;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 게임 오버 UI를 제어하는 매니저입니다.
    /// GameOverState에서 호출되며, 추후 UI 연출 연결 예정입니다.
    /// </summary>
    public class GameOverManager : MonoBehaviour
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
        /// 게임 오버 UI를 표시합니다. (임시 구현)
        /// </summary>
        public void ShowGameOver()
        {
            Debug.Log("[GameOverManager] 게임 오버 UI 호출됨 (실제 UI 연결 필요)");
        }
    }
}
