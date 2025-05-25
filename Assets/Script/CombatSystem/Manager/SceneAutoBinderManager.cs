using UnityEngine;
using Game.CombatSystem.Manager;

namespace Game.Utility
{
    /// <summary>
    /// 씬에서 자동으로 오브젝트를 찾아 참조를 연결하는 유틸 매니저입니다.
    /// Unity 6000.0 이상에서는 FindFirstObjectByType을 사용합니다.
    /// </summary>
    public class SceneAutoBinderManager : MonoBehaviour
    {
        [SerializeField] private CombatSlotManager battleSlotManager;
        [SerializeField] private PlayerHandManager playerHandManager;
        [SerializeField] private EnemyHandManager enemyHandManager;

        private void Awake()
        {
            battleSlotManager ??= FindFirstObjectByType<CombatSlotManager>();
            playerHandManager ??= FindFirstObjectByType<PlayerHandManager>();
            enemyHandManager ??= FindFirstObjectByType<EnemyHandManager>();

            if (battleSlotManager == null)
                Debug.LogWarning("[SceneAutoBinderManager] CombatSlotManager를 찾지 못했습니다.");

            if (playerHandManager == null)
                Debug.LogWarning("[SceneAutoBinderManager] PlayerHandManager를 찾지 못했습니다.");

            if (enemyHandManager == null)
                Debug.LogWarning("[SceneAutoBinderManager] EnemyHandManager를 찾지 못했습니다.");

            Debug.Log("[SceneAutoBinderManager] 자동 바인딩 완료");
        }
        public void Initialize()
        {
            battleSlotManager ??= FindFirstObjectByType<CombatSlotManager>();
            playerHandManager ??= FindFirstObjectByType<PlayerHandManager>();
            enemyHandManager ??= FindFirstObjectByType<EnemyHandManager>();

            Debug.Log("[SceneAutoBinderManager] 수동 바인딩 완료");
        }

    }
}
