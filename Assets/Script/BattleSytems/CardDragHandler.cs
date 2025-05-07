using UnityEngine;
using UnityEngine.EventSystems;
using Game.Interface;

namespace Game.Utility
{
    /// <summary>
    /// 카드 드래그 동작을 처리합니다.
    /// 카드 UI에 부착되어 드래그를 지원합니다.
    /// </summary>
    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Vector3 originalPosition;
        private Transform originalParent;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalPosition = rectTransform.position;
            originalParent = transform.parent;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            rectTransform.position = originalPosition;
            transform.SetParent(originalParent);
            canvasGroup.blocksRaycasts = true;
        }
    }
}
