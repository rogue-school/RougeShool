using UnityEngine;
using Game.Interface;
using Game.Cards;

namespace Game.Managers
{
    /// <summary>
    /// 적의 핸드 슬롯(3칸)을 관리하고, 전투 시 1번 슬롯의 카드를 꺼내는 매니저입니다.
    /// </summary>
    public class EnemyHandManager : MonoBehaviour
    {
        [SerializeField] private ISkillCard[] handSlots = new ISkillCard[3];
        [SerializeField] private EnemySkillDeck enemyDeck;

        public void InitializeHand()
        {
            for (int i = 0; i < handSlots.Length; i++)
                handSlots[i] = enemyDeck.GetRandomCard();
        }

        public void AdvanceSlots()
        {
            handSlots[0] = handSlots[1];
            handSlots[1] = handSlots[2];
            handSlots[2] = enemyDeck.GetRandomCard();
        }

        public ISkillCard GetCardForCombat()
        {
            return handSlots[0];
        }
    }
}
