using UnityEngine;
using Game.Enemy;
using Game.Data;
using Game.Interface;
using Game.Slots;
using Game.Managers;

namespace Game.Initialization
{
    public class EnemyInitializer : MonoBehaviour
    {
        [SerializeField] private EnemyCharacter enemyPrefab;
        [SerializeField] private EnemyCharacterData defaultData;

        public void Setup()
        {
            if (defaultData == null)
            {
                Debug.LogWarning("[EnemyInitializer] 기본 데이터가 없습니다.");
                return;
            }

            var slot = SlotRegistry.Instance.GetCharacterSlot(SlotOwner.ENEMY);

            if (slot == null)
            {
                Debug.LogError("[EnemyInitializer] 적 캐릭터 슬롯이 등록되지 않았습니다.");
                return;
            }

            var enemyGO = Instantiate(enemyPrefab, ((MonoBehaviour)slot).transform.position, Quaternion.identity);
            EnemyCharacter enemy = enemyGO.GetComponent<EnemyCharacter>();
            enemy.SetCharacterData(defaultData);
            slot.SetCharacter(enemy);

            Debug.Log("[EnemyInitializer] 적 캐릭터가 슬롯에 배치되었습니다.");
        }
    }
}
