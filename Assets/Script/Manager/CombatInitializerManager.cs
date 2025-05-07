using UnityEngine;
using Game.Combat.Turn;
using Game.Battle.Initialization;
using Game.Initialization;
using Game.Managers;

namespace Game.Combat.Initialization
{
    /// <summary>
    /// 전투(컴뱃) 초기화 파트를 개별적으로 제어할 수 있는 매니저입니다.
    /// </summary>
    public class CombatInitializerManager : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterInitializer playerInitializer;
        [SerializeField] private EnemyInitializer enemyInitializer;
        [SerializeField] private SlotInitializer slotInitializer;
        [SerializeField] private UIInitializer uiInitializer;

        /// <summary>
        /// 모든 컴뱃 초기화 파트를 일괄 실행합니다.
        /// </summary>
        public void InitializeAll()
        {
            playerInitializer.Setup();
            enemyInitializer.Setup();
            slotInitializer.AutoBindAllSlots();
            HandInitializer.SetupHands();
            uiInitializer.SetupCharacterUI();
            Debug.Log("[CombatInitializerManager] 모든 컴뱃 초기화 완료");
        }

        /// <summary>
        /// 슬롯만 따로 초기화하고 싶을 때 호출할 수 있습니다.
        /// </summary>
        public void InitializeOnlySlots()
        {
            slotInitializer.AutoBindAllSlots();
        }

        /// <summary>
        /// 핸드만 따로 초기화하고 싶을 때 호출할 수 있습니다.
        /// </summary>
        public void InitializeOnlyHands()
        {
            HandInitializer.SetupHands();
        }
    }
}
