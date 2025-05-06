using Game.Battle.Initialization;

namespace Game.Battle
{
    public static class BattleStartupFacade
    {
        public static void InitializeBattle()
        {
            new CharacterInitializer().Setup();
            SlotInitializer.AutoBindAllSlots();
            HandInitializer.SetupHands();
        }
    }
}
