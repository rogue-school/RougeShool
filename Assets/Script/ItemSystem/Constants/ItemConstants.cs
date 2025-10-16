namespace Game.ItemSystem.Constants
{
    /// <summary>
    /// 아이템 시스템 상수 정의
    /// 하드코딩된 값들을 중앙화하여 관리합니다.
    /// </summary>
    public static class ItemConstants
    {
        #region 슬롯 관련 상수

        /// <summary>
        /// 액티브 아이템 슬롯 수
        /// </summary>
        public const int ACTIVE_SLOT_COUNT = 4;

        /// <summary>
        /// 최대 성급 (패시브 아이템)
        /// </summary>
        public const int MAX_STAR_RANK = 3;

        #endregion

        #region 기본 보상 상수

        /// <summary>
        /// 기본 액티브 보상 개수
        /// </summary>
        public const int DEFAULT_ACTIVE_REWARD_COUNT = 3;

        /// <summary>
        /// 기본 패시브 보상 개수
        /// </summary>
        public const int DEFAULT_PASSIVE_REWARD_COUNT = 1;

        #endregion

        #region 성능 관련 상수

        /// <summary>
        /// 오브젝트 풀 초기 크기
        /// </summary>
        public const int POOL_INITIAL_SIZE = 10;

        /// <summary>
        /// 오브젝트 풀 최대 크기
        /// </summary>
        public const int POOL_MAX_SIZE = 50;

        /// <summary>
        /// 캐시 최대 크기
        /// </summary>
        public const int CACHE_MAX_SIZE = 100;

        #endregion

        #region UI 관련 상수

        /// <summary>
        /// UI 업데이트 간격 (초)
        /// </summary>
        public const float UI_UPDATE_INTERVAL = 0.1f;

        /// <summary>
        /// 애니메이션 지속 시간 (초)
        /// </summary>
        public const float ANIMATION_DURATION = 0.3f;

        /// <summary>
        /// 툴팁 지연 시간 (초)
        /// </summary>
        public const float TOOLTIP_DELAY = 0.5f;

        #endregion

        #region 리소스 경로 상수

        /// <summary>
        /// 기본 아이템 리소스 경로
        /// </summary>
        public const string DEFAULT_ITEM_PATH = "Data/Item";

        /// <summary>
        /// 기본 아이템 프리팹 경로
        /// </summary>
        public const string DEFAULT_ITEM_PREFAB_PATH = "Prefabs/Item";

        #endregion

        #region 로깅 관련 상수

        /// <summary>
        /// 디버그 모드 활성화 여부
        /// </summary>
        public const bool DEBUG_MODE = false;

        /// <summary>
        /// 상세 로깅 활성화 여부
        /// </summary>
        public const bool VERBOSE_LOGGING = false;

        #endregion

        #region 효과 관련 상수

        /// <summary>
        /// 기본 효과 파워
        /// </summary>
        public const int DEFAULT_EFFECT_POWER = 1;

        /// <summary>
        /// 최대 효과 파워
        /// </summary>
        public const int MAX_EFFECT_POWER = 10;

        /// <summary>
        /// 기본 효과 지속 시간 (턴)
        /// </summary>
        public const int DEFAULT_EFFECT_DURATION = 1;

        #endregion

        #region 검증 관련 상수

        /// <summary>
        /// 최소 아이템 ID 길이
        /// </summary>
        public const int MIN_ITEM_ID_LENGTH = 1;

        /// <summary>
        /// 최대 아이템 ID 길이
        /// </summary>
        public const int MAX_ITEM_ID_LENGTH = 50;

        /// <summary>
        /// 최소 아이템 이름 길이
        /// </summary>
        public const int MIN_ITEM_NAME_LENGTH = 1;

        /// <summary>
        /// 최대 아이템 이름 길이
        /// </summary>
        public const int MAX_ITEM_NAME_LENGTH = 100;

        #endregion
    }
}
