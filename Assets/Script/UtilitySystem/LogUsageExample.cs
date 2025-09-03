using UnityEngine;
using Game.Utility;

namespace Game.Utility
{
    /// <summary>
    /// GameLogger와 PerformanceProfiler 사용 예시
    /// 실제 프로젝트에서 어떻게 사용하는지 보여주는 샘플
    /// </summary>
    public class LogUsageExample : MonoBehaviour
    {
        [Header("로그 제어")]
        [SerializeField] private bool enablePerformanceLogging = true;
        [SerializeField] private bool enableVerboseLogging = false;

        private void Start()
        {
            // 로그 레벨 설정
            GameLogger.SetLogLevel(GameLogger.LogLevel.Debug);
            
            // 성능 로깅 활성화
            if (enablePerformanceLogging)
            {
                GameLogger.ToggleLogCategory(GameLogger.LogCategory.Performance, true);
            }
            
            // 상세 로깅 활성화
            if (enableVerboseLogging)
            {
                GameLogger.ToggleLogCategory(GameLogger.LogCategory.AnimationVerbose, true);
            }

            // 예시 로그 출력
            DemonstrateLogging();
        }

        private void Update()
        {
            // 프레임 시간 측정
            PerformanceProfiler.UpdateFrameTime();
            
            // 성능 경고 체크 (1초마다)
            if (Time.frameCount % 60 == 0)
            {
                PerformanceProfiler.CheckMemoryWarning(500); // 500MB 임계값
            }
        }

        private void DemonstrateLogging()
        {
            // 기본 로그 사용법
            GameLogger.LogCombat("전투 시스템 초기화 시작");
            GameLogger.LogAnimation("애니메이션 시스템 초기화");
            GameLogger.LogSlot("슬롯 시스템 초기화");
            GameLogger.LogCharacter("캐릭터 시스템 초기화");
            GameLogger.LogSkillCard("스킬카드 시스템 초기화");
            GameLogger.LogDatabase("데이터베이스 로드");
            GameLogger.LogUI("UI 시스템 초기화");
            GameLogger.LogAudio("오디오 시스템 초기화");

            // 상세 로그 (에디터에서만)
            GameLogger.LogAnimationVerbose("상세 애니메이션 정보");

            // 성능 측정 예시
            PerformanceProfiler.StartMeasurement("초기화 과정");
            
            // 시뮬레이션된 작업
            System.Threading.Thread.Sleep(100);
            
            PerformanceProfiler.EndMeasurement("초기화 과정");

            // 메모리 사용량 로그
            PerformanceProfiler.LogMemoryUsage("초기화 후");

            // 성능 통계 출력
            PerformanceProfiler.PrintPerformanceStats();
        }

        /// <summary>
        /// 메서드 성능 측정 예시
        /// </summary>
        public void ExampleMethodProfiling()
        {
            // 방법 1: 직접 측정
            PerformanceProfiler.StartMeasurement("복잡한 계산");
            
            // 시뮬레이션된 복잡한 계산
            float result = 0f;
            for (int i = 0; i < 1000000; i++)
            {
                result += Mathf.Sin(i * 0.01f);
            }
            
            PerformanceProfiler.EndMeasurement("복잡한 계산");

            // 방법 2: 래퍼 사용
            PerformanceProfiler.ProfileMethod("간단한 계산", () =>
            {
                // 시뮬레이션된 간단한 계산
                for (int i = 0; i < 100000; i++)
                {
                    result += Mathf.Cos(i * 0.01f);
                }
            });

            GameLogger.LogPerformance($"계산 결과: {result}");
        }

        /// <summary>
        /// 코루틴 성능 측정 예시
        /// </summary>
        public System.Collections.IEnumerator ExampleCoroutineProfiling()
        {
            return PerformanceProfiler.ProfileCoroutine("비동기 작업", AsyncWork());
        }

        private System.Collections.IEnumerator AsyncWork()
        {
            // 시뮬레이션된 비동기 작업
            yield return new WaitForSeconds(1f);
            
            GameLogger.LogPerformance("비동기 작업 완료");
        }

        /// <summary>
        /// 에러 로그 예시
        /// </summary>
        public void ExampleErrorLogging()
        {
            try
            {
                // 시뮬레이션된 에러 상황
                throw new System.Exception("테스트 에러");
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"에러 발생: {ex.Message}");
            }
        }

        /// <summary>
        /// 경고 로그 예시
        /// </summary>
        public void ExampleWarningLogging()
        {
            // 시뮬레이션된 경고 상황
            if (Time.time > 100f)
            {
                GameLogger.LogWarning("게임 시간이 100초를 초과했습니다");
            }
        }

        /// <summary>
        /// 로그 상태 출력
        /// </summary>
        [ContextMenu("로그 상태 출력")]
        public void PrintLogStatus()
        {
            GameLogger.PrintLogStatus();
        }

        /// <summary>
        /// 성능 통계 출력
        /// </summary>
        [ContextMenu("성능 통계 출력")]
        public void PrintPerformanceStats()
        {
            PerformanceProfiler.PrintPerformanceStats();
        }

        /// <summary>
        /// 모든 로그 활성화
        /// </summary>
        [ContextMenu("모든 로그 활성화")]
        public void EnableAllLogs()
        {
            GameLogger.EnableAllLogs();
        }

        /// <summary>
        /// 모든 로그 비활성화
        /// </summary>
        [ContextMenu("모든 로그 비활성화")]
        public void DisableAllLogs()
        {
            GameLogger.DisableAllLogs();
        }

        /// <summary>
        /// 가비지 컬렉션 강제 실행
        /// </summary>
        [ContextMenu("가비지 컬렉션 실행")]
        public void ForceGarbageCollection()
        {
            PerformanceProfiler.ForceGarbageCollection();
        }
    }
} 