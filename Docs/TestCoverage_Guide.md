# Unity 테스트 커버리지 가이드

## 📊 테스트 커버리지란?

테스트 커버리지는 코드의 어느 부분이 테스트로 검증되었는지를 나타내는 지표입니다.

### 현재 프로젝트 상태
- **현재 커버리지**: 1.51% (17/1,125 public 메서드)
- **목표**: 30% 이상

---

## 🛠️ Unity에서 테스트 커버리지 측정하기

### 1. Code Coverage 패키지 설치

Unity 2019.3 이상에서는 **Code Coverage** 패키지를 사용합니다.

#### 설치 방법:
1. **Window → Package Manager** 열기
2. **Packages: In Project** → **Unity Registry**로 변경
3. **Code Coverage** 검색
4. **Install** 클릭

또는 `Packages/manifest.json`에 추가:
```json
{
  "dependencies": {
    "com.unity.testtools.codecoverage": "1.2.4"
  }
}
```

---

## 📋 테스트 커버리지 측정 단계

### Step 1: Unity Test Framework 설정

1. **Window → General → Test Runner** 열기
2. **EditMode** 또는 **PlayMode** 탭 선택
3. 테스트 작성 및 실행

### Step 2: Code Coverage 활성화

1. **Window → Analysis → Code Coverage** 열기
2. **Enable Code Coverage** 체크
3. **Coverage Options** 설정:
   - ✅ **Generate HTML Report**: HTML 리포트 생성
   - ✅ **Generate Badge**: 배지 생성
   - ✅ **Generate Additional Metrics**: 추가 메트릭 생성
   - ✅ **Auto Open Report**: 리포트 자동 열기

### Step 3: 테스트 실행 및 커버리지 확인

1. **Test Runner**에서 테스트 실행
2. **Code Coverage** 창에서 실시간 커버리지 확인
3. **Generate Report** 버튼으로 상세 리포트 생성

---

## 📈 커버리지 리포트 읽는 방법

### HTML 리포트 구조:
```
CoverageReport/
├── index.html          # 메인 리포트
├── Summary.xml         # 요약 데이터
└── [클래스별 상세 리포트]
```

### 리포트에서 확인할 수 있는 정보:
- **Line Coverage**: 라인 커버리지 (%)
- **Branch Coverage**: 분기 커버리지 (%)
- **Method Coverage**: 메서드 커버리지 (%)
- **클래스별 상세**: 각 클래스의 커버리지 상태
  - 🟢 녹색: 테스트됨
  - 🔴 빨간색: 테스트 안됨
  - 🟡 노란색: 부분적으로 테스트됨

---

## 🎯 커버리지 목표 설정

### 프로젝트별 권장 커버리지:
```
REQUIRED (필수):
├── Core Logic (CharacterSystem, CombatSystem): 80%+
├── Managers: 70%+
├── Validators/Utilities: 90%+
└── UI Controllers: 50%+ (MonoBehaviour dependencies)

OPTIONAL (선택):
├── Data classes (ScriptableObjects): 30%
├── Simple DTOs/POCOs: 20%
└── Unity lifecycle methods: 20%
```

---

## 🔧 커버리지 측정 설정 예시

### Code Coverage 설정 파일 생성

`Assets/Editor/CodeCoverageSettings.asset` (자동 생성됨)

### 커버리지 포함/제외 설정

**Include/Exclude 패턴**:
- `+` : 포함할 패턴
- `-` : 제외할 패턴

예시:
```
+Game.CharacterSystem.*
+Game.CombatSystem.*
-Game.CharacterSystem.Data.*  # Data 클래스 제외
-Game.*.Editor.*               # Editor 스크립트 제외
```

---

## 📝 테스트 작성 예시

### EditMode 테스트 (빠른 단위 테스트)

```csharp
using NUnit.Framework;
using Game.CharacterSystem.Utility;

namespace Tests.EditMode.CharacterSystem
{
    public class CardValidatorTests
    {
        [Test]
        public void CanPlayCard_NullCard_ReturnsFalse()
        {
            // Arrange
            var validator = new CardValidator();
            var mockPlayer = CreateMockPlayer();

            // Act
            bool result = validator.CanPlayCard(null, mockPlayer);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void CanPlayCard_InsufficientMana_ReturnsFalse()
        {
            // Arrange
            var validator = new CardValidator();
            var mockPlayer = CreateMockPlayer(mana: 5);
            var expensiveCard = CreateCard(cost: 10);

            // Act
            bool result = validator.CanPlayCard(expensiveCard, mockPlayer);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
```

---

## 🚀 CI/CD에서 커버리지 측정

### 커맨드라인에서 테스트 실행:

```bash
# Unity 에디터에서 테스트 실행
Unity.exe -runTests -batchmode -projectPath . -testResults TestResults.xml -testPlatform EditMode

# 커버리지 리포트 생성
Unity.exe -runTests -batchmode -projectPath . -enableCodeCoverage -coverageResultsPath CoverageResults
```

### GitHub Actions 예시:

```yaml
- name: Run Tests with Coverage
  run: |
    Unity.exe -runTests \
      -batchmode \
      -projectPath . \
      -testResults TestResults.xml \
      -testPlatform EditMode \
      -enableCodeCoverage \
      -coverageResultsPath CoverageResults
```

---

## 📊 커버리지 향상 전략

### 1. 우선순위 높은 클래스부터 테스트
- Core System (AudioManager 등)
- Character System (CharacterBase, PlayerCharacter 등)
- Combat System (CombatExecutionManager 등)

### 2. 점진적 커버리지 향상
- 목표: 1.51% → 10% → 20% → 30%
- 단계별로 목표 설정

### 3. 커버리지 모니터링
- 매 커밋마다 커버리지 확인
- 커버리지가 떨어지면 알림 설정

---

## ⚠️ 주의사항

### 커버리지 함정:
1. **높은 커버리지 ≠ 좋은 테스트**
   - 의미 있는 테스트가 중요
   - 단순히 커버리지만 높이는 것은 의미 없음

2. **100% 커버리지는 불필요**
   - 핵심 로직에 집중
   - UI나 Data 클래스는 낮은 커버리지도 허용

3. **커버리지 측정 비용**
   - 코드 실행 속도 저하 가능
   - 개발 중에는 선택적으로 사용

---

## 🔗 참고 자료

- [Unity Test Framework 문서](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [Code Coverage 패키지 문서](https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@latest)
- [NUnit 문서](https://docs.nunit.org/)

---

**마지막 업데이트**: 2024년

