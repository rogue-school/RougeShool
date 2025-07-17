using UnityEngine;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 게임 오버 상황에서 UI를 제어하고 처리를 담당하는 매니저입니다.
    /// </summary>
    public class GameOverManager : MonoBehaviour, IGameOverManager
    {
        #region 인스펙터 설정

        [SerializeField]
        private GameObject gameOverUI;

        #endregion

        #region 게임 오버 처리

        /// <summary>
        /// 게임 오버 UI를 화면에 표시합니다.
        /// </summary>
        public void ShowGameOverUI()
        {
            Debug.Log("[GameOverManager] 게임 오버 UI 호출됨");
            if (gameOverUI != null)
                gameOverUI.SetActive(true);
        }

        /// <summary>
        /// 게임 오버 처리를 시작합니다.
        /// </summary>
        public void TriggerGameOver()
        {
            Debug.Log("[GameOverManager] 게임 오버 처리 시작");
            ShowGameOverUI();
        }

        private void OnGameOverCharacterAnimation(string characterId, GameObject characterObject)
        {
            // AnimationFacade.Instance.PlayCharacterAnimation(characterId, "gameover", characterObject); // 제거
        }

        #endregion
    }
}
