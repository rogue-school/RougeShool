using UnityEngine;
using Game.Interface;
using Game.Battle;
using Game.Characters;

namespace Game.Effect
{
    /// <summary>
    /// 이 카드가 선공 슬롯에 있을 경우, 상대방의 후공 공격을 무효화합니다.
    /// 플레이어 또는 적 모두 사용할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "GuardEffect", menuName = "CardEffects/GuardEffect")]
    public class GuardEffect : ScriptableObject, ICardEffect
    {
        [SerializeField] private bool isPlayer; // 이 효과가 플레이어인지 적에게 적용되는지 여부

        /// <summary>
        /// 카드 실행 시 호출됩니다.
        /// 이펙트는 해당 턴 동안 상대의 후공 공격을 막습니다.
        /// </summary>
        public void ExecuteEffect(CharacterBase caster, CharacterBase target)
        {
            if (isPlayer)
            {
                BattleTurnManager.Instance.ActivatePlayerGuard();
                Debug.Log("[GuardEffect] 플레이어의 가드 활성화");
            }
            else
            {
                BattleTurnManager.Instance.ActivateEnemyGuard();
                Debug.Log("[GuardEffect] 적의 가드 활성화");
            }
        }
    }
}

