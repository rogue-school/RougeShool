using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Save
{
    /// <summary>
    /// 씬 전체 저장을 관리하는 매니저
    /// </summary>
    public class SaveManager : MonoBehaviour, ICoreSystemInitializable
    {
        public static SaveManager Instance { get; private set; }
        
        [Header("저장 설정")]
        [SerializeField] private string saveFileName = "GameSave.json";
        
        // 초기화 상태
        public bool IsInitialized { get; private set; } = false;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[SaveManager] 초기화 완료");
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 현재 씬 전체 저장
        /// </summary>
        public async Task<bool> SaveCurrentScene()
        {
            try
            {
                Debug.Log("[SaveManager] 씬 저장 시작");
                
                // 현재 씬의 모든 데이터 수집
                var sceneData = CollectSceneData();
                
                // JSON으로 직렬화
                string jsonData = JsonUtility.ToJson(sceneData, true);
                
                // 파일로 저장
                string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
                await File.WriteAllTextAsync(filePath, jsonData);
                
                Debug.Log($"[SaveManager] 씬 저장 완료: {filePath}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveManager] 씬 저장 실패: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 저장된 씬 로드
        /// </summary>
        public async Task<bool> LoadSavedScene()
        {
            try
            {
                Debug.Log("[SaveManager] 씬 로드 시작");
                
                string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning("[SaveManager] 저장 파일이 존재하지 않습니다");
                    return false;
                }
                
                // 파일에서 데이터 읽기
                string jsonData = await File.ReadAllTextAsync(filePath);
                
                // JSON 역직렬화
                var sceneData = JsonUtility.FromJson<SceneSaveData>(jsonData);
                
                // 씬 데이터 복원
                RestoreSceneData(sceneData);
                
                Debug.Log("[SaveManager] 씬 로드 완료");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveManager] 씬 로드 실패: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 세이브 초기화
        /// </summary>
        public void ClearSave()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log("[SaveManager] 세이브 초기화 완료");
                }
                else
                {
                    Debug.Log("[SaveManager] 삭제할 세이브 파일이 없습니다");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveManager] 세이브 초기화 실패: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 현재 씬 데이터 수집
        /// </summary>
        private SceneSaveData CollectSceneData()
        {
            var sceneData = new SceneSaveData();
            
            // 현재 씬 이름
            sceneData.sceneName = SceneManager.GetActiveScene().name;
            
            // 게임 상태
            sceneData.gameState = Game.CoreSystem.Manager.GameStateManager.Instance.CurrentGameState;
            
            // 씬의 모든 GameObject 정보 수집
            sceneData.gameObjects = new List<GameObjectData>();
            
            var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj.scene.name == sceneData.sceneName)
                {
                    var objData = new GameObjectData
                    {
                        name = obj.name,
                        position = obj.transform.position,
                        rotation = obj.transform.rotation,
                        scale = obj.transform.localScale,
                        active = obj.activeInHierarchy
                    };
                    sceneData.gameObjects.Add(objData);
                }
            }
            
            return sceneData;
        }
        
        /// <summary>
        /// 씬 데이터 복원
        /// </summary>
        private void RestoreSceneData(SceneSaveData sceneData)
        {
            // 게임 상태 복원
            Game.CoreSystem.Manager.GameStateManager.Instance.ChangeGameState(sceneData.gameState);
            
            // GameObject 정보 복원
            foreach (var objData in sceneData.gameObjects)
            {
                var obj = GameObject.Find(objData.name);
                if (obj != null)
                {
                    obj.transform.position = objData.position;
                    obj.transform.rotation = objData.rotation;
                    obj.transform.localScale = objData.scale;
                    obj.SetActive(objData.active);
                }
            }
        }
        
        /// <summary>
        /// 저장 파일 존재 여부 확인
        /// </summary>
        public bool HasSaveFile()
        {
            string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
            return File.Exists(filePath);
        }
        
        #region ICoreSystemInitializable 구현
        /// <summary>
        /// 시스템 초기화 수행
        /// </summary>
        public IEnumerator Initialize()
        {
            GameLogger.LogInfo("SaveManager 초기화 시작", GameLogger.LogCategory.UI);
            
            // 저장 디렉토리 확인 및 생성
            string saveDirectory = Path.GetDirectoryName(Path.Combine(Application.persistentDataPath, saveFileName));
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
            
            // 초기화 완료
            IsInitialized = true;
            
            GameLogger.LogInfo("SaveManager 초기화 완료", GameLogger.LogCategory.UI);
            yield return null;
        }
        
        /// <summary>
        /// 초기화 실패 시 호출
        /// </summary>
        public void OnInitializationFailed()
        {
            GameLogger.LogError("SaveManager 초기화 실패", GameLogger.LogCategory.Error);
            IsInitialized = false;
        }
        
        /// <summary>
        /// 저장 데이터 초기화 (게임 진행 리셋)
        /// </summary>
        public void ResetSaveData()
        {
            try
            {
                string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
                
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    GameLogger.LogInfo("저장 데이터 초기화 완료", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogInfo("삭제할 저장 데이터가 없습니다", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"저장 데이터 초기화 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        #endregion
    }
    
    /// <summary>
    /// 씬 저장 데이터 구조
    /// </summary>
    [System.Serializable]
    public class SceneSaveData
    {
        public string sceneName;
        public Game.CoreSystem.Manager.GameState gameState;
        public List<GameObjectData> gameObjects;
    }
    
    /// <summary>
    /// GameObject 저장 데이터 구조
    /// </summary>
    [System.Serializable]
    public class GameObjectData
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public bool active;
    }
}
