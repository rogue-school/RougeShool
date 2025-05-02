using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Cards
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector3 originalPosition;
        private Transform originalParent;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalPosition = rectTransform.position;
            originalParent = transform.parent;
            canvasGroup.blocksRaycasts = false;
            transform.SetParent(transform.root); // 드래그 중 최상위로
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.position = originalPosition;
            transform.SetParent(originalParent);
            canvasGroup.blocksRaycasts = true;
        }
    }
}
