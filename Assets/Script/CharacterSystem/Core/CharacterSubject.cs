using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.UI;
using Game.CombatSystem;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 캐릭터 이벤트 주제 (Observer Pattern)
    /// 캐릭터의 상태 변화를 관찰자들에게 알립니다.
    /// </summary>
    public class CharacterSubject : MonoBehaviour, ICharacterSubject
    {
        [Header("관찰자 관리")]
        [SerializeField] private List<ICharacterObserver> observers = new();

        private ICharacter character;

        #region Unity Lifecycle

        private void Awake()
        {
            character = GetComponent<ICharacter>();
        }

        #endregion

        #region ICharacterSubject 구현

        public void RegisterObserver(ICharacterObserver observer)
        {
            if (observer != null && !observers.Contains(observer))
            {
                observers.Add(observer);
                Debug.Log($"[CharacterSubject] 관찰자 등록: {observer.GetType().Name}");
            }
        }

        public void RemoveObserver(ICharacterObserver observer)
        {
            if (observers.Remove(observer))
            {
                Debug.Log($"[CharacterSubject] 관찰자 제거: {observer.GetType().Name}");
            }
        }

        public void NotifyObservers(string eventType, object data)
        {
            foreach (var observer in observers.ToArray())
            {
                try
                {
                    NotifyObserver(observer, eventType, data);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[CharacterSubject] 관찰자 알림 중 오류: {e.Message}");
                }
            }
        }

        #endregion

        #region 이벤트 알림 메서드

        /// <summary>
        /// 캐릭터 피해 이벤트를 알립니다.
        /// </summary>
        /// <param name="damage">피해량</param>
        public void NotifyDamaged(int damage)
        {
            NotifyObservers("Damaged", damage);
            foreach (var observer in observers)
            {
                observer.OnCharacterDamaged(character, damage);
            }
        }

        /// <summary>
        /// 캐릭터 회복 이벤트를 알립니다.
        /// </summary>
        /// <param name="heal">회복량</param>
        public void NotifyHealed(int heal)
        {
            NotifyObservers("Healed", heal);
            foreach (var observer in observers)
            {
                observer.OnCharacterHealed(character, heal);
            }
        }

        /// <summary>
        /// 캐릭터 가드 이벤트를 알립니다.
        /// </summary>
        /// <param name="guard">가드량</param>
        public void NotifyGuarded(int guard)
        {
            NotifyObservers("Guarded", guard);
            foreach (var observer in observers)
            {
                observer.OnCharacterGuarded(character, guard);
            }
        }

        /// <summary>
        /// 캐릭터 사망 이벤트를 알립니다.
        /// </summary>
        public void NotifyDied()
        {
            NotifyObservers("Died", null);
            foreach (var observer in observers)
            {
                observer.OnCharacterDied(character);
            }
        }

        /// <summary>
        /// 캐릭터 상태 변경 이벤트를 알립니다.
        /// </summary>
        /// <param name="oldState">이전 상태</param>
        /// <param name="newState">새로운 상태</param>
        public void NotifyStateChanged(string oldState, string newState)
        {
            NotifyObservers("StateChanged", new { oldState, newState });
            foreach (var observer in observers)
            {
                observer.OnCharacterStateChanged(character, oldState, newState);
            }
        }

        #endregion

        #region 내부 메서드

        /// <summary>
        /// 개별 관찰자에게 이벤트를 알립니다.
        /// </summary>
        /// <param name="observer">알릴 관찰자</param>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="data">이벤트 데이터</param>
        private void NotifyObserver(ICharacterObserver observer, string eventType, object data)
        {
            if (observer == null) return;

            switch (eventType)
            {
                case "Damaged":
                    if (data is int damage)
                        observer.OnCharacterDamaged(character, damage);
                    break;
                case "Healed":
                    if (data is int heal)
                        observer.OnCharacterHealed(character, heal);
                    break;
                case "Guarded":
                    if (data is int guard)
                        observer.OnCharacterGuarded(character, guard);
                    break;
                case "Died":
                    observer.OnCharacterDied(character);
                    break;
                case "StateChanged":
                    if (data is System.Dynamic.ExpandoObject stateData)
                    {
                        // 동적 객체 처리 (실제로는 더 안전한 방법 사용 권장)
                        observer.OnCharacterStateChanged(character, "", "");
                    }
                    break;
            }
        }

        #endregion

        #region 편의 메서드

        /// <summary>
        /// 등록된 관찰자 수를 반환합니다.
        /// </summary>
        /// <returns>관찰자 수</returns>
        public int GetObserverCount()
        {
            return observers.Count;
        }

        /// <summary>
        /// 모든 관찰자를 제거합니다.
        /// </summary>
        public void ClearObservers()
        {
            observers.Clear();
            Debug.Log("[CharacterSubject] 모든 관찰자 제거");
        }

        /// <summary>
        /// 관찰자 목록을 출력합니다.
        /// </summary>
        public void PrintObservers()
        {
            Debug.Log($"[CharacterSubject] 등록된 관찰자 ({observers.Count}개):");
            for (int i = 0; i < observers.Count; i++)
            {
                Debug.Log($"  {i + 1}. {observers[i].GetType().Name}");
            }
        }

        #endregion
    }

    #region 구체적인 관찰자 클래스들

    /// <summary>
    /// 캐릭터 UI 관찰자
    /// </summary>
    public class CharacterUIObserver : MonoBehaviour, ICharacterObserver
    {
        private CharacterUIController uiController;

        private void Awake()
        {
            uiController = GetComponent<CharacterUIController>();
        }

        public void OnCharacterDamaged(ICharacter character, int damage)
        {
            uiController?.UpdateDamageDisplay(damage);
        }

        public void OnCharacterHealed(ICharacter character, int heal)
        {
            uiController?.UpdateHealDisplay(heal);
        }

        public void OnCharacterGuarded(ICharacter character, int guard)
        {
            uiController?.UpdateGuardDisplay(guard);
        }

        public void OnCharacterDied(ICharacter character)
        {
            uiController?.ShowDeathEffect();
        }

        public void OnCharacterStateChanged(ICharacter character, string oldState, string newState)
        {
            uiController?.UpdateStateDisplay(newState);
        }
    }

    /// <summary>
    /// 캐릭터 애니메이션 관찰자
    /// </summary>
    public class CharacterAnimationObserver : MonoBehaviour, ICharacterObserver
    {
        private CombatAnimationHelper animationHelper;

        private void Awake()
        {
            animationHelper = FindFirstObjectByType<CombatAnimationHelper>();
        }

        public void OnCharacterDamaged(ICharacter character, int damage)
        {
            // 애니메이션 시스템에 피해 이벤트 전달
            if (animationHelper != null)
            {
                // 실제 구현에서는 캐릭터 데이터를 가져와서 사용
                animationHelper.PlayCharacterAnimation(character.GetCharacterName(), "Damaged", character.Transform.gameObject);
            }
        }

        public void OnCharacterHealed(ICharacter character, int heal)
        {
            // 애니메이션 시스템에 회복 이벤트 전달
            if (animationHelper != null)
            {
                animationHelper.PlayCharacterAnimation(character.GetCharacterName(), "Healed", character.Transform.gameObject);
            }
        }

        public void OnCharacterGuarded(ICharacter character, int guard)
        {
            // 애니메이션 시스템에 가드 이벤트 전달
            if (animationHelper != null)
            {
                animationHelper.PlayCharacterAnimation(character.GetCharacterName(), "Guarded", character.Transform.gameObject);
            }
        }

        public void OnCharacterDied(ICharacter character)
        {
            // 애니메이션 시스템에 사망 이벤트 전달
            if (animationHelper != null)
            {
                animationHelper.PlayCharacterDeathAnimation(character.GetCharacterName(), character.Transform.gameObject);
            }
        }

        public void OnCharacterStateChanged(ICharacter character, string oldState, string newState)
        {
            // 애니메이션 시스템에 상태 변경 이벤트 전달
            if (animationHelper != null)
            {
                animationHelper.PlayCharacterAnimation(character.GetCharacterName(), newState, character.Transform.gameObject);
            }
        }
    }

    /// <summary>
    /// 캐릭터 로그 관찰자
    /// </summary>
    public class CharacterLogObserver : MonoBehaviour, ICharacterObserver
    {
        public void OnCharacterDamaged(ICharacter character, int damage)
        {
            Debug.Log($"[CharacterLogObserver] {character.GetCharacterName()} 피해: {damage}");
        }

        public void OnCharacterHealed(ICharacter character, int heal)
        {
            Debug.Log($"[CharacterLogObserver] {character.GetCharacterName()} 회복: {heal}");
        }

        public void OnCharacterGuarded(ICharacter character, int guard)
        {
            Debug.Log($"[CharacterLogObserver] {character.GetCharacterName()} 가드: {guard}");
        }

        public void OnCharacterDied(ICharacter character)
        {
            Debug.Log($"[CharacterLogObserver] {character.GetCharacterName()} 사망");
        }

        public void OnCharacterStateChanged(ICharacter character, string oldState, string newState)
        {
            Debug.Log($"[CharacterLogObserver] {character.GetCharacterName()} 상태 변경: {oldState} → {newState}");
        }
    }

    #endregion
} 