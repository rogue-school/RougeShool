using UnityEngine;
using Zenject;
using Game.AnimationSystem.Interface;
using Game.AnimationSystem.Manager;
using Game.CoreSystem.Interface;

namespace Game.AnimationSystem.Manager
{
    /// <summary>
    /// AnimationSystem용 Zenject Installer.
    /// 애니메이션 시스템의 모든 컴포넌트를 바인딩합니다.
    /// </summary>
    public class AnimationSystemInstaller : MonoInstaller<AnimationSystemInstaller>
    {
        [Header("애니메이션 매니저")]
        [SerializeField] private AnimationFacade animationFacade;

        public override void InstallBindings()
        {
            BindAnimationManagers();
            BindAnimationServices();
        }

        private void BindAnimationManagers()
        {
            // AnimationFacade 바인딩
            if (animationFacade != null)
            {
                Container.Bind<AnimationFacade>().FromInstance(animationFacade).AsSingle();
                Container.Bind<IAnimationFacade>().FromInstance(animationFacade).AsSingle();
            }
            else
            {
                Debug.LogWarning("[AnimationSystemInstaller] AnimationFacade가 연결되지 않았습니다.");
            }
        }

        private void BindAnimationServices()
        {
            // AnimationDatabaseManager는 CoreSystemInstaller에서 이미 바인딩됨
            // 여기서는 추가적인 애니메이션 서비스들을 바인딩할 수 있음
        }
    }
}
