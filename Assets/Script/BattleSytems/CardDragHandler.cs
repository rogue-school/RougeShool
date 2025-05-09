using UnityEngine.EventSystems;
using UnityEngine;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Vector3 OriginalPosition { get; private set; }
    public Transform OriginalParent { get; private set; }
    public bool droppedSuccessfully = false;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OriginalPosition = transform.localPosition;
        OriginalParent = transform.parent;
        droppedSuccessfully = false;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(GetComponent<Canvas>());

        if (!droppedSuccessfully)
        {
            transform.SetParent(OriginalParent);
            transform.localPosition = OriginalPosition;
            transform.localScale = Vector3.one;
        }
    }
}
