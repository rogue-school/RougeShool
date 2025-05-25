using Game.CombatSystem.Interface;

namespace Game.System
{
    public static class GameDataResetter
    {
        private static IBattleResetService _battleResetService;

        public static void Initialize(IBattleResetService battleResetService)
        {
            _battleResetService = battleResetService;
        }

        public static void Reset()
        {
            _battleResetService?.ResetAll();
        }
    }
}
