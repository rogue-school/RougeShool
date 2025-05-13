using UnityEngine;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 전투 승리 UI 및 보상 처리 매니저입니다.
    /// VictoryState에서 호출되며, 추후 연출 및 보상 시스템 연결 예정입니다.
    /// </summary>
    public class VictoryManager : MonoBehaviour
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
        /// 전투 승리 UI 및 보상을 표시합니다. (임시 구현)
        /// </summary>
        public void ShowVictory()
        {
            Debug.Log("[VictoryManager] 전투 승리 UI 호출됨 (실제 보상 시스템 연결 필요)");
        }
    }
}
