using UnityEngine;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    public class GameOverManager : MonoBehaviour, IGameOverManager
    {
        [SerializeField] private GameObject gameOverUI;

        public void ShowGameOverUI()
        {
            Debug.Log("[GameOverManager] 게임 오버 UI 호출됨");
            if (gameOverUI != null)
                gameOverUI.SetActive(true);
        }

        public void TriggerGameOver()
        {
            Debug.Log("[GameOverManager] 게임 오버 처리 시작");
            ShowGameOverUI();
        }
    }
}
