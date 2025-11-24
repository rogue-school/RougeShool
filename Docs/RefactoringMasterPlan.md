# RougeShool 전역 리팩토링 마스터 플랜

> 작성일: 2025-11-24  
> 작성자: Cursor AI 지원  
> 목적: 새롭게 정의된 Cursor User Rule과 RougeShool 리팩토링 원칙을 전 시스템에 일관되게 적용하기 위한 실행 계획

## 📋 관련 문서

### 🎯 최종 실행 문서

- **[최종 리팩토링 계획](./FinalRefactoringPlan.md)**: ⭐ **이 문서를 바탕으로 리팩토링을 진행합니다** - 모든 계획을 통합한 최종 실행 계획

### 📚 참고 문서

- **[코드 품질 진단 리포트](./CodeQualityDiagnosisReport.md)**: 현재 프로젝트의 구체적인 개선 필요 사항 진단 결과
- **[완전 재작성 리팩토링 계획](./CompleteRefactoringPlan.md)**: 기존 코드를 개선하는 것이 아닌, 완전히 새로운 코드로 재작성하며 레거시 코드를 제거하는 전면 리팩토링 계획
- **[코드 로직 문서](./CodeLogicDocumentation.md)**: 프로젝트의 주요 함수, 변수, 코드 로직을 설명하는 참조 문서
- **[스크립트 상세 분석 및 재작성 계획](./DetailedScriptAnalysis.md)**: 모든 스크립트를 하나하나 체크하여 제거할 코드와 새로 작성할 코드를 정확하게 판단한 상세 분석 문서
- **[전체 스크립트 체크리스트](./CompleteScriptChecklist.md)**: 326개 모든 스크립트를 시스템별로 분류하여 하나하나 체크한 완전한 체크리스트
- **[아키텍처 리팩토링 계획](./ArchitectureRefactoringPlan.md)**: 시스템별 분리 방식에서 더 깔끔하고 확장 가능한 아키텍처(레이어드 + 기능 기반 Hybrid 구조)로 완전 재구성하는 계획
- **[마이그레이션 실행 계획](./MigrationExecutionPlan.md)**: Hybrid 구조로 실제 마이그레이션을 단계별로 실행하는 구체적인 가이드

---

## 1. 리팩토링 비전과 범위
| 구분 | 내용 |
| --- | --- |
| 지향점 | SOLID 100%, Zenject DI 전면 적용, Update() 제거, DOTween 메모리 안전 수칙 준수, 모든 사용자 노출 문자열의 한국어화 |
| 범위 | `Assets/Script` 내 Core/Combat/Character/SkillCard/Item/Stage/Save/UI/VFX/Tutorial/Utility 전 영역 및 관련 ScriptableObject |
| 제외 | 상용 플러그인(`Assets/Plugins`, `Packages`)과 외부 노드 모듈은 구조 개선 대상에서 제외 |

---

## 2. 공통 개선 원칙
1. **3-계층 예외 처리**: Validation(throw only) → Operation(log + wrap) → Boundary(UI에서 log 후 복구) 흐름 고정.
2. **DI/의존성 검증**: Zenject `[Inject]` 경로 통일, 필수 의존성 `ArgumentNullException` 강제, Optional은 조기 반환 로그.
3. **성능 보증**: 객체 풀 3컬렉션 패턴, DOTween AutoKill/OnDisable 정리, Update 금지 및 이벤트/코루틴/시퀀스 전환.
4. **로컬라이징 표준**: Inspector `Header/Tooltip`, 로그, 예외 메시지는 전부 한국어.
5. **문서화**: 공개 API는 XML, 시스템별 문서는 `Docs/` 하위에 추가(본 문서 포함), 설계 변경 시 변경 기록 필수.
6. **테스트 관점**: 메서드는 순수 함수화 및 주입형 설계로 테스트 가능성 확보 (테스트 코드는 별도 요청 시 작성).

---

## 3. 시스템별 진단 및 개선 과제

### 3.1 CoreSystem
| 우선순위 | 현 상태 | 개선 항목 | 산출물 |
| --- | --- | --- | --- |
| 🔥 | 일부 매니저가 `FindObjectOfType` 캐싱 사용 | BaseCoreManager 파생 클래스에 전면 DI 주입, 캐싱 제거 | `CoreSystem/Manager/*` |
| 🔥 | SaveManager가 다중 책임(저장+프로그레스+오디오) | 저장/프로그레스/설정 분리, StageProgressCollector를 인터페이스 화 | `CoreSystem/Save/*`, `SaveSystem/Manager/*` |
| ⚠️ | 공통 초기화/라이프사이클 혼재 | `ICoreSystemInitializable` 단계 정의(Init/Ready/Link) 문서화 | 본 문서 + 각 클래스 주석 |

### 3.2 CombatSystem
| 우선순위 | 현 상태 | 개선 항목 | 산출물 |
| --- | --- | --- | --- |
| 🔥 | `CombatStateMachine`에 디버그/부활/턴 로직 혼재 | 상태 전환/부활 처리/디버그 출력을 별도 서비스로 분리 | `CombatSystem/State/*` |
| 🔥 | Installer가 8개 이상의 private helper 포함 | Installer를 Role별(partial 클래스 또는 static helper)로 분리, Bind 순서 문서화 | `CombatSystem/Core/CombatInstaller.cs` |
| ⚠️ | Slot/Turn 매니저 규칙 문서 단일(본 문서) | `Docs/CombatSystem_개발문서.md` 업데이트 및 슬롯 상태 다이어그램 최신화 | Docs |

### 3.3 CharacterSystem
| 우선순위 | 현 상태 | 개선 항목 | 산출물 |
| --- | --- | --- | --- |
| 🔥 | Player/Enemy 매니저 로직 중복 (스탯 계산 등) | 공통 `CharacterRuntimeContext` 도입, 전략 패턴으로 효과 계산 분리 | `CharacterSystem/Manager/*`, `CharacterSystem/Core/*` |
| ⚠️ | Initialization 단계 명세 부족 | `Initialization` 폴더 내 Builder/Validator 패턴 도입, Inspector 한글화 | `CharacterSystem/Initialization/*` |

### 3.4 SkillCardSystem
| 우선순위 | 현 상태 | 개선 항목 | 산출물 |
| --- | --- | --- | --- |
| 🔥 | Effect/Executor에 중복된 `PlayVFX/PlaySFX` 코드 | `SkillCardEffectPipeline` 서비스로 통합, DOTween 안전 수칙 내장 | `SkillCardSystem/Effect/*`, `SkillCardSystem/Executor/*` |
| 🔥 | DragDrop/Slot/Deck 간 의존성이 순환 | 이벤트 허브(or Signal) 도입, Slot Registry를 단일 진실 소스로 통합 | `SkillCardSystem/Slot/*`, `SkillCardSystem/DragDrop/*` |
| ⚠️ | 카드 정의/런타임 설정 상수 분산 | ScriptableObject 규격 통일 + `CardBalanceData` 도입 | `SkillCardSystem/Data/*` |

### 3.5 ItemSystem
| 우선순위 | 현 상태 | 개선 항목 | 산출물 |
| --- | --- | --- | --- |
| ⚠️ | Effect/Runtime에 Update 사용 흔적 | 코루틴/이벤트 기반 처리로 전환, 풀링 템플릿 도입 | `ItemSystem/Effect/*`, `ItemSystem/Runtime/*` |
| ⚠️ | Editor/Runtime 로직 혼재 | Editor 전용 assembly 정의, `UNITY_EDITOR` 분기 제거 | `ItemSystem/Editor/*` |

### 3.6 StageSystem & Tutorial
| 우선순위 | 현 상태 | 개선 항목 | 산출물 |
| --- | --- | --- | --- |
| ⚠️ | StageManager가 Save/Combat에 직접 의존 | Stage Flow 인터페이스 도입, Zenject 시그널로 이벤트 전환 | `StageSystem/Manager/*` |
| ⚠️ | Tutorial Overlay가 직렬화 데이터 결핍 | Scriptable Tutorial Step 도입, UI 시스템과 분리 | `TutorialSystem/*`, `UISystem/*` |

### 3.7 UISystem & VFX/Utility
| 우선순위 | 현 상태 | 개선 항목 | 산출물 |
| --- | --- | --- | --- |
| 🔥 | 다수 UI 컨트롤러가 싱글톤 패턴 사용 | PanelManager 기반 이벤트 버스로 통일, Zenject installer 연결 | `UISystem/*` |
| ⚠️ | VFX Pool에 수명 정리 누락 | DOTween Sequence 수명 추적 + Pool push 시 Kill 필수화 | `VFXSystem/Pool/*` |
| ⚠️ | UtilitySystem/GameFlow에 Update 타이머 | Request/Unit 패턴으로 전환, GC-free 큐 구현 | `UtilitySystem/GameFlow/*` |

---

## 4. 실행 웨이브 및 타임라인
| 웨이브 | 기간 | 대상 | 핵심 산출물 | 위험 완화 |
| --- | --- | --- | --- | --- |
| Wave 0 (주차 47) | 11/25 ~ 11/29 | 규칙 내재화 & 진단 | 본 플랜, 각 시스템 진단 로그 | 변경 영향도 분석 문서화 |
| Wave 1 (주차 48) | 12/02 ~ 12/13 | CoreSystem + Save + Stage | BaseCoreManager 개선, Save 분리, Stage Flow 인터페이스 | 리그레션 체크리스트 |
| Wave 2 (주차 49) | 12/16 ~ 12/27 | Combat + SkillCard + Character | Installer 분할, Slot Registry 통합, Character context 도입 | 테스트 플레이 시나리오 6종 |
| Wave 3 (주차 1) | 01/06 ~ 01/17 | UI + Item + VFX + Tutorial | UI 이벤트 버스, Item 풀링, VFX 수명, Tutorial Scriptable | UX QA + 메모리 프로파일 |
| Wave 4 (주차 2) | 01/20 ~ 01/24 | 통합/검증 | 회귀 테스트, 문서 업데이트, 코드 스캔(Forbidden API, DOTween) | 품질 게이트 리포트 |

---

## 5. 품질 게이트 & 체크리스트
1. **코드 품질**: Magic Number 제거, 상수화, XML 문서, 주입 필드 null-check.
2. **아키텍처**: Zenject Installer별 책임 분리, Signal/Service 레이어 명시.
3. **성능**: 모든 Pool에서 `Init/Push/Pull` 3-set 확인, GC < 1KB/frame 로깅.
4. **로깅/현지화**: GameLogger 카테고리 재검토(Combat/Character/UI/Audio/Save 등), Inspector 한글 라벨 자동화 스크립트 검토.
5. **문서화**: 각 시스템 변경 시 `Docs/` 내 대응 문서 혹은 섹션 업데이트, 변경 기록 날짜/담당자 기재.

---

## 6. 후속 액션
1. 시스템별 세부 설계 문서 템플릿 공유 (1~2p, 문제/대안/결정/Checklist).
2. 각 웨이브 시작 전 `Impact Review` 미팅 노트 작성.
3. 리팩토링 완료 후 `RougeShool_QualityGateReport` 업데이트 및 repo 루트에 배치.
4. User Rule 업데이트 발생 시 본 문서 상단 버전+링크 갱신.

---

## 7. 변경 기록
| 날짜 | 담당 | 내용 |
| --- | --- | --- |
| 2025-11-24 | Cursor AI | 전역 리팩토링 마스터 플랜 초안 작성 |


