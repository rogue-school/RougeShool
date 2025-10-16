using System.Collections.Generic;
using System.Linq;
using Game.ItemSystem.Data;
using Game.ItemSystem.Effect;
using Game.ItemSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Utility
{
    /// <summary>
    /// 효과 설정 헬퍼 클래스
    /// 효과 설정 관련 중복 로직을 통합하여 코드 중복을 제거합니다.
    /// </summary>
    public static class EffectConfigHelper
    {
        /// <summary>
        /// 유효한 효과 설정들을 반환합니다.
        /// </summary>
        /// <param name="config">효과 구성 설정</param>
        /// <returns>유효한 효과 설정들의 열거</returns>
        public static IEnumerable<ItemEffectConfig> GetValidEffects(ItemEffectConfiguration config)
        {
            if (config?.effects == null)
            {
                GameLogger.LogWarning("[EffectConfigHelper] 효과 구성이 null입니다", GameLogger.LogCategory.Core);
                return Enumerable.Empty<ItemEffectConfig>();
            }

            return config.effects.Where(e => e.effectSO != null);
        }

        /// <summary>
        /// 효과 설정들을 처리합니다.
        /// </summary>
        /// <param name="config">효과 구성 설정</param>
        /// <param name="processor">각 효과에 대해 실행할 처리 함수</param>
        public static void ProcessEffects(ItemEffectConfiguration config, System.Action<ItemEffectConfig> processor)
        {
            if (processor == null)
            {
                GameLogger.LogError("[EffectConfigHelper] 처리 함수가 null입니다", GameLogger.LogCategory.Core);
                return;
            }

            foreach (var effect in GetValidEffects(config))
            {
                try
                {
                    processor(effect);
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogError($"[EffectConfigHelper] 효과 처리 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Core);
                }
            }
        }

        /// <summary>
        /// 특정 타입의 효과를 찾습니다.
        /// </summary>
        /// <typeparam name="T">찾을 효과 타입</typeparam>
        /// <param name="config">효과 구성 설정</param>
        /// <returns>찾은 효과 또는 null</returns>
        public static T FindEffect<T>(ItemEffectConfiguration config) where T : ItemEffectSO
        {
            return GetValidEffects(config)
                .Select(e => e.effectSO)
                .OfType<T>()
                .FirstOrDefault();
        }

        /// <summary>
        /// 특정 타입의 모든 효과들을 찾습니다.
        /// </summary>
        /// <typeparam name="T">찾을 효과 타입</typeparam>
        /// <param name="config">효과 구성 설정</param>
        /// <returns>찾은 효과들의 열거</returns>
        public static IEnumerable<T> FindAllEffects<T>(ItemEffectConfiguration config) where T : ItemEffectSO
        {
            return GetValidEffects(config)
                .Select(e => e.effectSO)
                .OfType<T>();
        }

        /// <summary>
        /// 효과 설정의 실행 순서로 정렬합니다.
        /// </summary>
        /// <param name="effects">정렬할 효과 설정들</param>
        /// <returns>실행 순서로 정렬된 효과 설정들</returns>
        public static IEnumerable<ItemEffectConfig> SortByExecutionOrder(IEnumerable<ItemEffectConfig> effects)
        {
            return effects.OrderBy(e => e.executionOrder);
        }

        /// <summary>
        /// 효과 설정의 유효성을 검사합니다.
        /// </summary>
        /// <param name="config">검사할 효과 설정</param>
        /// <returns>유효성 여부</returns>
        public static bool ValidateEffectConfig(ItemEffectConfig config)
        {
            if (config == null)
            {
                GameLogger.LogError("[EffectConfigHelper] 효과 설정이 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            if (config.effectSO == null)
            {
                GameLogger.LogError("[EffectConfigHelper] 효과 SO가 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            if (config.useCustomSettings && config.customSettings == null)
            {
                GameLogger.LogWarning("[EffectConfigHelper] 커스텀 설정 사용이 활성화되었지만 설정이 null입니다", GameLogger.LogCategory.Core);
            }

            return true;
        }

        /// <summary>
        /// 효과 구성의 전체 유효성을 검사합니다.
        /// </summary>
        /// <param name="config">검사할 효과 구성</param>
        /// <returns>유효성 여부</returns>
        public static bool ValidateEffectConfiguration(ItemEffectConfiguration config)
        {
            if (config == null)
            {
                GameLogger.LogError("[EffectConfigHelper] 효과 구성이 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            if (config.effects == null || config.effects.Count == 0)
            {
                GameLogger.LogWarning("[EffectConfigHelper] 효과가 설정되지 않았습니다", GameLogger.LogCategory.Core);
                return false;
            }

            bool allValid = true;
            foreach (var effect in config.effects)
            {
                if (!ValidateEffectConfig(effect))
                {
                    allValid = false;
                }
            }

            return allValid;
        }

        /// <summary>
        /// 효과 설정에서 커스텀 파워를 가져옵니다.
        /// </summary>
        /// <param name="config">효과 설정</param>
        /// <returns>커스텀 파워 값</returns>
        public static int GetCustomPower(ItemEffectConfig config)
        {
            if (!config.useCustomSettings || config.customSettings == null)
                return 0;

            return GetCustomPowerFromSettings(config.customSettings, config.effectSO);
        }

        /// <summary>
        /// 커스텀 설정에서 파워를 가져옵니다.
        /// </summary>
        /// <param name="settings">커스텀 설정</param>
        /// <param name="effectSO">효과 SO</param>
        /// <returns>파워 값</returns>
        private static int GetCustomPowerFromSettings(ItemEffectCustomSettings settings, ItemEffectSO effectSO)
        {
            if (effectSO is HealEffectSO && settings is HealEffectCustomSettings healSettings)
                return healSettings.healAmount;
            
            if (effectSO is AttackBuffEffectSO && settings is AttackBuffEffectCustomSettings buffSettings)
                return buffSettings.buffAmount;
            
            if (effectSO is RerollEffectSO && settings is RerollEffectCustomSettings rerollSettings)
                return rerollSettings.rerollCount;
            
            if (effectSO is ShieldBreakerEffectSO && settings is ShieldBreakerEffectCustomSettings shieldSettings)
                return shieldSettings.duration;
            
            return 0;
        }
    }
}
