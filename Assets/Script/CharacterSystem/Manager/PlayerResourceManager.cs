using UnityEngine;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 플레이어 리소스(마나, 에너지 등) 관리 클래스
    /// SRP(Single Responsibility Principle)를 준수하기 위해 PlayerManager에서 분리
    /// </summary>
    public class PlayerResourceManager
    {
        #region 상태

        private int currentResource;
        private int maxResource;
        private string resourceName;

        #endregion

        #region 이벤트

        /// <summary>
        /// 리소스가 변경될 때 발생하는 이벤트 (현재값, 최대값)
        /// </summary>
        public event System.Action<int, int> OnResourceChanged;

        #endregion

        #region 프로퍼티

        /// <summary>
        /// 현재 리소스 양
        /// </summary>
        public int CurrentResource => currentResource;

        /// <summary>
        /// 최대 리소스 양
        /// </summary>
        public int MaxResource => maxResource;

        /// <summary>
        /// 리소스 이름 (마나, 에너지 등)
        /// </summary>
        public string ResourceName => resourceName;

        #endregion

        #region 초기화

        /// <summary>
        /// 리소스를 초기화합니다.
        /// </summary>
        /// <param name="characterData">캐릭터 데이터</param>
        public void Initialize(PlayerCharacterData characterData)
        {
            if (characterData == null)
            {
                GameLogger.LogError("캐릭터 데이터가 null입니다. 리소스를 초기화할 수 없습니다.", GameLogger.LogCategory.Error);
                return;
            }

            maxResource = characterData.MaxResource;
            resourceName = characterData.ResourceName;
            currentResource = Mathf.Clamp(characterData.InitialResource, 0, maxResource);

            GameLogger.LogInfo($"{characterData.DisplayName} 리소스 초기화: {resourceName} {currentResource}/{maxResource}", GameLogger.LogCategory.Character);
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        #endregion

        #region 리소스 소비

        /// <summary>
        /// 리소스를 소비합니다.
        /// </summary>
        /// <param name="amount">소비할 양</param>
        /// <returns>소비 성공 여부</returns>
        public bool ConsumeResource(int amount)
        {
            if (amount < 0)
            {
                GameLogger.LogWarning("음수 리소스는 소비할 수 없습니다.", GameLogger.LogCategory.Character);
                return false;
            }

            if (currentResource < amount)
            {
                GameLogger.LogWarning($"리소스 부족: {currentResource}/{maxResource} (필요: {amount})", GameLogger.LogCategory.Character);
                return false;
            }

            currentResource -= amount;
            GameLogger.LogInfo($"리소스 소모: {amount} (남은 양: {currentResource}/{maxResource})", GameLogger.LogCategory.Character);
            OnResourceChanged?.Invoke(currentResource, maxResource);
            return true;
        }

        /// <summary>
        /// 리소스가 충분한지 확인합니다.
        /// </summary>
        /// <param name="amount">필요한 양</param>
        /// <returns>충분 여부</returns>
        public bool HasEnoughResource(int amount)
        {
            return currentResource >= amount;
        }

        #endregion

        #region 리소스 회복

        /// <summary>
        /// 리소스를 회복합니다.
        /// </summary>
        /// <param name="amount">회복할 양</param>
        public void RestoreResource(int amount)
        {
            if (amount < 0)
            {
                GameLogger.LogWarning("음수 리소스는 회복할 수 없습니다.", GameLogger.LogCategory.Character);
                return;
            }

            currentResource = Mathf.Min(currentResource + amount, maxResource);
            GameLogger.LogInfo($"리소스 회복: {amount} (현재 양: {currentResource}/{maxResource})", GameLogger.LogCategory.Character);
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        /// <summary>
        /// 리소스를 최대치로 회복합니다.
        /// </summary>
        public void RestoreResourceToMax()
        {
            currentResource = maxResource;
            GameLogger.LogInfo($"리소스 최대치 회복: {currentResource}/{maxResource}", GameLogger.LogCategory.Character);
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        #endregion

        #region 리소스 설정

        /// <summary>
        /// 최대 리소스를 설정합니다.
        /// </summary>
        /// <param name="newMaxResource">새로운 최대 리소스</param>
        public void SetMaxResource(int newMaxResource)
        {
            if (newMaxResource < 0)
            {
                GameLogger.LogWarning("최대 리소스는 0 이상이어야 합니다.", GameLogger.LogCategory.Character);
                return;
            }

            maxResource = newMaxResource;
            currentResource = Mathf.Min(currentResource, maxResource); // 현재 리소스가 최대치를 초과하지 않도록 조정
            GameLogger.LogInfo($"최대 리소스 변경: {maxResource}", GameLogger.LogCategory.Character);
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        /// <summary>
        /// 현재 리소스를 직접 설정합니다 (디버그용).
        /// </summary>
        /// <param name="amount">설정할 리소스 양</param>
        public void SetCurrentResource(int amount)
        {
            currentResource = Mathf.Clamp(amount, 0, maxResource);
            GameLogger.LogInfo($"리소스 직접 설정: {currentResource}/{maxResource}", GameLogger.LogCategory.Character);
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        #endregion

        #region 리셋

        /// <summary>
        /// 리소스를 리셋합니다.
        /// </summary>
        public void Reset()
        {
            currentResource = 0;
            maxResource = 0;
            resourceName = string.Empty;
            OnResourceChanged?.Invoke(currentResource, maxResource);
            GameLogger.LogInfo("리소스 리셋 완료", GameLogger.LogCategory.Character);
        }

        #endregion
    }
}
