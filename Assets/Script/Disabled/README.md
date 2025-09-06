# Disabled Scripts

이 폴더는 더 이상 사용하지 않는 스크립트들을 보관하는 곳입니다.

## 보관된 스크립트들

### GameManager.cs
- **작성자**: 다른 학생
- **목적**: 게임 상태 관리 및 씬 전환
- **비활성화 이유**: 새로운 CoreSystem으로 대체됨
- **대체 스크립트**: 
  - `Game.CoreSystem.Manager.GameStateManager`
  - `Game.CoreSystem.Manager.SceneTransitionManager`

## 새로운 시스템

### CoreSystem
- `GameStateManager`: 게임 상태 관리
- `SceneTransitionManager`: 씬 전환 관리
- `AudioManager`: 오디오 관리
- `SaveManager`: 저장/로드 관리

### 사용법
```csharp
// 구버전 (비활성화됨)
GameManager.Instance.StartBattle();

// 신버전 (현재 사용)
Game.CoreSystem.Manager.SceneTransitionManager.Instance.TransitionToBattleScene();
```
