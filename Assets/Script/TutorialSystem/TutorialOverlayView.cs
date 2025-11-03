using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.TutorialSystem
{
    /// <summary>
    /// 튜토리얼 오버레이(디밍 + 말풍선 + 다음 버튼)를 제어하는 뷰입니다.
    /// </summary>
    public class TutorialOverlayView : MonoBehaviour
    {
        [Header("오버레이 구성")]

        [Tooltip("다음 버튼")]
        [SerializeField] private Button nextButton;

        [Header("옵션")]
        [Tooltip("표시/숨김 트윈 시간")]
        [SerializeField] private float tweenTime = 0.2f;

        private CanvasGroup _canvasGroup;
        private RectTransform _rootRect;

        // 하이라이트(사중 레이어) 제거 모드: 단순 페이지 시스템만 사용

        [Header("페이지 구성")]
        [Tooltip("페이지들을 담는 루트 (자식 각각이 한 페이지). pages 리스트가 비어있을 때만 사용")]
        [SerializeField] private RectTransform pagesRoot;

        [Tooltip("명시적 페이지 순서(1:1 매핑). 이 리스트가 비어있지 않으면 이 순서를 사용합니다")]
        [SerializeField] private System.Collections.Generic.List<RectTransform> pages = new System.Collections.Generic.List<RectTransform>();

        [Tooltip("초기 페이지 인덱스 (0부터)")]
        [SerializeField] private int initialPageIndex = 0;

        /// <summary>
        /// 외부에서 완료를 구독할 수 있는 이벤트
        /// </summary>
        public event System.Action Completed;
        /// <summary>
        /// 페이지 변경 시 통지 (현재 페이지 RectTransform, 인덱스)
        /// </summary>
        public event System.Action<RectTransform, int> PageChanged;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            _rootRect = transform as RectTransform;

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(NextPage);
            }

            // 초기 비활성화 상태
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            // 단순 페이지 모드에서는 추가 초기화 불필요
        }

        /// <summary>
        /// 메시지를 설정하고 오버레이를 표시합니다.
        /// </summary>
        public void Show(string richMessage)
        {
            // 방어 코드: Awake 이전 호출 대비 초기화 보장
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                if (nextButton != null)
                {
                    // 중복 등록 방지: 일단 모두 제거 후 다시 등록
                    nextButton.onClick.RemoveListener(NextPage);
                    nextButton.onClick.AddListener(NextPage);
                }
            }

            _canvasGroup.DOFade(1f, tweenTime);
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            // 페이지 모드일 경우 첫 페이지로 초기화
            if (pagesRoot != null)
            {
                SetActivePage(initialPageIndex);
            }
        }

        /// <summary>
        /// 오버레이를 숨깁니다.
        /// </summary>
        public void Hide()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            _canvasGroup.DOFade(0f, tweenTime);
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            // 단순 페이지 모드: 추가 정리 없음
        }

        // 말풍선 기능 제거(단순 페이지 모드)

        // 하이라이트 관련 API 제거 - 단순 페이지 시스템 요구 사항에 맞춤

        // ===== 페이지 모드 =====
        private int _currentPage = -1;

        /// <summary>
        /// 첫 페이지부터 시작합니다.
        /// </summary>
        public void ShowFirstPage()
        {
            Show(null);
            int count = GetPageCount();
            if (count <= 0) return;
            int clamped = Mathf.Clamp(initialPageIndex, 0, count - 1);
            SetActivePage(clamped);
        }

        /// <summary>
        /// 다음 페이지로 넘어갑니다. 마지막이면 완료 이벤트를 발행하고 닫습니다.
        /// </summary>
        public void NextPage()
        {
            if (pagesRoot == null)
            {
                // 단일 메시지 모드: 닫기 동작으로 사용
                Hide();
                Completed?.Invoke();
                return;
            }

            int pageCount = pagesRoot.childCount;
            if (pageCount <= 0)
            {
                Hide();
                Completed?.Invoke();
                return;
            }

            int next = (_currentPage < 0) ? initialPageIndex : _currentPage + 1;
            if (next >= pageCount)
            {
                Hide();
                Completed?.Invoke();
                return;
            }

            SetActivePage(next);
        }

        private void SetActivePage(int index)
        {
            int pageCount = GetPageCount();
            if (pageCount <= 0) return;
            for (int i = 0; i < pageCount; i++)
            {
                var rt = GetPageAt(i);
                if (rt == null) continue;
                var child = rt.gameObject;
                if (child != null) child.SetActive(i == index);
            }
            _currentPage = index;
            RectTransform current = null;
            if (_currentPage >= 0 && _currentPage < pageCount)
            {
                current = GetPageAt(_currentPage);
            }
            PageChanged?.Invoke(current, _currentPage);
        }

        /// <summary>
        /// 지정한 페이지를 즉시 표시합니다(오버레이가 숨김 상태라면 보여줍니다).
        /// </summary>
        public void ShowPage(int index)
        {
            int pageCount = GetPageCount();
            if (pageCount <= 0) return;
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            if (_canvasGroup != null && _canvasGroup.alpha < 1f)
            {
                Show(null);
            }
            if (index < 0) index = 0;
            if (index >= pageCount) index = pageCount - 1;
            if (index == _currentPage) { PageChanged?.Invoke(GetPageAt(index), index); return; }
            SetActivePage(index);
        }

        /// <summary>
        /// 해당 페이지(RectTransform 참조)를 표시합니다.
        /// </summary>
        public void ShowPage(RectTransform page)
        {
            if (page == null) return;
            int pageCount = GetPageCount();
            for (int i = 0; i < pageCount; i++)
            {
                if (GetPageAt(i) == page)
                {
                    ShowPage(i);
                    return;
                }
            }
        }

        private int GetPageCount()
        {
            if (pages != null && pages.Count > 0) return pages.Count;
            if (pagesRoot != null) return pagesRoot.childCount;
            return 0;
        }

        private RectTransform GetPageAt(int index)
        {
            if (pages != null && pages.Count > 0)
            {
                if (index >= 0 && index < pages.Count) return pages[index];
                return null;
            }
            if (pagesRoot != null)
            {
                if (index >= 0 && index < pagesRoot.childCount) return pagesRoot.GetChild(index) as RectTransform;
            }
            return null;
        }
    }
}


