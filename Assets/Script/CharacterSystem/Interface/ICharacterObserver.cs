using UnityEngine;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 캐릭터 이벤트 관찰자 인터페이스 (Observer Pattern)
    /// </summary>
    public interface ICharacterObserver
    {
        /// <summary>
        /// 캐릭터가 피해를 받았을 때 호출됩니다.
        /// </summary>
        /// <param name="character">피해를 받은 캐릭터</param>
        /// <param name="damage">받은 피해량</param>
        void OnCharacterDamaged(ICharacter character, int damage);

        /// <summary>
        /// 캐릭터가 회복되었을 때 호출됩니다.
        /// </summary>
        /// <param name="character">회복된 캐릭터</param>
        /// <param name="heal">회복량</param>
        void OnCharacterHealed(ICharacter character, int heal);

        /// <summary>
        /// 캐릭터가 가드를 얻었을 때 호출됩니다.
        /// </summary>
        /// <param name="character">가드를 얻은 캐릭터</param>
        /// <param name="guard">얻은 가드량</param>
        void OnCharacterGuarded(ICharacter character, int guard);

        /// <summary>
        /// 캐릭터가 사망했을 때 호출됩니다.
        /// </summary>
        /// <param name="character">사망한 캐릭터</param>
        void OnCharacterDied(ICharacter character);

        /// <summary>
        /// 캐릭터 상태가 변경되었을 때 호출됩니다.
        /// </summary>
        /// <param name="character">상태가 변경된 캐릭터</param>
        /// <param name="oldState">이전 상태</param>
        /// <param name="newState">새로운 상태</param>
        void OnCharacterStateChanged(ICharacter character, string oldState, string newState);
    }

    /// <summary>
    /// 캐릭터 이벤트 주제 인터페이스 (Subject in Observer Pattern)
    /// </summary>
    public interface ICharacterSubject
    {
        /// <summary>
        /// 관찰자를 등록합니다.
        /// </summary>
        /// <param name="observer">등록할 관찰자</param>
        void RegisterObserver(ICharacterObserver observer);

        /// <summary>
        /// 관찰자를 제거합니다.
        /// </summary>
        /// <param name="observer">제거할 관찰자</param>
        void RemoveObserver(ICharacterObserver observer);

        /// <summary>
        /// 모든 관찰자에게 이벤트를 알립니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="data">이벤트 데이터</param>
        void NotifyObservers(string eventType, object data);
    }
} 