using UnityEngine;
using Game.Managers;
using Game.Characters;
using Game.Interface;
using Game.Battle;

namespace Game.Cards
{
    public class BlockEffect : ICardEffect
    {
        private int dummyValue;

        public BlockEffect(int value)
        {
            dummyValue = value;
        }

        public void ExecuteEffect(CharacterBase caster, CharacterBase target)
        {
            if (caster.CompareTag("Player"))
            {
                BattleTurnManager.Instance.ActivatePlayerBlock();
                Debug.Log("[BlockEffect] 플레이어가 방어 태세를 취했습니다. 다음 적의 공격은 무효화됩니다.");
            }
            else if (caster.CompareTag("Enemy"))
            {
                BattleTurnManager.Instance.ActivateEnemyBlock();
                Debug.Log("[BlockEffect] 적이 방어 태세를 취했습니다. 다음 플레이어의 공격은 무효화됩니다.");
            }
        }
    }
}
