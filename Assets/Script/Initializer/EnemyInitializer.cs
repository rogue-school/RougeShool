using UnityEngine;
using Game.Characters;
using Game.Slots;
using Game.Managers;

namespace Game.Battle
{
    /// <summary>
    /// 전투 시작 시 적 캐릭터를 지정된 슬롯에 배치하고 초기화합니다.
    /// 이후 교체 소환도 같은 위치에서 이뤄질 수 있습니다.
    /// </summary>
    public class EnemyInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;

        private void Awake()
        {
            Setup();
        }

        /// <summary>
        /// 외부에서 수동으로 호출 가능
        /// </summary>
        public void Setup()
        {
            var slot = SlotRegistry.Instance.GetCharacterSlot(CharacterSlotPosition.ENEMY_POSITION);

            if (slot == null)
            {
                Debug.LogError("[EnemyInitializer] ENEMY_POSITION 슬롯을 찾을 수 없습니다.");
                return;
            }

            GameObject instance = Instantiate(enemyPrefab, slot.transform.position, Quaternion.identity);
            CharacterBase character = instance.GetComponent<CharacterBase>();

            if (character == null)
            {
                Debug.LogError("[EnemyInitializer] 생성된 오브젝트에 CharacterBase가 없습니다.");
                return;
            }

            character.Initialize(character.MaxHP);
            Debug.Log("[EnemyInitializer] 적 캐릭터가 슬롯에 배치되었습니다.");
        }
    }
}
