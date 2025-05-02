using UnityEngine;
using Game.Cards;
using Game.UI;

namespace Game.Managers
{
    public class EnemyCardManager : MonoBehaviour
    {
        public CardUI enemyCardUIPrefab;
        public Transform[] enemyHandSlots;
        public PlayerCardData[] enemyCards;

        public void PrepareEnemyTurn()
        {
            ClearEnemyHand();

            for (int i = 0; i < enemyHandSlots.Length; i++)
            {
                if (i >= enemyCards.Length) break;

                var card = enemyCards[i];
                var cardUI = Instantiate(enemyCardUIPrefab, enemyHandSlots[i]);
                cardUI.SetCard(card);
            }

            Debug.Log("[EnemyCardManager] 적 핸드 슬롯에 카드 준비 완료");
        }

        private void ClearEnemyHand()
        {
            foreach (Transform slot in enemyHandSlots)
            {
                if (slot.childCount > 0)
                {
                    Destroy(slot.GetChild(0).gameObject);
                }
            }
        }
    }
}

