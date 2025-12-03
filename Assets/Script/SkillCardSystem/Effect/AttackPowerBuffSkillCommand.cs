using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Effect;
using Game.ItemSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 시전자에게 공격력 증가 버프를 부여하는 커맨드입니다.
    /// ItemSystem의 AttackPowerBuffEffect를 사용하여 데미지 계산과 연동됩니다.
    /// </summary>
    public class AttackPowerBuffSkillCommand : ICardEffectCommand
    {
        private readonly int _attackPowerBonus;
        private readonly int _duration;
        private readonly Sprite _icon;

        public AttackPowerBuffSkillCommand(int attackPowerBonus, int duration, Sprite icon = null)
        {
            _attackPowerBonus = attackPowerBonus;
            _duration = duration;
            _icon = icon;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source == null)
            {
                GameLogger.LogWarning("[AttackPowerBuffSkillCommand] 시전자가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (_attackPowerBonus <= 0)
            {
                GameLogger.LogWarning("[AttackPowerBuffSkillCommand] 공격력 증가 수치가 0 이하입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (_duration <= 0)
            {
                GameLogger.LogWarning("[AttackPowerBuffSkillCommand] 버프 지속 턴 수가 0 이하입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (context.Source is not ICharacter character)
            {
                GameLogger.LogWarning("[AttackPowerBuffSkillCommand] 시전자가 캐릭터가 아닙니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 아이템 전용 턴 정책을 그대로 재사용 (대상 자신의 턴에만 감소)
            var effect = new AttackPowerBuffEffect(
                _attackPowerBonus,
                _duration,
                ItemEffectTurnPolicy.TargetTurnOnly,
                _icon,
                sourceItemName: null);

            character.RegisterPerTurnEffect(effect);

            GameLogger.LogInfo(
                $"[AttackPowerBuffSkillCommand] '{character.GetCharacterName()}'에게 공격력 버프 적용: +{_attackPowerBonus}, {_duration}턴 지속",
                GameLogger.LogCategory.SkillCard);
        }
    }
}


