using UnityEngine;
using Game.Interface;
using Game.Characters;
using Game.Managers;
using Game.Effect;

namespace Game.Effects
{
    /// <summary>
    /// 플레이어가 방어 상태에 진입하게 만드는 카드 이펙트입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "CardEffect/Guard")]
    public class GuardEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int value)
        {
            // 플레이어 방어 상태 등록
            BattleTurnManager.Instance?.RegisterPlayerGuard();
            Debug.Log("[GuardEffectSO] 플레이어가 방어 상태에 진입했습니다.");
        }
    }
}
