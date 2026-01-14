using Zenject;

namespace Game.ItemSystem.Service.Reward
{
	public class RewardInstaller : MonoInstaller<RewardInstaller>
	{
		public override void InstallBindings()
		{
			Container.Bind<IRewardGenerator>().To<RewardGenerator>().AsSingle();
		}
	}
}
