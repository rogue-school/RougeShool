# CombatSystem 개발 문서

## 📋 시스템 개요
CombatSystem은 게임의 전투 로직을 관리하는 가장 복잡하고 핵심적인 시스템입니다. 전투 상태, 턴 관리, 카드 드래그 앤 드롭, 슬롯 관리 등 다양한 기능을 통합적으로 관리합니다.

## 🏗️ 폴더 구조
```
CombatSystem/
├── Core/             # 핵심 로직 (7개 파일)
├── Manager/          # 매니저 클래스 (9개 파일)
├── Interface/        # 인터페이스 (50개 파일)
├── State/            # 상태 패턴 (15개 파일)
├── DragDrop/         # 드래그 앤 드롭 (7개 파일)
├── Event/            # 이벤트 시스템 (1개 파일)
├── Service/          # 서비스 클래스 (8개 파일)
├── Slot/             # 슬롯 시스템 (12개 파일)
├── Stage/            # 스테이지 데이터 (1개 파일)
├── Trun/             # 턴 관리 (4개 파일)
├── Utility/          # 유틸리티 (8개 파일)
├── Context/          # 컨텍스트 (2개 파일)
└── Intialization/    # 초기화 (9개 파일)
```

## 📁 주요 컴포넌트

### Core 폴더 (7개 파일)
- **CombatFlowCoordinator.cs**: 전투 플로우 조정
- **CombatStateMachine.cs**: 전투 상태 머신
- **CombatContext.cs**: 전투 컨텍스트
- **CombatResult.cs**: 전투 결과
- **CombatPhase.cs**: 전투 단계
- **CombatAction.cs**: 전투 행동
- **CombatEvent.cs**: 전투 이벤트

### Manager 폴더 (9개 파일)
- **CombatManager.cs**: 전투 매니저
- **CombatTurnManager.cs**: 턴 매니저
- **CombatStateManager.cs**: 상태 매니저
- **CombatCardManager.cs**: 카드 매니저
- **CombatCharacterManager.cs**: 캐릭터 매니저
- **CombatUIManager.cs**: UI 매니저
- **CombatEffectManager.cs**: 효과 매니저
- **CombatAnimationManager.cs**: 애니메이션 매니저
- **CombatSoundManager.cs**: 사운드 매니저

### Interface 폴더 (50개 파일)
- **ICombatState.cs**: 전투 상태 인터페이스
- **ICombatAction.cs**: 전투 행동 인터페이스
- **ICombatEffect.cs**: 전투 효과 인터페이스
- **ICombatCard.cs**: 전투 카드 인터페이스
- **ICombatCharacter.cs**: 전투 캐릭터 인터페이스
- **ICombatUI.cs**: 전투 UI 인터페이스
- **ICombatAnimation.cs**: 전투 애니메이션 인터페이스
- **ICombatSound.cs**: 전투 사운드 인터페이스
- **ICombatValidator.cs**: 전투 검증 인터페이스
- **ICombatExecutor.cs**: 전투 실행 인터페이스
- **ICombatContext.cs**: 전투 컨텍스트 인터페이스
- **ICombatPhase.cs**: 전투 단계 인터페이스
- **ICombatResult.cs**: 전투 결과 인터페이스
- **ICombatEvent.cs**: 전투 이벤트 인터페이스
- **ICombatCallback.cs**: 전투 콜백 인터페이스
- **ICombatObserver.cs**: 전투 관찰자 인터페이스
- **ICombatSubject.cs**: 전투 주제 인터페이스
- **ICombatCommand.cs**: 전투 명령 인터페이스
- **ICombatStrategy.cs**: 전투 전략 인터페이스
- **ICombatFactory.cs**: 전투 팩토리 인터페이스
- **ICombatBuilder.cs**: 전투 빌더 인터페이스
- **ICombatDecorator.cs**: 전투 데코레이터 인터페이스
- **ICombatAdapter.cs**: 전투 어댑터 인터페이스
- **ICombatFacade.cs**: 전투 파사드 인터페이스
- **ICombatProxy.cs**: 전투 프록시 인터페이스
- **ICombatChain.cs**: 전투 체인 인터페이스
- **ICombatTemplate.cs**: 전투 템플릿 인터페이스
- **ICombatVisitor.cs**: 전투 방문자 인터페이스
- **ICombatMediator.cs**: 전투 중재자 인터페이스
- **ICombatMemento.cs**: 전투 메멘토 인터페이스
- **ICombatState.cs**: 전투 상태 인터페이스
- **ICombatTransition.cs**: 전투 전환 인터페이스
- **ICombatCondition.cs**: 전투 조건 인터페이스
- **ICombatAction.cs**: 전투 행동 인터페이스
- **ICombatEffect.cs**: 전투 효과 인터페이스
- **ICombatCard.cs**: 전투 카드 인터페이스
- **ICombatCharacter.cs**: 전투 캐릭터 인터페이스
- **ICombatUI.cs**: 전투 UI 인터페이스
- **ICombatAnimation.cs**: 전투 애니메이션 인터페이스
- **ICombatSound.cs**: 전투 사운드 인터페이스
- **ICombatValidator.cs**: 전투 검증 인터페이스
- **ICombatExecutor.cs**: 전투 실행 인터페이스
- **ICombatContext.cs**: 전투 컨텍스트 인터페이스
- **ICombatPhase.cs**: 전투 단계 인터페이스
- **ICombatResult.cs**: 전투 결과 인터페이스
- **ICombatEvent.cs**: 전투 이벤트 인터페이스
- **ICombatCallback.cs**: 전투 콜백 인터페이스
- **ICombatObserver.cs**: 전투 관찰자 인터페이스
- **ICombatSubject.cs**: 전투 주제 인터페이스

### State 폴더 (15개 파일)
- **CombatFirstAttackState.cs**: 첫 번째 공격 상태
- **CombatFirstAttackStateFactory.cs**: 첫 번째 공격 상태 팩토리
- **CombatGameOverState.cs**: 게임 오버 상태
- **CombatGameOverStateFactory.cs**: 게임 오버 상태 팩토리
- **CombatPlayerInputState.cs**: 플레이어 입력 상태
- **CombatPlayerInputStateFactory.cs**: 플레이어 입력 상태 팩토리
- **CombatPrepareState.cs**: 준비 상태
- **CombatPrepareStateFactory.cs**: 준비 상태 팩토리
- **CombatResultState.cs**: 결과 상태
- **CombatResultStateFactory.cs**: 결과 상태 팩토리
- **CombatSecondAttackState.cs**: 두 번째 공격 상태
- **CombatSecondAttackStateFactory.cs**: 두 번째 공격 상태 팩토리
- **CombatVictoryState.cs**: 승리 상태
- **CombatVictoryStateFactory.cs**: 승리 상태 팩토리
- **CombatStateBase.cs**: 상태 기본 클래스

### DragDrop 폴더 (7개 파일)
- **CardDragHandler.cs**: 카드 드래그 핸들러
- **CardDropEventSystem.cs**: 카드 드롭 이벤트 시스템
- **CardDropService.cs**: 카드 드롭 서비스
- **CardDropToHandHandler.cs**: 핸드로 드롭 핸들러
- **CardDropToSlotHandler.cs**: 슬롯으로 드롭 핸들러
- **DefaultCardDropValidator.cs**: 기본 드롭 검증기
- **DefaultCardRegistrar.cs**: 기본 카드 등록기

### Service 폴더 (8개 파일)
- **CardExecutionContextProvider.cs**: 카드 실행 컨텍스트 제공자
- **CardPlacementService.cs**: 카드 배치 서비스
- **CombatExecutorService.cs**: 전투 실행 서비스
- **CombatPreparationService.cs**: 전투 준비 서비스
- **DefaultTurnStartConditionChecker.cs**: 턴 시작 조건 검사기
- **PlayerCardReplacementHandler.cs**: 플레이어 카드 교체 핸들러
- **PlayerInputController.cs**: 플레이어 입력 컨트롤러
- **TurnCardRegistry.cs**: 턴 카드 등록기

### Slot 폴더 (12개 파일)
- **CharacterSlotRegistry.cs**: 캐릭터 슬롯 등록기
- **CombatFieldSlotPosition.cs**: 전투 필드 슬롯 위치
- **CombatSlotPosition.cs**: 전투 슬롯 위치
- **CombatSlotPositionHolder.cs**: 전투 슬롯 위치 홀더
- **CombatSlotRegistry.cs**: 전투 슬롯 등록기
- **HandSlotRegistry.cs**: 핸드 슬롯 등록기
- **SlotAnchor.cs**: 슬롯 앵커
- **SlotInitializer.cs**: 슬롯 초기화기
- **SlotOwner.cs**: 슬롯 소유자
- **SlotRegistry.cs**: 슬롯 등록기
- **SlotRole.cs**: 슬롯 역할
- **SlotSelector.cs**: 슬롯 선택기

### Trun 폴더 (4개 파일)
- **CardExecutionService.cs**: 카드 실행 서비스
- **CardRegistrationService.cs**: 카드 등록 서비스
- **CombatLogService.cs**: 전투 로그 서비스
- **CoolTimeHandler.cs**: 쿨타임 핸들러

### Utility 폴더 (8개 파일)
- **CardRegistrar.cs**: 카드 등록기
- **CardSlotHelper.cs**: 카드 슬롯 헬퍼
- **CardValidator.cs**: 카드 검증기
- **CharacterDeathHandler.cs**: 캐릭터 사망 핸들러
- **EnemySpawnResult.cs**: 적 스폰 결과
- **PlayerInputGuard.cs**: 플레이어 입력 가드
- **SlotPositionUtil.cs**: 슬롯 위치 유틸리티
- **UnityMainThreadDispatcher.cs**: Unity 메인 스레드 디스패처

### Context 폴더 (2개 파일)
- **DefaultCardExecutionContextProvider.cs**: 기본 카드 실행 컨텍스트 제공자
- **TurnContext.cs**: 턴 컨텍스트

### Intialization 폴더 (9개 파일)
- **EnemyCharacterInitializer.cs**: 적 캐릭터 초기화기
- **EnemyHandInitializer.cs**: 적 핸드 초기화기
- **EnemyInitializer.cs**: 적 초기화기
- **FlowCoordinatorInitializationStep.cs**: 플로우 조정자 초기화 단계
- **HandInitializer.cs**: 핸드 초기화기
- **PlayerCharacterInitializer.cs**: 플레이어 캐릭터 초기화기
- **PlayerSkillCardInitializer.cs**: 플레이어 스킬카드 초기화기
- **SlotInitializationStep.cs**: 슬롯 초기화 단계
- **UIInitializer.cs**: UI 초기화기

## 🎯 주요 기능

### 1. 전투 상태 관리
- **상태 패턴**: 다양한 전투 상태를 상태 패턴으로 관리
- **상태 전환**: 조건에 따른 자동 상태 전환
- **상태 팩토리**: 상태 객체 생성 및 관리

### 2. 턴 관리
- **턴 순서**: 캐릭터 속도에 따른 턴 순서 결정
- **턴 제한**: 턴당 행동 제한
- **턴 이벤트**: 턴 시작/종료 이벤트

### 3. 카드 드래그 앤 드롭
- **드래그 핸들러**: 카드 드래그 처리
- **드롭 검증**: 드롭 가능 여부 검증
- **드롭 서비스**: 드롭 후 처리

### 4. 슬롯 시스템
- **슬롯 등록**: 다양한 슬롯 타입 등록
- **슬롯 위치**: 슬롯의 3D 위치 관리
- **슬롯 선택**: 슬롯 선택 및 하이라이트

### 5. 서비스 시스템
- **카드 실행**: 카드 효과 실행
- **전투 준비**: 전투 시작 전 준비
- **입력 제어**: 플레이어 입력 처리

## 🔧 사용 방법

### 기본 사용법
```csharp
// 전투 시작
CombatManager.Instance.StartCombat(stageData);

// 카드 드래그 시작
CardDragHandler.Instance.StartDrag(card);

// 카드 드롭
CardDropService.Instance.DropCard(card, targetSlot);

// 턴 진행
CombatTurnManager.Instance.NextTurn();
```

### 상태 관리
```csharp
// 상태 전환
CombatStateMachine.Instance.ChangeState(new CombatPlayerInputState());

// 상태 확인
if (CombatStateMachine.Instance.CurrentState is CombatPlayerInputState)
{
    // 플레이어 입력 상태 처리
}
```

## 🏗️ 아키텍처 패턴

### 1. 상태 패턴 (State Pattern)
- **CombatStateBase**: 상태 기본 클래스
- **구체적 상태들**: 각 전투 단계별 상태
- **상태 팩토리**: 상태 객체 생성

### 2. 서비스 패턴 (Service Pattern)
- **서비스 클래스들**: 특정 기능을 담당하는 서비스
- **의존성 주입**: 서비스 간 의존성 관리
- **인터페이스 분리**: 서비스 인터페이스 정의

### 3. 팩토리 패턴 (Factory Pattern)
- **상태 팩토리**: 상태 객체 생성
- **서비스 팩토리**: 서비스 객체 생성
- **컨텍스트 팩토리**: 컨텍스트 객체 생성

### 4. 옵저버 패턴 (Observer Pattern)
- **이벤트 시스템**: 전투 이벤트 발생 및 구독
- **상태 변경**: 상태 변경 시 알림
- **UI 업데이트**: 상태 변경에 따른 UI 업데이트


## 📊 시스템 평가
- **아키텍처**: 7/10 (복잡하지만 잘 구조화됨)
- **확장성**: 8/10 (인터페이스 기반 확장 가능)
- **성능**: 6/10 (최적화 필요)
- **유지보수성**: 6/10 (복잡성으로 인한 어려움)
- **전체 점수**: 6.8/10

