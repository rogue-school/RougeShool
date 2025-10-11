using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Game.ItemSystem.Effect;

namespace Game.ItemSystem.Runtime
{
    /// <summary>
    /// 아이템 효과 커맨드 팩토리입니다.
    /// </summary>
    public class ItemEffectCommandFactory
    {
        /// <summary>
        /// 효과 설정으로부터 커맨드를 생성합니다.
        /// </summary>
        /// <param name="effectConfig">효과 설정</param>
        /// <returns>효과 커맨드</returns>
        public IItemEffectCommand CreateCommand(ItemEffectConfig effectConfig)
        {
            if (effectConfig?.effectSO == null)
                return null;

            // ClownPotionEffectSO는 특별한 처리가 필요합니다
            if (effectConfig.effectSO is ClownPotionEffectSO clownEffectSO)
            {
                if (effectConfig.useCustomSettings && effectConfig.customSettings is ClownPotionEffectCustomSettings clownSettings)
                {
                    return clownEffectSO.CreateEffectCommand(clownSettings);
                }
                else
                {
                    return clownEffectSO.CreateEffectCommand(0);
                }
            }

            // TimeStopEffectSO도 특별한 처리가 필요합니다
            if (effectConfig.effectSO is TimeStopEffectSO timeStopEffectSO)
            {
                if (effectConfig.useCustomSettings && effectConfig.customSettings is TimeStopEffectCustomSettings timeStopSettings)
                {
                    return timeStopEffectSO.CreateEffectCommand(timeStopSettings);
                }
                else
                {
                    return timeStopEffectSO.CreateEffectCommand(0);
                }
            }

            // DiceOfFateEffectSO도 특별한 처리가 필요합니다
            if (effectConfig.effectSO is DiceOfFateEffectSO diceOfFateEffectSO)
            {
                if (effectConfig.useCustomSettings && effectConfig.customSettings is DiceOfFateEffectCustomSettings diceSettings)
                {
                    return diceOfFateEffectSO.CreateEffectCommand(diceSettings);
                }
                else
                {
                    return diceOfFateEffectSO.CreateEffectCommand(0);
                }
            }

            int power = 0;
            if (effectConfig.useCustomSettings)
            {
                power = GetCustomPower(effectConfig.customSettings, effectConfig.effectSO);
            }

            return effectConfig.effectSO.CreateEffectCommand(power);
        }

        /// <summary>
        /// 커스텀 설정에서 파워를 가져옵니다.
        /// </summary>
        /// <param name="settings">커스텀 설정</param>
        /// <param name="effectSO">효과 SO</param>
        /// <returns>파워 값</returns>
        private int GetCustomPower(ItemEffectCustomSettings settings, ItemEffectSO effectSO)
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
