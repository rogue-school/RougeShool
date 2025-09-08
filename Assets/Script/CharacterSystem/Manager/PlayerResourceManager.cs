using UnityEngine;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 플레이어 캐릭터의 리소스를 관리하는 매니저입니다.
    /// </summary>
    public class PlayerResourceManager : MonoBehaviour, IPlayerResourceManager
    {
        #region 필드

        private int currentResource;
        private int maxResource;
        private string resourceName;

        #endregion

        #region 프로퍼티

        public int CurrentResource => currentResource;
        public int MaxResource => maxResource;
        public string ResourceName => resourceName;

        #endregion

        #region 초기화

        /// <summary>
        /// 캐릭터 데이터를 기반으로 리소스 매니저를 초기화합니다.
        /// </summary>
        /// <param name="characterData">캐릭터 데이터</param>
        public void Initialize(PlayerCharacterData characterData)
        {
            if (characterData == null)
            {
                Debug.LogError("[PlayerResourceManager] 캐릭터 데이터가 null입니다.");
                return;
            }

            maxResource = characterData.MaxResource;
            resourceName = characterData.ResourceName;
            currentResource = maxResource; // 시작 시 최대치로 설정

            Debug.Log($"[PlayerResourceManager] {characterData.DisplayName} 리소스 초기화: {resourceName} {currentResource}/{maxResource}");
        }

        #endregion

        #region 리소스 관리

        public bool ConsumeResource(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("[PlayerResourceManager] 음수 리소스 소모는 불가능합니다.");
                return false;
            }

            if (currentResource < amount)
            {
                Debug.LogWarning($"[PlayerResourceManager] 리소스 부족: {currentResource}/{maxResource} (필요: {amount})");
                return false;
            }

            currentResource -= amount;
            Debug.Log($"[PlayerResourceManager] 리소스 소모: {amount} (남은 양: {currentResource}/{maxResource})");
            return true;
        }

        public void RestoreResource(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("[PlayerResourceManager] 음수 리소스 회복은 불가능합니다.");
                return;
            }

            currentResource = Mathf.Min(currentResource + amount, maxResource);
            Debug.Log($"[PlayerResourceManager] 리소스 회복: {amount} (현재 양: {currentResource}/{maxResource})");
        }

        public void RestoreToMax()
        {
            currentResource = maxResource;
            Debug.Log($"[PlayerResourceManager] 리소스 최대치 회복: {currentResource}/{maxResource}");
        }

        public bool HasEnoughResource(int amount)
        {
            return currentResource >= amount;
        }

        #endregion
    }
}
