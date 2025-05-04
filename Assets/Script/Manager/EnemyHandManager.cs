using UnityEngine;
using Game.Cards;
using Game.Interface;
using Game.UI;
using Game.Enemy;
using Game.Utility;
using System;

namespace Game.Managers
{
    /// <summary>
    /// 적의 핸드 카드와 슬롯을 관리합니다.
    /// 팩토리를 통해 실제 전투 카드 인스턴스를 생성합니다.
    /// </summary>
    public class EnemyHandManager : MonoBehaviour
    {
        private EnemySkillDeck enemyDeck;
        private EnemyCardSlotUI[] handSlots;
        private SkillCardUI cardUIPrefab;

        private void Awake()
        {
            // 덱 자동 로드 (Resources/EnemyDecks/DefaultEnemyDeck.asset)
            enemyDeck = Resources.Load<EnemySkillDeck>("EnemyDecks/DefaultEnemyDeck");

            // 카드 프리팹 자동 로드 (Resources/UI/SkillCardUI.prefab)
            cardUIPrefab = Resources.Load<SkillCardUI>("UI/SkillCardUI");

            handSlots = FindObjectsOfType<EnemyCardSlotUI>();
            Array.Sort(handSlots, (a, b) => a.name.CompareTo(b.name));
        }

        private void Start()
        {
            GenerateInitialHand();
        }

        public void GenerateInitialHand()
        {
            if (enemyDeck == null || enemyDeck.cards.Count == 0) return;

            for (int i = 0; i < handSlots.Length; i++)
            {
                var so = enemyDeck.GetRandomCard();

                // 임시 수치 지정 (향후 외부 수치 테이블 연동 가능)
                int damage = UnityEngine.Random.Range(5, 15);

                var runtimeCard = SkillCardFactory.CreateEnemyCard(so, damage);

                var cardUI = Instantiate(cardUIPrefab, handSlots[i].transform);
                cardUI.SetCard(runtimeCard);

                handSlots[i].SetCard(runtimeCard);
            }
        }

        public ISkillCard GetSlotCard(int index)
        {
            if (index < 0 || index >= handSlots.Length) return null;
            return handSlots[index].GetCard();
        }

        public ISkillCard GetCardForCombat()
        {
            return GetSlotCard(0);
        }

        public void AdvanceSlots()
        {
            for (int i = 0; i < handSlots.Length - 1; i++)
            {
                var nextCard = handSlots[i + 1].GetCard();
                handSlots[i].SetCard(nextCard);
            }

            var so = enemyDeck.GetRandomCard();
            var runtimeCard = SkillCardFactory.CreateEnemyCard(so, UnityEngine.Random.Range(5, 15));
            handSlots[handSlots.Length - 1].SetCard(runtimeCard);
        }
    }
}
