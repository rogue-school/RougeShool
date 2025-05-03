using UnityEngine;
using Game.Cards;
using Game.Characters;
using Game.UI;

namespace Game.Managers
{
    /// <summary>
    /// 플레이어의 핸드 슬롯에 선택된 캐릭터의 스킬 카드를 배치합니다.
    /// </summary>
    public class PlayerHandManager : MonoBehaviour
    {
        [SerializeField] private PlayerCardSlotUI[] cardSlots;

        private PlayerCharacter player;

        private void Start()
        {
            // 선택된 플레이어 자동 찾기
            player = GameObject.FindWithTag("Player")?.GetComponent<PlayerCharacter>();

            if (player == null || player.characterData == null)
            {
                Debug.LogWarning("[PlayerHandManager] 플레이어 캐릭터 또는 데이터가 없습니다.");
                return;
            }

            var cards = player.characterData.skillCards;

            for (int i = 0; i < cardSlots.Length; i++)
            {
                if (i < cards.Count)
                    cardSlots[i].SetCard(cards[i]);
                else
                    cardSlots[i].ClearSlot();
            }
        }
    }
}
