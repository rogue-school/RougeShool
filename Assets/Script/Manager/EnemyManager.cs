using UnityEngine;
using Game.Enemy;

namespace Game.Managers
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
        /// 현재 씬에 등장한 적을 설정합니다.
        /// </summary>
        public void SetEnemy(EnemyCharacter enemy)
        {
            currentEnemy = enemy;
        }

        /// <summary>
        /// 현재 적 캐릭터를 반환합니다.
        /// </summary>
        public EnemyCharacter GetCurrentEnemy()
        {
            return currentEnemy;
        }

        /// <summary>
        /// 현재 적 캐릭터가 존재하는지 여부
        /// </summary>
        public bool HasEnemy()
        {
            return currentEnemy != null;
        }
    }
}
