using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.IManager;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 모든 슬롯을 중앙에서 관리하는 싱글톤 클래스입니다.
    /// </summary>
    public class SlotRegistry : MonoBehaviour, ISlotRegistry
    {
        public static SlotRegistry Instance { get; private set; }

        private Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots = new();
        private Dictionary<CombatSlotPosition, ICombatCardSlot> combatSlots = new();
        private Dictionary<SlotOwner, ICharacterSlot> characterSlots = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            //Debug.Log("[SlotRegistry] 인스턴스 초기화 완료");

            if (transform.parent != null)
            {
                //Debug.LogWarning("[SlotRegistry] DontDestroyOnLoad를 위해 부모에서 분리합니다.");
                transform.SetParent(null);
            }

            DontDestroyOnLoad(gameObject);
        }

        #region Hand Slots

        public void RegisterHandSlots(IHandCardSlot[] slots)
        {
            handSlots.Clear();
            //Debug.Log($"[SlotRegistry] Hand 슬롯 등록 시작: {slots.Length}개");

            foreach (var slot in slots)
            {
                var key = slot.GetSlotPosition();
                if (!handSlots.ContainsKey(key))
                {
                    handSlots.Add(key, slot);
                    //Debug.Log($" → 등록됨: {key} / {((MonoBehaviour)slot).name}");
                }
            }
        }

        public IHandCardSlot GetHandSlot(SkillCardSlotPosition position)
            => handSlots.TryGetValue(position, out var slot) ? slot : null;

        public IEnumerable<IHandCardSlot> GetAllHandSlots() => handSlots.Values;

        public IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner)
            => handSlots.Values.Where(slot => slot.GetOwner() == owner);

        #endregion

        #region Combat Slots

        public void RegisterCombatSlots(ICombatCardSlot[] slots)
        {
            combatSlots.Clear();
            //Debug.Log($"[SlotRegistry] Combat 슬롯 등록 시작: {slots.Length}개");

            foreach (var slot in slots)
            {
                var key = slot.GetCombatPosition();
                if (!combatSlots.ContainsKey(key))
                {
                    combatSlots.Add(key, slot);
                    //Debug.Log($" → 등록됨: {key} / {((MonoBehaviour)slot).name}");
                }
            }
        }

        public ICombatCardSlot GetCombatSlot(CombatSlotPosition position)
            => combatSlots.TryGetValue(position, out var slot) ? slot : null;

        public IEnumerable<ICombatCardSlot> GetAllCombatSlots() => combatSlots.Values;

        public IEnumerable<ICombatCardSlot> GetCombatSlots()
        {
            //Debug.Log("[SlotRegistry] 모든 전투 슬롯 반환 요청됨");
            return combatSlots.Values;
        }

        #endregion

        #region Character Slots

        public void RegisterCharacterSlots(ICharacterSlot[] slots)
        {
            characterSlots.Clear();
           // Debug.Log($"[SlotRegistry] Character 슬롯 등록 시작: {slots.Length}개");

            foreach (var slot in slots)
            {
                var owner = slot.GetOwner();
                if (!characterSlots.ContainsKey(owner))
                {
                    characterSlots.Add(owner, slot);
                    //Debug.Log($" → 등록됨: {owner} / {((MonoBehaviour)slot).name}");
                }
                else
                {
                    //Debug.LogWarning($"[SlotRegistry] 중복된 소유자 슬롯: {owner}");
                }
            }
        }

        public ICharacterSlot GetCharacterSlot(SlotOwner owner)
        {
            //Debug.Log($"[SlotRegistry] GetCharacterSlot 호출 - 요청: {owner}");

            if (characterSlots.TryGetValue(owner, out var slot))
            {
                //Debug.Log($" → 슬롯 반환됨: {((MonoBehaviour)slot).name}");
                return slot;
            }

            //Debug.LogWarning($"[SlotRegistry] {owner}용 캐릭터 슬롯을 찾을 수 없습니다.");
            return null;
        }

        public IEnumerable<ICharacterSlot> GetCharacterSlots()
        {
            //Debug.Log("[SlotRegistry] 모든 캐릭터 슬롯 요청됨");
            return characterSlots.Values;
        }

        public IEnumerable<ICharacterSlot> GetCharacterSlots(SlotOwner owner)
        {
            if (characterSlots.TryGetValue(owner, out var slot))
            {
                yield return slot;
            }
        }
        public void Initialize()
        {
            if (transform.parent != null)
            {
                //Debug.LogWarning("[SlotRegistry] DontDestroyOnLoad를 위해 부모에서 분리합니다.");
                transform.SetParent(null);
            }

            DontDestroyOnLoad(gameObject);
           // Debug.Log("[SlotRegistry] 수동 초기화 완료");
        }

        #endregion
    }
}
