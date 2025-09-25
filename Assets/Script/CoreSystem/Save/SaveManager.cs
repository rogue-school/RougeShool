using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.CoreSystem.Save
{
	/// <summary>
	/// 씬 전체 저장을 관리하는 매니저 (Zenject DI 기반)
	/// </summary>
	public class SaveManager : MonoBehaviour, ICoreSystemInitializable, ISaveManager
	{
		
		[Header("저장 설정")]
		[SerializeField] private string saveFileName = "GameSave.json";
		
		// 오디오 설정 키
		private const string KEY_BGM_VOLUME = "audio_bgm_volume";
		private const string KEY_SFX_VOLUME = "audio_sfx_volume";
		
		// 덱 구성 저장 키
		private const string KEY_PLAYER_DECK_CONFIG = "player_deck_configuration";
		
		// 초기화 상태
		public bool IsInitialized { get; private set; } = false;
		
		// 의존성 주입
		[Inject] private IGameStateManager gameStateManager;
		
		private void Awake()
		{
			Debug.Log("[SaveManager] 초기화 완료");
		}
		
		/// <summary>
		/// 오디오 설정 저장
		/// </summary>
		public void SaveAudioSettings(float bgmVolume, float sfxVolume)
		{
			PlayerPrefs.SetFloat(KEY_BGM_VOLUME, Mathf.Clamp01(bgmVolume));
			PlayerPrefs.SetFloat(KEY_SFX_VOLUME, Mathf.Clamp01(sfxVolume));
			PlayerPrefs.Save();
			GameLogger.LogInfo($"오디오 설정 저장 - BGM:{bgmVolume:F2}, SFX:{sfxVolume:F2}", GameLogger.LogCategory.UI);
		}
		
		/// <summary>
		/// 오디오 설정 로드 (없으면 기본값 반환)
		/// </summary>
		public (float bgmVolume, float sfxVolume) LoadAudioSettings(float defaultBgmVolume = 0.7f, float defaultSfxVolume = 1.0f)
		{
			float bgm = PlayerPrefs.HasKey(KEY_BGM_VOLUME) ? PlayerPrefs.GetFloat(KEY_BGM_VOLUME) : defaultBgmVolume;
			float sfx = PlayerPrefs.HasKey(KEY_SFX_VOLUME) ? PlayerPrefs.GetFloat(KEY_SFX_VOLUME) : defaultSfxVolume;
			return (Mathf.Clamp01(bgm), Mathf.Clamp01(sfx));
		}
		
		/// <summary>
		/// 현재 씬 전체 저장
		/// </summary>
		public async Task SaveCurrentScene()
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
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[SaveManager] 씬 저장 실패: {ex.Message}");
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
			sceneData.gameState = gameStateManager.CurrentGameState;
			
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
			gameStateManager.ChangeGameState(sceneData.gameState);
			
            // GameObject 정보 복원 (UI 오브젝트는 제외하여 Canvas/RectTransform 비율 붕괴 방지)
            foreach (var objData in sceneData.gameObjects)
            {
                var obj = GameObject.Find(objData.name);
                if (obj == null)
                {
                    continue;
                }

                // UI 계층(RectTransform 보유) 객체는 트랜스폼 복원을 건너뛴다
                if (obj.GetComponent<RectTransform>() != null || obj.GetComponentInParent<Canvas>() != null)
                {
                    // 활성 상태만 동기화(선택), 트랜스폼은 건너뜀
                    obj.SetActive(objData.active);
                    continue;
                }

                obj.transform.position = objData.position;
                obj.transform.rotation = objData.rotation;
                obj.transform.localScale = objData.scale;
                obj.SetActive(objData.active);
            }
		}
		
		/// <summary>
		/// 저장 파일 존재 여부 확인
		/// </summary>
		public bool HasSaveFile()
		{
			string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
			bool exists = File.Exists(filePath);
			
			Debug.Log($"[SaveManager] 저장 파일 확인:");
			Debug.Log($"  - 파일 경로: {filePath}");
			Debug.Log($"  - 파일 존재: {exists}");
			Debug.Log($"  - 파일명: {saveFileName}");
			
			if (exists)
			{
				try
				{
					var fileInfo = new FileInfo(filePath);
					Debug.Log($"  - 파일 크기: {fileInfo.Length} bytes");
					Debug.Log($"  - 생성 시간: {fileInfo.CreationTime}");
					Debug.Log($"  - 수정 시간: {fileInfo.LastWriteTime}");
				}
				catch (System.Exception ex)
				{
					Debug.LogWarning($"[SaveManager] 파일 정보 조회 실패: {ex.Message}");
				}
			}
			
			return exists;
		}
		
		/// <summary>
		/// 게임 상태 저장
		/// </summary>
		public async Task SaveGameState(string saveName)
		{
			try
			{
				Debug.Log($"[SaveManager] 게임 상태 저장 시작: {saveName}");
				
				// 현재 씬의 모든 데이터 수집
				var sceneData = CollectSceneData();
				
				// JSON으로 직렬화
				string jsonData = JsonUtility.ToJson(sceneData, true);
				
				// 파일로 저장
				string filePath = Path.Combine(Application.persistentDataPath, $"{saveName}.json");
				await File.WriteAllTextAsync(filePath, jsonData);
				
				Debug.Log($"[SaveManager] 게임 상태 저장 완료: {filePath}");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[SaveManager] 게임 상태 저장 실패: {ex.Message}");
			}
		}
		
		/// <summary>
		/// 게임 상태 로드
		/// </summary>
		public async Task LoadGameState(string saveName)
		{
			try
			{
				Debug.Log($"[SaveManager] 게임 상태 로드 시작: {saveName}");
				
				string filePath = Path.Combine(Application.persistentDataPath, $"{saveName}.json");
				
				if (!File.Exists(filePath))
				{
					Debug.LogWarning($"[SaveManager] 저장 파일이 존재하지 않습니다: {filePath}");
					return;
				}
				
				// 파일에서 데이터 읽기
				string jsonData = await File.ReadAllTextAsync(filePath);
				
				// JSON 역직렬화
				var sceneData = JsonUtility.FromJson<SceneSaveData>(jsonData);
				
				// 씬 데이터 복원
				RestoreSceneData(sceneData);
				
				Debug.Log($"[SaveManager] 게임 상태 로드 완료: {saveName}");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[SaveManager] 게임 상태 로드 실패: {ex.Message}");
			}
		}
		
		/// <summary>
		/// 자동 저장 실행
		/// </summary>
		public async Task TriggerAutoSave(string condition)
		{
			try
			{
				Debug.Log($"[SaveManager] 자동 저장 실행: {condition}");
				
				// 현재 씬의 모든 데이터 수집
				var sceneData = CollectSceneData();
				
				// JSON으로 직렬화
				string jsonData = JsonUtility.ToJson(sceneData, true);
				
				// 자동 저장 파일로 저장
				string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
				string filePath = Path.Combine(Application.persistentDataPath, $"AutoSave_{condition}_{timestamp}.json");
				await File.WriteAllTextAsync(filePath, jsonData);
				
				Debug.Log($"[SaveManager] 자동 저장 완료: {filePath}");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[SaveManager] 자동 저장 실패: {ex.Message}");
			}
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
		
		/// <summary>
		/// 플레이어 덱 구성을 저장합니다.
		/// </summary>
		/// <param name="deckConfigurationJson">덱 구성 JSON 데이터</param>
		public void SavePlayerDeckConfiguration(string deckConfigurationJson)
		{
			try
			{
				PlayerPrefs.SetString(KEY_PLAYER_DECK_CONFIG, deckConfigurationJson);
				PlayerPrefs.Save();
				GameLogger.LogInfo("플레이어 덱 구성 저장 완료", GameLogger.LogCategory.UI);
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"플레이어 덱 구성 저장 실패: {ex.Message}", GameLogger.LogCategory.Error);
			}
		}
		
		/// <summary>
		/// 플레이어 덱 구성을 로드합니다.
		/// </summary>
		/// <returns>덱 구성 JSON 데이터 (없으면 빈 문자열)</returns>
		public string LoadPlayerDeckConfiguration()
		{
			try
			{
				string deckConfig = PlayerPrefs.GetString(KEY_PLAYER_DECK_CONFIG, "");
				if (!string.IsNullOrEmpty(deckConfig))
				{
					GameLogger.LogInfo("플레이어 덱 구성 로드 완료", GameLogger.LogCategory.UI);
				}
				return deckConfig;
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"플레이어 덱 구성 로드 실패: {ex.Message}", GameLogger.LogCategory.Error);
				return "";
			}
		}
		
		/// <summary>
		/// 플레이어 덱 구성을 삭제합니다.
		/// </summary>
		public void DeletePlayerDeckConfiguration()
		{
			try
			{
				PlayerPrefs.DeleteKey(KEY_PLAYER_DECK_CONFIG);
				PlayerPrefs.Save();
				GameLogger.LogInfo("플레이어 덱 구성 삭제 완료", GameLogger.LogCategory.UI);
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"플레이어 덱 구성 삭제 실패: {ex.Message}", GameLogger.LogCategory.Error);
			}
		}
		
		/// <summary>
		/// 저장된 전투 데이터가 있는지 확인합니다.
		/// 이어하기 기능을 위해 사용됩니다.
		/// </summary>
		public bool HasCombatSaveData()
		{
			try
			{
				// 저장된 전투 데이터 키 확인 (예: 최근 저장된 전투 상태)
				string combatSaveKey = "CombatSaveData_Recent";
				return PlayerPrefs.HasKey(combatSaveKey) && !string.IsNullOrEmpty(PlayerPrefs.GetString(combatSaveKey));
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"전투 저장 데이터 확인 실패: {ex.Message}", GameLogger.LogCategory.Error);
				return false;
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
		public Game.CoreSystem.Interface.GameState gameState;
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
