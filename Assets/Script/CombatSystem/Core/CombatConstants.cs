namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 전투 시스템에서 사용되는 상수 정의
    /// 매직 넘버와 하드코딩된 문자열을 방지합니다.
    /// </summary>
    public static class CombatConstants
    {
        /// <summary>
        /// 플레이어 마커 카드의 고유 ID
        /// </summary>
        public const string PLAYER_MARKER_ID = "PLAYER_MARKER";

        /// <summary>
        /// 슬롯 GameObject 이름
        /// </summary>
        public static class SlotNames
        {
            public const string BATTLE_SLOT = "BattleSlot";
            public const string WAIT_SLOT_1 = "WaitSlot1";
            public const string WAIT_SLOT_2 = "WaitSlot2";
            public const string WAIT_SLOT_3 = "WaitSlot3";
            public const string WAIT_SLOT_4 = "WaitSlot4";
        }

        /// <summary>
        /// 애니메이션 지속 시간 (초)
        /// </summary>
        public static class AnimationDurations
        {
            /// <summary>카드 이동 애니메이션 시간</summary>
            public const float CARD_MOVE = 0.2f;

            /// <summary>카드 스폰 애니메이션 시간</summary>
            public const float CARD_SPAWN = 0.2f;

            /// <summary>적 입장 애니메이션 시간</summary>
            public const float ENEMY_ENTRANCE = 1.5f;

            /// <summary>전투 초기화 대기 시간</summary>
            public const float INIT_WAIT = 0.2f;
        }

        /// <summary>
        /// 초기 슬롯 설정 관련 상수
        /// </summary>
        public static class InitialSetup
        {
            /// <summary>초기 슬롯에 생성할 카드 개수</summary>
            public const int INITIAL_CARD_COUNT = 5;

            /// <summary>적 카드 생성 재시도 횟수</summary>
            public const int ENEMY_CARD_RETRY_COUNT = 5;

            /// <summary>이펙트 기본 지속 시간</summary>
            public const float DEFAULT_EFFECT_DURATION = 2.0f;
        }

        /// <summary>
        /// 로그 태그
        /// </summary>
        public static class LogTags
        {
            public const string TURN_MANAGER = "[TurnManager]";
            public const string COMBAT_STATE = "[CombatState]";
            public const string CARD_EXECUTOR = "[CardExecutor]";
            public const string SLOT_MOVEMENT = "[SlotMovement]";
        }
    }
}
