using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Slot;
using Game.IManager;
using Game.CharacterSystem.Core;

namespace Game.SkillCardSystem.Executor
{
    /// <summary>
    /// 전투 슬롯에 배치된 스킬 카드를 실행하는 서비스 클래스입니다.
    /// </summary>
    public class CardExecutor : ICardExecutor
    {
        private readonly IPlayerManager playerManager;
        private readonly IEnemyManager enemyManager;

        public CardExecutor(IPlayerManager playerManager, IEnemyManager enemyManager)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
        }

        public void ExecuteCard(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogWarning("[CardExecutor] 실행할 카드가 null입니다.");
                return;
            }

            var slot = card.GetCombatSlot();
            if (!slot.HasValue)
            {
                Debug.LogError("[CardExecutor] 카드의 전투 슬롯 정보가 없습니다.");
                return;
            }

            // 시전자/대상자 추출
            ICharacter caster = (slot.Value == CombatSlotPosition.FIRST) ? enemyManager.GetEnemy() : playerManager.GetPlayer();
            ICharacter target = (caster == enemyManager.GetEnemy()) ? playerManager.GetPlayer() : enemyManager.GetEnemy();

            if (caster is not CharacterBase casterChar || target is not CharacterBase targetChar)
            {
                Debug.LogError("[CardExecutor] 캐릭터가 CharacterBase 타입이 아닙니다.");
                return;
            }

            foreach (var effect in card.CreateEffects())
            {
                int power = card.GetEffectPower(effect);
                effect.ExecuteEffect(casterChar, targetChar, power);

                Debug.Log($"[CardExecutor] 실행됨: {card.CardData.Name} → {effect.GetType().Name}, power: {power}");
            }
        }
    }
}
