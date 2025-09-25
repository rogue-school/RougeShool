using UnityEngine;
using Zenject;
using Game.SaveSystem.Manager;
using Game.SaveSystem.Interface;

namespace Game.SaveSystem.Installer
{
    /// <summary>
    /// CoreScene 전용 SaveSystem 바인딩용 Installer.
    /// 코어에서 생성해 DontDestroyOnLoad로 전투 씬까지 유지합니다.
    /// </summary>
    public class SaveSystemInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // ICardStateCollector
            var collector = FindFirstObjectByType<CardStateCollector>();
            if (collector == null)
            {
                // 전투 씬 의존성이 있어 NonLazy를 제거하여 생성 시점을 지연합니다.
                Container.BindInterfacesAndSelfTo<CardStateCollector>()
                    .FromNewComponentOnNewGameObject()
                    .AsSingle();
            }
            else
            {
                Container.Bind<ICardStateCollector>().FromInstance(collector).AsSingle();
            }

            // ICardStateRestorer
            var restorer = FindFirstObjectByType<CardStateRestorer>();
            if (restorer == null)
            {
                // 전투 씬 의존성이 있어 NonLazy를 제거하여 생성 시점을 지연합니다.
                Container.BindInterfacesAndSelfTo<CardStateRestorer>()
                    .FromNewComponentOnNewGameObject()
                    .AsSingle();
            }
            else
            {
                Container.Bind<ICardStateRestorer>().FromInstance(restorer).AsSingle();
            }

            // AutoSaveManager (코어에서 생성하여 전투 씬까지 유지)
            var auto = FindFirstObjectByType<AutoSaveManager>();
            if (auto == null)
            {
                Container.BindInterfacesAndSelfTo<AutoSaveManager>()
                    .FromNewComponentOnNewGameObject()
                    .AsSingle()
                    .NonLazy();
            }
            else
            {
                Container.Bind<AutoSaveManager>().FromInstance(auto).AsSingle();
            }
        }
    }
}


