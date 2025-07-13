using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Validator;
using Game.Audio;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;

namespace Game.SkillCardSystem.Executor
{
    /// <summary>
    /// 스킬 카드의 이펙트를 실제로 실행하는 클래스입니다.
    /// 유효성 검사 후 커맨드 패턴 기반으로 이펙트를 처리합니다.
    /// </summary>
    public class CardExecutor : ICardExecutor
    {
        private readonly ICardEffectCommandFactory commandFactory;
        private readonly ICardExecutionValidator validator;

        /// <summary>
        /// CardExecutor 생성자.
        /// </summary>
        /// <param name="commandFactory">카드 이펙트 커맨드 생성 팩토리</param>
        /// <param name="validator">카드 실행 유효성 검사기</param>
        public CardExecutor(
            ICardEffectCommandFactory commandFactory,
            ICardExecutionValidator validator)
        {
            this.commandFactory = commandFactory;
            this.validator = validator;
        }

        /// <summary>
        /// 주어진 카드와 컨텍스트를 기반으로 이펙트를 실행합니다.
        /// </summary>
        /// <param name="card">실행할 스킬 카드</param>
        /// <param name="context">실행 컨텍스트 (시전자/대상 등)</param>
        /// <param name="turnManager">전투 턴 매니저 (이펙트가 턴에 영향을 미칠 경우 사용)</param>
        public void Execute(ISkillCard card, ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (!validator.CanExecute(card, context))
            {
                Debug.LogWarning($"[CardExecutor] 실행 조건 불충족: {card?.GetCardName() ?? "알 수 없음"}");
                return;
            }

            var effects = card.CreateEffects();
            if (effects == null || effects.Count == 0)
            {
                Debug.LogWarning($"[CardExecutor] '{card.GetCardName()}'에 등록된 이펙트가 없습니다.");
                return;
            }

            foreach (var effect in effects)
            {
                if (effect == null) continue;

                int power = card.GetEffectPower(effect);
                var command = commandFactory.Create(effect, power);
                command?.Execute(context, turnManager);

                Debug.Log($"[CardExecutor] {card.GetCardName()} → {effect.GetEffectName()}, power: {power}");
            }

            // 효과 후 카드 사운드 재생
            var clip = card.CardData?.SfxClip;
            if (clip != null)
            {
                AudioManager.Instance?.PlaySFX(clip);
            }

            // 카드 비주얼 이펙트 재생
            var vfx = card.CardData?.VisualEffectPrefab;
            if (vfx != null)
            {
                ICharacter target = context.Target;
                if (target != null)
                {
                    Vector3 spawnPos = target.Transform.position + new Vector3(0, 0, 0);
                    var instance = GameObject.Instantiate(vfx, spawnPos, Quaternion.identity);
                    GameObject.Destroy(instance, card.CardData.EffectDuration);
                }
            }
        }
    }
}
