using UnityEngine;

namespace Game.CombatSystem.Utility
{
    /// <summary>
    /// 전투 시스템에서 사용되는 모든 상수를 정의하는 클래스
    /// 매직 넘버를 제거하고 유지보수성을 향상시킵니다.
    /// </summary>
    public static class CombatConstants
    {
        #region 타이밍 상수
        /// <summary>
        /// 카드 생성 지연 시간
        /// </summary>
        public const float CARD_SPAWN_DELAY = 0.5f;
        
        /// <summary>
        /// 애니메이션 지연 시간
        /// </summary>
        public const float ANIMATION_DELAY = 0.2f;
        
        /// <summary>
        /// 상태 전환 지연 시간
        /// </summary>
        public const float STATE_TRANSITION_DELAY = 0.1f;
        
        /// <summary>
        /// 카드 소멸 애니메이션 대기 시간
        /// </summary>
        public const float CARD_VANISH_DELAY = 0.5f;
        
        /// <summary>
        /// 슬롯 채우기 간격
        /// </summary>
        public const float SLOT_FILL_INTERVAL = 0.3f;
        
        /// <summary>
        /// 애니메이션 타임아웃 시간
        /// </summary>
        public const float ANIMATION_TIMEOUT = 5.0f;
        
        /// <summary>
        /// 대기 타임아웃 시간
        /// </summary>
        public const float WAIT_TIMEOUT = 10.0f;
        #endregion

        #region UI 상수
        /// <summary>
        /// 카드 UI 기본 스케일
        /// </summary>
        public static readonly Vector3 CARD_DEFAULT_SCALE = Vector3.one;
        
        /// <summary>
        /// 카드 UI 최소 스케일
        /// </summary>
        public static readonly Vector3 CARD_MIN_SCALE = Vector3.zero;
        
        /// <summary>
        /// 카드 UI 최대 스케일
        /// </summary>
        public static readonly Vector3 CARD_MAX_SCALE = Vector3.one * 1.2f;
        
        /// <summary>
        /// 카드 UI 기본 위치
        /// </summary>
        public static readonly Vector3 CARD_DEFAULT_POSITION = Vector3.zero;
        #endregion

        #region 애니메이션 상수
        /// <summary>
        /// 기본 애니메이션 지속 시간
        /// </summary>
        public const float DEFAULT_ANIMATION_DURATION = 0.2f;
        
        /// <summary>
        /// 이동 애니메이션 지속 시간
        /// </summary>
        public const float MOVE_ANIMATION_DURATION = 0.2f;
        
        /// <summary>
        /// 스케일 애니메이션 지속 시간
        /// </summary>
        public const float SCALE_ANIMATION_DURATION = 0.2f;
        
        /// <summary>
        /// 페이드 애니메이션 지속 시간
        /// </summary>
        public const float FADE_ANIMATION_DURATION = 0.18f;
        
        /// <summary>
        /// 소멸 애니메이션 지속 시간
        /// </summary>
        public const float VANISH_ANIMATION_DURATION = 0.25f;
        #endregion

        #region 게임플레이 상수
        /// <summary>
        /// 선공 확률 (50%)
        /// </summary>
        public const float FIRST_ATTACK_PROBABILITY = 0.5f;
        
        /// <summary>
        /// 최대 카드 수
        /// </summary>
        public const int MAX_CARD_COUNT = 10;
        
        /// <summary>
        /// 최대 핸드 슬롯 수
        /// </summary>
        public const int MAX_HAND_SLOT_COUNT = 5;
        
        /// <summary>
        /// 최대 전투 슬롯 수
        /// </summary>
        public const int MAX_COMBAT_SLOT_COUNT = 2;
        #endregion

        #region 로깅 상수
        /// <summary>
        /// 로그 태그
        /// </summary>
        public const string LOG_TAG = "[CombatSystem]";
        
        /// <summary>
        /// 에러 태그
        /// </summary>
        public const string ERROR_TAG = "[CombatSystem][ERROR]";
        
        /// <summary>
        /// 경고 태그
        /// </summary>
        public const string WARNING_TAG = "[CombatSystem][WARNING]";
        #endregion

        #region 검증 상수
        /// <summary>
        /// 최대 재시도 횟수
        /// </summary>
        public const int MAX_RETRY_COUNT = 3;
        
        /// <summary>
        /// 최대 대기 프레임 수
        /// </summary>
        public const int MAX_WAIT_FRAMES = 300; // 5초 (60fps 기준)
        #endregion
    }
} 