using UnityEngine;
using Game.Combat.Initialization;
using Game.Battle.Initialization;
using Game.Initialization;

namespace Game.Combat
{
    /// <summary>
    /// 컴뱃(전투) 초기화의 시작점 클래스입니다.
    /// 캐릭터, 슬롯, 핸드 등의 초기화를 통합 실행합니다.
    /// </summary>
    public static class CombatStartupFacade
    {
        /// <summary>
        /// 컴뱃 시스템 전체 초기화를 실행합니다.
        /// </summary>
        public static void InitializeCombat()
        {
            new PlayerCharacterInitializer().Setup();
            new EnemyInitializer().Setup();

            SlotInitializer slotInitializer = Object.FindObjectOfType<SlotInitializer>();
            slotInitializer?.AutoBindAllSlots();

            HandInitializer.SetupHands();
        }
    }
}