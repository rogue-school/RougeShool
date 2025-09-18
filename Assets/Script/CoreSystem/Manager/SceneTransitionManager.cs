using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections;
using Game.UtilitySystem.GameFlow;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Audio;
using Zenject;

namespace Game.CoreSystem.Manager
{
	/// <summary>
	/// 3개 씬 구조를 위한 씬 전환 매니저 (Zenject DI 기반)
	/// </summary>
	public class SceneTransitionManager : MonoBehaviour, ISceneLoader, ICoreSystemInitializable, ISceneTransitionManager
	{
		
		[Header("씬 설정")]
		[SerializeField] private string coreSceneName = "CoreScene";
		[SerializeField] private string mainSceneName = "MainScene";
		[SerializeField] private string battleSceneName = "BattleScene";
		[SerializeField] private string stageSceneName = "StageScene";
		
		[Header("전환 설정")]
		[SerializeField] private float transitionDuration = 0.1f; // 0.1초로 더 단축
		[SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
		
		[Header("UI 참조")]
		[SerializeField] private CanvasGroup transitionCanvas;
		[SerializeField] private UnityEngine.UI.Image transitionImage;
		
		// 초기화 상태
		public bool IsInitialized { get; private set; } = false;
		public bool IsTransitioning { get; private set; } = false; // 전환 상태 추가
		[SerializeField] private UnityEngine.UI.Text loadingText;
		
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
		
		private void Awake()
		{
			InitializeTransition();
		}
		
		private void Start()
		{
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
		
		#region ICoreSystemInitializable 구현
		public IEnumerator Initialize()
		{
			GameLogger.LogInfo("SceneTransitionManager 초기화 시작", GameLogger.LogCategory.UI);
			
			// 초기 상태 설정
			if (transitionCanvas != null)
			{
				transitionCanvas.alpha = 0f;
				transitionCanvas.interactable = false;
				transitionCanvas.blocksRaycasts = false;
			}
			
			// 초기화 완료
			IsInitialized = true;
			
			GameLogger.LogInfo("SceneTransitionManager 초기화 완료", GameLogger.LogCategory.UI);
			yield return null;
		}
		
		public void OnInitializationFailed()
		{
			GameLogger.LogError("SceneTransitionManager 초기화 실패", GameLogger.LogCategory.Error);
			IsInitialized = false;
		}
		#endregion
	}
}
