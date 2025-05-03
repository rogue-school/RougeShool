using UnityEngine;
using Game.Interface;
using Game.Cards;
using Game.Managers;

namespace Game.Enemy
{
    /// <summary>
    /// 적의 핸드에서 전투 슬롯으로 카드를 등록하고,
    /// 전투 종료 후 핸드를 갱신하는 매니저입니다.
    /// </summary>
    public class EnemyCombatCardManager : MonoBehaviour
    {
        [SerializeField] private EnemyHandManager handManager;
        [SerializeField] private ICardSlot enemyCombatSlot;

        public void PrepareEnemyTurn()
        {
            ISkillCard card = handManager.GetCardForCombat();
            if (card != null)
            {
                enemyCombatSlot.SetCard(card);
            }
        }

        public void AdvanceEnemyHand()
        {
            handManager.AdvanceSlots();
        }
    }
}
