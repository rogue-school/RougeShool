using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Slot;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Core;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 전투 화면에서 캐릭터를 배치하는 슬롯 UI 클래스입니다.
    /// 캐릭터를 슬롯에 설정하고, UI 컨트롤러도 연동합니다.
    /// </summary>
    public class CharacterSlotUI : MonoBehaviour, ICharacterSlot
    {
        [Header("슬롯 설정")]
        [Tooltip("이 슬롯의 위치 (플레이어 / 적)")]
        [SerializeField] private CharacterSlotPosition slotPosition;

        [Tooltip("이 슬롯의 소유자 (플레이어 / 적)")]
        [SerializeField] private SlotOwner owner;

        private ICharacter character;

        #region 슬롯 제어

        /// <summary>
        /// 슬롯에 캐릭터를 설정하고 UI 및 Transform을 초기화합니다.
        /// </summary>
        /// <param name="character">슬롯에 배치할 캐릭터</param>
        public void SetCharacter(ICharacter character)
        {
            this.character = character;

            if (character == null)
            {
                Debug.LogWarning($"[CharacterSlotUI] null 캐릭터가 슬롯에 설정됨. 위치: {slotPosition}");
                return;
            }

            SetupCharacterUI(character);
            AttachCharacterTransform(character);
        }

        /// <summary>
        /// 슬롯에서 캐릭터를 제거합니다.
        /// </summary>
        public void Clear()
        {
            character = null;
            // 필요 시: 슬롯 하위에서 기존 캐릭터 오브젝트 제거 가능
        }

        #endregion

        #region 슬롯 내부 처리

        /// <summary>
        /// 레거시 CharacterUIController 제거로 더 이상 슬롯에서 UI를 직접 연결하지 않습니다.
        /// </summary>
        private void SetupCharacterUI(ICharacter character)
        {
            // 의도적으로 비워둠: UI는 각 매니저/컨트롤러에서 직접 연결
        }

        /// <summary>
        /// 캐릭터의 Transform을 슬롯에 연결 및 정렬합니다.
        /// </summary>
        private void AttachCharacterTransform(ICharacter character)
        {
            if (character is MonoBehaviour mb)
            {
                var trans = mb.transform;
                trans.SetParent(transform, false);

                if (trans is RectTransform rt)
                {
                    // UI 기반 배치
                    rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.localRotation = Quaternion.identity;
                    rt.localScale = Vector3.one;
                }
                else
                {
                    // 일반 Transform 배치
                    trans.localPosition = Vector3.zero;
                    trans.localRotation = Quaternion.identity;
                    trans.localScale = Vector3.one;
                }
            }
        }

        #endregion

        #region 슬롯 조회

        /// <summary>
        /// 현재 슬롯에 배치된 캐릭터를 반환합니다.
        /// </summary>
        public ICharacter GetCharacter() => character;

        /// <summary>
        /// 슬롯의 위치 정보를 반환합니다.
        /// </summary>
        public CharacterSlotPosition GetSlotPosition() => slotPosition;

        /// <summary>
        /// 슬롯의 소유자 정보를 반환합니다.
        /// </summary>
        public SlotOwner GetOwner() => owner;

        /// <summary>
        /// 슬롯의 Transform을 반환합니다.
        /// </summary>
        public Transform GetTransform() => transform;

        #endregion
    }
}
