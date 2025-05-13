using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Core;
using Game.CombatSystem.Enemy;
using Game.CombatSystem.Slot;

namespace Game.SkillCardSystem.Executor
{
    /// <summary>
    /// 전투 슬롯에 배치된 카드를 실행하는 로직을 담당합니다.
    /// </summary>
    public static class CardExecutor
    {
        public static void Execute(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogWarning("[CardExecutor] 실행할 카드가 null입니다.");
                return;
            }

            ICharacter enemy = EnemyManager.Instance.GetCurrentEnemy();
            ICharacter player = PlayerManager.Instance.GetPlayer();

            // nullable → 명시적 변환 처리
            var nullableSlot = card.GetCombatSlot();
            if (!nullableSlot.HasValue)
            {
                Debug.LogError("[CardExecutor] 카드의 전투 슬롯 정보가 없습니다.");
                return;
            }

            CombatSlotPosition slotPosition = nullableSlot.Value;

            ICharacter owner = (slotPosition == CombatSlotPosition.FIRST) ? enemy : player;
            ICharacter target = (owner == enemy) ? player : enemy;

            // CharacterBase로 캐스팅
            if (owner is CharacterBase ownerChar && target is CharacterBase targetChar)
            {
                foreach (var effect in card.CreateEffects())
                {
                    int power = card.GetEffectPower(effect);
                    effect.ExecuteEffect(ownerChar, targetChar, power);
                }

                Debug.Log($"[CardExecutor] 카드 실행 완료: {card.GetCardName()} → 공격자: {ownerChar}, 대상: {targetChar}");
            }
            else
            {
                Debug.LogError("[CardExecutor] 캐릭터 타입이 CharacterBase로 캐스팅되지 않았습니다.");
            }
        }
    }
}
