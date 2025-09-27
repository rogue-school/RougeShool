# RougeShool MCP Server

RougeShool Unity 프로젝트를 위한 Model Context Protocol (MCP) 서버입니다.
AI가 기존 코드를 제대로 파악하고 활용할 수 있도록 코드베이스 분석, 파일 분석, 코드 재사용 제안 기능을 제공합니다.

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

### 3. 개발 모드 실행
```bash
npm run dev
```

## 🔧 Cursor 설정

Cursor에서 MCP 서버를 사용하려면 `.cursor/mcp.json` 파일에 다음 설정을 추가하세요:

```json
{
  "mcpServers": {
    "rougeshool-code-analysis": {
      "command": "node",
      "args": ["mcp_server/src/index.js"],
      "cwd": ".",
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
├── src/
│   └── index.js          # 메인 MCP 서버 코드
├── package.json          # 프로젝트 설정
├── package-lock.json     # 의존성 잠금
├── node_modules/         # 설치된 패키지
└── README.md            # 이 파일
```

## 🔧 사용 가능한 MCP 도구

- `get_codebase_state`: 코드베이스 상태 분석
- `analyze_file`: 파일 분석
- `suggest_code_reuse`: 코드 재사용 제안
- `predict_conflicts`: 충돌 예측

## 📄 라이선스

MIT License