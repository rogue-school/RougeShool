using UnityEngine;
using Game.Cards;
using UnityEngine;
using Game.Interface;
using Game.UI;

namespace Game.Managers
{
    /// <summary>
    /// 플레이어의 핸드 슬롯에 초기 카드를 설정하는 클래스입니다.
    /// </summary>
    public class PlayerHandManager : MonoBehaviour
    {
        [SerializeField] private SkillCardUI[] handSlots;

        public void InitializeHand(ISkillCard[] initialCards)
        {
            for (int i = 0; i < handSlots.Length; i++)
            {
                if (i < initialCards.Length)
                    handSlots[i].SetCard(initialCards[i]);
                else
                    handSlots[i].SetCard(null);
            }
        }
    }
}
