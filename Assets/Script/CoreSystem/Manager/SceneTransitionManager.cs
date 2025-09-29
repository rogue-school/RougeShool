using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections;
using Game.UtilitySystem.GameFlow;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Audio;
using Game.StageSystem.Manager;
using Zenject;

namespace Game.CoreSystem.Manager
{
	/// <summary>
	/// 3개 씬 구조를 위한 씬 전환 매니저 (Zenject DI 기반)
	/// </summary>
	public class SceneTransitionManager : BaseCoreManager<ISceneTransitionManager>, ISceneLoader, ISceneTransitionManager
	{
		
		#region SceneTransitionManager 전용 설정
		
		[Header("씬 설정")]
		[Tooltip("코어 씬 이름")]
		[SerializeField] private string coreSceneName = "CoreScene";
		[Tooltip("메인 씬 이름")]
		[SerializeField] private string mainSceneName = "MainScene";
		[Tooltip("전투 씬 이름")]
		[SerializeField] private string battleSceneName = "BattleScene";
		[Tooltip("스테이지 씬 이름")]
		[SerializeField] private string stageSceneName = "StageScene";
		
		[Header("전환 설정")]
		[Tooltip("전환 지속 시간")]
		[SerializeField] private float transitionDuration = 0.1f;
		[Tooltip("전환 커브")]
		[SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
		
		[Header("UI 참조")]
		[Tooltip("전환 캔버스 그룹")]
		[SerializeField] private CanvasGroup transitionCanvas;
		[Tooltip("전환 이미지")]
		[SerializeField] private UnityEngine.UI.Image transitionImage;
		[Tooltip("로딩 텍스트")]
		[SerializeField] private UnityEngine.UI.Text loadingText;
		
		#endregion
		
		// 초기화 상태는 베이스 클래스에서 관리
		public bool IsTransitioning { get; private set; } = false; // 전환 상태 추가
		
		// 이벤트
		public System.Action<string> OnSceneTransitionStart { get; set; }
		public System.Action<string> OnSceneTransitionEnd { get; set; }

		private AudioEventTrigger cachedAudioEventTrigger;
		
		// 의존성 주입
		private IAudioManager audioManager;
		
		[Inject]
		public void Construct(IAudioManager audioManager)
		{
			this.audioManager = audioManager;
		}
		
		protected override void Awake()
		{
			base.Awake();
			InitializeTransition();
		}
		
		protected override void Start()
		{
			base.Start();
			// 씬 전환 상태 초기화 (씬 로드 후 상태 리셋)
			IsTransitioning = false;
		}
		
		/// <summary>
		/// 전환 시스템 초기화
		/// </summary>
		private void InitializeTransition()
		{
			// 전환 캔버스 설정
			if (transitionCanvas == null)
			{
				var canvasObj = new GameObject("TransitionCanvas");
				canvasObj.transform.SetParent(transform);
				
				var canvas = canvasObj.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvas.sortingOrder = 1000; // 최상위 레이어
				
				transitionCanvas = canvasObj.AddComponent<CanvasGroup>();
				transitionCanvas.alpha = 0f;
				transitionCanvas.interactable = false;
				transitionCanvas.blocksRaycasts = false;
			}
			
			Debug.Log("[SceneTransitionManager] 초기화 완료");
		}
		
		/// <summary>
		/// 코어 씬으로 전환
		/// </summary>
		public async Task TransitionToCoreScene()
		{
			await TransitionToScene(coreSceneName, TransitionType.Fade);
		}
		
		/// <summary>
		/// 메인 씬으로 전환
		/// </summary>
		public async Task TransitionToMainScene()
		{
			// 현재 씬이 StageScene이면 진행 상황 저장
			if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == stageSceneName)
			{
				await SaveProgressFromStageScene();
			}
			
			// 스테이지 매니저가 있으면 스테이지 BGM 정리
			var stageManager = FindFirstObjectByType<StageManager>();
			if (stageManager != null)
			{
				stageManager.CleanupStageBGM();
			}
			
			// 기존 BGM 정지 (추가 안전장치)
			StopCurrentBGM();
			
			await TransitionToScene(mainSceneName, TransitionType.Fade);
			TryPlayBGMForScene(mainSceneName);
		}
		
		/// <summary>
		/// 전투 씬으로 전환
		/// </summary>
		public async Task TransitionToBattleScene()
		{
			await TransitionToScene(battleSceneName, TransitionType.Fade);
			TryPlayBGMForScene(battleSceneName);
		}

		/// <summary>
		/// 스테이지 씬으로 전환
		/// </summary>
		public async Task TransitionToStageScene()
		{
			await TransitionToScene(stageSceneName, TransitionType.Fade);
			TryPlayBGMForScene(stageSceneName);
		}
		
		/// <summary>
		/// 지정된 씬으로 전환
		/// </summary>
		public async Task TransitionToScene(string sceneName)
		{
			await TransitionToScene(sceneName, TransitionType.Fade);
		}
		
		/// <summary>
		/// 씬 전환 실행 (페이드 효과 비활성화)
		/// </summary>
		public async Task TransitionToScene(string sceneName, TransitionType transitionType)
		{
			// 중복 호출 방지
			if (IsTransitioning)
			{
				Debug.LogWarning($"[SceneTransitionManager] 이미 씬 전환이 진행 중입니다. 요청 무시: {sceneName}");
				return;
			}
			
			Debug.Log($"[SceneTransitionManager] 씬 전환 시작: {sceneName}");
			
			IsTransitioning = true; // 전환 시작
			OnSceneTransitionStart?.Invoke(sceneName);
			
			try
			{
				// 페이드 효과 비활성화 - 바로 씬 로딩
				await LoadSceneAsync(sceneName);
				
				OnSceneTransitionEnd?.Invoke(sceneName);
			}
			finally
			{
				// 전환 상태를 반드시 리셋
				IsTransitioning = false;
				Debug.Log($"[SceneTransitionManager] 씬 전환 완료: {sceneName}");
			}
		}
		
		/// <summary>
		/// 현재 재생 중인 BGM 정지
		/// </summary>
		private void StopCurrentBGM()
		{
			if (audioManager != null)
			{
				audioManager.StopBGM();
				Debug.Log("[SceneTransitionManager] 현재 BGM 정지 완료");
			}
		}
		
		/// <summary>
		/// BGM 트리거: 씬 이름 기준으로 적절한 BGM 재생 시도
		/// </summary>
		private void TryPlayBGMForScene(string sceneName)
		{
			if (string.IsNullOrEmpty(sceneName)) return;
			if (cachedAudioEventTrigger == null)
			{
				cachedAudioEventTrigger = FindFirstObjectByType<AudioEventTrigger>();
			}
			if (cachedAudioEventTrigger == null) return;
			
			switch (sceneName)
			{
				case var s when s == mainSceneName:
					cachedAudioEventTrigger.OnMainMenuBGM();
					break;
				case var s when s == battleSceneName:
					cachedAudioEventTrigger.OnBattleBGM();
					break;
				case var s when s == stageSceneName:
					cachedAudioEventTrigger.OnMainMenuBGM();
					break;
				default:
					// 리소스 경로 규칙이 있을 경우 OnSceneBGM으로 대체 가능
					break;
			}
		}
		
		/// <summary>
		/// 비동기 씬 로딩
		/// </summary>
		private async Task LoadSceneAsync(string sceneName)
		{
			var operation = SceneManager.LoadSceneAsync(sceneName);
			operation.allowSceneActivation = false;
			
			while (operation.progress < 0.9f)
			{
				// 로딩 진행률 표시
				float progress = operation.progress / 0.9f;
				UpdateLoadingProgress(progress);
				
				await Task.Yield();
			}
			
			// 씬 활성화
			operation.allowSceneActivation = true;
			
			// 씬 로딩 완료 대기
			while (!operation.isDone)
			{
				await Task.Yield();
			}
		}
		
		/// <summary>
		/// 로딩 진행률 업데이트
		/// </summary>
		private void UpdateLoadingProgress(float progress)
		{
			if (loadingText != null)
			{
				loadingText.text = $"로딩 중... {Mathf.RoundToInt(progress * 100)}%";
			}
		}
		
		/// <summary>
		/// 전환 시작 (페이드 아웃 - 화면을 어둡게)
		/// </summary>
		private async Task StartTransition(TransitionType transitionType)
		{
			if (transitionCanvas != null)
			{
				// 캔버스가 비활성화되어 있다면 활성화
				if (!transitionCanvas.gameObject.activeInHierarchy)
				{
					transitionCanvas.gameObject.SetActive(true);
				}
				transitionCanvas.blocksRaycasts = true;
				transitionCanvas.interactable = true;
			}
			
			switch (transitionType)
			{
				case TransitionType.Fade:
					await FadeTransition(false); // false = 페이드 아웃 (화면을 어둡게)
					break;
				case TransitionType.Slide:
					await SlideTransition(false); // false = 슬라이드 아웃
					break;
			}
		}
		
		/// <summary>
		/// 전환 종료 (페이드 인 - 화면을 밝게)
		/// </summary>
		private async Task EndTransition(TransitionType transitionType)
		{
			switch (transitionType)
			{
				case TransitionType.Fade:
					await FadeTransition(true); // true = 페이드 인 (화면을 밝게)
					break;
				case TransitionType.Slide:
					await SlideTransition(true); // true = 슬라이드 인
					break;
			}
			
			if (transitionCanvas != null)
			{
				// 캔버스를 비활성화하지 말고 알파값만 0으로 설정
				transitionCanvas.alpha = 0f;
				transitionCanvas.blocksRaycasts = false;
				transitionCanvas.interactable = false;
			}
		}
		
		private async Task FadeTransition(bool fadeIn)
		{
			if (transitionCanvas == null) return;
			
			float startAlpha = fadeIn ? 0f : 1f; // fadeIn이면 투명에서 시작, 아니면 불투명에서 시작
			float endAlpha = fadeIn ? 1f : 0f;   // fadeIn이면 불투명으로 끝, 아니면 투명으로 끝
			
			transitionCanvas.alpha = startAlpha;
			
			float elapsedTime = 0f;
			while (elapsedTime < transitionDuration)
			{
				elapsedTime += Time.deltaTime;
				float progress = elapsedTime / transitionDuration;
				float curveValue = transitionCurve.Evaluate(progress);
				
				transitionCanvas.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
				
				await Task.Yield();
			}
			
			transitionCanvas.alpha = endAlpha;
		}
		
		private async Task SlideTransition(bool slideIn)
		{
			// 슬라이드 전환 로직 구현 (필요시)
			await Task.Delay(Mathf.RoundToInt(transitionDuration * 1000));
		}
		
		public void LoadScene(string sceneName)
		{
			// 동기적으로 씬 로드 (기존 SceneLoader와 호환성을 위해)
			SceneManager.LoadScene(sceneName);
		}
		
		#region 데이터 보존 기능
		
		/// <summary>
		/// 씬 전환 전 현재 진행 상황 저장
		/// </summary>
		private async Task SaveCurrentProgressBeforeTransition()
		{
			try
			{
				var saveManager = FindFirstObjectByType<Game.CoreSystem.Save.SaveManager>();
				if (saveManager != null)
				{
					await saveManager.SaveCurrentProgress("SceneTransition");
					GameLogger.LogInfo("[SceneTransitionManager] 씬 전환 전 진행 상황 저장 완료", GameLogger.LogCategory.Save);
				}
				else
				{
					GameLogger.LogWarning("[SceneTransitionManager] SaveManager를 찾을 수 없습니다", GameLogger.LogCategory.Save);
				}
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"[SceneTransitionManager] 전환 전 저장 실패: {ex.Message}", GameLogger.LogCategory.Error);
			}
		}
		
		/// <summary>
		/// 씬 전환 후 저장된 진행 상황 로드
		/// </summary>
		private async Task LoadProgressAfterTransition()
		{
			try
			{
				// 씬 로드 완료 후 잠시 대기 (매니저들이 초기화될 시간 확보)
				await Task.Delay(100);
				
				var saveManager = FindFirstObjectByType<Game.CoreSystem.Save.SaveManager>();
				if (saveManager != null && saveManager.HasStageProgressSave())
				{
					bool loadSuccess = await saveManager.LoadStageProgress();
					if (loadSuccess)
					{
						GameLogger.LogInfo("[SceneTransitionManager] 씬 전환 후 진행 상황 로드 완료", GameLogger.LogCategory.Save);
					}
					else
					{
						GameLogger.LogWarning("[SceneTransitionManager] 진행 상황 로드 실패", GameLogger.LogCategory.Save);
					}
				}
				else
				{
					GameLogger.LogInfo("[SceneTransitionManager] 저장된 진행 상황이 없습니다", GameLogger.LogCategory.Save);
				}
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"[SceneTransitionManager] 전환 후 로드 실패: {ex.Message}", GameLogger.LogCategory.Error);
			}
		}
		
		/// <summary>
		/// StageScene에서 다른 씬으로 전환할 때 진행 상황 저장
		/// </summary>
		private async Task SaveProgressFromStageScene()
		{
			try
			{
				var stageManager = FindFirstObjectByType<StageManager>();
				if (stageManager != null)
				{
					await stageManager.SaveProgressBeforeSceneTransition();
				}
				else
				{
					GameLogger.LogWarning("[SceneTransitionManager] StageManager를 찾을 수 없습니다", GameLogger.LogCategory.Save);
				}
			}
			catch (System.Exception ex)
			{
				GameLogger.LogError($"[SceneTransitionManager] StageScene에서 저장 실패: {ex.Message}", GameLogger.LogCategory.Error);
			}
		}
		
		#endregion
		
		#region 베이스 클래스 구현

		protected override System.Collections.IEnumerator OnInitialize()
		{
			// 초기 상태 설정
			if (transitionCanvas != null)
			{
				transitionCanvas.alpha = 0f;
				transitionCanvas.interactable = false;
				transitionCanvas.blocksRaycasts = false;
			}
			
			// UI 연결
			ConnectUI();
			
			// 참조 검증
			ValidateReferences();
			
			yield return null;
		}

		public override void Reset()
		{
			IsTransitioning = false;
			
			if (transitionCanvas != null)
			{
				transitionCanvas.alpha = 0f;
				transitionCanvas.interactable = false;
				transitionCanvas.blocksRaycasts = false;
			}
			
			if (enableDebugLogging)
			{
				GameLogger.LogInfo("SceneTransitionManager 리셋 완료", GameLogger.LogCategory.UI);
			}
		}

		#endregion
	}
}
