using UnityEngine;
using Game.CombatSystem.Slot;
using Game.IManager;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Intialization
{
    /// <summary>
    /// 캐릭터 UI 슬롯에 플레이어와 적 캐릭터를 배치하는 초기화 스크립트입니다.
    /// 전투 UI에 등장 캐릭터를 정확히 표시하기 위해 사용됩니다.
    /// </summary>
    public class UIInitializer : MonoBehaviour
    {
        #region 필드

        private IPlayerManager playerManager;
        private IEnemyManager enemyManager;
        private ICharacterSlotRegistry characterSlotRegistry;

        #endregion

        #region 초기화

        /// <summary>
        /// 매니저 및 레지스트리를 주입합니다.
        /// </summary>
        /// <param name="playerManager">플레이어 매니저</param>
        /// <param name="enemyManager">적 매니저</param>
        /// <param name="characterSlotRegistry">캐릭터 슬롯 레지스트리</param>
        public void Initialize(IPlayerManager playerManager, IEnemyManager enemyManager, ICharacterSlotRegistry characterSlotRegistry)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
            this.characterSlotRegistry = characterSlotRegistry;
        }

        #endregion

        #region UI 배치 처리

        /// <summary>
        /// 각 캐릭터 슬롯에 해당 캐릭터를 배치합니다.
        /// </summary>
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

        #endregion
    }
}
