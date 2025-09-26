using UnityEngine;
using Zenject;
using Game.SaveSystem.Manager;

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


