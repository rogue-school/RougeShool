using UnityEngine;
using System.Collections.Generic;
using Game.CoreSystem.Utility;

namespace Game.VFXSystem.Pool
{
    /// <summary>
    /// 버프/디버프 아이콘 오브젝트 풀
    /// UI 아이콘을 재사용하여 GC 압박을 줄입니다.
    /// </summary>
    public class BuffIconPool : MonoBehaviour
    {
        #region 설정

        [Header("풀 설정")]
        [Tooltip("버프/디버프 아이콘 프리팹")]
        [SerializeField] private GameObject iconPrefab;

        [Tooltip("초기 풀 크기")]
        [SerializeField] private int initialPoolSize = 15;

        [Tooltip("최대 풀 크기")]
        [SerializeField] private int maxPoolSize = 50;

        #endregion

        #region 내부 상태

        private Queue<GameObject> availableIcons = new Queue<GameObject>();
        private HashSet<GameObject> activeIcons = new HashSet<GameObject>();
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
            poolContainer = new GameObject("BuffIconPool_Container").transform;
            poolContainer.SetParent(transform);
            poolContainer.gameObject.SetActive(false); // 비활성 컨테이너

            // 초기 오브젝트 생성
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewIcon();
            }

            GameLogger.LogInfo($"버프 아이콘 풀 초기화 완료 - 크기: {initialPoolSize}", GameLogger.LogCategory.UI);
        }

        private GameObject CreateNewIcon()
        {
            if (iconPrefab == null)
            {
                GameLogger.LogError("버프 아이콘 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.Error);
                return null;
            }

            GameObject icon = Instantiate(iconPrefab, poolContainer);
            icon.SetActive(false);
            availableIcons.Enqueue(icon);
            return icon;
        }

        #endregion

        #region 풀 관리

        /// <summary>
        /// 풀에서 아이콘을 가져옵니다.
        /// </summary>
        /// <param name="parent">부모 Transform</param>
        /// <returns>버프 아이콘 GameObject</returns>
        public GameObject Get(Transform parent)
        {
            GameObject icon;

            // 사용 가능한 아이콘이 있으면 재사용
            if (availableIcons.Count > 0)
            {
                icon = availableIcons.Dequeue();
            }
            else
            {
                // 최대 크기에 도달하지 않았으면 새로 생성
                if (activeIcons.Count < maxPoolSize)
                {
                    icon = CreateNewIcon();
                    if (icon == null) return null;
                    availableIcons.Dequeue(); // 방금 추가한 것을 꺼냄
                }
                else
                {
                    GameLogger.LogWarning($"버프 아이콘 풀이 최대 크기에 도달했습니다. (Max: {maxPoolSize})", GameLogger.LogCategory.UI);
                    return null;
                }
            }

            // 아이콘 설정
            icon.transform.SetParent(parent, false);
            icon.SetActive(true);
            activeIcons.Add(icon);

            return icon;
        }

        /// <summary>
        /// 아이콘을 풀에 반환합니다.
        /// </summary>
        /// <param name="icon">반환할 GameObject</param>
        public void Return(GameObject icon)
        {
            if (icon == null) return;

            if (!activeIcons.Contains(icon))
            {
                GameLogger.LogWarning("풀에 속하지 않은 아이콘을 반환하려고 시도했습니다.", GameLogger.LogCategory.UI);
                return;
            }

            activeIcons.Remove(icon);
            icon.SetActive(false);
            icon.transform.SetParent(poolContainer);
            availableIcons.Enqueue(icon);
        }

        /// <summary>
        /// 모든 활성 아이콘을 풀에 반환합니다.
        /// </summary>
        public void ReturnAll()
        {
            var iconsToReturn = new List<GameObject>(activeIcons);
            foreach (var icon in iconsToReturn)
            {
                Return(icon);
            }

            GameLogger.LogInfo("모든 버프 아이콘을 풀에 반환했습니다.", GameLogger.LogCategory.UI);
        }

        #endregion

        #region 정보

        /// <summary>
        /// 사용 가능한 아이콘 수
        /// </summary>
        public int AvailableCount => availableIcons.Count;

        /// <summary>
        /// 활성화된 아이콘 수
        /// </summary>
        public int ActiveCount => activeIcons.Count;

        /// <summary>
        /// 총 풀 크기
        /// </summary>
        public int TotalPoolSize => availableIcons.Count + activeIcons.Count;

        #endregion

        #region 디버그

        [ContextMenu("풀 상태 출력")]
        private void LogPoolStatus()
        {
            GameLogger.LogInfo($"[BuffIconPool] 사용 가능: {AvailableCount}, 활성화: {ActiveCount}, 총 크기: {TotalPoolSize}", GameLogger.LogCategory.UI);
        }

        #endregion
    }
}
