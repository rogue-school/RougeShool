using UnityEngine;
using Game.Enemy;
using Game.Data;
using Game.Slots;
using Game.Managers;

namespace Game.Initialization
{
    /// <summary>
    /// 적 캐릭터를 전투 슬롯에 배치하고 초기화합니다.
    /// </summary>
    public class EnemyInitializer : MonoBehaviour
    {
        [SerializeField] private EnemyCharacter enemyPrefab;

        private EnemyCharacter spawnedEnemy;

        /// <summary>
        /// 외부에서 주어진 EnemyCharacterData를 이용해 적을 소환합니다.
        /// </summary>
        public void SetupWithData(EnemyCharacterData enemyData)
        {
            if (enemyData == null)
            {
                Debug.LogWarning("[EnemyInitializer] 적 캐릭터 데이터가 null입니다.");
                return;
            }

            var slot = SlotRegistry.Instance.GetCharacterSlot(SlotOwner.ENEMY);

            if (slot == null)
            {
                Debug.LogError("[EnemyInitializer] 적 캐릭터 슬롯이 등록되지 않았습니다.");
                return;
            }

            var enemyGO = Instantiate(enemyPrefab, ((MonoBehaviour)slot).transform.position, Quaternion.identity);
            spawnedEnemy = enemyGO.GetComponent<EnemyCharacter>();

            if (spawnedEnemy == null)
            {
                Debug.LogError("[EnemyInitializer] EnemyCharacter 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            spawnedEnemy.Initialize(enemyData);
            slot.SetCharacter(spawnedEnemy);

            Debug.Log($"[EnemyInitializer] 적 캐릭터가 슬롯에 배치되었습니다: {enemyData.displayName}");
        }

        /// <summary>
        /// 생성된 적 캐릭터 반환
        /// </summary>
        public EnemyCharacter GetSpawnedEnemy() => spawnedEnemy;
    }
}
