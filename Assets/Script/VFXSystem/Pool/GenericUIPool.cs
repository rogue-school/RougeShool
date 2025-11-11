using UnityEngine;
using System.Collections.Generic;
using Game.CoreSystem.Utility;

namespace Game.VFXSystem.Pool
{
    /// <summary>
    /// 제네릭 UI 오브젝트 풀
    /// 다양한 UI 요소를 재사용하기 위한 범용 풀링 시스템
    /// </summary>
    /// <typeparam name="T">풀링할 컴포넌트 타입</typeparam>
    public class GenericUIPool<T> where T : UnityEngine.Component
    {
        private readonly GameObject prefab;
        private readonly Transform container;
        private readonly int maxPoolSize;
        private readonly Queue<T> availableObjects = new Queue<T>();
        private readonly HashSet<T> activeObjects = new HashSet<T>();
        private readonly string poolName;

        /// <summary>
        /// GenericUIPool 생성자
        /// </summary>
        /// <param name="prefab">풀링할 프리팹</param>
        /// <param name="container">풀 컨테이너 Transform</param>
        /// <param name="initialSize">초기 풀 크기</param>
        /// <param name="maxSize">최대 풀 크기</param>
        /// <param name="poolName">풀 이름 (디버그용)</param>
        public GenericUIPool(GameObject prefab, Transform container, int initialSize = 10, int maxSize = 50, string poolName = "GenericUIPool")
        {
            this.prefab = prefab;
            this.container = container;
            this.maxPoolSize = maxSize;
            this.poolName = poolName;

            // 초기 오브젝트 생성
            for (int i = 0; i < initialSize; i++)
            {
                CreateNew();
            }

            GameLogger.LogInfo($"{poolName} 초기화 완료 - 크기: {initialSize}", GameLogger.LogCategory.UI);
        }

        private T CreateNew()
        {
            if (prefab == null)
            {
                GameLogger.LogError($"{poolName}: 프리팹이 null입니다.", GameLogger.LogCategory.Error);
                return null;
            }

            GameObject obj = Object.Instantiate(prefab, container);
            obj.SetActive(false);

            T component = obj.GetComponent<T>();
            if (component == null)
            {
                GameLogger.LogError($"{poolName}: 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다.", GameLogger.LogCategory.Error);
                Object.Destroy(obj);
                return null;
            }

            availableObjects.Enqueue(component);
            return component;
        }

        /// <summary>
        /// 풀에서 오브젝트를 가져옵니다.
        /// </summary>
        /// <param name="parent">부모 Transform (선택적)</param>
        /// <returns>컴포넌트 인스턴스</returns>
        public T Get(Transform parent = null)
        {
            T component;

            // 사용 가능한 오브젝트가 있으면 재사용
            if (availableObjects.Count > 0)
            {
                component = availableObjects.Dequeue();
            }
            else
            {
                // 최대 크기에 도달하지 않았으면 새로 생성
                if (activeObjects.Count < maxPoolSize)
                {
                    component = CreateNew();
                    if (component == null) return null;
                    availableObjects.Dequeue(); // 방금 추가한 것을 꺼냄
                }
                else
                {
                    GameLogger.LogWarning($"{poolName}: 풀이 최대 크기에 도달했습니다. (Max: {maxPoolSize})", GameLogger.LogCategory.UI);
                    return null;
                }
            }

            // 오브젝트 설정
            if (parent != null)
            {
                component.transform.SetParent(parent, false);
            }
            component.gameObject.SetActive(true);
            activeObjects.Add(component);

            return component;
        }

        /// <summary>
        /// 오브젝트를 풀에 반환합니다.
        /// </summary>
        /// <param name="component">반환할 컴포넌트</param>
        public void Return(T component)
        {
            if (component == null) return;

            if (!activeObjects.Contains(component))
            {
                GameLogger.LogWarning($"{poolName}: 풀에 속하지 않은 오브젝트를 반환하려고 시도했습니다.", GameLogger.LogCategory.UI);
                return;
            }

            activeObjects.Remove(component);
            component.gameObject.SetActive(false);
            component.transform.SetParent(container);
            availableObjects.Enqueue(component);
        }

        /// <summary>
        /// 모든 활성 오브젝트를 풀에 반환합니다.
        /// </summary>
        public void ReturnAll()
        {
            var objectsToReturn = new List<T>(activeObjects);
            foreach (var obj in objectsToReturn)
            {
                Return(obj);
            }

            GameLogger.LogInfo($"{poolName}: 모든 오브젝트 반환 완료", GameLogger.LogCategory.UI);
        }

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
    }
}
