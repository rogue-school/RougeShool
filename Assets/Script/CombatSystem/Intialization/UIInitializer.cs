using UnityEngine;
using Game.CombatSystem.Slot;
using Game.IManager;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Intialization
{
    /// <summary>
    /// 캐릭터 UI 슬롯에 캐릭터를 배치하는 초기화 스크립트입니다.
    /// </summary>
    public class UIInitializer : MonoBehaviour
    {
        private IPlayerManager playerManager;
        private IEnemyManager enemyManager;
        private ICharacterSlotRegistry characterSlotRegistry;

        public void Initialize(IPlayerManager playerManager, IEnemyManager enemyManager, ICharacterSlotRegistry characterSlotRegistry)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
            this.characterSlotRegistry = characterSlotRegistry;
        }

        public void SetupCharacterUI()
        {
            foreach (var slot in characterSlotRegistry.GetAllCharacterSlots())
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
