using Zenject;
using UnityEngine;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Save;
using Game.CoreSystem.UI;
using Game.CharacterSystem.Interface;

namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// MainScene 전용 Zenject Installer.
    /// CoreScene에서 이미 AsSingle로 등록된 전역 매니저들을 재해결하여
    /// MainScene의 컴포넌트(예: PlayerCharacterSelector)에 주입 가능하게 합니다.
    /// </summary>
    public class MainSceneInstaller : MonoInstaller<MainSceneInstaller>
    {
        public override void InstallBindings()
        {
            // 코어 매니저들은 DontDestroyOnLoad로 살아있음
            // 현재 씬 컨텍스트에는 부모 컨텍스트가 없으므로 FromResolve()는 순환 호출을 유발할 수 있음
            // 안전하게 전역에서 직접 탐색하여 바인딩
            
            // 게임 상태 매니저 바인딩
            Container.Bind<IGameStateManager>().FromMethod(_ =>
            {
                return Object.FindFirstObjectByType<GameStateManager>(FindObjectsInactive.Include);
            }).AsSingle();

            // 저장 매니저 바인딩
            Container.Bind<ISaveManager>().FromMethod(_ =>
            {
                return Object.FindFirstObjectByType<SaveManager>(FindObjectsInactive.Include);
            }).AsSingle();

            // 설정 매니저 바인딩
            Container.Bind<SettingsManager>().FromMethod(_ =>
            {
                return Object.FindFirstObjectByType<SettingsManager>(FindObjectsInactive.Include);
            }).AsSingle();

            // 플레이어 캐릭터 선택 매니저 바인딩
            Container.Bind<IPlayerCharacterSelectionManager>().FromMethod(_ =>
            {
                return Object.FindFirstObjectByType<PlayerCharacterSelectionManager>(FindObjectsInactive.Include);
            }).AsSingle();

            // 씬 전환 매니저 바인딩
            Container.Bind<ISceneTransitionManager>().FromMethod(_ =>
            {
                return Object.FindFirstObjectByType<SceneTransitionManager>(FindObjectsInactive.Include);
            }).AsSingle();

            // 필요 시 Canvas 등 씬 전용 컴포넌트 주입(원하면 주석 해제)
            // Container.Bind<UnityEngine.Canvas>().FromComponentInHierarchy().AsSingle();
        }
    }
}


