using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Interface;

namespace Game.UI
{
    /// <summary>
    /// 카드의 시각적 정보를 UI에 표시합니다.
    /// </summary>
    public class SkillCardUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image artworkImage;

        /// <summary>
        /// 카드 정보를 UI에 설정합니다.
        /// </summary>
        /// <param name="card">설정할 카드 데이터</param>
        public void SetCard(ISkillCard card)
        {
            if (card == null)
            {
                nameText.text = "";
                descriptionText.text = "";
                artworkImage.sprite = null;
                artworkImage.enabled = false;
                return;
            }

            nameText.text = card.GetCardName();
            descriptionText.text = card.GetDescription();
            artworkImage.sprite = card.GetArtwork();
            artworkImage.enabled = true;
        }

        /// <summary>
        /// 카드 UI를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            SetCard(null); // UI 요소 초기화
        }
    }
}
