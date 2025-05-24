using Game.Utility.GameFlow;

namespace Game.Utility.Extensions
{
    /// <summary>
    /// ISceneLoader 확장 메서드 모음
    /// </summary>
    public static class SceneLoaderExtensions
    {
        /// <summary>
        /// 전투 씬으로 전환합니다.
        /// </summary>
        public static void LoadCombatScene(this ISceneLoader loader)
        {
            loader.LoadScene("Combat"); // 실제 Combat 씬 이름에 맞게 조정 필요
        }

        /// <summary>
        /// 메인 메뉴 씬으로 전환합니다 (예시)
        /// </summary>
        public static void LoadMainMenuScene(this ISceneLoader loader)
        {
            loader.LoadScene("MainMenu");
        }
    }
}
