using Game.IManager;

public interface IPlayerCharacterInitializer
{
    void Setup();
    void Inject(IPlayerManager playerManager);
}
