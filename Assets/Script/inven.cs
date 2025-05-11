using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventoryAnimator : MonoBehaviour
{
    public RectTransform inventoryPanel;
    public Button bagButton;
    public Button closeButton;

    public float animationDuration = 0.3f;
    public Vector2 closedPosition = new Vector2(300f, 0f); // 오른쪽 밖
    public Vector2 openPosition = new Vector2(0f, 0f);      // 오른쪽에 붙게

    private bool isOpen = false;
    private Coroutine animationCoroutine;

    private void Start()
    {
        // Null 체크 추가 (디버깅에 도움)
        if (inventoryPanel == null || closeButton == null || bagButton == null)
        {
            Debug.LogError("InventoryAnimator: UI references are not assigned.");
            return;
        }

        inventoryPanel.gameObject.SetActive(false); // 처음엔 안 보이게
        closeButton.onClick.AddListener(CloseInventory);
        bagButton.onClick.AddListener(OpenInventory);
    }

    private void OpenInventory()
    {
        if (isOpen) return;
        isOpen = true;

        inventoryPanel.gameObject.SetActive(true); // 열기 전에 활성화
        inventoryPanel.anchoredPosition = closedPosition;

        AnimatePanel(openPosition);
    }

    private void CloseInventory()
    {
        if (!isOpen) return;
        isOpen = false;

        AnimatePanel(closedPosition, () => {
            inventoryPanel.gameObject.SetActive(false); // 닫고 나서 비활성화
        });
    }

    private void AnimatePanel(Vector2 targetPosition, System.Action onComplete = null)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(SmoothMove(targetPosition, onComplete));
    }

    private IEnumerator SmoothMove(Vector2 targetPosition, System.Action onComplete)
    {
        Vector2 startPos = inventoryPanel.anchoredPosition;
        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;
            inventoryPanel.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        inventoryPanel.anchoredPosition = targetPosition;
        onComplete?.Invoke();
    }
}
