using UnityEngine;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 승리 시 UI를 표시하고 관련 처리를 수행하는 매니저입니다.
    /// </summary>
    public class VictoryManager : MonoBehaviour, IVictoryManager
    {
        #region 인스펙터 필드

        [SerializeField] private GameObject victoryUI;

        #endregion

        #region UI 처리

        /// <summary>
        /// 승리 UI를 화면에 표시합니다.
        /// </summary>
        public void ShowVictoryUI()
        {
            Debug.Log("[VictoryManager] 전투 승리 UI 호출됨");
            if (victoryUI != null)
                victoryUI.SetActive(true);
        }

        /// <summary>
        /// 승리 시 처리 로직을 실행합니다.
        /// </summary>
        public void ProcessVictory()
        {
            Debug.Log("[VictoryManager] 승리 처리 시작");
            ShowVictoryUI();
        }

        #endregion

        private void OnVictoryCharacterAnimation(string characterId, GameObject characterObject)
        {
            // AnimationFacade.Instance.PlayCharacterAnimation(characterId, "victory", characterObject); // 제거
        }
    }
}
