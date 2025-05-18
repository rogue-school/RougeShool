using UnityEngine;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 승리 UI 및 보상 처리 매니저입니다.
    /// </summary>
    public class VictoryManager : MonoBehaviour, IVictoryManager
    {
        public static VictoryManager Instance { get; private set; }

        [SerializeField] private GameObject victoryUI;

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
        /// 인터페이스 구현용 승리 처리 메서드
        /// </summary>
        public void ShowVictoryUI()
        {
            Debug.Log("[VictoryManager] 전투 승리 UI 호출됨 (실제 보상 시스템 연결 필요)");

            if (victoryUI != null)
                victoryUI.SetActive(true);
        }
    }
}
