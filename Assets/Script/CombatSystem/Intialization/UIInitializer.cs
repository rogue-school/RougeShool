using UnityEngine;
using Game.CombatSystem.Slot;
using Game.IManager;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Intialization
{
    /// <summary>
    /// 캐릭터 UI 슬롯에 캐릭터를 배치하는 초기화 스크립트입니다.
    /// </summary>
    public class UIInitializer : MonoBehaviour
    {
        private IPlayerManager playerManager;
        private IEnemyManager enemyManager;
        private ISlotRegistry slotRegistry;

        public void Initialize(IPlayerManager playerManager, IEnemyManager enemyManager, ISlotRegistry slotRegistry)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
            this.slotRegistry = slotRegistry;
        }

        private void Awake()
        {
            // 기본 동작은 제거하거나 외부에서 Initialize 호출 보장
        }

        public void SetupCharacterUI()
        {
            foreach (var slot in slotRegistry.GetCharacterSlots())
            {
                switch (slot.GetOwner())
                {
                    case SlotOwner.PLAYER:
                        slot.SetCharacter(playerManager.GetPlayer());
                        break;

                    case SlotOwner.ENEMY:
                        slot.SetCharacter(enemyManager.GetCurrentEnemy());
                        break;
                }
            }

            Debug.Log("[UIInitializer] 캐릭터 UI 슬롯 초기화 완료 (DI 기반)");
        }
    }
}
