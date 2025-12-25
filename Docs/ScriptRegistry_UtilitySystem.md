## UtilitySystem 스크립트 레지스트리

**루트 폴더**: `Assets/Script/UtilitySystem/`  
**목적**: 공통 게임 흐름/씬 로딩/오브젝트 생존, 전투 슬롯 드랍 핸들러 주입, UI 애니메이션/호버 효과/Transform 확장 등 게임 전역에 걸친 보조 유틸리티를 제공  
**비고**: CoreSystem/CombatSystem/StageSystem/UISystem/CharacterSystem/SkillCardSystem에서 직접 또는 간접적으로 사용  
**최신 업데이트**: 리팩토링으로 UIAnimationHelper, HoverEffectHelper, TransformExtensions, BaseTooltipManager 추가됨

---

## 스크립트 목록

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 역할 | 주요 공개 메서드(대표) | 주요 필드/프로퍼티(대표) | Zenject 바인딩(있으면) | 주요 참조자(사용처) | 상태 |
|--------------|--------------|-----------|------|------------------------|---------------------------|------------------------|----------------------|------|
| **DropHandlerInjector** | `Game.UtilitySystem` | `DropHandlerInjector.cs` | Combat 슬롯들에 카드 드랍 핸들러를 주입하기 위한 정적 유틸리티 (현재는 새 시스템으로 단순화) | `InjectToAllCombatSlots(CardDropService dropService)` | 없음 (정적 메서드만 존재) | DI 바인딩 없음 (정적 헬퍼) | Combat 초기화 로직/실험 코드에서 사용 가능, 주석 상 "실제 슬롯 주입은 다른 시스템에서 처리"로 정리됨 | ✅ 사용 중 (헬퍼, 레거시 통합 후보) |
| **DontDestroyOnLoadContainer** | `Game.UtilitySystem` | `DontDestroyOnLoadContainer.cs` | 씬 전환 시 유지되어야 하는 오브젝트들을 자식으로 두는 컨테이너 | `Awake()` 등 | 내부 Transform/리스트 | 씬 컴포넌트, DI 없음 | Core/Combat/Stage 등 전역 오브젝트 관리 | ✅ 사용 중 |
| **IGameContext / ISceneLoader** | `Game.UtilitySystem.GameFlow` | `GameFlow/IGameContext.cs`, `GameFlow/ISceneLoader.cs` | 게임 컨텍스트/씬 로더 인터페이스 | `LoadSceneAsync(...)` 등 | - | 인터페이스 타입 | GameContext, SceneTransitionManager/Stage 흐름과 연계 가능 | ✅ 사용 중 |
| **GameContext** | `Game.UtilitySystem.GameFlow` | `GameFlow/GameContext.cs` | 게임 전체 상태/컨텍스트를 나타내는 유틸리티(씬/모드/세션 정보 등) | `Initialize(...)` 등 | 현재 씬/모드/플래그 | DI 또는 정적 접근(코드 참조 있음) | Core/Stage/Combat 흐름 제어 보조 | ✅ 사용 중 |
| **UIAnimationHelper** | `Game.UtilitySystem` | `UIAnimationHelper.cs` | UI 페이드 인/아웃 애니메이션 공통 헬퍼 (DOTween 기반) | `FadeIn(...)`, `FadeOut(...)` | - | 정적 클래스, DI 없음 | ItemTooltip, SkillCardTooltip, BuffDebuffTooltip, TutorialOverlayView 등 UI 컴포넌트 | ✅ 사용 중 |
| **HoverEffectHelper** | `Game.UtilitySystem` | `HoverEffectHelper.cs` | 호버 스케일 효과 공통 헬퍼 (DOTween 기반) | `PlayHoverScale(...)`, `ResetScale(...)` | - | 정적 클래스, DI 없음 | SkillCardUI, ActiveItemUI, PassiveItemIcon, RewardSlotUIController, BuffDebuffIcon 등 UI 컴포넌트 | ✅ 사용 중 |
| **TransformExtensions** | `Game.UtilitySystem` | `TransformExtensions.cs` | Transform 확장 메서드 (자식 찾기 등) | `FindChildByName(...)` | - | 정적 Extension 메서드, DI 없음 | SkillCardTooltip 등 UI 컴포넌트 | ✅ 사용 중 |
| **BaseTooltipManager** | `Game.UtilitySystem` | `BaseTooltipManager.cs` | 모든 툴팁 매니저의 공통 기능을 제공하는 추상 베이스 클래스 (제네릭) | `Initialize()`, `ForceHideTooltip()` 등 | `tooltipPrefab`, `currentTooltip`, `showDelay`, `hideDelay` | 추상 클래스, 직접 DI 바인딩 없음 (상속용) | `ItemTooltipManager`, `SkillCardTooltipManager`, `BuffDebuffTooltipManager` 등 툴팁 매니저들의 베이스 | ✅ 사용 중 |

> **사용 여부 메모**: UtilitySystem 폴더는 규모가 작고, 대부분 정적 헬퍼/인터페이스/컨텍스트 역할을 합니다.  
> `BaseTooltipManager`는 추상 베이스 클래스로, 다른 시스템의 툴팁 매니저들이 공통 기능을 재사용할 수 있도록 제공됩니다.  
> `DropHandlerInjector`는 새 Drag&Drop 시스템으로 인해 실제 로직이 대부분 이동했지만, 주석과 구조상 아직 레거시 호환/헬퍼로 남아 있으므로 `✅ 사용 중 (통합/정리 후보)`로 분류했습니다.

---

## 스크립트 상세 분석 (레벨 3)

### GameContext

#### 클래스 구조

```csharp
public class GameContext : IGameContext
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_currentSceneName` | `string` | `private` | 빈 문자열 | 현재 씬 이름 | 디버그/로직에서 현재 씬을 식별 |
| `_gameMode` | `GameMode` (enum) | `private` | 기본 모드 | 게임 모드 | 메인 메뉴/싱글 전투/튜토리얼 등 모드 구분 |
| `_flags` | `Dictionary<string, bool>` | `private` | 빈 | 플래그 테이블 | 튜토리얼 표시 여부, 특정 연출 완료 여부 등 상태 플래그 저장 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `Initialize(string sceneName, GameMode mode)` | `void` | `string sceneName, GameMode mode` | `public` | 1. `_currentSceneName`와 `_gameMode` 설정<br>2. 내부 플래그 초기화 | 새 씬 또는 모드로 진입할 때 컨텍스트를 재설정 |
| `SetFlag(string key, bool value)` | `void` | `string key, bool value` | `public` | 1. null/빈 문자열 검사<br>2. `_flags[key] = value` 할당 | 전역 수준의 간단한 상태 플래그 설정 |
| `GetFlag(string key)` | `bool` | `string key` | `public` | 1. 키 존재 여부 확인<br>2. 없으면 false 반환<br>3. 있으면 해당 값 반환 | 다른 시스템에서 컨텍스트 플래그를 조회 |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `ISceneLoader` 구현체 / SceneTransitionManager | 인터페이스 조합 | 씬 로딩 시 컨텍스트 업데이트 | 어떤 씬/모드로 이동했는지 기록 |
| `ItemService` / `TutorialManager` 등 | 매개변수 또는 DI | 게임 컨텍스트 → 로직 분기 | 모드/플래그에 따라 동작을 다르게 할 때 사용 |

---

## 레거시/미사용 코드 정리

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 상태 | 비고 |
|--------------|--------------|-----------|------|------|
| **DropHandlerInjector** | `Game.UtilitySystem` | `DropHandlerInjector.cs` | 🟡 레거시/통합 후보 | 싱글게임용 전투 슬롯 드랍 핸들러 일괄 주입용 정적 유틸입니다. 주석 기준으로 실제 슬롯 주입 로직은 새로운 시스템으로 이동했으며, 이 스크립트는 현재 호환성/실험용 헬퍼 위치에 있습니다. 완전 전환 후에는 제거 또는 신규 Drag&Drop 구조에 맞게 통합하는 리팩터링 후보입니다. |

그 외 `DontDestroyOnLoadContainer`, `GameContext`/`IGameContext`/`ISceneLoader`는 전역 오브젝트 생존 및 게임 컨텍스트/씬 로딩 흐름에서 여전히 사용 중입니다.

---

## 폴더 구조

```text
Assets/Script/UtilitySystem/
├── BaseTooltipManager.cs
├── DontDestroyOnLoadContainer.cs
├── DropHandlerInjector.cs
├── HoverEffectHelper.cs
├── TransformExtensions.cs
├── UIAnimationHelper.cs
└── GameFlow/
    ├── GameContext.cs
    ├── IGameContext.cs
    └── ISceneLoader.cs
```

**참고**: `SkillCardConfigExtensions.cs`는 `Assets/Script/SkillCardSystem/Utility/`에 위치하지만, UtilitySystem과 유사한 역할을 합니다.


