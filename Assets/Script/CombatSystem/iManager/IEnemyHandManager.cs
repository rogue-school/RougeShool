using System.Collections;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;

/// <summary>
/// 적의 스킬 카드 핸드를 관리하는 인터페이스입니다.
/// 핸드 초기화, 슬롯 제어, 카드 등록 및 전투 슬롯 등록 등을 제공합니다.
/// </summary>
public interface IEnemyHandManager
{
    IEnumerator PopCardAndRegisterToCombatSlotCoroutine(ICombatFlowCoordinator flowCoordinator);

    /// <summary>
    /// 적 캐릭터를 기반으로 핸드를 초기화합니다.
    /// </summary>
    void Initialize(IEnemyCharacter enemy);

    /// <summary>
    /// 적의 초기 핸드를 생성합니다.
    /// </summary>
    void GenerateInitialHand();

    /// <summary>
    /// 핸드 슬롯을 후방부터 순차적으로 채웁니다.
    /// </summary>
    /// <param name="delay">각 슬롯 채움 사이의 지연 시간 (초)</param>
    IEnumerator StepwiseFillSlotsFromBack(float delay = 0.5f);

    /// <summary>
    /// 해당 슬롯의 카드 및 UI 정보를 확인합니다 (카드 제거는 하지 않음).
    /// </summary>
    (ISkillCard card, ISkillCardUI cardUI) PeekCardInSlot(SkillCardSlotPosition position);

    /// <summary>
    /// 적의 전투에서 사용할 다음 카드를 반환합니다.
    /// </summary>
    ISkillCard GetCardForCombat();

    /// <summary>
    /// 지정된 슬롯 위치의 카드를 반환합니다.
    /// </summary>
    ISkillCard GetSlotCard(SkillCardSlotPosition pos);

    /// <summary>
    /// 인덱스 기반으로 카드 UI를 반환합니다.
    /// </summary>
    ISkillCardUI GetCardUI(int index);

    /// <summary>
    /// 핸드를 초기화합니다 (등록 정보만 제거).
    /// </summary>
    void ClearHand();

    /// <summary>
    /// 핸드의 모든 카드 오브젝트를 완전히 제거합니다.
    /// </summary>
    void ClearAllCards();

    /// <summary>
    /// 현재 핸드 슬롯의 상태를 디버깅 로그로 출력합니다.
    /// </summary>
    void LogHandSlotStates();

    /// <summary>
    /// 지정된 슬롯에서 카드를 제거하고 UI를 반환합니다.
    /// </summary>
    SkillCardUI RemoveCardFromSlot(SkillCardSlotPosition pos);

    /// <summary>
    /// 지정된 슬롯의 카드와 UI를 제거하고 반환합니다.
    /// </summary>
    (ISkillCard card, SkillCardUI ui) PopCardFromSlot(SkillCardSlotPosition pos);

    /// <summary>
    /// 사용 가능한 첫 슬롯의 카드와 UI를 제거하고 반환합니다.
    /// </summary>
    (ISkillCard card, SkillCardUI ui) PopFirstAvailableCard();

    /// <summary>
    /// 주어진 슬롯에 배치할 카드를 선택합니다.
    /// </summary>
    ISkillCard PickCardForSlot(SkillCardSlotPosition pos);

    /// <summary>
    /// 카드와 UI를 슬롯에 등록합니다.
    /// </summary>
    void RegisterCardToSlot(SkillCardSlotPosition pos, ISkillCard card, SkillCardUI ui);

    /// <summary>
    /// 지정된 적 캐릭터가 이미 초기화되어 있는지 확인합니다.
    /// </summary>
    bool HasInitializedEnemy(IEnemyCharacter enemy);

    /// <summary>
    /// 카드 하나를 꺼내 전투 슬롯에 등록하고, 슬롯 위치까지 함께 반환합니다.
    /// </summary>
    (ISkillCard card, SkillCardUI ui, CombatSlotPosition pos) PopCardAndRegisterToCombatSlot(ICombatFlowCoordinator coordinator);
}
