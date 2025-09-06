using UnityEngine;

namespace Game.UtilitySystem
{
    /// <summary>
    /// 범용 DontDestroyOnLoad 컨테이너
    /// 모든 씬에서 사용 가능한 컨테이너 관리 스크립트
    /// </summary>
    public class DontDestroyOnLoadContainer : MonoBehaviour
    {
        [Header("컨테이너 설정")]
        [SerializeField] private bool applyToSelf = true;
        [SerializeField] private bool applyToChildren = true;
        [SerializeField] private bool applyToNewChildren = true;
        [SerializeField] private bool persistAcrossScenes = true;
        
        [Header("디버그 정보")]
        [SerializeField] private bool enableDebugLogging = false;
        
        private void Awake()
        {
            // 컨테이너 자체에 DontDestroyOnLoad 적용
            if (applyToSelf && persistAcrossScenes)
            {
                // 루트 오브젝트인지 확인 후 DontDestroyOnLoad 적용
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                    if (enableDebugLogging)
                    {
                        Debug.Log($"[DontDestroyOnLoadContainer] {gameObject.name} 컨테이너에 DontDestroyOnLoad 적용");
                    }
                }
                else
                {
                    if (enableDebugLogging)
                    {
                        Debug.LogWarning($"[DontDestroyOnLoadContainer] {gameObject.name}은 루트 오브젝트가 아니므로 DontDestroyOnLoad를 적용할 수 없습니다.");
                    }
                }
            }
            
            // 모든 하위 오브젝트에 DontDestroyOnLoad 적용
            if (applyToChildren && persistAcrossScenes)
            {
                ApplyDontDestroyOnLoadToChildren();
            }
        }
        
        /// <summary>
        /// 모든 하위 오브젝트에 DontDestroyOnLoad 적용
        /// 주의: DontDestroyOnLoad는 루트 오브젝트에만 적용 가능하므로 자식 오브젝트는 부모와 함께 유지됨
        /// </summary>
        private void ApplyDontDestroyOnLoadToChildren()
        {
            // 현재 오브젝트가 루트가 아니면 자식들에게 DontDestroyOnLoad를 적용하지 않음
            if (transform.parent != null)
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"[DontDestroyOnLoadContainer] {gameObject.name}이 루트 오브젝트가 아니므로 자식들에게 DontDestroyOnLoad를 적용하지 않습니다.");
                }
                return;
            }
            
            // DontDestroyOnLoad는 루트 오브젝트에만 적용 가능
            // 자식 오브젝트들은 부모가 DontDestroyOnLoad되면 자동으로 함께 유지됨
            if (enableDebugLogging)
            {
                Debug.Log($"[DontDestroyOnLoadContainer] {gameObject.name}의 자식 오브젝트들은 부모와 함께 유지됩니다. (자식 수: {transform.childCount})");
            }
        }
        
        /// <summary>
        /// 새로운 오브젝트가 추가될 때 호출
        /// </summary>
        public void AddObject(GameObject newObject)
        {
            if (newObject != null)
            {
                newObject.transform.SetParent(transform);
                
                if (applyToNewChildren && transform.parent == null)
                {
                    DontDestroyOnLoad(newObject);
                    if (enableDebugLogging)
                    {
                        Debug.Log($"[DontDestroyOnLoadContainer] {newObject.name} 추가 및 DontDestroyOnLoad 적용");
                    }
                }
                else if (enableDebugLogging)
                {
                    Debug.LogWarning($"[DontDestroyOnLoadContainer] {gameObject.name}이 루트 오브젝트가 아니므로 {newObject.name}에 DontDestroyOnLoad를 적용하지 않습니다.");
                }
            }
        }
        
        /// <summary>
        /// 특정 오브젝트를 컨테이너에서 제거
        /// </summary>
        public void RemoveObject(GameObject objectToRemove)
        {
            if (objectToRemove != null && objectToRemove.transform.parent == transform)
            {
                objectToRemove.transform.SetParent(null);
                if (enableDebugLogging)
                {
                    Debug.Log($"[DontDestroyOnLoadContainer] {objectToRemove.name} 제거");
                }
            }
        }
        
        /// <summary>
        /// 컨테이너의 모든 하위 오브젝트 목록 반환
        /// </summary>
        public Transform[] GetAllChildren()
        {
            Transform[] children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }
            return children;
        }
        
        /// <summary>
        /// 컨테이너가 비어있는지 확인
        /// </summary>
        public bool IsEmpty()
        {
            return transform.childCount == 0;
        }
        
        /// <summary>
        /// 컨테이너의 오브젝트 개수 반환
        /// </summary>
        public int GetObjectCount()
        {
            return transform.childCount;
        }
    }
}
