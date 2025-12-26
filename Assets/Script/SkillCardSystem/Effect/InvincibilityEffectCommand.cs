using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CoreSystem.Utility;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 무적 버프를 적용하는 커맨드. 소스 캐릭터에게 InvincibilityBuff를 적용합니다.
    /// </summary>
    public class InvincibilityEffectCommand : ICardEffectCommand
    {
        private readonly int duration;
        private readonly Sprite icon;
        private readonly GameObject activateEffectPrefab;
        private readonly AudioClip activateSfxClip;

        public InvincibilityEffectCommand(int duration = 2, Sprite icon = null, GameObject activateEffectPrefab = null, AudioClip activateSfxClip = null)
        {
            this.duration = duration;
            this.icon = icon;
            this.activateEffectPrefab = activateEffectPrefab;
            this.activateSfxClip = activateSfxClip;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source is not ICharacter character)
            {
                GameLogger.LogWarning("[InvincibilityEffectCommand] 소스가 캐릭터가 아니거나 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // SkillCardDefinition의 커스텀 설정이 있으면 우선 사용
            int finalDuration = duration;
            Sprite finalIcon = icon;
            GameObject finalEffectPrefab = activateEffectPrefab;
            AudioClip finalSfxClip = activateSfxClip;
            
            if (context.Card?.CardDefinition != null)
            {
                var cfg = context.Card.CardDefinition.configuration;
                if (cfg != null && cfg.hasEffects)
                {
                    // 해당 카드의 EffectConfiguration 중 InvincibilityEffectSO 항목의 커스텀 설정을 확인
                    foreach (var eff in cfg.effects)
                    {
                        if (eff?.effectSO is InvincibilityEffectSO && eff.useCustomSettings)
                        {
                            // 커스텀 설정에서 duration 읽기
                            finalDuration = eff.customSettings.invincibilityDuration;
                            GameLogger.LogInfo($"[InvincibilityEffectCommand] 커스텀 설정 사용: duration={finalDuration}", GameLogger.LogCategory.SkillCard);
                            break;
                        }
                    }
                }
            }

            var buff = new InvincibilityBuff(finalDuration, finalIcon, finalEffectPrefab);
            character.RegisterPerTurnEffect(buff);
            
            // 즉시 무적 상태 활성화
            if (character is CharacterBase characterBase)
            {
                characterBase.SetInvincible(true);
            }
            
            GameLogger.LogInfo($"[InvincibilityEffectCommand] {character.GetCharacterName()}에게 무적 버프 적용 ({finalDuration}턴)", GameLogger.LogCategory.SkillCard);
        }
    }
}

