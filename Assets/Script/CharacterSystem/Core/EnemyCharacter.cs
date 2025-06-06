using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Context;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Deck;
using Game.SkillCardSystem.Interface;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class EnemyCharacter : CharacterBase, IEnemyCharacter
{
    [SerializeField] private EnemyCharacterData characterData;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image portraitImage;

    private EnemySkillDeck skillDeck;
    private ICharacterDeathListener deathListener;
    private bool isDead = false;

    public EnemyCharacterData Data => characterData;
    public override bool IsPlayerControlled() => false;
    public string GetName() => GetCharacterName();
    public bool IsMarkedDead => isDead;

    public void Initialize(EnemyCharacterData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[EnemyCharacter] 초기화 실패 - null 데이터");
            return;
        }

        characterData = data;
        skillDeck = data.EnemyDeck;

        SetMaxHP(data.MaxHP);
        ApplyPassiveEffects();
        RefreshUI();

        Debug.Log($"[EnemyCharacter] '{characterData.DisplayName}' 초기화 완료");
    }

    public void SetDeathListener(ICharacterDeathListener listener)
    {
        deathListener = listener;
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        RefreshUI();

        // 캐릭터 사망 처리 위임 (즉시 처리 X)
        if (IsDead() && !isDead)
        {
            MarkAsDead();
        }
    }

    public override void Heal(int amount)
    {
        base.Heal(amount);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (characterData == null)
            return;

        nameText.text = GetCharacterName();
        hpText.text = $"HP {currentHP} / {GetMaxHP()}";
        portraitImage.sprite = characterData.Portrait;
    }

    private void ApplyPassiveEffects()
    {
        if (characterData?.PassiveEffects == null) return;

        foreach (var effect in characterData.PassiveEffects)
        {
            if (effect is ICardEffect cardEffect)
            {
                var context = new DefaultCardExecutionContext(null, this, this);
                cardEffect.ApplyEffect(context, 0);
            }
        }
    }

    public EnemySkillDeck.CardEntry GetRandomCardEntry()
    {
        if (skillDeck == null)
        {
            Debug.LogError("[EnemyCharacter] 스킬 덱이 null입니다.");
            return null;
        }

        var entry = skillDeck.GetRandomEntry();

        if (entry?.card == null)
        {
            Debug.LogError("[EnemyCharacter] 카드 선택 실패: entry 또는 card가 null입니다.");
        }
        else
        {
            Debug.Log($"[EnemyCharacter] 카드 선택 완료: {entry.card.name} (확률: {entry.probability})");
        }

        return entry;
    }

    public override string GetCharacterName()
    {
        return characterData?.DisplayName ?? "Unnamed Enemy";
    }

    /// <summary>
    /// 외부에서 호출하여 사망 처리 트리거를 발생시킴
    /// </summary>
    public void MarkAsDead()
    {
        if (isDead)
            return;

        isDead = true;
        Debug.Log($"[EnemyCharacter] '{GetCharacterName()}' 사망 처리 (MarkAsDead 호출)");
        deathListener?.OnCharacterDied(this);
    }

    // 실제 Die 호출 시 아무 처리도 하지 않도록 override
    public override void Die()
    {
        // 내부에서는 사망 플래그만 남기고 외부 위임 처리
        MarkAsDead();
    }
}
