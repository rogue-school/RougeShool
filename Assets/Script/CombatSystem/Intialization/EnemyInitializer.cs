using UnityEngine;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Initialization
{
    /// <summary>
    /// 적 캐릭터를 생성하고 슬롯에 배치하는 초기화 클래스입니다.
    /// </summary>
    public class EnemyInitializer : MonoBehaviour, IEnemyInitializer
    {
        [Header("기본 적 프리팹 및 데이터")]
        [SerializeField] private GameObject defaultEnemyPrefab;
        [SerializeField] private EnemyCharacterData defaultEnemyData;

        private IEnemyCharacter spawnedEnemy;
        private ISlotRegistry slotRegistry;

        #region 의존성 주입

        /// <summary>
        /// 슬롯 레지스트리를 주입합니다.
        /// </summary>
        /// <param name="slotRegistry">캐릭터 슬롯 레지스트리</param>
        public void Inject(ISlotRegistry slotRegistry)
        {
            this.slotRegistry = slotRegistry;
        }

        #endregion

        #region 캐릭터 초기화 및 배치

        /// <summary>
        /// 주어진 데이터로 적 캐릭터를 생성하고 슬롯에 배치합니다.
        /// </summary>
        /// <param name="data">적 캐릭터 데이터</param>
        public void SetupWithData(EnemyCharacterData data)
        {
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

        /// <summary>
        /// 생성된 적 캐릭터를 반환합니다.
        /// </summary>
        public IEnemyCharacter GetSpawnedEnemy()
        {
            return spawnedEnemy;
        }

        #endregion

        #region 내부 유틸리티 메서드

        /// <summary>
        /// 적용 가능한 적 캐릭터 슬롯을 반환합니다.
        /// </summary>
        private ICharacterSlot GetEnemySlot()
        {
            if (slotRegistry == null)
            {
                Debug.LogError("[EnemyInitializer] ISlotRegistry가 주입되지 않았습니다.");
                return null;
            }

            var slot = slotRegistry.GetCharacterSlotRegistry().GetCharacterSlot(SlotOwner.ENEMY);

            if (slot == null)
                Debug.LogError("[EnemyInitializer] ENEMY용 캐릭터 슬롯을 찾지 못했습니다.");

            return slot;
        }

        /// <summary>
        /// 슬롯에 존재하는 모든 자식 오브젝트를 제거합니다.
        /// </summary>
        private void ClearSlotChildren(ICharacterSlot slot)
        {
            foreach (Transform child in ((MonoBehaviour)slot).transform)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// 적 캐릭터 프리팹을 슬롯에 인스턴스화하고 EnemyCharacter 컴포넌트를 반환합니다.
        /// </summary>
        private EnemyCharacter InstantiateAndConfigureEnemy(EnemyCharacterData data, ICharacterSlot slot)
        {
            var prefab = data.Prefab ?? defaultEnemyPrefab;
            if (prefab == null)
            {
                Debug.LogError("[EnemyInitializer] 사용할 적 프리팹이 없습니다.");
                return null;
            }

            var instance = Instantiate(prefab, ((MonoBehaviour)slot).transform);
            instance.name = $"Enemy_{data.DisplayName}";

            if (!instance.TryGetComponent(out EnemyCharacter enemy))
            {
                Debug.LogError("[EnemyInitializer] EnemyCharacter 컴포넌트를 찾지 못했습니다.");
                Destroy(instance);
                return null;
            }

            return enemy;
        }

        /// <summary>
        /// 생성된 적 캐릭터에 데이터를 적용합니다.
        /// </summary>
        private void ApplyCharacterData(EnemyCharacter enemy, EnemyCharacterData data)
        {
            enemy.Initialize(data);
        }

        /// <summary>
        /// 슬롯에 적 캐릭터를 등록하고 내부 참조를 갱신합니다.
        /// </summary>
        private void RegisterToSlot(ICharacterSlot slot, EnemyCharacter enemy)
        {
            slot.SetCharacter(enemy);
            spawnedEnemy = enemy;
        }

        #endregion
    }
}
