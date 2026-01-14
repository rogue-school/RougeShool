using UnityEngine;
using System.Collections.Generic;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.UI;

namespace Game.VFXSystem.Pool
{
    /// <summary>
    /// 스킬 카드 UI 오브젝트 풀
    /// 카드 생성/제거 시 GC 압박을 줄이기 위한 재사용 시스템
    /// </summary>
    public class SkillCardUIPool : MonoBehaviour
    {
        #region 설정

        [Header("풀 설정")]
        [Tooltip("스킬 카드 UI 프리팹")]
        [SerializeField] private GameObject cardUIPrefab;

        [Tooltip("초기 풀 크기")]
        [SerializeField] private int initialPoolSize = 20;

        [Tooltip("최대 풀 크기")]
        [SerializeField] private int maxPoolSize = 100;

        #endregion

        #region 내부 상태

        private Queue<SkillCardUI> availableCards = new Queue<SkillCardUI>();
        private HashSet<SkillCardUI> activeCards = new HashSet<SkillCardUI>();
        private Transform poolContainer;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            InitializePool();
        }

        #endregion

        #region 초기화

        private void InitializePool()
        {
            // 풀 컨테이너 생성
            poolContainer = new GameObject("SkillCardUIPool_Container").transform;
            poolContainer.SetParent(transform);
            poolContainer.gameObject.SetActive(false); // 비활성 컨테이너

            // 초기 오브젝트 생성
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewCard();
            }

            GameLogger.LogInfo($"스킬 카드 UI 풀 초기화 완료 - 크기: {initialPoolSize}", GameLogger.LogCategory.SkillCard);
        }

        private SkillCardUI CreateNewCard()
        {
            if (cardUIPrefab == null)
            {
                GameLogger.LogError("카드 UI 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.Error);
                return null;
            }

            GameObject cardObj = Instantiate(cardUIPrefab, poolContainer);
            cardObj.SetActive(false);

            SkillCardUI cardUI = cardObj.GetComponent<SkillCardUI>();
            if (cardUI == null)
            {
                GameLogger.LogError("카드 UI 프리팹에 SkillCardUI 컴포넌트가 없습니다.", GameLogger.LogCategory.Error);
                Destroy(cardObj);
                return null;
            }

            availableCards.Enqueue(cardUI);
            return cardUI;
        }

        #endregion

        #region 풀 관리

        /// <summary>
        /// 풀에서 카드 UI를 가져옵니다.
        /// </summary>
        /// <param name="parent">부모 Transform</param>
        /// <returns>스킬 카드 UI</returns>
        public SkillCardUI Get(Transform parent = null)
        {
            SkillCardUI cardUI;

            // 사용 가능한 카드가 있으면 재사용
            if (availableCards.Count > 0)
            {
                cardUI = availableCards.Dequeue();
            }
            else
            {
                // 최대 크기에 도달하지 않았으면 새로 생성
                if (activeCards.Count < maxPoolSize)
                {
                    cardUI = CreateNewCard();
                    if (cardUI == null) return null;
                    availableCards.Dequeue(); // 방금 추가한 것을 꺼냄
                }
                else
                {
                    GameLogger.LogWarning($"스킬 카드 UI 풀이 최대 크기에 도달했습니다. (Max: {maxPoolSize})", GameLogger.LogCategory.SkillCard);
                    return null;
                }
            }

            // 카드 설정
            if (parent != null)
            {
                cardUI.transform.SetParent(parent, false);
            }
            cardUI.gameObject.SetActive(true);
            activeCards.Add(cardUI);

            return cardUI;
        }

        /// <summary>
        /// 카드 UI를 풀에 반환합니다.
        /// </summary>
        /// <param name="cardUI">반환할 카드 UI</param>
        public void Return(SkillCardUI cardUI)
        {
            if (cardUI == null) return;

            if (!activeCards.Contains(cardUI))
            {
                GameLogger.LogWarning("풀에 속하지 않은 카드 UI를 반환하려고 시도했습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            activeCards.Remove(cardUI);
            cardUI.gameObject.SetActive(false);
            cardUI.transform.SetParent(poolContainer);

            // 카드 UI는 SetCard() 호출 시 자동으로 초기화됨

            availableCards.Enqueue(cardUI);
        }

        /// <summary>
        /// 모든 활성 카드를 풀에 반환합니다.
        /// </summary>
        public void ReturnAll()
        {
            var cardsToReturn = new List<SkillCardUI>(activeCards);
            foreach (var card in cardsToReturn)
            {
                Return(card);
            }

            GameLogger.LogInfo("모든 스킬 카드 UI를 풀에 반환했습니다.", GameLogger.LogCategory.SkillCard);
        }

        #endregion

        #region 정보

        /// <summary>
        /// 사용 가능한 카드 수
        /// </summary>
        public int AvailableCount => availableCards.Count;

        /// <summary>
        /// 활성화된 카드 수
        /// </summary>
        public int ActiveCount => activeCards.Count;

        /// <summary>
        /// 총 풀 크기
        /// </summary>
        public int TotalPoolSize => availableCards.Count + activeCards.Count;

        #endregion

        #region 디버그

        [ContextMenu("풀 상태 출력")]
        private void LogPoolStatus()
        {
            GameLogger.LogInfo($"[SkillCardUIPool] 사용 가능: {AvailableCount}, 활성화: {ActiveCount}, 총 크기: {TotalPoolSize}", GameLogger.LogCategory.SkillCard);
        }

        #endregion
    }
}
