using Game.SkillCardSystem.Data;

namespace Game.SkillCardSystem.Utility
{
    /// <summary>
    /// SkillCardConfiguration에 대한 Extension 메서드
    /// 리소스 검증 로직을 통합하여 중복 코드를 제거합니다.
    /// </summary>
    public static class SkillCardConfigExtensions
    {
        /// <summary>
        /// 리소스 비용이 있는지 확인합니다
        /// </summary>
        /// <param name="config">카드 구성 설정</param>
        /// <returns>리소스 비용이 있으면 true</returns>
        public static bool HasResourceCost(this CardConfiguration config)
        {
            return config != null 
                && config.hasResource 
                && config.resourceConfig != null 
                && config.resourceConfig.cost > 0;
        }
    }
}

