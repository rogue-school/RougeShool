using UnityEngine;
using System.Collections.Generic;
using Game.Slots;
using Game.Data;
using Game.Managers;

namespace Game.Enemy
{
    /// <summary>
    /// 적 캐릭터를 전투 UI 슬롯에 배치하는 스포너 매니저입니다.
    /// </summary>
    public class EnemySpawnerManager : MonoBehaviour
    {
        [Header("전투 슬롯에 등장할 적 프리팹 (기본값)")]
        [SerializeField] private GameObject enemyPrefab;

        private readonly List<EnemyCharacter> spawnedEnemies = new();

        public EnemyCharacter SpawnEnemy(EnemyCharacterData data)
        {
            var slot = SlotRegistry.Instance.GetCharacterSlot(SlotOwner.ENEMY);

            if (slot == null)
            {
                Debug.LogError("[EnemySpawnerManager] ENEMY 슬롯을 찾지 못했습니다.");
                return null;
            }

            // 기존 자식 제거
            foreach (Transform child in slot.GetTransform())
            {
                Destroy(child.gameObject);
            }

            GameObject instance = Instantiate(data.prefab != null ? data.prefab : enemyPrefab, slot.GetTransform());
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;

            EnemyCharacter enemy = instance.GetComponent<EnemyCharacter>();
            if (enemy == null)
            {
                Debug.LogError("[EnemySpawnerManager] EnemyCharacter 컴포넌트를 찾을 수 없습니다.");
                return null;
            }

            enemy.SetCharacterData(data);
            slot.SetCharacter(enemy);
            spawnedEnemies.Add(enemy);

            Debug.Log($"[EnemySpawnerManager] 적 캐릭터 배치 완료: {data.displayName}");
            return enemy;
        }


        public List<EnemyCharacter> GetAllEnemies() => spawnedEnemies;
    }
}
