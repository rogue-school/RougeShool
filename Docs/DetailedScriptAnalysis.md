# RougeShool 스크립트 상세 분석 및 재작성 계획

> 작성일: 2025-11-24  
> 목적: 모든 스크립트를 하나하나 체크하여 제거할 코드와 새로 작성할 코드를 정확하게 판단

---

## 📊 전체 스크립트 현황

- **총 스크립트 파일**: 326개
- **체크 완료**: 326개 (100%)
- **네임스페이스 불일치**: 3개 발견
- **네임스페이스 없는 파일**: 5개 발견
- **사용하지 않는 스크립트**: 12개 발견
- **재작성 필요**: 5개 발견
- **레거시 코드 제거**: 2개 발견

> **상세 체크리스트**: [전체 스크립트 체크리스트](./CompleteScriptChecklist.md) 참조

---

## 🗑️ 즉시 삭제 대상 스크립트

### 1. 테스트/디버그 코드

#### `Assets/Script/ItemSystem/Runtime/TestItemButton.cs`
- **문제**: 프로덕션 코드에 테스트 코드 포함
- **참조**: 자기 자신만 참조
- **조치**: ✅ **완전 삭제**

---

#### `Assets/Script/UISystem/play.cs`
- **문제**: 
  - 네임스페이스 없음
  - 소문자 클래스명 (`play`)
  - 유저룰 위반
- **참조**: 자기 자신만 참조 (사용 안함)
- **조치**: ✅ **완전 삭제**

**제거할 코드**:
```csharp
// ❌ 전체 파일 삭제
using UnityEngine;

public class play : MonoBehaviour
{
    public GameObject weaponSelectionImage;

    public void ShowWeaponSelection()
    {
        if (weaponSelectionImage != null)
        {
            weaponSelectionImage.SetActive(true);
        }
    }
}
```

---

#### `Assets/Script/UISystem/Xbutton.cs`
- **문제**: 
  - 네임스페이스 없음
  - 클래스명 오타 (`Xbotton` → `Xbutton`)
  - 유저룰 위반
- **참조**: 자기 자신만 참조 (사용 안함)
- **조치**: ✅ **완전 삭제**

**제거할 코드**:
```csharp
// ❌ 전체 파일 삭제
using UnityEngine;

public class Xbotton : MonoBehaviour
{
    public GameObject targetToHide;

    public void HideTarget()
    {
        if (targetToHide != null)
        {
            targetToHide.SetActive(false);
        }
    }
}
```

---

### 2. 사용하지 않는 유틸리티 클래스

#### `Assets/Script/CoreSystem/Utility/DIOptimizationUtility.cs`
- **문제**: 자기 자신만 참조 (실제 사용 안함)
- **참조**: 자기 자신만 참조
- **조치**: ✅ **완전 삭제** 또는 Editor 폴더로 이동

**제거할 코드**: 전체 파일 (289줄)

---

#### `Assets/Script/CoreSystem/Utility/ComponentInteractionOptimizer.cs`
- **문제**: ComponentRoleManager만 참조 (실제 사용 안함)
- **참조**: ComponentRoleManager.cs에서만 참조
- **조치**: ✅ **완전 삭제** 또는 Editor 폴더로 이동

**제거할 코드**: 전체 파일 (299줄)

---

#### `Assets/Script/CoreSystem/Utility/ComponentRoleManager.cs`
- **문제**: ComponentInteractionOptimizer만 참조 (실제 사용 안함)
- **참조**: ComponentInteractionOptimizer.cs에서만 참조
- **조치**: ✅ **완전 삭제** 또는 Editor 폴더로 이동

**제거할 코드**: 전체 파일 (약 200줄)

---

#### `Assets/Script/UtilitySystem/DontDestroyOnLoadContainer.cs`
- **문제**: 자기 자신만 참조 (사용 안함)
- **참조**: 자기 자신만 참조
- **조치**: ✅ **완전 삭제** 또는 기능이 필요하면 재작성

**제거할 코드**: 전체 파일 (143줄)

---

#### `Assets/Script/UtilitySystem/DropHandlerInjector.cs`
- **문제**: 
  - 주석 처리된 코드 포함
  - 실제 기능 없음 (로그만 출력)
  - "CombatSlotManager 제거로 인한 단순화" 주석
- **참조**: 자기 자신만 참조
- **조치**: ✅ **완전 삭제**

**제거할 코드**:
```csharp
// ❌ 전체 파일 삭제
// 실제 기능 없음, 주석 처리된 코드만 존재
```

---

### 3. 사용하지 않는 상태 클래스

#### `Assets/Script/CombatSystem/Core/DefaultCombatState.cs`
- **문제**: 자기 자신만 참조 (실제 사용 안함)
- **참조**: 자기 자신만 참조
- **조치**: ✅ **완전 삭제**

**제거할 코드**: 전체 파일 (64줄)

---

### 4. 사용하지 않는 헬퍼 클래스

#### `Assets/Script/CharacterSystem/Data/PlayerCharacterTypeHelper.cs`
- **문제**: 자기 자신만 참조 (실제 사용 안함)
- **참조**: 자기 자신만 참조
- **조치**: ✅ **완전 삭제** 또는 PlayerCharacterData에 통합

**제거할 코드**: 전체 파일 (59줄)

---

#### `Assets/Script/CombatSystem/Utility/SlotSelector.cs`
- **문제**: 
  - TODO 주석 포함
  - `object` 타입 사용 (타입 안전성 없음)
  - 실제 기능 미구현
- **참조**: CombatInstaller.cs에서만 참조 (사용 여부 불명확)
- **조치**: ⚠️ **검토 후 삭제 또는 재작성**

**제거할 코드**:
```csharp
// ❌ 제거할 부분
private readonly object combatSlotRegistry; // TODO: 적절한 타입으로 교체 필요
var allSlots = new List<ICombatCardSlot>(); // 임시로 빈 리스트 반환
```

---

#### `Assets/Script/SkillCardSystem/Manager/BaseSkillCardManager.cs`
- **문제**: 자기 자신만 참조 (상속받는 클래스 없음)
- **참조**: 자기 자신만 참조
- **조치**: ✅ **완전 삭제** 또는 실제 사용 클래스가 있으면 유지

**제거할 코드**: 전체 파일 (95줄)

---

#### `Assets/Script/ItemSystem/Service/Reward/RewardInstaller.cs`
- **문제**: 자기 자신만 참조 (사용 안함)
- **참조**: 자기 자신만 참조
- **조치**: ✅ **완전 삭제** 또는 실제 사용 시 재작성

**제거할 코드**: 전체 파일

---

#### `Assets/Script/ItemSystem/Data/Reward/RewardProfile.cs`
- **문제**: DefaultRewardPolicy만 사용 (ScriptableObject 미사용)
- **참조**: IRewardGenerator.cs에서만 참조
- **조치**: ⚠️ **검토 후 삭제 또는 재작성**

**제거할 코드**: ScriptableObject 부분 (enum은 유지 가능)

---

## 🔧 네임스페이스 불일치 수정

### 1. CardDragHandler.cs

**현재 상태**:
- **파일 위치**: `Assets/Script/SkillCardSystem/DragDrop/CardDragHandler.cs`
- **네임스페이스**: `Game.CombatSystem.DragDrop` ❌
- **문제**: 폴더와 네임스페이스 불일치

**수정 방안**:
```csharp
// ❌ 현재 (잘못된 네임스페이스)
namespace Game.CombatSystem.DragDrop

// ✅ 수정 (폴더 구조에 맞게)
namespace Game.SkillCardSystem.DragDrop
```

**또는 파일 이동**:
- `Assets/Script/CombatSystem/DragDrop/CardDragHandler.cs`로 이동
- 네임스페이스 유지

**권장**: 네임스페이스 수정 (SkillCardSystem이 더 적절)

---

### 2. SlotInitializationStep.cs

**현재 상태**:
- **파일 위치**: `Assets/Script/CombatSystem/Initialization/SlotInitializationStep.cs`
- **네임스페이스**: `Game.CombatSystem.Intialization` ❌ (오타)
- **문제**: 네임스페이스 오타

**수정 방안**:
```csharp
// ❌ 현재 (오타)
namespace Game.CombatSystem.Intialization

// ✅ 수정
namespace Game.CombatSystem.Initialization
```

---

### 3. CardInstaller.cs

**현재 상태**:
- **파일 위치**: `Assets/Script/SkillCardSystem/Installer/CardInstaller.cs`
- **네임스페이스**: `Game.SkillCardSystem.Installation` ❌
- **문제**: 폴더명과 네임스페이스 불일치

**수정 방안**:
```csharp
// ❌ 현재
namespace Game.SkillCardSystem.Installation

// ✅ 수정
namespace Game.SkillCardSystem.Installer
```

---

## 📝 코드 품질 문제 수정

### 1. ExitGame.cs

**현재 상태**:
- **파일**: `Assets/Script/UISystem/ExitGame.cs`
- **문제**: 
  - 인코딩 문제 (한글 깨짐)
  - `Debug.Log` 사용 (GameLogger 사용해야 함)
  - 네임스페이스 없음

**제거할 코드**:
```csharp
// ❌ 현재 코드
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log(" մϴ.");  // 인코딩 문제
        Application.Quit(); //
    }
}
```

**새로 작성할 코드**:
```csharp
// ✅ 완전히 새로 작성
using UnityEngine;
using Game.CoreSystem.Utility;

namespace Game.UISystem
{
    /// <summary>
    /// 게임 종료를 처리하는 컨트롤러
    /// </summary>
    public class ExitGameController : MonoBehaviour
    {
        /// <summary>
        /// 게임을 종료합니다
        /// </summary>
        public void QuitGame()
        {
            GameLogger.LogInfo("게임을 종료합니다", GameLogger.LogCategory.UI);
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
```

**변경 사항**:
- 클래스명: `ExitGame` → `ExitGameController`
- 네임스페이스 추가: `Game.UISystem`
- `Debug.Log` → `GameLogger.LogInfo`
- 인코딩 문제 해결
- Editor 모드 지원 추가

---

### 2. Newgame.cs

**현재 상태**:
- **파일**: `Assets/Script/UISystem/Newgame.cs`
- **문제**: 
  - 클래스명 소문자 시작 (`Newgame`)
  - `Debug.Log` 사용

**제거할 코드**:
```csharp
// ❌ 현재 코드
Debug.Log($"[NewGame] 새 게임 시작 - 씬: {sceneToLoad}");
```

**새로 작성할 코드**:
```csharp
// ✅ 수정
GameLogger.LogInfo($"[NewGameController] 새 게임 시작 - 씬: {sceneToLoad}", GameLogger.LogCategory.UI);
```

**변경 사항**:
- 클래스명: `Newgame` → `NewGameController`
- `Debug.Log` → `GameLogger.LogInfo`

---

### 3. WeaponSelector.cs

**현재 상태**:
- **파일**: `Assets/Script/UISystem/WeaponSelector.cs`
- **문제**: 
  - 네임스페이스 없음
  - 유저룰 미준수

**제거할 코드**:
```csharp
// ❌ 현재 코드
using UnityEngine;

public class WeaponSelector : MonoBehaviour
{
    public GameObject weaponSelectionImage;

    public void ShowWeaponSelection()
    {
        if (weaponSelectionImage != null)
        {
            weaponSelectionImage.SetActive(true);
        }
    }
}
```

**새로 작성할 코드**:
```csharp
// ✅ 완전히 새로 작성
using UnityEngine;
using Game.CoreSystem.Utility;

namespace Game.UISystem
{
    /// <summary>
    /// 무기 선택 UI를 제어하는 컨트롤러
    /// </summary>
    public class WeaponSelectorController : MonoBehaviour
    {
        #region Inspector Fields

        [Header("무기 선택 UI")]
        [Tooltip("무기 선택 이미지 GameObject")]
        [SerializeField] private GameObject weaponSelectionImage;

        #endregion

        #region Public Methods

        /// <summary>
        /// 무기 선택 UI를 표시합니다
        /// </summary>
        public void ShowWeaponSelection()
        {
            if (weaponSelectionImage == null)
            {
                GameLogger.LogWarning("무기 선택 이미지가 할당되지 않았습니다", GameLogger.LogCategory.UI);
                return;
            }

            weaponSelectionImage.SetActive(true);
            GameLogger.LogInfo("무기 선택 UI 표시", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 무기 선택 UI를 숨깁니다
        /// </summary>
        public void HideWeaponSelection()
        {
            if (weaponSelectionImage == null)
            {
                return;
            }

            weaponSelectionImage.SetActive(false);
            GameLogger.LogInfo("무기 선택 UI 숨김", GameLogger.LogCategory.UI);
        }

        #endregion
    }
}
```

**변경 사항**:
- 네임스페이스 추가: `Game.UISystem`
- 클래스명: `WeaponSelector` → `WeaponSelectorController`
- Inspector 한글화 추가
- 예외 처리 추가
- GameLogger 사용
- XML 문서화 추가

---

## 🏗️ 네임스페이스 및 폴더 구조 개선안

### 현재 문제점

1. **네임스페이스 불일치**: 폴더 구조와 네임스페이스가 일치하지 않음
2. **네임스페이스 오타**: `Intialization` → `Initialization`
3. **폴더명 불일치**: `Installer` vs `Installation`

### 개선된 네임스페이스 구조

```
Game.
├── CoreSystem/
│   ├── Audio/
│   ├── Manager/
│   ├── Save/
│   ├── Statistics/
│   ├── UI/
│   └── Utility/
├── CombatSystem/
│   ├── Core/
│   ├── Data/
│   ├── DragDrop/
│   ├── Event/
│   ├── Factory/
│   ├── Initialization/  ← 오타 수정
│   ├── Interface/
│   ├── Manager/
│   ├── Service/
│   ├── Slot/
│   ├── State/
│   ├── UI/
│   └── Utility/
├── CharacterSystem/
│   ├── Core/
│   ├── Data/
│   ├── Effect/
│   ├── Initialization/
│   ├── Interface/
│   ├── Manager/
│   ├── Slot/
│   ├── UI/
│   └── Utility/
├── SkillCardSystem/
│   ├── Data/
│   ├── Deck/
│   ├── DragDrop/
│   ├── Editor/
│   ├── Effect/
│   ├── Executor/
│   ├── Factory/
│   ├── Installer/  ← 네임스페이스 수정
│   ├── Interface/
│   ├── Manager/
│   ├── Runtime/
│   ├── Service/
│   ├── Slot/
│   ├── UI/
│   └── Validator/
├── ItemSystem/
│   ├── Cache/
│   ├── Constants/
│   ├── Data/
│   ├── Editor/
│   ├── Effect/
│   ├── Interface/
│   ├── Manager/
│   ├── Runtime/
│   ├── Service/
│   ├── UI/
│   └── Utility/
├── StageSystem/
│   ├── Data/
│   ├── Interface/
│   ├── Manager/
│   ├── State/
│   └── UI/
├── SaveSystem/
│   ├── Data/
│   ├── Installer/
│   └── Manager/
├── UISystem/
│   └── (모든 UI 컨트롤러)
├── TutorialSystem/
│   └── Editor/
├── VFXSystem/
│   ├── Component/
│   ├── Manager/
│   └── Pool/
└── UtilitySystem/
    └── GameFlow/
```

### 네임스페이스 규칙

1. **폴더 구조와 일치**: 네임스페이스는 폴더 구조를 반영
2. **PascalCase**: 모든 네임스페이스는 PascalCase
3. **일관성**: 동일한 폴더 구조는 동일한 네임스페이스

---

## 📋 파일별 상세 재작성 계획

### CoreSystem

#### 1. SaveManager.cs

**제거할 코드**:
```csharp
// ❌ FindObjectOfType 캐싱 (63-120줄)
#region FindObjectOfType 캐싱 헬퍼

private Game.StageSystem.Manager.StageManager cachedStageManager;
private Game.CombatSystem.Manager.TurnManager cachedTurnManager;
// ... 8개 매니저 캐싱

private Game.StageSystem.Manager.StageManager GetCachedStageManager()
{
    if (cachedStageManager == null)
        cachedStageManager = FindObjectOfType<Game.StageSystem.Manager.StageManager>();
    return cachedStageManager;
}
// ... 7개 GetCached 메서드

#endregion
```

**새로 작성할 코드**:
```csharp
// ✅ 완전히 새로 작성
namespace Game.CoreSystem.Save
{
    /// <summary>
    /// 게임 저장/로드를 담당하는 매니저
    /// </summary>
    public class SaveManager : MonoBehaviour, ISaveManager
    {
        #region Constants

        private const string SAVE_FILE_NAME = "GameSave.json";
        private const string STAGE_PROGRESS_FILE_NAME = "StageProgress.json";
        private const string KEY_BGM_VOLUME = "audio_bgm_volume";
        private const string KEY_SFX_VOLUME = "audio_sfx_volume";
        private const string KEY_PLAYER_DECK_CONFIG = "player_deck_configuration";

        #endregion

        #region Dependency Injection

        [Inject] private IGameStateManager gameStateManager;
        [Inject] private IStageManager stageManager;
        [Inject] private ITurnManager turnManager;
        [Inject] private ICombatFlowManager combatFlowManager;
        [Inject] private IPlayerManager playerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private ICardSlotRegistry slotRegistry;
        [Inject] private IPlayerHandManager playerHandManager;

        #endregion

        #region Fields

        [Header("저장 설정")]
        [Tooltip("저장 파일 이름")]
        [SerializeField] private string saveFileName = SAVE_FILE_NAME;

        [Tooltip("스테이지 진행 파일 이름")]
        [SerializeField] private string stageProgressFileName = STAGE_PROGRESS_FILE_NAME;

        private StageProgressCollector progressCollector;

        #endregion

        #region Properties

        public bool IsInitialized { get; private set; } = false;

        #endregion

        // ... 나머지 구현
    }
}
```

**변경 사항**:
- FindObjectOfType 완전 제거
- 모든 의존성을 DI로 주입
- 상수 정의 추가
- Inspector 한글화
- XML 문서화

---

#### 2. SceneTransitionManager.cs

**제거할 코드**:
```csharp
// ❌ FindObjectOfType 사용 부분
```

**새로 작성할 코드**:
```csharp
// ✅ 완전히 새로 작성
namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// 씬 전환을 담당하는 매니저
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour, ISceneTransitionManager
    {
        #region Dependency Injection

        [Inject] private IGameStateManager gameStateManager;
        [Inject] private IAudioManager audioManager;

        #endregion

        #region Fields

        [Header("씬 전환 설정")]
        [Tooltip("전환 중 여부")]
        [SerializeField] private bool isTransitioning = false;

        #endregion

        #region Properties

        public bool IsTransitioning => isTransitioning;

        #endregion

        // ... 나머지 구현
    }
}
```

---

### CombatSystem

#### 3. TurnManager.cs

**제거할 코드**:
```csharp
// ❌ 레거시 TurnType enum (420-431줄)
#region 레거시 타입 정의 (하위 호환성)

/// <summary>
/// 레거시 턴 타입 (하위 호환성을 위해 유지)
/// </summary>
public enum TurnType
{
    Player,
    Enemy
}

#endregion

// ❌ 변환 메서드 (400-417줄)
#region 타입 변환 헬퍼

/// <summary>
/// 새로운 TurnType을 레거시 TurnType으로 변환
/// </summary>
private TurnType ConvertToLegacyTurnType(Interface.TurnType newType)
{
    return newType == Interface.TurnType.Player ? TurnType.Player : TurnType.Enemy;
}

/// <summary>
/// 레거시 TurnType을 새로운 TurnType으로 변환
/// </summary>
private Interface.TurnType ConvertToNewTurnType(TurnType legacyType)
{
    return legacyType == TurnType.Player ? Interface.TurnType.Player : Interface.TurnType.Enemy;
}

#endregion
```

**새로 작성할 코드**:
```csharp
// ✅ 완전히 새로 작성
namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 턴 관리를 담당하는 매니저
    /// </summary>
    public class TurnManager : MonoBehaviour, ITurnManager
    {
        #region Dependency Injection

        [Inject] private ITurnController turnController;

        #endregion

        #region Fields

        [Header("턴 관리 설정")]
        [Tooltip("현재 페이즈")]
        [SerializeField] private CombatPhase currentPhase = CombatPhase.Setup;

        #endregion

        // 레거시 TurnType enum 완전 제거
        // 변환 메서드 완전 제거
        // Interface.TurnType만 사용

        // ... 나머지 구현
    }
}
```

**변경 사항**:
- 레거시 `TurnType` enum 완전 제거
- 변환 메서드 완전 제거
- `Interface.TurnType`만 사용
- 모든 변수/메서드명 새 유저룰에 맞게 재명명

---

#### 4. TurnStartButtonHandler.cs

**제거할 코드**: 전체 파일

**이유**: 상태 패턴으로 전환되어 사용되지 않음

---

#### 5. SlotSelector.cs

**제거할 코드**:
```csharp
// ❌ TODO 주석 및 object 타입
private readonly object combatSlotRegistry; // TODO: 적절한 타입으로 교체 필요

// 임시로 빈 리스트 반환
var allSlots = new List<ICombatCardSlot>(); // 임시로 빈 리스트 반환
```

**새로 작성할 코드**:
```csharp
// ✅ 완전히 새로 작성
namespace Game.CombatSystem.Utility
{
    /// <summary>
    /// 전투 슬롯 선택을 담당하는 유틸리티
    /// </summary>
    public class SlotSelector
    {
        #region Fields

        private readonly ICardSlotRegistry slotRegistry;

        #endregion

        #region Constructor

        /// <summary>
        /// 슬롯 셀렉터를 초기화합니다
        /// </summary>
        /// <param name="slotRegistry">카드 슬롯 레지스트리</param>
        public SlotSelector(ICardSlotRegistry slotRegistry)
        {
            this.slotRegistry = slotRegistry ?? throw new ArgumentNullException(nameof(slotRegistry));
        }

        #endregion

        // ... 나머지 구현
    }
}
```

**변경 사항**:
- `object` 타입 → `ICardSlotRegistry` 인터페이스
- TODO 주석 제거
- 실제 구현 추가

---

### SkillCardSystem

#### 6. CardDragHandler.cs

**제거할 코드**: 없음 (기능은 사용 중)

**수정할 코드**:
```csharp
// ❌ 현재 (잘못된 네임스페이스)
namespace Game.CombatSystem.DragDrop

// ✅ 수정
namespace Game.SkillCardSystem.DragDrop
```

**변경 사항**:
- 네임스페이스만 수정 (폴더 구조에 맞게)

---

#### 7. CardInstaller.cs

**제거할 코드**: 없음

**수정할 코드**:
```csharp
// ❌ 현재 (폴더명과 불일치)
namespace Game.SkillCardSystem.Installation

// ✅ 수정
namespace Game.SkillCardSystem.Installer
```

---

### ItemSystem

#### 8. TestItemButton.cs

**제거할 코드**: 전체 파일 삭제

---

## 📊 네임스페이스 및 폴더 구조 재정의

### 새로운 네임스페이스 규칙

```
규칙:
1. 네임스페이스 = 폴더 구조 (정확히 일치)
2. 폴더명 = 네임스페이스 마지막 부분
3. 모든 네임스페이스는 Game.으로 시작
```

### 폴더 구조 개선안

#### 현재 구조 문제점
```
❌ SkillCardSystem/DragDrop/ → Game.CombatSystem.DragDrop (불일치)
❌ CombatSystem/Initialization/ → Game.CombatSystem.Intialization (오타)
❌ SkillCardSystem/Installer/ → Game.SkillCardSystem.Installation (불일치)
```

#### 개선된 구조
```
✅ SkillCardSystem/DragDrop/ → Game.SkillCardSystem.DragDrop
✅ CombatSystem/Initialization/ → Game.CombatSystem.Initialization
✅ SkillCardSystem/Installer/ → Game.SkillCardSystem.Installer
```

---

## 🔄 파일 이동 및 네임스페이스 수정 계획

### Phase 1: 네임스페이스 수정 (즉시)

| 파일 | 현재 네임스페이스 | 수정할 네임스페이스 | 작업 |
|------|------------------|-------------------|------|
| `CardDragHandler.cs` | `Game.CombatSystem.DragDrop` | `Game.SkillCardSystem.DragDrop` | 네임스페이스 수정 |
| `SlotInitializationStep.cs` | `Game.CombatSystem.Intialization` | `Game.CombatSystem.Initialization` | 네임스페이스 수정 (오타) |
| `CardInstaller.cs` | `Game.SkillCardSystem.Installation` | `Game.SkillCardSystem.Installer` | 네임스페이스 수정 |

### Phase 2: 사용하지 않는 파일 삭제 (즉시)

| 파일 | 이유 | 작업 |
|------|------|------|
| `TestItemButton.cs` | 테스트 코드 | 삭제 |
| `play.cs` | 네임스페이스 없음, 사용 안함 | 삭제 |
| `Xbutton.cs` | 네임스페이스 없음, 오타, 사용 안함 | 삭제 |
| `DefaultCombatState.cs` | 사용 안함 | 삭제 |
| `PlayerCharacterTypeHelper.cs` | 사용 안함 | 삭제 |
| `BaseSkillCardManager.cs` | 상속받는 클래스 없음 | 삭제 |
| `RewardInstaller.cs` | 사용 안함 | 삭제 |
| `DIOptimizationUtility.cs` | 사용 안함 | 삭제 또는 Editor로 이동 |
| `ComponentInteractionOptimizer.cs` | 사용 안함 | 삭제 또는 Editor로 이동 |
| `ComponentRoleManager.cs` | 사용 안함 | 삭제 또는 Editor로 이동 |
| `DontDestroyOnLoadContainer.cs` | 사용 안함 | 삭제 또는 재작성 |
| `DropHandlerInjector.cs` | 기능 없음 | 삭제 |

### Phase 3: 코드 품질 개선 (재작성)

| 파일 | 문제 | 작업 |
|------|------|------|
| `ExitGame.cs` | 인코딩, Debug.Log | 완전 재작성 |
| `Newgame.cs` | 클래스명, Debug.Log | 완전 재작성 |
| `WeaponSelector.cs` | 네임스페이스 없음 | 완전 재작성 |
| `SlotSelector.cs` | TODO, object 타입 | 완전 재작성 |

---

## 📝 변경 기록

| 날짜 | 담당 | 내용 |
|------|------|------|
| 2025-11-24 | Cursor AI | 스크립트 상세 분석 및 재작성 계획 초안 작성 |

---

## 🔗 관련 문서

- [완전 재작성 리팩토링 계획](./CompleteRefactoringPlan.md)
- [코드 로직 문서](./CodeLogicDocumentation.md)

