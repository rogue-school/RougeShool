using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Utility
{
    public static class PlayerInputGuard
    {
        public static bool CanProceed(ICombatFlowCoordinator flow)
            => flow != null && flow.IsPlayerInputEnabled();
    }
}
