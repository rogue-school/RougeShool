using UnityEngine;
using System.Collections.Generic;
using Game.CoreSystem.Utility;

namespace Game.VFXSystem.Pool
{
    /// <summary>
    /// 데미지 텍스트 오브젝트 풀
    /// GC 압박을 줄이기 위한 오브젝트 재사용 시스템
    /// </summary>
    public class DamageTextPool : MonoBehaviour
    {
        #region 설정

        [Header("풀 설정")]
        [Tooltip("데미지 텍스트 프리팹")]
        [SerializeField] private GameObject damageTextPrefab;

        [Tooltip("초기 풀 크기")]
        [SerializeField] private int initialPoolSize = 10;

        [Tooltip("최대 풀 크기")]
        [SerializeField] private int maxPoolSize = 50;

        #endregion

        #region 내부 상태

        private Queue<GameObject> availableObjects = new Queue<GameObject>();
        private HashSet<GameObject> activeObjects = new HashSet<GameObject>();
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
            poolContainer = new GameObject("DamageTextPool_Container").transform;
            poolContainer.SetParent(transform);

            // 초기 오브젝트 생성
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewObject();
            }

            GameLogger.LogInfo($"데미지 텍스트 풀 초기화 완료 - 크기: {initialPoolSize}", GameLogger.LogCategory.Combat);
        }

        private GameObject CreateNewObject()
        {
            if (damageTextPrefab == null)
            {
                GameLogger.LogError("데미지 텍스트 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.Error);
                return null;
            }

            GameObject obj = Instantiate(damageTextPrefab, poolContainer);
            obj.SetActive(false);
            availableObjects.Enqueue(obj);
            return obj;
        }

        #endregion

        #region 풀 관리

        /// <summary>
        /// 풀에서 오브젝트를 가져옵니다.
        /// </summary>
        /// <param name="position">생성 위치</param>
        /// <param name="parent">부모 Transform (선택적)</param>
        /// <returns>데미지 텍스트 GameObject</returns>
        public GameObject Get(Vector3 position, Transform parent = null)
        {
            GameObject obj;

            // 사용 가능한 오브젝트가 있으면 재사용
            if (availableObjects.Count > 0)
            {
                obj = availableObjects.Dequeue();
            }
            else
            {
                // 최대 크기에 도달하지 않았으면 새로 생성
                if (activeObjects.Count < maxPoolSize)
                {
                    obj = CreateNewObject();
                    if (obj == null) return null;
                    availableObjects.Dequeue(); // 방금 추가한 것을 꺼냄
                }
                else
                {
                    GameLogger.LogWarning($"데미지 텍스트 풀이 최대 크기에 도달했습니다. (Max: {maxPoolSize})", GameLogger.LogCategory.Combat);
                    return null;
                }
            }

            // 오브젝트 설정
            obj.transform.position = position;
            obj.transform.SetParent(parent);
            obj.SetActive(true);

            activeObjects.Add(obj);

            return obj;
        }

        /// <summary>
        /// 오브젝트를 풀에 반환합니다.
        /// </summary>
        /// <param name="obj">반환할 GameObject</param>
        public void Return(GameObject obj)
        {
            if (obj == null) return;

            if (!activeObjects.Contains(obj))
            {
                GameLogger.LogWarning("풀에 속하지 않은 오브젝트를 반환하려고 시도했습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            activeObjects.Remove(obj);
            obj.SetActive(false);
            obj.transform.SetParent(poolContainer);
            availableObjects.Enqueue(obj);
        }

        /// <summary>
        /// 모든 활성 오브젝트를 풀에 반환합니다.
        /// </summary>
        public void ReturnAll()
        {
            // activeObjects를 복사하여 반복 (컬렉션 수정 중 반복 방지)
            var objectsToReturn = new List<GameObject>(activeObjects);
            foreach (var obj in objectsToReturn)
            {
                Return(obj);
            }

            GameLogger.LogInfo("모든 데미지 텍스트를 풀에 반환했습니다.", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 정보

        /// <summary>
        /// 사용 가능한 오브젝트 수
        /// </summary>
        public int AvailableCount => availableObjects.Count;

        /// <summary>
        /// 활성화된 오브젝트 수
        /// </summary>
        public int ActiveCount => activeObjects.Count;

        /// <summary>
        /// 총 풀 크기
        /// </summary>
        public int TotalPoolSize => availableObjects.Count + activeObjects.Count;

        #endregion

        #region 디버그

        [ContextMenu("풀 상태 출력")]
        private void LogPoolStatus()
        {
            GameLogger.LogInfo($"[DamageTextPool] 사용 가능: {AvailableCount}, 활성화: {ActiveCount}, 총 크기: {TotalPoolSize}", GameLogger.LogCategory.Combat);
        }

        #endregion
    }
}
