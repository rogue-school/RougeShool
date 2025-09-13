using UnityEngine;
using Game.CombatSystem.Manager;
using Game.SkillCardSystem.Manager;

namespace Game.Utility
{
    /// <summary>
    /// 씬 내에서 주요 매니저 오브젝트를 자동으로 검색하여 연결하는 유틸리티 클래스입니다.
    /// Unity 6000.0 이상 버전에서는 <c>FindFirstObjectByType</c>을 사용합니다.
    /// </summary>
    public class SceneAutoBinderManager : MonoBehaviour
    {
        #region 필드

        [SerializeField] private CombatSlotManager battleSlotManager;
        [SerializeField] private PlayerHandManager playerHandManager;
        [SerializeField] private EnemyHandManager enemyHandManager;

        #endregion

        #region Unity 라이프사이클

        private void Awake()
        {
            AutoBindReferences();

            if (battleSlotManager == null)
                Debug.LogWarning("[SceneAutoBinderManager] CombatSlotManager를 찾지 못했습니다.");

            if (playerHandManager == null)
                Debug.LogWarning("[SceneAutoBinderManager] PlayerHandManager를 찾지 못했습니다.");

            if (enemyHandManager == null)
                Debug.LogWarning("[SceneAutoBinderManager] EnemyHandManager를 찾지 못했습니다.");

            Debug.Log("[SceneAutoBinderManager] 자동 바인딩 완료");
        }

        #endregion

        #region 바인딩 메서드

        /// <summary>
        /// 필요한 매니저들을 FindFirstObjectByType을 통해 자동으로 바인딩합니다.
        /// </summary>
        private void AutoBindReferences()
        {
            battleSlotManager ??= FindFirstObjectByType<CombatSlotManager>();
            playerHandManager ??= FindFirstObjectByType<PlayerHandManager>();
            enemyHandManager ??= FindFirstObjectByType<EnemyHandManager>();
        }

        /// <summary>
        /// 외부에서 수동으로 바인딩을 요청할 수 있는 메서드입니다.
        /// </summary>
        public void Initialize()
        {
            AutoBindReferences();
            Debug.Log("[SceneAutoBinderManager] 수동 바인딩 완료");
        }

        private void OnSceneCharacterSpawned(string characterId, GameObject characterObject)
        {
            // AnimationFacade.Instance.PlayCharacterAnimation(characterId, "spawn", characterObject); // 제거
        }

        #endregion
    }
}
