using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using UnityEngine;
using Game.VFXSystem.Manager;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 출혈 효과 커맨드 생성 전략입니다.
    /// </summary>
    public class BleedEffectStrategy : IEffectCommandStrategy
    {
        public bool CanHandle(SkillCardEffectSO effectSO)
        {
            return effectSO is BleedEffectSO;
        }

        public ICardEffectCommand CreateCommand(EffectConfiguration config)
        {
            if (config.effectSO is not BleedEffectSO bleedEffectSO)
                return null;

            // VFXManager 찾기 (DI 없이 직접 찾기)
            var vfxManager = Game.VFXSystem.Manager.VFXManager.Instance;
            
            // visualEffectPrefab과 perTurnEffectPrefab 가져오기 (리플렉션 사용)
            var visualEffectPrefab = GetVisualEffectPrefab(bleedEffectSO);
            var perTurnEffectPrefab = GetPerTurnEffectPrefab(bleedEffectSO);

            if (config.useCustomSettings && config.customSettings != null)
            {
                // EffectCustomSettings에서 이펙트/아이콘 가져오기
                var activateEffectPrefab = config.customSettings.bleedActivateEffectPrefab ?? visualEffectPrefab;
                var perTurnEffect = config.customSettings.bleedPerTurnEffectPrefab ?? perTurnEffectPrefab ?? activateEffectPrefab;
                var icon = config.customSettings.bleedIcon ?? bleedEffectSO.GetIcon();
                
                // BleedEffectCommand 생성 (턴당 사운드는 BleedEffectCommand에서 context로부터 가져옴)
                return new BleedEffectCommand(
                    config.customSettings.bleedAmount,
                    config.customSettings.bleedDuration,
                    icon,
                    activateEffectPrefab,
                    perTurnEffect,
                    vfxManager
                );
            }

            return null;
        }

        /// <summary>
        /// BleedEffectSO에서 visualEffectPrefab을 가져옵니다 (리플렉션 사용).
        /// </summary>
        private static GameObject GetVisualEffectPrefab(BleedEffectSO bleedEffectSO)
        {
            var field = typeof(BleedEffectSO).GetField("visualEffectPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(bleedEffectSO) as GameObject;
        }

        /// <summary>
        /// BleedEffectSO에서 perTurnEffectPrefab을 가져옵니다 (리플렉션 사용).
        /// </summary>
        private static GameObject GetPerTurnEffectPrefab(BleedEffectSO bleedEffectSO)
        {
            var field = typeof(BleedEffectSO).GetField("perTurnEffectPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(bleedEffectSO) as GameObject;
        }
    }
}
