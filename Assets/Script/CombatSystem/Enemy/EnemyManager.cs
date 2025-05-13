using UnityEngine;
using Game.CharacterSystem.Core;

namespace Game.CombatSystem.Enemy
{
    /// <summary>
    /// 전투 씬에 등장한 현재 적 캐릭터를 관리하는 매니저입니다.
    /// 적은 항상 한 명만 존재합니다.
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        private EnemyCharacter currentEnemy;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 적 캐릭터 등록 (겹치는 Register 메서드 통합)
        /// </summary>
        public void SetEnemy(EnemyCharacter enemy)
        {
            if (enemy == null)
            {
                Debug.LogWarning("[EnemyManager] 등록된 적이 null입니다.");
                return;
            }

            currentEnemy = enemy;
            Debug.Log($"[EnemyManager] 적 등록 완료: {enemy.name}");
        }

        public EnemyCharacter GetCurrentEnemy()
        {
            if (currentEnemy == null)
                Debug.LogWarning("[EnemyManager] 적 캐릭터가 설정되지 않았습니다.");
            return currentEnemy;
        }

        public bool HasEnemy() => currentEnemy != null;
    }
}
