using UnityEngine;
using Game.Managers;
using Game.Interface;
using Game.Slots;

namespace Game.Initialization
{
    /// <summary>
    /// 캐릭터 UI 슬롯에 캐릭터를 배치하는 초기화 스크립트입니다.
    /// </summary>
    public class UIInitializer : MonoBehaviour
    {
        private void Awake()
        {
            SetupCharacterUI();
        }

        /// <summary>
        /// 외부에서 수동으로 호출할 수 있는 UI 초기화 메서드
        /// </summary>
        public void SetupCharacterUI()
        {
            foreach (var slot in SlotRegistry.Instance.GetCharacterSlots())
            {
                switch (slot.GetOwner())
                {
                    case SlotOwner.PLAYER:
                        slot.SetCharacter(PlayerManager.Instance.GetPlayer());
                        break;

                    case SlotOwner.ENEMY:
                        slot.SetCharacter(EnemyManager.Instance.GetCurrentEnemy());
                        break;
                }
            }

            Debug.Log("[UIInitializer] 캐릭터 UI 슬롯 초기화 완료");
        }
    }
}