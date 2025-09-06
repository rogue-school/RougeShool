namespace Game.UtilitySystem.GameFlow
{
    /// <summary>
    /// 씬 로딩을 위한 인터페이스
    /// </summary>
    public interface ISceneLoader
    {
        /// <summary>
        /// 씬을 로드합니다.
        /// </summary>
        /// <param name="sceneName">로드할 씬 이름</param>
        void LoadScene(string sceneName);
    }
}
