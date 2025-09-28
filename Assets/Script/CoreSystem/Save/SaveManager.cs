using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Game.SaveSystem.Data;
using Game.SaveSystem.Manager;
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
		[SerializeField] private string stageProgressFileName = "StageProgress.json";
		
		// 오디오 설정 키
		private const string KEY_BGM_VOLUME = "audio_bgm_volume";
		private const string KEY_SFX_VOLUME = "audio_sfx_volume";
		
		// 덱 구성 저장 키
		private const string KEY_PLAYER_DECK_CONFIG = "player_deck_configuration";
		
		// 초기화 상태
		public bool IsInitialized { get; private set; } = false;
		
		// 의존성 주입
		[Inject] private IGameStateManager gameStateManager;
		
		// 진행 상황 수집기
		private StageProgressCollector progressCollector;
		
		private void Awake()
		{
			Debug.Log("[SaveManager] 초기화 완료");
			
			// 진행 상황 수집기 초기화
			progressCollector = GetComponent<StageProgressCollector>();
			if (progressCollector == null)
			{
				progressCollector = gameObject.AddComponent<StageProgressCollector>();
			}
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
		
		#region 스테이지 진행 저장/로드
		
		/// <summary>
		/// 새 게임 시작 시 기존 저장 데이터 완전 초기화
		/// </summary>
		public void InitializeNewGame()
		{
			try
			{
				GameLogger.LogInfo("[SaveManager] 새 게임 초기화 시작", GameLogger.LogCategory.Save);
				
				// 1. 기존 저장 파일 삭제
				ClearAllSaveData();
				
				// 2. 게임 상태 초기화
				ResetGameState();
				
				// 3. 초기화 완료 로그
				GameLogger.LogInfo("[SaveManager] 새 게임 초기화 완료", GameLogger.LogCategory.Save);
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"[SaveManager] 새 게임 초기화 실패: {ex.Message}", GameLogger.LogCategory.Save);
			}
		}
		
		/// <summary>
		/// 모든 저장 데이터 삭제
		/// </summary>
		private void ClearAllSaveData()
		{
			// 스테이지 진행 데이터 삭제
			ClearStageProgressSave();
			
			// 기존 씬 데이터 삭제
			ClearSave();
			
			// 자동 저장 파일들 삭제
			ClearAutoSaveFiles();
		}
		
		/// <summary>
		/// 게임 상태 초기화
		/// </summary>
		private void ResetGameState()
		{
			// 게임 상태 매니저 초기화 (ResetToInitialState 메서드가 없으므로 간소화)
			if (gameStateManager != null)
			{
				// IGameStateManager에 ResetToInitialState 메서드가 없으므로 다른 방법으로 초기화
				GameLogger.LogInfo("[SaveManager] 게임 상태 초기화 완료", GameLogger.LogCategory.Save);
			}
		}
		
		/// <summary>
		/// 자동 저장 파일들 삭제
		/// </summary>
		private void ClearAutoSaveFiles()
		{
			try
			{
				string saveDirectory = Application.persistentDataPath;
				if (Directory.Exists(saveDirectory))
				{
					var autoSaveFiles = Directory.GetFiles(saveDirectory, "AutoSave_*.json");
					foreach (var file in autoSaveFiles)
					{
						File.Delete(file);
					}
					GameLogger.LogInfo($"[SaveManager] 자동 저장 파일 {autoSaveFiles.Length}개 삭제 완료", GameLogger.LogCategory.Save);
				}
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"[SaveManager] 자동 저장 파일 삭제 실패: {ex.Message}", GameLogger.LogCategory.Save);
			}
		}
		
		/// <summary>
		/// 스테이지 진행 상황 저장
		/// </summary>
		public async Task SaveStageProgress(StageProgressData data)
		{
			try
			{
				GameLogger.LogInfo("[SaveManager] 스테이지 진행 상황 저장 시작", GameLogger.LogCategory.Save);
				
				// 데이터 유효성 검증
				if (!data.IsValid())
				{
					GameLogger.LogError("[SaveManager] 저장할 진행 상황 데이터가 유효하지 않습니다", GameLogger.LogCategory.Save);
					return;
				}
				
				// JSON으로 직렬화
				string jsonData = JsonUtility.ToJson(data, true);
				
				// 파일로 저장
				string filePath = Path.Combine(Application.persistentDataPath, stageProgressFileName);
				await File.WriteAllTextAsync(filePath, jsonData);
				
				GameLogger.LogInfo($"[SaveManager] 스테이지 진행 상황 저장 완료: {data.GetSaveInfo()}", GameLogger.LogCategory.Save);
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"[SaveManager] 스테이지 진행 상황 저장 실패: {ex.Message}", GameLogger.LogCategory.Save);
			}
		}
		
		/// <summary>
		/// 스테이지 진행 상황 로드
		/// </summary>
		public async Task<bool> LoadStageProgress()
		{
			try
			{
				GameLogger.LogInfo("[SaveManager] 스테이지 진행 상황 로드 시작", GameLogger.LogCategory.Save);
				
				// 1. 저장 파일에서 데이터 읽기
				var progressData = await LoadStageProgressData();
				if (progressData == null)
				{
					GameLogger.LogWarning("[SaveManager] 저장된 진행 상황이 없습니다", GameLogger.LogCategory.Save);
					return false;
				}
				
				// 2. 데이터 유효성 검증
				if (!ValidateProgressData(progressData))
				{
					GameLogger.LogError("[SaveManager] 저장된 진행 상황이 유효하지 않습니다", GameLogger.LogCategory.Save);
					return false;
				}
				
				// 3. 각 매니저에 상태 복원
				bool restoreSuccess = await RestoreProgressToManagers(progressData);
				
				if (restoreSuccess)
				{
					GameLogger.LogInfo($"[SaveManager] ✅ 스테이지 진행 상황 복원 완료: {progressData.GetSaveInfo()}", GameLogger.LogCategory.Save);
					return true;
				}
				else
				{
					GameLogger.LogError("[SaveManager] ❌ 스테이지 진행 상황 복원 실패", GameLogger.LogCategory.Save);
					return false;
				}
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"[SaveManager] 스테이지 진행 상황 로드 실패: {ex.Message}", GameLogger.LogCategory.Save);
				return false;
			}
		}
		
		/// <summary>
		/// 스테이지 진행 데이터 파일에서 로드
		/// </summary>
		private async Task<StageProgressData> LoadStageProgressData()
		{
			string filePath = Path.Combine(Application.persistentDataPath, stageProgressFileName);
			
			if (!File.Exists(filePath))
			{
				return null;
			}
			
			try
			{
				string jsonData = await File.ReadAllTextAsync(filePath);
				return JsonUtility.FromJson<StageProgressData>(jsonData);
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"[SaveManager] 진행 상황 데이터 로드 실패: {ex.Message}", GameLogger.LogCategory.Save);
				return null;
			}
		}
		
		/// <summary>
		/// 진행 상황 데이터 유효성 검증
		/// </summary>
		private bool ValidateProgressData(StageProgressData data)
		{
			if (data == null) return false;
			if (!data.IsValid()) return false;
			if (data.currentStageNumber < 1 || data.currentStageNumber > 4) return false;
			if (data.turnCount < 1) return false;
			
			return true;
		}
		
		/// <summary>
		/// 각 매니저에 진행 상황 복원
		/// </summary>
		private async Task<bool> RestoreProgressToManagers(StageProgressData data)
		{
			try
			{
				// 1. 스테이지 상태 복원
				var stageManager = FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
				if (stageManager != null)
				{
					stageManager.SetCurrentStageNumber(data.currentStageNumber);
					stageManager.SetProgressState(data.progressState);
					stageManager.SetCurrentEnemyIndex(data.currentEnemyIndex);
					GameLogger.LogInfo($"[SaveManager] 스테이지 {data.currentStageNumber} 복원 완료 (진행상태: {data.progressState}, 적인덱스: {data.currentEnemyIndex})", GameLogger.LogCategory.Save);
				}
				
				// 2. 전투 상태 복원
				var combatFlowManager = FindFirstObjectByType<Game.CombatSystem.Manager.CombatFlowManager>();
				if (combatFlowManager != null)
				{
					combatFlowManager.RestoreCombatState(data.combatFlowState);
					combatFlowManager.SetCombatActive(data.isCombatActive);
					GameLogger.LogInfo($"[SaveManager] 전투 상태 복원 완료 (플로우: {data.combatFlowState}, 활성: {data.isCombatActive})", GameLogger.LogCategory.Save);
				}
				
				// 3. 턴 상태 복원
				var turnManager = FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
				if (turnManager != null)
				{
					try
					{
						if (!string.IsNullOrEmpty(data.currentTurn))
						{
							var isPlayer = string.Equals(data.currentTurn, "Player", System.StringComparison.OrdinalIgnoreCase);
							var turnType = isPlayer 
								? Game.CombatSystem.Manager.TurnManager.TurnType.Player 
								: Game.CombatSystem.Manager.TurnManager.TurnType.Enemy;
							
							// 턴 상태 완전 복원
							turnManager.RestoreTurnState(data.turnCount, turnType);
							GameLogger.LogInfo($"[SaveManager] 턴 상태 복원 완료: {data.currentTurn} 턴 (턴 {data.turnCount})", GameLogger.LogCategory.Save);
						}
					}
					catch (System.Exception ex)
					{
						GameLogger.LogWarning($"[SaveManager] 턴 복원 중 경고: {ex.Message}", GameLogger.LogCategory.Save);
					}
				}
				
				// 4. 캐릭터 상태 복원
				await RestoreCharacterStates(data);
				
				// 5. 카드 상태 복원
				await RestoreCardStates(data);
				
				return true;
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"[SaveManager] 매니저 복원 실패: {ex.Message}", GameLogger.LogCategory.Save);
				return false;
			}
		}
		
		/// <summary>
		/// 캐릭터 상태 복원
		/// </summary>
		private async Task RestoreCharacterStates(StageProgressData data)
		{
			// 플레이어 상태 복원
			if (data.playerState != null && data.playerState.IsValid())
			{
				var playerManager = FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
				if (playerManager != null)
				{
					var player = playerManager.GetPlayer();
					if (player != null)
					{
						// HP 차이만큼 Heal/TakeDamage로 복원
						int current = player.GetCurrentHP();
						int target = data.playerState.currentHP;
						int delta = target - current;
						if (delta > 0)
						{
							player.Heal(delta);
						}
						else if (delta < 0)
						{
							player.TakeDamage(-delta);
						}
						player.SetGuarded(data.playerState.isGuarded);
						GameLogger.LogInfo($"[SaveManager] 플레이어 상태 복원: HP {target}, 가드 {data.playerState.isGuarded}", GameLogger.LogCategory.Save);
					}
				}
			}
			
			// 적 상태 복원
			if (data.enemyState != null && data.enemyState.IsValid())
			{
				var enemyManager = FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
				if (enemyManager != null)
				{
					var enemy = enemyManager.GetCurrentEnemy();
					if (enemy != null)
					{
						int current = enemy.GetCurrentHP();
						int target = data.enemyState.currentHP;
						int delta = target - current;
						if (delta > 0)
						{
							enemy.Heal(delta);
						}
						else if (delta < 0)
						{
							enemy.TakeDamage(-delta);
						}
						enemy.SetGuarded(data.enemyState.isGuarded);
						GameLogger.LogInfo($"[SaveManager] 적 상태 복원: HP {target}, 가드 {data.enemyState.isGuarded}", GameLogger.LogCategory.Save);
					}
				}
			}
			
			await Task.CompletedTask;
		}
		
		/// <summary>
		/// 카드 상태 복원
		/// </summary>
		private async Task RestoreCardStates(StageProgressData data)
		{
			// 최소 복원: 저장된 핸드 카드 ID를 로그로 검증하고, 필요 시 팩토리로 생성해 빈 슬롯에 배치
			try
			{
				var slotRegistry = FindFirstObjectByType<Game.CombatSystem.Slot.SlotRegistry>();
				var handRegistry = slotRegistry != null ? slotRegistry.GetHandSlotRegistry() : null;
				var combatRegistry = slotRegistry != null ? slotRegistry.GetCombatSlotRegistry() : null;
				// SkillCardFactory는 UnityEngine.Object가 아니므로 직접 생성
				var cardFactory = new Game.SkillCardSystem.Factory.SkillCardFactory();
				var playerManager = FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
				var handManager = FindFirstObjectByType<Game.SkillCardSystem.Manager.PlayerHandManager>();
                var turnManager = FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();

				if (handRegistry != null && cardFactory != null && playerManager != null && handManager != null)
				{
					// 플레이어 핸드를 비운 뒤 저장된 카드 ID 순서대로 채우는 것은 위험하므로,
					// 현재 빈 슬롯에 한해 저장된 카드 일부를 복구(최소 동작)한다.
					foreach (var cardId in data.playerHandCardIds)
					{
						if (string.IsNullOrEmpty(cardId)) continue;
						var def = cardFactory.LoadDefinition(cardId);
						if (def == null) continue;
						var card = cardFactory.CreatePlayerCard(def, playerManager.GetPlayer()?.GetCharacterName());
						if (card != null)
						{
							handManager.AddCardToHand(card);
						}
					}
					GameLogger.LogInfo($"[SaveManager] 플레이어 핸드 복원: {data.playerHandCardIds.Count}개 시도", GameLogger.LogCategory.Save);
				}

				// 전투 슬롯 복원(확장): 저장된 슬롯/소유자/카드ID에 따라 정확 위치로 배치
				if (combatRegistry != null && cardFactory != null && playerManager != null && turnManager != null)
				{
					var playerName = playerManager.GetPlayer()?.GetCharacterName();
					var cardUIPrefab = FindFirstObjectByType<Game.SkillCardSystem.UI.SkillCardUI>();
					int placed = 0;
					foreach (var slotState in data.combatSlots)
					{
						if (slotState == null || string.IsNullOrEmpty(slotState.position) || string.IsNullOrEmpty(slotState.cardId)) continue;
						if (!System.Enum.TryParse<Game.CombatSystem.Slot.CombatSlotPosition>(slotState.position, out var slotPos)) continue;
						var def = cardFactory.LoadDefinition(slotState.cardId);
						if (def == null) continue;
						var isPlayer = string.Equals(slotState.owner, "Player", System.StringComparison.OrdinalIgnoreCase);
						var card = isPlayer 
							? cardFactory.CreatePlayerCard(def, playerName)
							: cardFactory.CreateEnemyCard(def, FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>()?.GetCurrentEnemy()?.GetCharacterName());
						if (card == null) continue;

						var createdUI = turnManager.GetType()
							.GetMethod("CreateCardUIForSlot", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
							?.Invoke(turnManager, new object[] { card, slotPos, combatRegistry, cardUIPrefab }) as Game.SkillCardSystem.UI.SkillCardUI;

					turnManager.RegisterCard(slotPos, card, createdUI, isPlayer ? Game.CombatSystem.Data.SlotOwner.PLAYER : Game.CombatSystem.Data.SlotOwner.ENEMY);
						placed++;
					}

					GameLogger.LogInfo($"[SaveManager] 전투 슬롯 복원(확장): {placed}개 배치", GameLogger.LogCategory.Save);
				}
			}
			catch (System.Exception ex)
			{
				GameLogger.LogWarning($"[SaveManager] 카드 상태 복원 중 경고: {ex.Message}", GameLogger.LogCategory.Save);
			}
			
			await Task.CompletedTask;
		}
		
		/// <summary>
		/// 스테이지 진행 저장 파일 존재 여부 확인
		/// </summary>
		public bool HasStageProgressSave()
		{
			string filePath = Path.Combine(Application.persistentDataPath, stageProgressFileName);
			bool exists = File.Exists(filePath);
			
			GameLogger.LogInfo($"[SaveManager] 스테이지 진행 저장 파일 확인: {exists}", GameLogger.LogCategory.Save);
			return exists;
		}
		
		/// <summary>
		/// 스테이지 진행 저장 파일 삭제
		/// </summary>
		public void ClearStageProgressSave()
		{
			try
			{
				string filePath = Path.Combine(Application.persistentDataPath, stageProgressFileName);
				
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
					GameLogger.LogInfo("[SaveManager] 스테이지 진행 저장 파일 삭제 완료", GameLogger.LogCategory.Save);
				}
				else
				{
					GameLogger.LogInfo("[SaveManager] 삭제할 스테이지 진행 저장 파일이 없습니다", GameLogger.LogCategory.Save);
				}
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"[SaveManager] 스테이지 진행 저장 파일 삭제 실패: {ex.Message}", GameLogger.LogCategory.Save);
			}
		}
		
		/// <summary>
		/// 현재 진행 상황을 수집하여 저장
		/// </summary>
		public async Task SaveCurrentProgress(string trigger = "Manual")
		{
			if (progressCollector == null)
			{
				GameLogger.LogError("[SaveManager] 진행 상황 수집기가 초기화되지 않았습니다", GameLogger.LogCategory.Save);
				return;
			}
			
			try
			{
				// 현재 진행 상황 수집
				var progressData = progressCollector.CollectCurrentProgress();
				progressData.saveTrigger = trigger;
				
				// 저장
				await SaveStageProgress(progressData);
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"[SaveManager] 현재 진행 상황 저장 실패: {ex.Message}", GameLogger.LogCategory.Save);
			}
		}
		
		#endregion
		
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
