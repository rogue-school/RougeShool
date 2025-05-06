using Game.Battle.Initialization;

namespace Game.Battle
{
    public static class BattleStartupFacade
    {
        /// <summary>
        /// 전투 초기화의 시작점: 캐릭터, 슬롯, 핸드 등 일괄 초기화
        /// </summary>
        public static void InitializeBattle()
        {
            new PlayerCharacterInitializer().Setup();
            new EnemyInitializer().Setup();
            SlotInitializer.AutoBindAllSlots();
            HandInitializer.SetupHands();
        }
    }
}
