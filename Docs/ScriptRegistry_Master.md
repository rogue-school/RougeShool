## RougeShool 스크립트 레지스트리 마스터 문서

**대상 루트**: `Assets/Script/` 전체  
**목적**: 각 시스템별 스크립트 레지스트리 문서를 한눈에 파악하고, 핵심 허브 스크립트/Zenject 바인딩/레거시 후보를 빠르게 조회하기 위한 상위 인덱스

---

## 시스템별 개요 및 문서 링크

| 시스템 | 루트 폴더 | 주요 책임 | 대표 허브 스크립트(레벨 3 분석 대상) | 스크립트 수(실제 `.cs`) | 레지스트리 문서 |
|--------|-----------|-----------|-------------------------------------|-------------------------|------------------|
| **CoreSystem** | `Assets/Script/CoreSystem/` | 오디오, 세이브, 통계, 씬 전환, 코어 매니저/유틸 | `CoreSystemInstaller`, `MainSceneInstaller`, `GameStateManager`, `AudioManager`, `SaveManager`, `GameLogger`, `ComponentInteractionOptimizer` | 36 | `Docs/ScriptRegistry_CoreSystem.md` |
| **CharacterSystem** | `Assets/Script/CharacterSystem/` | 플레이어/적 캐릭터 데이터·코어 로직·매니저·UI·페이즈 시스템 | `EnemyCharacter`, `PlayerCharacter`, `EnemyPhaseData` | 41 | `Docs/ScriptRegistry_CharacterSystem.md` |
| **CombatSystem** | `Assets/Script/CombatSystem/` | 전투 상태 머신, 턴/슬롯/실행, 전투 UI, 슬롯 이동/적 덱 캐시 관리 | `CombatInstaller`, `CombatStateMachine`, `SlotMovementController` | 63 | `Docs/ScriptRegistry_CombatSystem.md` |
| **SkillCardSystem** | `Assets/Script/SkillCardSystem/` | 카드 정의/팩토리/이펙트/덱·핸드/슬롯/툴팁 | `SkillCardFactory`, `CardCirculationSystem` | 103 | `Docs/ScriptRegistry_SkillCardSystem.md` |
| **ItemSystem** | `Assets/Script/ItemSystem/` | 액티브/패시브 아이템, 보상, 인벤토리/UI | `ItemService`, `RewardGenerator` | 52 | `Docs/ScriptRegistry_ItemSystem.md` |
| **StageSystem** | `Assets/Script/StageSystem/` | 스테이지 데이터/진행/적 생성·소환·보상 트리거 | `StageManager` | 8 | `Docs/ScriptRegistry_StageSystem.md` |
| **SaveSystem** | `Assets/Script/SaveSystem/` | 진행/슬롯 저장·복원, 자동 저장 | `AutoSaveManager` | 5 | `Docs/ScriptRegistry_SaveSystem.md` |
| **TutorialSystem** | `Assets/Script/TutorialSystem/` | 전투 튜토리얼, 오버레이 UI | `TutorialManager` | 3 | `Docs/ScriptRegistry_TutorialSystem.md` |
| **UISystem** | `Assets/Script/UISystem/` | 메인 메뉴/설정/무기 선택 UI | `MainMenuController` | 11 | `Docs/ScriptRegistry_UISystem.md` |
| **UtilitySystem** | `Assets/Script/UtilitySystem/` | 게임 컨텍스트, DontDestroy 컨테이너, 드랍 헬퍼, 공통 헬퍼 클래스 | `GameContext`, `UIAnimationHelper`, `HoverEffectHelper`, `TransformExtensions` | 8 | `Docs/ScriptRegistry_UtilitySystem.md` |
| **VFXSystem** | `Assets/Script/VFXSystem/` | VFX/데미지 텍스트/버프 아이콘/카드 UI 풀링 | `VFXManager` | 7 | `Docs/ScriptRegistry_VFXSystem.md` |

> **스크립트 수 안내**: 스크립트 수는 `glob_file_search` 기준 실제 `.cs` 파일 개수입니다 (2024년 검증 완료). 레지스트리 테이블은 여러 스크립트를 한 줄로 묶어 설명하는 경우가 있으므로, **행 수와 파일 수는 1:1이 아니지만 모든 파일이 최소 한 번 이상 문서에 등장**합니다.  
> **최신 업데이트**: UtilitySystem에 최근 리팩토링으로 추가된 헬퍼 클래스들(UIAnimationHelper, HoverEffectHelper, TransformExtensions)이 반영되었습니다.

---

## Zenject 바인딩/DI 허브 정리

| 시스템 | 주요 Zenject Installer / DI 허브 | 핵심 바인딩 요약 |
|--------|----------------------------------|-------------------|
| **CoreSystem** | `CoreSystemInstaller`, `MainSceneInstaller` | 코어 매니저(게임 상태, 씬 전환, 오디오, 세이브, 통계, 리더보드, 캐릭터 선택, 코루틴 실행기 등)를 AsSingle로 바인딩하고, MainScene에서 전역 매니저들을 재바인딩 |
| **CombatSystem** | `CombatInstaller` | 전투 턴/상태/실행/슬롯/통합 매니저/카드 순환/드랍 서비스/StageManager/AutoSaveManager 등을 AsSingle로 바인딩 |
| **SkillCardSystem** | `CardInstaller` | 카드 팩토리, 카드 레지스트리, 실행 컨텍스트/검증기/툴팁 매니저 등을 AsSingle로 구성 |
| **ItemSystem** | `RewardInstaller`, `CoreSystemInstaller.BindCoreServices` | `IItemService`, `IRewardGenerator`, `DefaultRewardService`를 전역 서비스로 등록 |
| **SaveSystem** | `SaveSystemInstaller` | AutoSaveManager/StageProgressCollector 등 저장 보조 컴포넌트 등록 |
| **StageSystem** | CombatInstaller 내 바인딩 | StageManager를 `IStageManager`로 전투 씬에 노출 |
| **UISystem** | 없음(씬 컴포넌트 + DI) | `MainMenuController`/`SettingsUIController` 등이 CoreSystem 인터페이스를 DI로 사용 |
| **UtilitySystem** | Core/Combat 인스톨러들과 조합 | `GameContext`/`ISceneLoader` 등은 다른 시스템에서 컨텍스트/씬 흐름을 추상화 |
| **VFXSystem** | Core/Combat 인스톨러에서 찾기/생성 | `VFXManager`를 씬에서 찾거나 생성해 AsSingle로 바인딩, `IVFXManager` 인터페이스(아이템 시스템)와 연동 |

자세한 바인딩 시그니처는 각 시스템 레지스트리의 **Zenject 바인딩(있으면)** 컬럼과 Core/Combat/Item/Save 쪽 레벨 3 상세 분석을 참고합니다.

---

## 레거시/미사용·통합 후보 스크립트 요약

각 시스템 레지스트리의 “레거시/미사용 코드 정리” 섹션에서 정리한 내용을 한 번에 모아봅니다.

| 스크립트 이름 | 네임스페이스 | 위치 | 상태 | 비고 |
|--------------|--------------|------|------|------|
| **DIOptimizationUtility** | `Game.CoreSystem.Utility` | `CoreSystem/Utility/DIOptimizationUtility.cs` | 🟡 레거시/미사용 헬퍼 | Zenject DI 최적화/검증용 정적 유틸로 설계되었으나, 현재 grep 기준 호출 지점이 없습니다. 추후 DI 구조 점검 시 재활용하거나, 사용 계획이 없으면 삭제 후보입니다. |
| **play / Xbutton / Newgame / ExitGame** | `Game.UISystem` | `UISystem/play.cs` 등 | 🟡 레거시/통합 후보 | 개별 버튼 OnClick에 직접 연결되는 레거시 스타일 핸들러입니다. 현재 일부 버튼에서 여전히 사용 중이며, 장기적으로는 `MainMenuController`/`PanelManager`로 로직을 통합 후 제거하는 리팩터링 후보입니다. |
| **DropHandlerInjector** | `Game.UtilitySystem` | `UtilitySystem/DropHandlerInjector.cs` | 🟡 레거시/통합 후보 | 싱글게임용 Combat 슬롯 드랍 핸들러 일괄 주입 정적 유틸입니다. 실제 슬롯 주입 로직은 새 Drag&Drop 시스템으로 이동했고, 이 스크립트는 호환·실험용 헬퍼 위치에 있습니다. 전환 완료 시 제거 또는 신규 구조에 맞춘 통합 대상입니다. |

> 그 외 시스템(Core/Character/Combat/SkillCard/Item/Save/Stage/Tutorial/VFX/UI/Utility)의 나머지 스크립트들은 grep/Installer/씬 컴포넌트 기준으로 **실제 실행 경로가 확인된 활성 스크립트**입니다.

---

## 레벨 3 상세 분석 대상 요약

각 레지스트리에서 레벨 3(깊은 분석)이 작성된 핵심 스크립트 목록입니다.

- **CoreSystem**: `CoreSystemInstaller`, `MainSceneInstaller`, `GameStateManager`, `AudioManager`, `SaveManager`, `GameLogger`, `ComponentInteractionOptimizer`  
- **CharacterSystem**: `EnemyCharacter`, `PlayerCharacter`, `EnemyPhaseData`  
- **CombatSystem**: `CombatInstaller`, `CombatStateMachine`, `SlotMovementController`  
- **SkillCardSystem**: `SkillCardFactory`, `CardCirculationSystem`  
- **ItemSystem**: `ItemService`, `RewardGenerator`  
- **SaveSystem**: `AutoSaveManager`  
- **StageSystem**: `StageManager`  
- **TutorialSystem**: `TutorialManager`  
- **UISystem**: `MainMenuController`  
- **UtilitySystem**: `GameContext`  
- **VFXSystem**: `VFXManager`  

세부 필드/함수/로직 흐름도/연결 관계는 각 시스템별 `## 스크립트 상세 분석 (레벨 3)` 섹션에서 확인할 수 있습니다.

---

## 활용 가이드

- **시스템 단위 구조 파악**이 필요할 때: 이 마스터 문서의 **시스템별 개요 표**에서 책임/허브 스크립트를 보고, 해당 시스템 레지스트리로 이동합니다.
- **DI/연결 관계 조사**가 필요할 때: **Zenject 바인딩/DI 허브 정리** 표에서 어느 Installer가 어떤 인터페이스를 바인딩하는지 확인하고, 세부 구현은 Core/Combat/Item/Save 레지스트리의 레벨 3 분석을 참고합니다.
- **레거시/정리 후보 파악**이 필요할 때: **레거시/미사용·통합 후보 스크립트 요약** 표를 기준으로 실제 제거/통합 작업을 진행할 수 있습니다.


