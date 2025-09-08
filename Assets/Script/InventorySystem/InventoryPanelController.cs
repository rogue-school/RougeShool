using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Game.CoreSystem.Audio;

public class InventoryPanelController : MonoBehaviour
{
	public RectTransform panel;      // 인벤토리 패널
	public Button bagButton;         // 가방 버튼
	public Button closeButton;       // 닫기 버튼
	public float slideDuration = 0.3f;

	private Vector2 shownPosition;
	private Vector2 hiddenPosition;
	private bool isOpen = false;

	void Start()
	{
		float panelWidth = panel.rect.width;

		// 표시될 위치 계산
		shownPosition = panel.anchoredPosition;

		// 숨김 위치 (오른쪽 바깥)
		hiddenPosition = new Vector2(shownPosition.x + panelWidth + 50f, shownPosition.y);

		// 시작 시 비활성화
		panel.gameObject.SetActive(false);

		bagButton.onClick.AddListener(OpenPanel);
		closeButton.onClick.AddListener(ClosePanel);
	}

	public void OpenPanel()
	{
		if (isOpen) return;
		isOpen = true;

		// 활성화 및 위치 초기화
		panel.gameObject.SetActive(true);
		panel.anchoredPosition = hiddenPosition;

		StopAllCoroutines();
		StartCoroutine(SlidePanel(hiddenPosition, shownPosition));

		// 오디오: 인벤토리 열기 사운드(기본 버튼/메뉴 오픈으로 대체)
		AudioManager.Instance?.PlayMenuOpenSound();
	}

	public void ClosePanel()
	{
		if (!isOpen) return;
		isOpen = false;

		StopAllCoroutines();
		StartCoroutine(SlidePanel(shownPosition, hiddenPosition, () =>
		{
			panel.gameObject.SetActive(false);
		}));

		// 오디오: 인벤토리 닫기 사운드(기본 버튼/메뉴 클로즈로 대체)
		AudioManager.Instance?.PlayMenuCloseSound();
	}

	private IEnumerator SlidePanel(Vector2 from, Vector2 to, System.Action onComplete = null)
	{
		float elapsed = 0f;
		while (elapsed < slideDuration)
		{
			panel.anchoredPosition = Vector2.Lerp(from, to, elapsed / slideDuration);
			elapsed += Time.deltaTime;
			yield return null;
		}
		panel.anchoredPosition = to;
		onComplete?.Invoke();
	}
}