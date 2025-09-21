using UnityEngine;
using System.Collections.Generic;
using Game.StageSystem.Data;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.StageSystem.Factory
{
    /// <summary>
    /// 스테이지 데이터 생성 팩토리
    /// 로그 스쿨 시스템: 4개 스테이지 생성 및 관리
    /// </summary>
    public static class StageDataFactory
    {

        #region 스테이지 데이터 생성

        /// <summary>
        /// 기본 스테이지 데이터 생성
        /// </summary>
        /// <param name="stageNumber">스테이지 번호 (1-4)</param>
        /// <param name="stageName">스테이지 이름</param>
        /// <param name="enemies">적 목록</param>
        /// <param name="difficulty">난이도 (1-5)</param>
        /// <returns>생성된 StageData</returns>
        public static StageData CreateStageData(int stageNumber, string stageName, List<EnemyCharacterData> enemies, int difficulty = 1)
        {
            var stageData = ScriptableObject.CreateInstance<StageData>();
            stageData.stageNumber = stageNumber;
            stageData.stageName = stageName;
            stageData.enemies = enemies ?? new List<EnemyCharacterData>();
            stageData.difficulty = difficulty;
            stageData.stageDescription = $"{stageName} - 난이도 {difficulty}";
            stageData.autoProgressToNext = true;
            stageData.stageReward = CreateDefaultRewards();

            GameLogger.LogInfo($"[StageDataFactory] 스테이지 {stageNumber} 데이터 생성 완료: {stageName}", GameLogger.LogCategory.Core);
            return stageData;
        }

        /// <summary>
        /// 기본 4개 스테이지 데이터 생성
        /// </summary>
        /// <returns>4개 스테이지 데이터 리스트</returns>
        public static List<StageData> CreateDefaultStages()
        {
            var stages = new List<StageData>();

            // 스테이지 1: 초보자용
            var stage1Enemies = new List<EnemyCharacterData>();
            // TODO: 실제 적 데이터 로드
            stages.Add(CreateStageData(1, "초보자의 길", stage1Enemies, 1));

            // 스테이지 2: 중급자용
            var stage2Enemies = new List<EnemyCharacterData>();
            // TODO: 실제 적 데이터 로드
            stages.Add(CreateStageData(2, "전사의 시험", stage2Enemies, 2));

            // 스테이지 3: 고급자용
            var stage3Enemies = new List<EnemyCharacterData>();
            // TODO: 실제 적 데이터 로드
            stages.Add(CreateStageData(3, "마법사의 도전", stage3Enemies, 3));

            // 스테이지 4: 최종 보스
            var stage4Enemies = new List<EnemyCharacterData>();
            // TODO: 실제 적 데이터 로드
            stages.Add(CreateStageData(4, "최종 보스전", stage4Enemies, 5));

            GameLogger.LogInfo("[StageDataFactory] 기본 4개 스테이지 데이터 생성 완료", GameLogger.LogCategory.Core);
            return stages;
        }

        /// <summary>
        /// 특정 스테이지 번호의 스테이지 데이터 생성
        /// </summary>
        /// <param name="stageNumber">스테이지 번호 (1-4)</param>
        /// <returns>스테이지 데이터</returns>
        public static StageData CreateStageByNumber(int stageNumber)
        {
            switch (stageNumber)
            {
                case 1:
                    return CreateStageData(1, "초보자의 길", new List<EnemyCharacterData>(), 1);
                case 2:
                    return CreateStageData(2, "전사의 시험", new List<EnemyCharacterData>(), 2);
                case 3:
                    return CreateStageData(3, "마법사의 도전", new List<EnemyCharacterData>(), 3);
                case 4:
                    return CreateStageData(4, "최종 보스전", new List<EnemyCharacterData>(), 5);
                default:
                    GameLogger.LogError($"[StageDataFactory] 잘못된 스테이지 번호: {stageNumber}", GameLogger.LogCategory.Core);
                    return null;
            }
        }

        #endregion

        #region 보상 데이터 생성

        /// <summary>
        /// 기본 적 처치 보상 데이터 생성
        /// </summary>
        /// <returns>생성된 StageRewardData</returns>
        public static StageRewardData CreateDefaultRewards()
        {
            var rewardData = ScriptableObject.CreateInstance<StageRewardData>();
            rewardData.InitializeDefaultEnemyDefeatRewards();

            GameLogger.LogInfo("[StageDataFactory] 기본 적 처치 보상 데이터 생성 완료", GameLogger.LogCategory.Core);
            return rewardData;
        }

        /// <summary>
        /// 스테이지별 보상 데이터 생성
        /// </summary>
        /// <param name="stageNumber">스테이지 번호</param>
        /// <returns>스테이지별 보상 데이터</returns>
        public static StageRewardData CreateStageRewards(int stageNumber)
        {
            var rewardData = ScriptableObject.CreateInstance<StageRewardData>();
            
            // 스테이지별 보상 설정
            switch (stageNumber)
            {
                case 1:
                    // 초보자용 보상
                    break;
                case 2:
                    // 중급자용 보상
                    break;
                case 3:
                    // 고급자용 보상
                    break;
                case 4:
                    // 최종 보스 보상
                    break;
            }

            GameLogger.LogInfo($"[StageDataFactory] 스테이지 {stageNumber} 보상 데이터 생성 완료", GameLogger.LogCategory.Core);
            return rewardData;
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 스테이지 데이터 유효성 검증
        /// </summary>
        /// <param name="stageData">검증할 스테이지 데이터</param>
        /// <returns>유효성 여부</returns>
        public static bool ValidateStageData(StageData stageData)
        {
            if (stageData == null)
            {
                GameLogger.LogError("[StageDataFactory] 스테이지 데이터가 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            if (!stageData.IsValid())
            {
                GameLogger.LogError($"[StageDataFactory] 스테이지 데이터가 유효하지 않습니다: {stageData.stageName}", GameLogger.LogCategory.Core);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 스테이지 리스트 유효성 검증
        /// </summary>
        /// <param name="stages">검증할 스테이지 리스트</param>
        /// <returns>유효성 여부</returns>
        public static bool ValidateStageList(List<StageData> stages)
        {
            if (stages == null || stages.Count == 0)
            {
                GameLogger.LogError("[StageDataFactory] 스테이지 리스트가 비어있습니다", GameLogger.LogCategory.Core);
                return false;
            }

            if (stages.Count != 4)
            {
                GameLogger.LogError($"[StageDataFactory] 스테이지 수가 올바르지 않습니다: {stages.Count}/4", GameLogger.LogCategory.Core);
                return false;
            }

            for (int i = 0; i < stages.Count; i++)
            {
                if (!ValidateStageData(stages[i]))
                {
                    return false;
                }
            }

            GameLogger.LogInfo("[StageDataFactory] 스테이지 리스트 유효성 검증 완료", GameLogger.LogCategory.Core);
            return true;
        }

        #endregion

    }
}
