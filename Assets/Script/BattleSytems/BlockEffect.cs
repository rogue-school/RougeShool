using UnityEngine;
using Game.Effects;
using Game.Units;

namespace Game.Cards
{
    /// <summary>
    /// 방어 카드 효과 - 현재는 특수 효과 없음 (디버프 제거, 리셋 등으로 확장 가능)
    /// </summary>
    public class BlockEffect : ICardEffect
    {
        private int dummyValue;

        public BlockEffect(int value)
        {
            dummyValue = value;
        }

        public void ExecuteEffect(Unit caster, Unit target)
        {
            // 추후 원하는 특수 방어 효과로 확장 가능
            Debug.Log($"[BlockEffect] {caster.name}이 방어 태세를 취했지만, 현재는 효과 없음.");
        }
    }
}
