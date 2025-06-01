using UnityEngine;
using Zenject;
using System.Collections;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Core;

namespace Game.CombatSystem.Initialization
{
    public class PlayerCharacterInitializer : MonoBehaviour, IPlayerCharacterInitializer, ICombatInitializerStep
    {
        [SerializeField] private PlayerCharacter playerPrefab;
        [SerializeField] private PlayerCharacterData defaultData;

        [Header("초기화 순서 (낮을수록 먼저 실행됨)")]
        [SerializeField] private int order = 20;
        public int Order => order;

        private IPlayerManager playerManager;
        private ISlotRegistry slotRegistry;

        [Inject]
        public void Inject(IPlayerManager playerManager, ISlotRegistry slotRegistry)
        {
            this.playerManager = playerManager;
            this.slotRegistry = slotRegistry;
            Debug.Log("[PlayerCharacterInitializer] IPlayerManager, ISlotRegistry 주입 완료");
        }

        public IEnumerator Initialize()
        {
            Debug.Log("[PlayerCharacterInitializer] Initialize() 시작");

            yield return new WaitUntil(() =>
                slotRegistry is SlotRegistry concrete && concrete.IsInitialized);

            Setup();
        }

        public void Setup()
        {
            Debug.Log("[PlayerCharacterInitializer] Setup() 호출됨");

            if (!ValidateData())
            {
                Debug.LogError("[PlayerCharacterInitializer] 유효하지 않은 초기화 데이터입니다.");
                return;
            }

            var slot = GetPlayerSlot();
            if (slot == null) return;

            Transform slotTransform = ((MonoBehaviour)slot).transform;

            // 기존 자식 제거
            foreach (Transform child in slotTransform)
                Destroy(child.gameObject);

            // 캐릭터 생성 및 부모 설정
            var player = Instantiate(playerPrefab);
            player.name = "PlayerCharacter";
            player.transform.SetParent(slotTransform, false); // worldPositionStays = false

            // RectTransform 정렬
            if (player.TryGetComponent(out RectTransform rt))
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.localPosition = Vector3.zero;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
            }

            // PlayerCharacter 컴포넌트 확인
            if (!player.TryGetComponent(out PlayerCharacter character))
            {
                Debug.LogError("[PlayerCharacterInitializer] PlayerCharacter 컴포넌트를 찾을 수 없습니다.");
                Destroy(player.gameObject);
                return;
            }

            // 데이터 설정
            var data = ResolvePlayerData();
            if (data == null)
            {
                Debug.LogError("[PlayerCharacterInitializer] 캐릭터 데이터가 없습니다.");
                return;
            }

            character.SetCharacterData(data);
            slot.SetCharacter(character);
            playerManager?.SetPlayer(character);

            // 카드 정보 출력
            var cards = data.SkillDeck?.GetCards();
            Debug.Log($"[PlayerCharacterInitializer] 카드 수: {cards?.Count}");

            if (cards != null)
            {
                foreach (var entry in cards)
                    Debug.Log($" → 카드: {entry.GetCardName()}, 효과 수: {entry.CreateEffects()?.Count ?? 0}");
            }
        }

        private bool ValidateData()
        {
            return playerPrefab != null && slotRegistry != null;
        }

        private ICharacterSlot GetPlayerSlot()
        {
            var registry = slotRegistry?.GetCharacterSlotRegistry();
            if (registry == null)
            {
                Debug.LogError("[PlayerCharacterInitializer] CharacterSlotRegistry가 null입니다.");
                return null;
            }

            var slot = registry.GetCharacterSlot(SlotOwner.PLAYER);
            if (slot == null)
                Debug.LogError("[PlayerCharacterInitializer] SlotOwner.PLAYER에 해당하는 캐릭터 슬롯이 없습니다.");

            return slot;
        }

        private PlayerCharacterData ResolvePlayerData()
        {
            if (PlayerCharacterSelector.SelectedCharacter != null)
                return PlayerCharacterSelector.SelectedCharacter;

            if (playerManager?.GetPlayer()?.Data != null)
                return playerManager.GetPlayer().Data;

            return defaultData;
        }
    }
}
