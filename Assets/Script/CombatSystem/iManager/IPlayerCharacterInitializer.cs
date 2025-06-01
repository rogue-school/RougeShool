using Game.IManager;
using Game.CombatSystem.Interface;

public interface IPlayerCharacterInitializer
{
    void Setup();
    void Inject(IPlayerManager playerManager, ISlotRegistry slotRegistry);
}
