using UnityEngine;
using Game.Interface;
using Game.Battle;
using Game.Characters;

namespace Game.Effect
{
    /// <summary>
    /// 선공 슬롯에 있을 경우, 상대 후공 카드의 효과를 무효화하는 가드 이펙트입니다.
    /// 플레이어/적 공용으로 사용 가능합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Assets/Effects/Guard Effect")]
    public class GuardEffect : ScriptableObject, ICardEffect
    {
        [SerializeField] private bool isPlayer; // 플레이어용인지 적용인지 설정

        public void ExecuteEffect(CharacterBase caster, CharacterBase target)
        {
            if (isPlayer)
            {
                BattleTurnManager.Instance.ActivatePlayerGuard();
                Debug.Log("[GuardEffect] 플레이어의 방어가 활성화됨");
            }
            else
            {
                BattleTurnManager.Instance.ActivateEnemyGuard();
                Debug.Log("[GuardEffect] 적의 방어가 활성화됨");
            }
        }
    }
}