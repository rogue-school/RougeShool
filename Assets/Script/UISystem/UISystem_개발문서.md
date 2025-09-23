# UISystem 개발 문서

## 📋 시스템 개요
UISystem은 게임의 사용자 인터페이스를 관리하는 시스템입니다. 다양한 UI 컨트롤러들을 통해 게임의 모든 UI 요소를 통합적으로 관리합니다.

## 🏗️ 폴더 구조 (현재 상태)
```
UISystem/
├── MainSceneController.cs      # 메인 씬 컨트롤러
├── SettingsUIController.cs     # 설정 UI 컨트롤러
├── WeaponSelector.cs           # 무기 선택기
├── PanelManager.cs             # 패널 매니저
├── UnderlineHoverEffect.cs     # 언더라인 호버 효과
├── Newgame.cs                 # 새 게임
├── ExitGame.cs                 # 게임 종료
├── Xbutton.cs                  # X 버튼
├── play.cs                     # 플레이 버튼
└── UISystem_개발문서.md        # 개발 문서
```

## 스크립트 목록(1:1 매핑)
- UISystem/PanelManager.cs
- UISystem/BaseUIController.cs
- UISystem/SettingsUIController.cs
- UISystem/Newgame.cs
- UISystem/MainSceneController.cs
- UISystem/Xbutton.cs
- UISystem/play.cs
- UISystem/WeaponSelector.cs
- UISystem/UnderlineHoverEffect.cs
- UISystem/ExitGame.cs

## 📁 주요 컴포넌트 (현재 상태)

### 현재 구현된 파일들
- **MainSceneController.cs**: 메인 씬 컨트롤러 (4.1KB, 129줄)
- **SettingsUIController.cs**: 설정 UI 컨트롤러 (3.1KB, 116줄)
- **WeaponSelector.cs**: 무기 선택기 (289B, 15줄)
- **PanelManager.cs**: 패널 매니저 (586B, 23줄)
- **UnderlineHoverEffect.cs**: 언더라인 호버 효과 (1.5KB, 52줄)
- **Newgame.cs**: 새 게임 (651B, 20줄)
- **ExitGame.cs**: 게임 종료 (194B, 10줄)
- **Xbutton.cs**: X 버튼 (248B, 14줄)
- **play.cs**: 플레이 버튼 (279B, 15줄)

## 🎯 주요 기능 (현재 구현 상태)

### 1. 메인 씬 컨트롤러
- **씬 관리**: 메인 씬의 전체적인 관리
- **UI 조정**: 메인 씬 내 UI 요소들 조정
- **이벤트 처리**: 메인 씬 관련 이벤트 처리

### 2. 설정 UI
- **게임 설정**: 게임 관련 설정 관리
- **UI 설정**: UI 관련 설정 관리
- **설정 저장**: 설정값 저장 및 로드

### 3. 무기 선택기
- **무기 선택**: 플레이어 캐릭터 무기 선택
- **무기 정보**: 선택한 무기의 정보 표시
- **선택 확인**: 무기 선택 확인

### 4. 패널 관리
- **패널 전환**: 다양한 패널 간 전환
- **패널 상태**: 패널의 표시/숨김 상태 관리
- **패널 애니메이션**: 패널 전환 애니메이션

### 5. UI 효과
- **언더라인 호버**: 텍스트에 언더라인 호버 효과
- **버튼 효과**: 버튼 클릭 효과
- **전환 효과**: UI 전환 시 효과

### 6. 게임 제어
- **새 게임**: 새 게임 시작
- **게임 종료**: 게임 종료 처리
- **플레이**: 게임 플레이 시작

## 🔧 사용 방법

### 기본 사용법 (현재 구현 상태)
```csharp
// 메인 씬 컨트롤러 사용
MainSceneController mainController = FindObjectOfType<MainSceneController>();
mainController.InitializeScene();

// 설정 UI 표시
SettingsUIController settingsController = FindObjectOfType<SettingsUIController>();
settingsController.ShowSettings();

// 무기 선택기 사용
WeaponSelector weaponSelector = FindObjectOfType<WeaponSelector>();
weaponSelector.SelectWeapon(weaponType);

// 패널 관리
PanelManager panelManager = FindObjectOfType<PanelManager>();
panelManager.ShowPanel(panelName);
```

### UI 효과 사용법
```csharp
// 언더라인 호버 효과
UnderlineHoverEffect hoverEffect = GetComponent<UnderlineHoverEffect>();
hoverEffect.EnableHoverEffect();

// 버튼 클릭 처리
public void OnPlayButtonClicked()
{
    // 플레이 버튼 클릭 처리
}

public void OnExitButtonClicked()
{
    // 종료 버튼 클릭 처리
}
```

## 🏗️ 아키텍처 패턴

### 1. 싱글톤 패턴 (Singleton Pattern)
- **각 UI 컨트롤러**: 각 UI 컨트롤러를 싱글톤으로 구현
- **전역 접근**: 어디서든 UI 컨트롤러에 접근 가능
- **상태 관리**: UI 상태를 전역에서 관리

### 2. MVC 패턴 (Model-View-Controller)
- **Model**: UI 데이터 모델
- **View**: UI 요소 (Unity UI)
- **Controller**: UI 컨트롤러 로직

### 3. 옵저버 패턴 (Observer Pattern)
- **이벤트 시스템**: UI 이벤트 발생 및 구독
- **상태 변경**: UI 상태 변경 알림
- **업데이트**: 상태 변경에 따른 UI 업데이트

### 4. 팩토리 패턴 (Factory Pattern)
- **UI 생성**: 동적 UI 요소 생성
- **UI 풀링**: UI 요소 풀링 관리

### 최근 변경(요약)
- **전투 HUD 연계(신규)**: TurnManager 로그 태그 적용 및 턴/큐 진행에 따른 HUD 업데이트 표준화
- **카드 Z-Order 보정(신규)**: 슬롯 이동 애니메이션 중 루트 캔버스로 승격+최상단 정렬 후 도착 슬롯 재부모화
- **드래그 제어(신규)**: SkillCardUI에서 적 카드/플레이어 마커 드래그 비활성, 플레이어 카드만 드래그 허용

## 전투 HUD 구성 및 흐름
- **구성 요소**:
  - 배틀/대기 슬롯 영역: BATTLE, WAIT1~4 표시 컨테이너
  - 플레이어 핸드 영역: 3개 슬롯 고정, 빈 슬롯은 비활성 처리
  - 턴 정보 영역: 현재 턴/턴 카운트 표시(로그와 동일 태그 사용 권장)
- **표시 규칙**:
  - 턴 시작 시: TurnManager가 큐 전진/보충을 완료한 뒤 HUD가 최종 상태를 반영
  - 플레이어 턴: 핸드가 비어있으면 3장 생성 후 슬롯에 부드럽게 배치
  - 적 턴: 플레이어 핸드를 즉시 정리하고(숨김 또는 제거), 배틀 슬롯의 적 카드 자동 실행 후 결과 반영
- **이벤트 연계**:
  - OnTurnChanged/OnTurnCountChanged: 턴 라벨/카운트 갱신
  - OnCardStateChanged: 슬롯 카드 UI 동기화(아이콘/이름/효과 라벨 등)
  - OnInitialSlotSetupCompleted: 초기 배치 애니메이션 종료 후 HUD 표시 개시

## 슬롯 UI 표시 규칙
- **카드 생성/배치**:
  - Wait4 생성 → Spawn Tween(페이드/스케일 업) → 배틀 슬롯 비면 전체 전진 애니메이션
  - 애니메이션 중 RectTransform.DOMove(월드 기준) 사용, 완료 후 목표 슬롯에 재부모화 및 앵커(0,0)
- **Z-Order 정책**:
  - 이동 시작 시 transform.SetParent(rootCanvas), transform.SetAsLastSibling()
  - 이동 종료 후 목표 슬롯으로 재부모화하여 정렬 복구
- **중복 방지**:
  - Wait4 점유 중일 때는 보충 금지
  - 초기 셋업 중 자동 보충/자동 실행 억제 플래그로 UI 혼선 방지

## 플레이어 핸드 UI 규칙
- **생성 타이밍**: 초기 슬롯 셋업 완료 후, 플레이어 턴 시작 시(큐 전진·보충이 끝난 다음 프레임) 3장 생성
- **드래그 가능 상태**: 플레이어 카드만 드래그 가능, 적 카드/플레이어 마커는 드래그 불가 및 Raycast 비활성
- **슬롯 배치**: DOTween으로 슬롯 중앙으로 이동 후 재부모화, 실패 시 원위치 스냅 백 애니메이션
- **정리 타이밍**: 적 턴 진입 시 즉시 ClearAll 호출로 시각적 일관성 유지

## 디버그/로깅 가이드
- **표준 태그**: `[T{turnCount}-{Player|Enemy}-F{frame}]` 프리픽스 사용
- **핵심 로그 지점**: 슬롯 전진 완료, Wait4 보충, 배틀 슬롯 자동 실행, 핸드 생성/정리

## 권장 인스펙터 설정
- 모든 카드 UI는 동일한 루트 캔버스 하에 존재
- CanvasGroup: 적/마커 카드 `interactable=false`, `blocksRaycasts=false`
- DOTween SafeMode 활성화, 트윈 캡용량(SetTweensCapacity) 프로젝트 전역 설정



