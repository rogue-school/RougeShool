using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.IManager;

namespace Game.CharacterSystem.Core
{
    public class PlayerCharacter : CharacterBase, IPlayerCharacter
    {
        [field: SerializeField] public PlayerCharacterData Data { get; private set; }

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image portraitImage;

        private ISkillCard lastUsedCard;
        private IPlayerHandManager handManager;
        public override bool IsPlayerControlled() => true;

        private void Awake()
        {
            if (Data != null)
                InitializeCharacter(Data);
        }

        public void SetCharacterData(PlayerCharacterData data)
        {
            Data = data;
            InitializeCharacter(data);
        }

        private void InitializeCharacter(PlayerCharacterData data)
        {
            SetMaxHP(data.MaxHP);
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (nameText != null) nameText.text = Data?.DisplayName ?? "???";
            if (hpText != null) hpText.text = $"{currentHP} / {Data?.MaxHP ?? 0}";
            if (portraitImage != null && Data != null) portraitImage.sprite = Data.Portrait;
        }

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            UpdateUI(); // 체력 감소 시 UI 갱신
        }

        public override void Heal(int amount)
        {
            base.Heal(amount);
            UpdateUI(); // 체력 회복 시 UI 갱신
        }

        public void InjectHandManager(IPlayerHandManager manager) => handManager = manager;

        public ISkillCard GetCardInHandSlot(SkillCardSlotPosition pos) => handManager?.GetCardInSlot(pos);
        public ISkillCardUI GetCardUIInHandSlot(SkillCardSlotPosition pos) => handManager?.GetCardUIInSlot(pos);

        public void SetLastUsedCard(ISkillCard card) => lastUsedCard = card;
        public ISkillCard GetLastUsedCard() => lastUsedCard;

        public void RestoreCardToHand(ISkillCard card)
        {
            Debug.Log($"[PlayerCharacter] 카드 복귀: {card?.CardData?.Name}");
        }

        public override string GetCharacterName() => Data.DisplayName;

        public override bool IsAlive() => base.IsAlive(); // 명시적으로 override
    }
}
