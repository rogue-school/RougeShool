using UnityEngine;
using UnityEngine.UI;
using Game.Interface;
using Game.Cards;

namespace Game.UI
{
    /// <summary>
    /// 카드의 이름, 설명, 이미지를 표시하는 UI 클래스입니다.
    /// </summary>
    public class SkillCardUI : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Image artworkImage;

        public void SetCard(ISkillCard card)
        {
            if (card == null)
            {
                nameText.text = "";
                descriptionText.text = "";
                artworkImage.sprite = null;
                artworkImage.enabled = false;
            }
            else
            {
                nameText.text = card.GetName();
                descriptionText.text = card.GetDescription();
                artworkImage.sprite = card.GetArtwork();
                artworkImage.enabled = true;
            }
        }
    }
}
