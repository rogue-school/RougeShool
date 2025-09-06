# SaveSystem 개발 문서

## 📋 시스템 개요
SaveSystem은 게임의 저장/로드 기능을 관리하는 시스템입니다. 현재 폴더 구조는 존재하지만 실제 구현은 CoreSystem/Save에 위치하고 있습니다.

## 🏗️ 현재 폴더 구조
```
SaveSystem/
└── Manager/           # 비어있음 (실제 구현은 CoreSystem/Save에 위치)
```

## 📁 실제 구현 위치
```
CoreSystem/Save/
└── SaveManager.cs     # 실제 저장 매니저 구현
```

## 📊 SaveManager.cs 분석

### 주요 기능
- **JSON 기반 저장**: JsonUtility를 활용한 데이터 직렬화
- **씬별 데이터 저장**: 각 씬의 데이터를 개별적으로 저장
- **GameObject 데이터**: GameObject의 컴포넌트 데이터 저장
- **자동 저장**: 게임 종료 시 자동 저장
- **데이터 검증**: 저장된 데이터의 유효성 검증

### 주요 메서드
- `SaveSceneData(string sceneName)`: 씬 데이터 저장
- `LoadSceneData(string sceneName)`: 씬 데이터 로드
- `SaveGameObject(GameObject obj)`: GameObject 데이터 저장
- `LoadGameObject(GameObject obj, GameObjectData data)`: GameObject 데이터 로드
- `SaveToFile(string fileName, string data)`: 파일로 저장
- `LoadFromFile(string fileName)`: 파일에서 로드
- `DeleteSaveData(string sceneName)`: 저장 데이터 삭제
- `GetSaveDataList()`: 저장된 데이터 목록 조회

### 데이터 구조
```csharp
[System.Serializable]
public struct SceneSaveData
{
    public string sceneName;
    public GameObjectData[] gameObjects;
    public string saveTime;
}

[System.Serializable]
public struct GameObjectData
{
    public string name;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    public ComponentData[] components;
}

[System.Serializable]
public struct ComponentData
{
    public string componentType;
    public string data;
}
```

## 🎯 시스템 특징

### 장점
1. **JSON 기반**: 가독성 좋은 데이터 형식
2. **씬별 저장**: 각 씬의 데이터를 독립적으로 관리
3. **컴포넌트 지원**: 다양한 컴포넌트 데이터 저장
4. **자동 저장**: 게임 종료 시 자동 저장

### 단점
1. **폴더 구조 불일치**: SaveSystem 폴더는 비어있고 CoreSystem에 구현
2. **제한적인 타입 지원**: 기본 타입만 지원, 복잡한 객체 저장 어려움
3. **성능 문제**: JSON 직렬화/역직렬화로 인한 성능 오버헤드
4. **데이터 검증 부족**: 저장된 데이터의 유효성 검증 미흡

## 🔧 사용 방법

### 기본 사용법
```csharp
// 씬 데이터 저장
SaveManager.Instance.SaveSceneData("CombatScene");

// 씬 데이터 로드
SaveManager.Instance.LoadSceneData("CombatScene");

// GameObject 저장
SaveManager.Instance.SaveGameObject(playerObject);

// 저장 데이터 목록 조회
var saveDataList = SaveManager.Instance.GetSaveDataList();
```

### 커스텀 데이터 저장
```csharp
// 커스텀 컴포넌트 데이터 저장
public class CustomComponent : MonoBehaviour
{
    public void SaveData()
    {
        var data = new ComponentData
        {
            componentType = "CustomComponent",
            data = JsonUtility.ToJson(customData)
        };
        // SaveManager에 전달
    }
}
```


## 📊 시스템 평가
- **아키텍처**: 6/10 (폴더 구조 불일치)
- **확장성**: 5/10 (제한적인 타입 지원)
- **성능**: 6/10 (JSON 기반으로 인한 오버헤드)
- **유지보수성**: 7/10 (단순한 구조)
- **전체 점수**: 6.0/10

