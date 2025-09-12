# InventorySystem 개발 문서

## 📋 시스템 개요
InventorySystem은 인벤토리 패널/슬롯/아이템 버튼을 관리합니다. 본 시스템은 애니메이션 시스템과 독립적이며, 필요한 경우 UI 애니메이션과 연계할 수 있습니다.

## 📁 구성 요소
- **InventoryPanelController.cs**: 인벤토리 패널 컨트롤러
- **InventorySlot.cs**: 슬롯 컴포넌트
- **ItemButton.cs**: 아이템 버튼
- **InventoryManager.cs**: 인벤토리 데이터/동기화 관리
- **InventoryRandomizer.cs**: 임의 아이템 배치 도우미

## 🔗 기타 시스템 연동 참고
- 애니메이션 시스템은 인스펙터로 스크립트 타입을 선택하며, 타입 미지정 시 슬롯별 `*Animation001`이 적용됩니다(전역/폴백 제거). 인벤토리 UI는 필요 시 해당 애니메이션을 재사용할 수 있으나, 기본적으로 독립 동작합니다.

## 📝 변경 기록(Delta)
- 2025-09-12 | Maintainer | 간단 문서 추가 및 애니메이션 연동 참고 기재 | 문서


