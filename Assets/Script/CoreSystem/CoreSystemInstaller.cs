using UnityEngine;
using Zenject;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Save;
using System.Linq;
using System.Collections.Generic;

namespace Game.CoreSystem
{
    /// <summary>
    /// CoreSystem용 Zenject 설치자
    /// 모든 코어 시스템 매니저와 서비스를 바인딩합니다.
    /// </summary>
    public class CoreSystemInstaller : MonoInstaller<CoreSystemInstaller>
    {
        [Header("CoreSystem 매니저들")]
        [SerializeField] private CoreSystemInitializer coreSystemInitializer;
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private SceneTransitionManager sceneTransitionManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private CoroutineRunner coroutineRunner;
        [SerializeField] private PlayerCharacterSelectionManager playerCharacterSelectionManager;

        public override void InstallBindings()
        {
            BindCoreManagers();
            BindCoreServices();
            BindCoreUtilities();
            BindCoreInterfaces();
        }

        #region 코어 매니저 바인딩

        /// <summary>
        /// 코어 매니저들을 바인딩합니다.
        /// </summary>
        private void BindCoreManagers()
        {
            // CoreSystemInitializer - 최우선 바인딩
            EnsureAndBindCoreManager<CoreSystemInitializer>(coreSystemInitializer, "CoreSystemInitializer");
            
            // GameStateManager
            EnsureAndBindCoreManagerWithInterface<GameStateManager, IGameStateManager>(gameStateManager, "GameStateManager");
            
            // SceneTransitionManager
            EnsureAndBindCoreManagerWithInterface<SceneTransitionManager, ISceneTransitionManager>(sceneTransitionManager, "SceneTransitionManager");
            
            // AudioManager
            EnsureAndBindCoreManagerWithInterface<AudioManager, IAudioManager>(audioManager, "AudioManager");
            
            // SaveManager
            EnsureAndBindCoreManagerWithInterface<SaveManager, ISaveManager>(saveManager, "SaveManager");
            
            
            // PlayerCharacterSelectionManager
            EnsureAndBindCoreManagerWithInterface<PlayerCharacterSelectionManager, IPlayerCharacterSelectionManager>(playerCharacterSelectionManager, "PlayerCharacterSelectionManager");
            
        }

        #endregion

        #region 코어 서비스 바인딩

        /// <summary>
        /// 코어 서비스들을 바인딩합니다.
        /// </summary>
        private void BindCoreServices()
        {
            // AudioPoolManager는 AudioManager에 포함되어 있으므로 별도 바인딩 불필요
            // CardStateCollector, CardStateRestorer는 SaveManager에 포함되어 있으므로 별도 바인딩 불필요
        }

        #endregion

        #region 코어 유틸리티 바인딩

        /// <summary>
        /// 코어 유틸리티들을 바인딩합니다.
        /// </summary>
        private void BindCoreUtilities()
        {
            // CoroutineRunner - 인터페이스와 함께 바인딩
            EnsureAndBindCoreManagerWithInterface<CoroutineRunner, ICoroutineRunner>(coroutineRunner, "CoroutineRunner");
            
            // GameLogger (정적 클래스이므로 인스턴스 바인딩 불필요)
            // GameLogger는 정적 메서드로 사용하므로 DI 바인딩하지 않음
        }

        #endregion

        #region 인터페이스 바인딩

        /// <summary>
        /// 코어 인터페이스들을 바인딩합니다.
        /// </summary>
        private void BindCoreInterfaces()
        {
            // ICoreSystemInitializable 구현체들을 자동으로 찾아서 바인딩
            var initializableComponents = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<ICoreSystemInitializable>();
            
            // 이미 바인딩된 ICoreSystemInitializable 컴포넌트들 제외
            var alreadyBoundComponents = new HashSet<ICoreSystemInitializable>
            {
                gameStateManager,
                sceneTransitionManager,
                audioManager,
                saveManager,
                playerCharacterSelectionManager
            }.Where(x => x != null);
            
            var componentsToBind = initializableComponents.Except(alreadyBoundComponents);
            
            foreach (var component in componentsToBind)
            {
                Container.BindInterfacesAndSelfTo(component.GetType()).FromInstance(component).AsSingle();
            }
            
            // ICoreSystemInitializable 리스트로 바인딩 (CoreSystemInitializer에서 사용)
            Container.Bind<List<ICoreSystemInitializable>>().FromMethod(context =>
            {
                return new List<ICoreSystemInitializable>(initializableComponents);
            }).AsSingle();
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 코어 매니저를 보장하고 바인딩합니다.
        /// </summary>
        private void EnsureAndBindCoreManager<T>(T instance, string gameObjectName) where T : MonoBehaviour
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
                if (instance == null)
                {
                    var go = new GameObject(gameObjectName);
                    instance = go.AddComponent<T>();
                    DontDestroyOnLoad(go);
                }
            }
            
            // 의존성 주입 예약
            Container.QueueForInject(instance);
            Container.Bind<T>().FromInstance(instance).AsSingle();
        }

        /// <summary>
        /// 코어 매니저를 보장하고 인터페이스와 함께 바인딩합니다.
        /// </summary>
        private void EnsureAndBindCoreManagerWithInterface<TConcrete, TInterface>(TConcrete instance, string gameObjectName) 
            where TConcrete : MonoBehaviour, TInterface
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<TConcrete>();
                if (instance == null)
                {
                    var go = new GameObject(gameObjectName);
                    instance = go.AddComponent<TConcrete>();
                    DontDestroyOnLoad(go);
                }
            }
            
            // 의존성 주입 예약
            Container.QueueForInject(instance);
            
            // 구체 타입과 인터페이스를 하나의 바인딩으로 처리
            Container.BindInterfacesAndSelfTo<TConcrete>().FromInstance(instance).AsSingle();
        }

        #endregion

    }
}
