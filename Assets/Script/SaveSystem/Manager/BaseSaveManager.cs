using UnityEngine;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.SaveSystem.Manager
{
    /// <summary>
    /// SaveSystem 매니저들의 공통 베이스 클래스
    /// 인스펙터 필드 표준화 및 공통 기능을 제공합니다.
    /// </summary>
    public abstract class BaseSaveManager<T> : MonoBehaviour 
        where T : class
    {
        #region 기본 설정

        [Header("매니저 기본 설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] protected bool enableDebugLogging = true;

        [Tooltip("자동 초기화 활성화")]
        [SerializeField] protected bool autoInitialize = true;

        [Tooltip("씬 전환 시 유지 여부")]
        [SerializeField] protected bool persistAcrossScenes = true;

        #endregion

        #region 저장 설정

        [Header("저장 설정")]
        [Tooltip("자동 저장 활성화")]
        [SerializeField] protected bool enableAutoSave = true;

        [Tooltip("저장 간격 (초)")]
        [SerializeField] protected float saveInterval = 30f;

        [Tooltip("최대 저장 파일 수")]
        [SerializeField] protected int maxSaveFiles = 10;

        [Tooltip("압축 저장 활성화")]
        [SerializeField] protected bool enableCompression = true;

        #endregion

        #region 데이터 및 설정

        [Header("데이터 및 설정")]
        [Tooltip("저장 설정 데이터")]
        [SerializeField] protected ScriptableObject saveConfig;

        [Tooltip("저장 파일 경로")]
        [SerializeField] protected string saveFilePath = "SaveData/";

        [Tooltip("백업 파일 경로")]
        [SerializeField] protected string backupFilePath = "Backup/";

        #endregion

        #region UI 연결

        [Header("UI 연결")]
        [Tooltip("저장 UI 컨트롤러")]
        [SerializeField] protected MonoBehaviour saveUIController;

        [Tooltip("로딩 UI 컨트롤러")]
        [SerializeField] protected MonoBehaviour loadUIController;

        [Tooltip("저장 진행률 표시")]
        [SerializeField] protected UnityEngine.UI.Slider progressSlider;

        #endregion

        #region 의존성 및 서비스

        [Header("의존성 및 서비스")]
        [Tooltip("데이터 수집기")]
        [SerializeField] protected MonoBehaviour dataCollector;

        [Tooltip("데이터 복원기")]
        [SerializeField] protected MonoBehaviour dataRestorer;

        [Tooltip("이벤트 트리거")]
        [SerializeField] protected MonoBehaviour eventTrigger;

        #endregion

        #region 초기화 상태

        public bool IsInitialized { get; protected set; } = false;

        #endregion

        #region 공통 초기화

        protected virtual void Awake()
        {
            if (persistAcrossScenes && transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else if (persistAcrossScenes)
            {
                GameLogger.LogWarning("루트 오브젝트가 아니므로 DontDestroyOnLoad를 적용할 수 없습니다.", GameLogger.LogCategory.Save);
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화 시작", GameLogger.LogCategory.Save);
            }
        }

        protected virtual void Start()
        {
            if (autoInitialize)
            {
                StartCoroutine(Initialize());
            }
        }

        #endregion

        #region 초기화

        public virtual System.Collections.IEnumerator Initialize()
        {
            if (IsInitialized)
            {
                yield break;
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화 중...", GameLogger.LogCategory.Save);
            }

            // 서브클래스에서 구현할 초기화 로직
            yield return StartCoroutine(OnInitialize());

            // 저장 시스템 설정
            SetupSaveSystem();

            IsInitialized = true;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화 완료", GameLogger.LogCategory.Save);
            }
        }

        public virtual void OnInitializationFailed()
        {
            GameLogger.LogError($"{GetType().Name} 초기화 실패", GameLogger.LogCategory.Error);
            IsInitialized = false;
        }

        #endregion

        #region 추상 메서드

        /// <summary>
        /// 서브클래스에서 구현할 초기화 로직
        /// </summary>
        protected abstract System.Collections.IEnumerator OnInitialize();

        /// <summary>
        /// 매니저 리셋 로직
        /// </summary>
        public abstract void Reset();

        #endregion

        #region 저장 시스템 설정

        /// <summary>
        /// 저장 시스템을 설정합니다.
        /// </summary>
        protected virtual void SetupSaveSystem()
        {
            // 저장 경로 설정
            if (string.IsNullOrEmpty(saveFilePath))
            {
                saveFilePath = Application.persistentDataPath + "/SaveData/";
            }

            if (string.IsNullOrEmpty(backupFilePath))
            {
                backupFilePath = Application.persistentDataPath + "/Backup/";
            }

            // 디렉토리 생성
            if (!System.IO.Directory.Exists(saveFilePath))
            {
                System.IO.Directory.CreateDirectory(saveFilePath);
            }

            if (!System.IO.Directory.Exists(backupFilePath))
            {
                System.IO.Directory.CreateDirectory(backupFilePath);
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name}: 저장 시스템 설정 완료", GameLogger.LogCategory.Save);
            }
        }

        #endregion

        #region 공통 유틸리티

        /// <summary>
        /// 필수 참조 필드의 유효성을 검사합니다.
        /// </summary>
        protected virtual bool ValidateReferences()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(saveFilePath))
            {
                GameLogger.LogWarning($"{GetType().Name}: 저장 파일 경로가 설정되지 않았습니다.", GameLogger.LogCategory.Save);
            }

            if (string.IsNullOrEmpty(backupFilePath))
            {
                GameLogger.LogWarning($"{GetType().Name}: 백업 파일 경로가 설정되지 않았습니다.", GameLogger.LogCategory.Save);
            }

            if (maxSaveFiles <= 0)
            {
                GameLogger.LogError($"{GetType().Name}: 최대 저장 파일 수가 0 이하입니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            if (saveInterval <= 0)
            {
                GameLogger.LogError($"{GetType().Name}: 저장 간격이 0 이하입니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// 저장 UI를 연결합니다.
        /// </summary>
        protected virtual void ConnectSaveUI()
        {
            if (saveUIController != null)
            {
                GameLogger.LogInfo($"{GetType().Name}: 저장 UI 컨트롤러 연결 - {saveUIController.GetType().Name}", GameLogger.LogCategory.Save);
            }

            if (loadUIController != null)
            {
                GameLogger.LogInfo($"{GetType().Name}: 로딩 UI 컨트롤러 연결 - {loadUIController.GetType().Name}", GameLogger.LogCategory.Save);
            }
        }

        /// <summary>
        /// 매니저 상태를 로깅합니다.
        /// </summary>
        protected virtual void LogManagerState()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 상태: 초기화={IsInitialized}, 디버그={enableDebugLogging}, 자동초기화={autoInitialize}, 자동저장={enableAutoSave}, 간격={saveInterval}초", GameLogger.LogCategory.Save);
            }
        }

        /// <summary>
        /// 저장 파일 경로를 생성합니다.
        /// </summary>
        protected virtual string GenerateSaveFilePath(string fileName)
        {
            return System.IO.Path.Combine(saveFilePath, fileName);
        }

        /// <summary>
        /// 백업 파일 경로를 생성합니다.
        /// </summary>
        protected virtual string GenerateBackupFilePath(string fileName)
        {
            return System.IO.Path.Combine(backupFilePath, fileName);
        }

        /// <summary>
        /// 저장 진행률을 업데이트합니다.
        /// </summary>
        protected virtual void UpdateProgress(float progress)
        {
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }
        }

        #endregion

        #region Unity 생명주기

        protected virtual void OnDestroy()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 파괴됨", GameLogger.LogCategory.Save);
            }
        }

        #endregion
    }
}
