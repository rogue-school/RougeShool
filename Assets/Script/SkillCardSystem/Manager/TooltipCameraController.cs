using UnityEngine;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 툴팁 표시 시 카메라 전환을 관리하는 컨트롤러
    /// 이펙트 카메라 우선 시스템을 구현합니다.
    /// </summary>
    public class TooltipCameraController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("카메라 설정")]
        [Tooltip("메인 카메라 (기본 게임 뷰)")]
        [SerializeField] private Camera mainCamera;
        
        [Tooltip("이펙트 카메라 (이펙트 전용)")]
        [SerializeField] private Camera effectsCamera;
        
        [Tooltip("카메라 전환 애니메이션 시간")]
        [SerializeField] private float transitionDuration = 0.2f;

        #endregion

        #region Private Fields

        private Camera originalActiveCamera;
        private bool isTransitioning = false;
        private bool isTooltipMode = false;
        
        // 오디오 리스너 관리
        private AudioListener mainAudioListener;
        private AudioListener effectsAudioListener;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeCameras();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 카메라들을 초기화합니다.
        /// </summary>
        private void InitializeCameras()
        {
            // 메인 카메라 자동 찾기
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = FindFirstObjectByType<Camera>();
                }
            }

            // 이펙트 카메라 자동 찾기 (여러 방법으로 시도)
            if (effectsCamera == null)
            {
                // 방법 1: 이름으로 찾기
                var effectsCameraGO = GameObject.Find("EffectsCamera");
                if (effectsCameraGO != null)
                {
                    effectsCamera = effectsCameraGO.GetComponent<Camera>();
                }

                // 방법 2: 모든 카메라 중에서 메인 카메라가 아닌 것 찾기
                if (effectsCamera == null)
                {
                    var allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                    foreach (var cam in allCameras)
                    {
                        if (cam != mainCamera && cam.gameObject.name.ToLower().Contains("effect"))
                        {
                            effectsCamera = cam;
                            break;
                        }
                    }
                }

                // 방법 3: 모든 카메라 중에서 메인 카메라가 아닌 것 찾기 (더 넓은 범위)
                if (effectsCamera == null)
                {
                    var allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                    foreach (var cam in allCameras)
                    {
                        if (cam != mainCamera && !cam.gameObject.name.ToLower().Contains("main"))
                        {
                            effectsCamera = cam;
                            GameLogger.LogInfo($"[TooltipCameraController] 대체 카메라 발견: {cam.name}", GameLogger.LogCategory.UI);
                            break;
                        }
                    }
                }
            }

            // 오디오 리스너 초기화
            InitializeAudioListeners();

            // 초기 상태 저장
            originalActiveCamera = mainCamera;

            // 안전한 로깅
            string mainCameraName = mainCamera != null ? mainCamera.name : "null";
            string effectsCameraName = effectsCamera != null ? effectsCamera.name : "null";
            
            GameLogger.LogInfo($"[TooltipCameraController] 카메라 초기화 완료 - Main: {mainCameraName}, Effects: {effectsCameraName}", GameLogger.LogCategory.UI);
            
            if (effectsCamera == null)
            {
                GameLogger.LogWarning("[TooltipCameraController] 이펙트 카메라를 찾을 수 없습니다. 툴팁 모드에서 메인 카메라를 유지합니다.", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 오디오 리스너들을 초기화합니다.
        /// </summary>
        private void InitializeAudioListeners()
        {
            // 메인 카메라의 오디오 리스너
            if (mainCamera != null)
            {
                mainAudioListener = mainCamera.GetComponent<AudioListener>();
                if (mainAudioListener == null)
                {
                    mainAudioListener = mainCamera.gameObject.AddComponent<AudioListener>();
                    GameLogger.LogInfo("[TooltipCameraController] 메인 카메라에 AudioListener 추가", GameLogger.LogCategory.UI);
                }
                
                // 메인 카메라에 권장 컴포넌트 추가
                EnsureRequiredComponents(mainCamera, "메인 카메라");
            }

            // 이펙트 카메라의 오디오 리스너 (있다면 비활성화)
            if (effectsCamera != null)
            {
                effectsAudioListener = effectsCamera.GetComponent<AudioListener>();
                if (effectsAudioListener != null)
                {
                    // 이펙트 카메라의 오디오 리스너는 비활성화 (중복 방지)
                    effectsAudioListener.enabled = false;
                    GameLogger.LogInfo("[TooltipCameraController] 이펙트 카메라의 AudioListener 비활성화", GameLogger.LogCategory.UI);
                }
                
                // 이펙트 카메라에 권장 컴포넌트 추가
                EnsureRequiredComponents(effectsCamera, "이펙트 카메라");
            }

            // 메인 카메라의 오디오 리스너 활성화
            if (mainAudioListener != null)
            {
                mainAudioListener.enabled = true;
                GameLogger.LogInfo("[TooltipCameraController] 메인 카메라의 AudioListener 활성화", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 카메라에 필요한 컴포넌트들을 확인하고 추가합니다.
        /// </summary>
        /// <param name="camera">설정할 카메라</param>
        /// <param name="cameraName">카메라 이름 (로깅용)</param>
        private void EnsureRequiredComponents(Camera camera, string cameraName)
        {
            if (camera == null) return;

            // Flare Layer 컴포넌트 확인 및 추가
            if (camera.GetComponent<FlareLayer>() == null)
            {
                camera.gameObject.AddComponent<FlareLayer>();
                GameLogger.LogInfo($"[TooltipCameraController] {cameraName}에 FlareLayer 추가", GameLogger.LogCategory.UI);
            }

            // GUILayer는 Unity에서 제거됨 - 더 이상 필요하지 않음
            // UI는 Canvas 시스템으로 처리됨
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 툴팁 표시 모드로 전환합니다.
        /// 이펙트 카메라를 활성화하여 이펙트를 볼 수 있게 합니다.
        /// </summary>
        public void EnterTooltipMode()
        {
            if (isTooltipMode || isTransitioning) return;

            GameLogger.LogInfo("[TooltipCameraController] 툴팁 모드 진입", GameLogger.LogCategory.UI);

            isTooltipMode = true;
            StartCoroutine(TransitionToEffectsCamera());
        }

        /// <summary>
        /// 툴팁 숨김 모드로 전환합니다.
        /// 메인 카메라로 복원합니다.
        /// </summary>
        public void ExitTooltipMode()
        {
            if (!isTooltipMode || isTransitioning) return;

            GameLogger.LogInfo("[TooltipCameraController] 툴팁 모드 종료", GameLogger.LogCategory.UI);

            isTooltipMode = false;
            StartCoroutine(TransitionToMainCamera());
        }

        /// <summary>
        /// 현재 툴팁 모드 상태를 반환합니다.
        /// </summary>
        public bool IsTooltipMode => isTooltipMode;

        #endregion

        #region Camera Transitions

        /// <summary>
        /// 이펙트 카메라로 전환합니다.
        /// </summary>
        private System.Collections.IEnumerator TransitionToEffectsCamera()
        {
            if (effectsCamera == null)
            {
                GameLogger.LogWarning("[TooltipCameraController] 이펙트 카메라가 없습니다. 메인 카메라 유지", GameLogger.LogCategory.UI);
                yield break;
            }

            isTransitioning = true;
            GameLogger.LogInfo("[TooltipCameraController] 이펙트 카메라로 전환 시작", GameLogger.LogCategory.UI);

            // 이펙트 카메라 활성화 및 우선순위 설정
            if (effectsCamera != null)
            {
                effectsCamera.enabled = true;
                effectsCamera.depth = 1; // 이펙트 카메라를 앞으로
                GameLogger.LogInfo("[TooltipCameraController] 이펙트 카메라 활성화 및 우선순위 설정", GameLogger.LogCategory.UI);
            }

            // 메인 카메라 우선순위 조정 (비활성화하지 않음)
            if (mainCamera != null)
            {
                mainCamera.depth = 0; // 메인 카메라를 뒤로
                GameLogger.LogInfo("[TooltipCameraController] 메인 카메라 우선순위 조정", GameLogger.LogCategory.UI);
            }

            // 오디오 리스너 전환: 메인 → 이펙트 (이펙트 카메라에 오디오 리스너가 있는 경우)
            if (effectsAudioListener != null)
            {
                if (mainAudioListener != null)
                    mainAudioListener.enabled = false;
                effectsAudioListener.enabled = true;
                GameLogger.LogInfo("[TooltipCameraController] 오디오 리스너를 이펙트 카메라로 전환", GameLogger.LogCategory.UI);
            }

            // 전환 완료 대기
            yield return new WaitForSeconds(transitionDuration);

            isTransitioning = false;
            GameLogger.LogInfo("[TooltipCameraController] 이펙트 카메라 전환 완료", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 메인 카메라로 전환합니다.
        /// </summary>
        private System.Collections.IEnumerator TransitionToMainCamera()
        {
            if (mainCamera == null)
            {
                GameLogger.LogWarning("[TooltipCameraController] 메인 카메라가 없습니다", GameLogger.LogCategory.UI);
                yield break;
            }

            isTransitioning = true;
            GameLogger.LogInfo("[TooltipCameraController] 메인 카메라로 복원 시작", GameLogger.LogCategory.UI);

            // 메인 카메라 우선순위 복원
            if (mainCamera != null)
            {
                mainCamera.depth = 1; // 메인 카메라를 앞으로
                GameLogger.LogInfo("[TooltipCameraController] 메인 카메라 우선순위 복원", GameLogger.LogCategory.UI);
            }

            // 이펙트 카메라 우선순위 조정
            if (effectsCamera != null)
            {
                effectsCamera.depth = 0; // 이펙트 카메라를 뒤로
                GameLogger.LogInfo("[TooltipCameraController] 이펙트 카메라 우선순위 조정", GameLogger.LogCategory.UI);
            }

            // 오디오 리스너 전환: 이펙트 → 메인
            if (effectsAudioListener != null)
            {
                effectsAudioListener.enabled = false;
            }
            if (mainAudioListener != null)
            {
                mainAudioListener.enabled = true;
                GameLogger.LogInfo("[TooltipCameraController] 오디오 리스너를 메인 카메라로 복원", GameLogger.LogCategory.UI);
            }

            // 전환 완료 대기
            yield return new WaitForSeconds(transitionDuration);

            isTransitioning = false;
            GameLogger.LogInfo("[TooltipCameraController] 메인 카메라 복원 완료", GameLogger.LogCategory.UI);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 현재 활성화된 카메라를 반환합니다.
        /// </summary>
        public Camera GetActiveCamera()
        {
            if (isTooltipMode && effectsCamera != null && effectsCamera.enabled)
            {
                return effectsCamera;
            }
            return mainCamera;
        }

        /// <summary>
        /// 카메라 상태를 디버깅합니다.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugCameraState()
        {
            GameLogger.LogInfo($"[TooltipCameraController] 카메라 상태:", GameLogger.LogCategory.UI);
            GameLogger.LogInfo($"  - isTooltipMode: {isTooltipMode}", GameLogger.LogCategory.UI);
            GameLogger.LogInfo($"  - isTransitioning: {isTransitioning}", GameLogger.LogCategory.UI);
            GameLogger.LogInfo($"  - mainCamera.enabled: {mainCamera?.enabled}", GameLogger.LogCategory.UI);
            GameLogger.LogInfo($"  - effectsCamera.enabled: {effectsCamera?.enabled}", GameLogger.LogCategory.UI);
            GameLogger.LogInfo($"  - mainAudioListener.enabled: {mainAudioListener?.enabled}", GameLogger.LogCategory.UI);
            GameLogger.LogInfo($"  - effectsAudioListener.enabled: {effectsAudioListener?.enabled}", GameLogger.LogCategory.UI);
        }

        #endregion
    }
}
