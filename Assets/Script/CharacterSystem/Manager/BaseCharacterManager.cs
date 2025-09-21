using UnityEngine;
using Zenject;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Interface;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 플레이어와 적 캐릭터 매니저의 공통 베이스 클래스입니다.
    /// BaseCoreManager를 상속받아 인스펙터 필드 구조와 기본 기능을 표준화합니다.
    /// </summary>
    /// <typeparam name="TCharacter">관리할 캐릭터 타입 (IPlayerCharacter 또는 IEnemyCharacter)</typeparam>
    public abstract class BaseCharacterManager<TCharacter> : BaseCoreManager<TCharacter>, ICoreSystemInitializable
        where TCharacter : class, ICharacter
    {
        #region 표준화된 인스펙터 필드

        [Header("캐릭터 프리팹 및 슬롯")]
        [Tooltip("캐릭터 프리팹")]
        [SerializeField] protected GameObject characterPrefab;

        [Tooltip("캐릭터가 배치될 슬롯 위치")]
        [SerializeField] protected Transform characterSlot;

        [Header("캐릭터 UI")]
        [Tooltip("캐릭터 UI 컨트롤러")]
        [SerializeField] protected MonoBehaviour characterUIController;

        #endregion

        #region 보호된 필드

        /// <summary>
        /// 현재 관리 중인 캐릭터
        /// </summary>
        protected TCharacter currentCharacter;

        #endregion

        #region BaseCoreManager 오버라이드

        /// <summary>
        /// 캐릭터 매니저는 캐릭터 프리팹이 필요합니다.
        /// </summary>
        protected override bool RequiresRelatedPrefab() => true;

        /// <summary>
        /// 캐릭터 매니저는 UI 컨트롤러가 선택사항입니다.
        /// </summary>
        protected override bool RequiresUIController() => false;

        /// <summary>
        /// 캐릭터 프리팹을 반환합니다.
        /// </summary>
        protected override GameObject GetRelatedPrefab() => characterPrefab;

        /// <summary>
        /// 캐릭터 UI 컨트롤러를 반환합니다.
        /// </summary>
        protected override MonoBehaviour GetUIController() => characterUIController;

        #endregion

        #region BaseCoreManager 추상 메서드 구현

        /// <summary>
        /// BaseCoreManager의 OnInitialize 추상 메서드 구현
        /// </summary>
        protected override System.Collections.IEnumerator OnInitialize()
        {
            GameLogger.LogInfo($"{GetType().Name} 초기화 시작", GameLogger.LogCategory.Character);
            
            // 캐릭터 매니저 특화 초기화 로직
            yield return InitializeCharacterManager();
            
            GameLogger.LogInfo($"{GetType().Name} 초기화 완료", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// BaseCoreManager의 Reset 추상 메서드 구현
        /// </summary>
        public override void Reset()
        {
            GameLogger.LogInfo($"{GetType().Name} 리셋 시작", GameLogger.LogCategory.Character);
            
            // 캐릭터 매니저 특화 리셋 로직
            ResetCharacter();
            
            GameLogger.LogInfo($"{GetType().Name} 리셋 완료", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 캐릭터 매니저 특화 초기화 로직
        /// </summary>
        protected virtual System.Collections.IEnumerator InitializeCharacterManager()
        {
            // 기본 구현: 즉시 완료
            yield return null;
        }

        #endregion

        #region 추상 메서드

        /// <summary>
        /// 캐릭터를 생성하고 등록합니다.
        /// </summary>
        public abstract void CreateAndRegisterCharacter();

        /// <summary>
        /// 현재 캐릭터를 반환합니다.
        /// </summary>
        /// <returns>현재 캐릭터</returns>
        public abstract TCharacter GetCharacter();

        /// <summary>
        /// 캐릭터를 설정합니다.
        /// </summary>
        /// <param name="character">설정할 캐릭터</param>
        public abstract void SetCharacter(TCharacter character);

        /// <summary>
        /// 캐릭터 등록을 해제합니다.
        /// </summary>
        public abstract void UnregisterCharacter();

        /// <summary>
        /// 매니저 상태를 초기화합니다.
        /// </summary>
        public abstract void ResetCharacter();

        #endregion

        #region 공통 보호 메서드

        /// <summary>
        /// 캐릭터 UI를 연결합니다.
        /// </summary>
        /// <param name="character">연결할 캐릭터</param>
        protected virtual void ConnectCharacterUI(TCharacter character)
        {
            if (characterUIController != null && character is ICharacter ic)
            {
                // SetTarget 메서드가 있는지 확인 후 호출
                var setTargetMethod = characterUIController.GetType().GetMethod("SetTarget");
                if (setTargetMethod != null)
                {
                    setTargetMethod.Invoke(characterUIController, new object[] { ic });
                    GameLogger.LogInfo($"캐릭터 UI 연결 완료: {character.GetCharacterName()}", GameLogger.LogCategory.Character);
                }
            }
        }

        /// <summary>
        /// 프리팹과 슬롯 참조의 유효성을 검증합니다.
        /// </summary>
        /// <returns>유효하면 true</returns>
        protected override bool ValidateReferences()
        {
            if (characterPrefab == null)
            {
                GameLogger.LogError("캐릭터 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.Character);
                return false;
            }

            if (characterSlot == null)
            {
                GameLogger.LogError("캐릭터 슬롯이 설정되지 않았습니다.", GameLogger.LogCategory.Character);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 캐릭터 인스턴스를 생성합니다.
        /// </summary>
        /// <returns>생성된 캐릭터 인스턴스</returns>
        protected virtual GameObject CreateCharacterInstance()
        {
            var instance = Instantiate(characterPrefab, characterSlot);
            
            // HideFlags 초기화 (Unity Assertion 에러 방지)
            instance.hideFlags = HideFlags.None;
            
            return instance;
        }

        #endregion

        #region 디버그 및 로깅

        /// <summary>
        /// 현재 상태를 로깅합니다.
        /// </summary>
        protected virtual void LogCurrentState()
        {
            string characterName = currentCharacter?.GetCharacterName() ?? "없음";
            GameLogger.LogInfo($"현재 캐릭터: {characterName}", GameLogger.LogCategory.Character);
        }

        #endregion
    }
}
