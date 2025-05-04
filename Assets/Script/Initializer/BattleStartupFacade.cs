using Game.Battle.Initialization;

namespace Game.Battle
{
    /// <summary>
    /// 전투 시작 시 모든 초기화 과정을 통합 호출하는 파사드입니다.
    /// </summary>
    public static class BattleStartupFacade
    {
        public static void InitializeBattle()
        {
            SlotInitializer.AutoBindAllSlots();
            CharacterInitializer.SetupCharacters();
            HandInitializer.SetupHands();
            UIInitializer.SetupCharacterUI();
        }
    }
}
