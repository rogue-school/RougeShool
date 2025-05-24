using UnityEngine;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Interface;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Game.IManager;
using Game.SkillCardSystem.Slot;

namespace Game.CharacterSystem.Core
{
    public class PlayerCharacter : MonoBehaviour, IPlayerCharacter
    {
        [Header("캐릭터 데이터")]
        [field: SerializeField] public PlayerCharacterData Data { get; private set; }

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image portraitImage;

        private int currentHP;
        private bool isGuarded;
        private ISkillCard lastUsedCard;
        private readonly List<IPerTurnEffect> perTurnEffects = new();

        private void Awake()
        {
            if (Data == null)
            {
                Debug.LogWarning("[PlayerCharacter] PlayerCharacterData가 설정되지 않았습니다. 외부 주입이 필요합니다.");
                return;
            }

            InitializeCharacter(Data);
        }

        public void SetCharacterData(PlayerCharacterData data)
        {
            Data = data;
            InitializeCharacter(data);
        }

        private void InitializeCharacter(PlayerCharacterData data)
        {
            currentHP = data.MaxHP;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (nameText != null)
                nameText.text = Data.DisplayName;

            if (hpText != null)
                hpText.text = $"{currentHP} / {Data.MaxHP}";

            if (portraitImage != null)
                portraitImage.sprite = Data.Portrait;
        }

        public void SetGuarded(bool isGuarded) => this.isGuarded = isGuarded;
        public bool IsGuarded() => isGuarded;

        public void SetLastUsedCard(ISkillCard card) => lastUsedCard = card;
        public ISkillCard GetLastUsedCard() => lastUsedCard;

        public void RestoreCardToHand(ISkillCard card)
        {
            if (card?.CardData.Name != null)
                Debug.Log($"[PlayerCharacter] 카드 복귀: {card.CardData.Name}");
        }

        public bool IsAlive() => currentHP > 0;
        public bool IsDead() => currentHP <= 0;

        public void TakeDamage(int amount)
        {
            currentHP = Mathf.Max(0, currentHP - amount);
            Debug.Log($"[PlayerCharacter] 피해 받음: {amount}, 남은 HP: {currentHP}");
            UpdateUI();
        }

        public void Heal(int amount)
        {
            currentHP = Mathf.Min(currentHP + amount, Data.MaxHP);
            UpdateUI();
        }

        public int GetCurrentHP() => currentHP;
        public int GetMaxHP() => Data.MaxHP;

        public string GetCharacterName() => Data.DisplayName;
        public int GetHP() => currentHP;

        private IPlayerHandManager handManager;

        public void RegisterPerTurnEffect(IPerTurnEffect effect)
        {
            if (effect != null)
                perTurnEffects.Add(effect);
        }

        public void ProcessTurnEffects()
        {
            foreach (var effect in perTurnEffects.ToArray())
            {
                effect.OnTurnStart(this);
                if (effect.IsExpired)
                    perTurnEffects.Remove(effect);
            }
        }
        public void InjectHandManager(IPlayerHandManager manager)
        {
            handManager = manager;
        }

        public ISkillCard GetCardInHandSlot(SkillCardSlotPosition pos)
        {
            return handManager?.GetCardInSlot(pos);
        }

        public ISkillCardUI GetCardUIInHandSlot(SkillCardSlotPosition pos)
        {
            return handManager?.GetCardUIInSlot(pos);
        }
    }
}
