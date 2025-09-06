# UISystem 개발 문서

## 📋 시스템 개요
UISystem은 게임의 사용자 인터페이스를 관리하는 시스템입니다. 다양한 UI 컨트롤러들을 통해 게임의 모든 UI 요소를 통합적으로 관리합니다.

## 🏗️ 폴더 구조
```
UISystem/
└── 루트/             # UI 컨트롤러들 (8개 파일)
```

## 📁 주요 컴포넌트

### 루트 폴더 (8개 파일)
- **MainMenuUIController.cs**: 메인 메뉴 UI 컨트롤러
- **CombatUIController.cs**: 전투 UI 컨트롤러
- **InventoryUIController.cs**: 인벤토리 UI 컨트롤러
- **CharacterUIController.cs**: 캐릭터 UI 컨트롤러
- **SkillCardUIController.cs**: 스킬카드 UI 컨트롤러
- **SettingsUIController.cs**: 설정 UI 컨트롤러
- **LoadingUIController.cs**: 로딩 UI 컨트롤러
- **GameOverUIController.cs**: 게임 오버 UI 컨트롤러

## 🎯 주요 기능

### 1. 메인 메뉴 UI
- **게임 시작**: 게임 시작 버튼
- **설정 접근**: 설정 메뉴 접근
- **종료**: 게임 종료
- **애니메이션**: 메뉴 전환 애니메이션

### 2. 전투 UI
- **전투 정보**: 전투 상태 정보 표시
- **카드 UI**: 스킬카드 UI 관리
- **캐릭터 UI**: 캐릭터 상태 UI
- **턴 표시**: 현재 턴 정보

### 3. 인벤토리 UI
- **아이템 표시**: 인벤토리 아이템 표시
- **드래그 앤 드롭**: 아이템 드래그 앤 드롭
- **정렬**: 아이템 정렬 기능
- **필터**: 아이템 필터링

### 4. 캐릭터 UI
- **캐릭터 정보**: 캐릭터 상태 정보
- **스킬 트리**: 스킬 트리 표시
- **장비**: 장비 관리
- **레벨업**: 레벨업 UI

### 5. 스킬카드 UI
- **카드 표시**: 스킬카드 시각적 표현
- **카드 정보**: 카드 상세 정보
- **드래그 앤 드롭**: 카드 드래그 앤 드롭
- **덱 관리**: 덱 구성 관리

### 6. 설정 UI
- **오디오 설정**: 볼륨 조절
- **그래픽 설정**: 그래픽 옵션
- **키 바인딩**: 키 설정
- **언어 설정**: 언어 선택

### 7. 로딩 UI
- **로딩 표시**: 로딩 진행률 표시
- **로딩 애니메이션**: 로딩 애니메이션
- **팁 표시**: 로딩 중 팁 표시
- **취소**: 로딩 취소

### 8. 게임 오버 UI
- **결과 표시**: 게임 결과 표시
- **재시작**: 게임 재시작
- **메인 메뉴**: 메인 메뉴로 돌아가기
- **통계**: 게임 통계 표시

## 🔧 사용 방법

### 기본 사용법
```csharp
// 메인 메뉴 UI 활성화
MainMenuUIController.Instance.Show();

// 전투 UI 업데이트
CombatUIController.Instance.UpdateCombatInfo(combatData);

// 인벤토리 UI 열기
InventoryUIController.Instance.OpenInventory();

// 설정 UI 표시
SettingsUIController.Instance.ShowSettings();
```

### UI 상태 관리
```csharp
// UI 상태 확인
if (MainMenuUIController.Instance.IsVisible)
{
    // 메인 메뉴가 보이는 상태
}

// UI 숨기기
CombatUIController.Instance.Hide();

// UI 애니메이션
LoadingUIController.Instance.PlayLoadingAnimation();
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


## 📊 시스템 평가
- **아키텍처**: 7/10 (단순하지만 효과적인 구조)
- **확장성**: 6/10 (새로운 UI 추가 시 구조 변경 필요)
- **성능**: 6/10 (최적화 필요)
- **유지보수성**: 7/10 (명확한 책임 분리)
- **전체 점수**: 6.5/10

