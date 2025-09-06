# SkillCardSystem 개발 문서

## 📋 시스템 개요
SkillCardSystem은 게임의 스킬카드 시스템을 관리하는 핵심 시스템입니다. 카드 데이터, 효과, 실행, 검증, UI 등을 통합적으로 관리하며, 복잡한 카드 효과를 모듈화된 방식으로 처리합니다.

## 🏗️ 폴더 구조
```
SkillCardSystem/
├── Core/             # 핵심 로직 (2개 파일)
├── Data/             # 카드 데이터 (2개 파일)
├── Deck/             # 덱 관리 (3개 파일)
├── Effect/           # 효과 구현 (4개 파일)
├── Effects/          # 효과 데이터 (4개 파일)
├── Executor/         # 실행기 (1개 파일)
├── Factory/          # 팩토리 패턴 (3개 파일)
├── Interface/        # 인터페이스 (3개 파일)
├── Runtime/          # 런타임 로직 (5개 파일)
├── Slot/             # 슬롯 시스템 (2개 파일)
├── UI/               # UI 관련 (2개 파일)
└── Validator/        # 검증기 (1개 파일)
```

## 📁 주요 컴포넌트

### Core 폴더 (2개 파일)
- **PlayerSkillCard.cs**: 플레이어 스킬카드 기본 클래스
- **EnemySkillCard.cs**: 적 스킬카드 기본 클래스

### Data 폴더 (2개 파일)
- **SkillCardData.cs**: 스킬카드 데이터 (ScriptableObject)
- **PlayerSkillCard.cs**: 플레이어 스킬카드 데이터

### Deck 폴더 (3개 파일)
- **PlayerSkillDeck.cs**: 플레이어 스킬 덱
- **EnemySkillDeck.cs**: 적 스킬 덱
- **PlayerSkillCardEntry.cs**: 플레이어 스킬카드 엔트리

### Effect 폴더 (4개 파일)
- **DamageEffectCommand.cs**: 데미지 효과 명령
- **GuardEffectCommand.cs**: 방어 효과 명령
- **RegenEffect.cs**: 재생 효과
- **ForceNextSlotEffectSO.cs**: 다음 슬롯 강제 효과

### Effects 폴더 (4개 파일)
- **SkillCardEffectSO.cs**: 스킬카드 효과 기본 클래스
- **DamageEffectSO.cs**: 데미지 효과 데이터
- **GuardEffectSO.cs**: 방어 효과 데이터
- **BleedEffectSO.cs**: 출혈 효과 데이터
- **RegenEffectSO.cs**: 재생 효과 데이터

### Executor 폴더 (1개 파일)
- **CardExecutor.cs**: 카드 실행기

### Factory 폴더 (3개 파일)
- **SkillCardFactory.cs**: 스킬카드 팩토리
- **CardEffectCommandFactory.cs**: 카드 효과 명령 팩토리
- **SkillCardEntry.cs**: 스킬카드 엔트리 팩토리

### Interface 폴더 (3개 파일)
- **ISkillCard.cs**: 스킬카드 인터페이스
- **IPerTurnEffect.cs**: 턴별 효과 인터페이스
- **ISkillCardUI.cs**: 스킬카드 UI 인터페이스

### Runtime 폴더 (5개 파일)
- **PlayerSkillCardRuntime.cs**: 플레이어 스킬카드 런타임
- **EnemySkillCardRuntime.cs**: 적 스킬카드 런타임
- **RuntimeSkillCard.cs**: 런타임 스킬카드
- **PlayerSkillCardInstance.cs**: 플레이어 스킬카드 인스턴스
- **SkillCardCooldownSystem.cs**: 스킬카드 쿨다운 시스템

### Slot 폴더 (2개 파일)
- **BaseCardSlotUI.cs**: 기본 카드 슬롯 UI
- **SkillCardSlotPosition.cs**: 스킬카드 슬롯 위치

### UI 폴더 (2개 파일)
- **SkillCardUI.cs**: 스킬카드 UI
- **SkillCardUIFactory.cs**: 스킬카드 UI 팩토리

### Validator 폴더 (1개 파일)
- **DefaultCardExecutionValidator.cs**: 기본 카드 실행 검증기

## 🎯 주요 기능

### 1. 카드 데이터 관리
- **ScriptableObject**: 카드 데이터를 에셋으로 관리
- **런타임 인스턴스**: 게임 중 동적 생성/수정
- **데이터 검증**: 카드 데이터의 유효성 검증

### 2. 효과 시스템
- **모듈화된 효과**: 각 효과를 독립적인 모듈로 구현
- **효과 조합**: 여러 효과를 조합하여 복잡한 카드 생성
- **효과 실행**: 효과의 순차적 실행 및 결과 처리

### 3. 덱 관리
- **덱 구성**: 플레이어/적 덱 구성 및 관리
- **카드 드로우**: 덱에서 카드 드로우
- **덱 셔플**: 덱 섞기 및 재구성

### 4. 실행 시스템
- **카드 실행**: 카드 효과의 실행
- **실행 검증**: 실행 가능 여부 검증
- **실행 결과**: 실행 결과 처리 및 피드백

### 5. UI 시스템
- **카드 UI**: 카드의 시각적 표현
- **슬롯 UI**: 카드 슬롯의 UI
- **인터랙션**: 카드 드래그 앤 드롭 등

## 🔧 사용 방법

### 기본 사용법
```csharp
// 스킬카드 생성
var card = SkillCardFactory.Instance.CreateSkillCard(cardData);

// 카드 실행
CardExecutor.Instance.ExecuteCard(card, target);

// 덱에서 카드 드로우
var drawnCard = PlayerSkillDeck.Instance.DrawCard();

// 효과 적용
var effect = new DamageEffect(10);
effect.Apply(target);
```

### 커스텀 효과 생성
```csharp
// ScriptableObject 기반 효과
[CreateAssetMenu(fileName = "NewEffect", menuName = "SkillCard/Effect")]
public class CustomEffectSO : SkillCardEffectSO
{
    public override void Execute(ISkillCard card, ICharacter target)
    {
        // 커스텀 효과 로직
    }
}
```

## 🏗️ 아키텍처 패턴

### 1. 팩토리 패턴 (Factory Pattern)
- **SkillCardFactory**: 스킬카드 객체 생성
- **CardEffectCommandFactory**: 효과 명령 객체 생성
- **SkillCardUIFactory**: UI 객체 생성

### 2. 명령 패턴 (Command Pattern)
- **EffectCommand**: 효과를 명령 객체로 캡슐화
- **CardExecutor**: 명령 실행 및 관리

### 3. 전략 패턴 (Strategy Pattern)
- **EffectSO**: 다양한 효과 전략 구현
- **Validator**: 다양한 검증 전략 구현

### 4. 옵저버 패턴 (Observer Pattern)
- **이벤트 시스템**: 카드 실행 이벤트 발생 및 구독
- **UI 업데이트**: 카드 상태 변경에 따른 UI 업데이트


## 📊 시스템 평가
- **아키텍처**: 9/10 (잘 구조화된 모듈화 설계)
- **확장성**: 8/10 (인터페이스 기반 확장 가능)
- **성능**: 7/10 (최적화 여지 있음)
- **유지보수성**: 8/10 (명확한 책임 분리)
- **전체 점수**: 8.0/10

