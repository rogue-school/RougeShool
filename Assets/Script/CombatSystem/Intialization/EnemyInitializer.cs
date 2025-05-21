using UnityEngine;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Initialization
{
    /// <summary>
    /// 적 캐릭터를 초기화하고 슬롯에 배치합니다.
    /// </summary>
    public class EnemyInitializer : MonoBehaviour, IEnemyInitializer
    {
        [Header("기본 적 프리팹 및 데이터")]
        [SerializeField] private GameObject defaultEnemyPrefab;
        [SerializeField] private EnemyCharacterData defaultEnemyData;

        private IEnemyCharacter spawnedEnemy;

        public void SetupWithData(EnemyCharacterData data)
        {
            Debug.Log("[EnemyInitializer] SetupWithData() 호출됨");

            if (data == null)
            {
                Debug.LogError("[EnemyInitializer] EnemyCharacterData가 null입니다.");
                return;
            }

            var slot = GetEnemySlot();
            if (slot == null)
            {
                Debug.LogError("[EnemyInitializer] 적 슬롯을 찾지 못했습니다. 초기화 중단");
                return;
            }

            ClearSlotChildren(slot);

            var enemy = InstantiateAndConfigureEnemy(data, slot);
            if (enemy == null)
            {
                Debug.LogError("[EnemyInitializer] 적 인스턴스 생성 실패");
                return;
            }

            ApplyCharacterData(enemy, data);
            RegisterToSlot(slot, enemy);
        }

        public IEnemyCharacter GetSpawnedEnemy()
        {
            Debug.Log("[EnemyInitializer] GetSpawnedEnemy() 호출됨");
            return spawnedEnemy;
        }

        private ICharacterSlot GetEnemySlot()
        {
            Debug.Log("[EnemyInitializer] 적 슬롯 가져오기 시도");

            var slot = SlotRegistry.Instance?.GetCharacterSlot(SlotOwner.ENEMY);
            if (slot == null)
            {
                Debug.LogError("[EnemyInitializer] ENEMY용 캐릭터 슬롯을 찾지 못했습니다.");
            }
            else
            {
                Debug.Log($"[EnemyInitializer] 슬롯 이름: {((MonoBehaviour)slot).name}");
            }

            return slot;
        }

        private void ClearSlotChildren(ICharacterSlot slot)
        {
            Debug.Log("[EnemyInitializer] 슬롯 자식 제거 시작");

            foreach (Transform child in ((MonoBehaviour)slot).transform)
            {
                Debug.Log($" → 제거 대상: {child.name}");
                Destroy(child.gameObject);
            }
        }

        private EnemyCharacter InstantiateAndConfigureEnemy(EnemyCharacterData data, ICharacterSlot slot)
        {
            Debug.Log("[EnemyInitializer] 적 프리팹 인스턴스화 시도");

            var prefab = data.prefab ?? defaultEnemyPrefab;
            if (prefab == null)
            {
                Debug.LogError("[EnemyInitializer] 사용할 적 프리팹이 없습니다.");
                return null;
            }

            var instance = Instantiate(prefab, ((MonoBehaviour)slot).transform);
            instance.name = $"Enemy_{data.displayName}";

            if (!instance.TryGetComponent(out EnemyCharacter enemy))
            {
                Debug.LogError("[EnemyInitializer] EnemyCharacter 컴포넌트를 찾지 못했습니다.");
                Destroy(instance);
                return null;
            }

            Debug.Log("[EnemyInitializer] 적 인스턴스 생성 및 컴포넌트 확인 완료");
            return enemy;
        }

        private void ApplyCharacterData(EnemyCharacter enemy, EnemyCharacterData data)
        {
            Debug.Log("[EnemyInitializer] 캐릭터 데이터 적용 시작");
            enemy.Initialize(data);
        }

        private void RegisterToSlot(ICharacterSlot slot, EnemyCharacter enemy)
        {
            Debug.Log("[EnemyInitializer] 슬롯에 적 캐릭터 등록 시작");
            slot.SetCharacter(enemy);

            spawnedEnemy = enemy;
            Debug.Log("[EnemyInitializer] 적 캐릭터 슬롯 등록 및 내부 참조 완료");
        }
    }
}
