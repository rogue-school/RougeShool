# RougeShool MCP Server (Clean Version)

RougeShool Unity 프로젝트를 위한 깔끔하게 정리된 Model Context Protocol (MCP) 서버입니다.
AI가 기존 코드를 제대로 파악하고 활용할 수 있도록 핵심 기능만 포함한 최적화된 버전입니다.

## 🎯 주요 기능

- **코드베이스 상태 분석**: 전체 프로젝트 구조 및 의존성 분석
- **파일 분석**: 특정 파일의 클래스, 메서드, 인터페이스 추출
- **코드 재사용 제안**: 기존 코드 활용 방안 제안
- **충돌 예측**: 네이밍, 인터페이스, 의존성 충돌 검사

## 🚀 설치 및 실행

### 1. 의존성 설치
```bash
npm install
```

### 2. 서버 실행
```bash
npm start
```

## 🔧 Cursor 설정

Cursor에서 MCP 서버를 사용하려면 `.cursor/mcp.json` 파일에 다음 설정을 추가하세요:

```json
{
  "mcpServers": {
    "rougeshool-code-analysis": {
      "command": "node",
      "args": ["mcp_server/index.js"],
      "cwd": "C:\\Users\\hjk73\\Documents\\GitHub\\RougeShool",
      "env": {
        "NODE_ENV": "development"
      }
    }
  }
}
```

## 📁 프로젝트 구조

```
mcp_server/
├── index.js          # 메인 MCP 서버 코드 (단일 파일)
├── package.json       # 프로젝트 설정
├── package-lock.json  # 의존성 잠금
├── node_modules/      # 설치된 패키지
└── README.md         # 이 파일
```

## 🔧 사용 가능한 MCP 도구

- `get_codebase_state`: 코드베이스 상태 분석
- `analyze_file`: 파일 분석
- `suggest_code_reuse`: 코드 재사용 제안
- `predict_conflicts`: 충돌 예측

## ✨ 특징

- **단일 파일 구조**: 복잡한 의존성 없이 하나의 파일로 모든 기능 제공
- **최적화된 성능**: 불필요한 기능 제거로 빠른 실행
- **깔끔한 코드**: 읽기 쉽고 유지보수하기 쉬운 구조
- **안정적인 연결**: 연결 문제 없는 안정적인 MCP 서버

## 📄 라이선스

MIT License