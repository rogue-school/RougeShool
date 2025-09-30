#!/usr/bin/env node

import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from '@modelcontextprotocol/sdk/types.js';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// 프로젝트 루트 경로 설정
const PROJECT_ROOT = path.resolve(__dirname, '..');
const SCRIPT_PATH = path.join(PROJECT_ROOT, 'Assets', 'Script');

/**
 * 깔끔하게 정리된 RougeShool MCP 서버
 * 핵심 기능만 포함한 최적화된 버전
 */
class CleanRougeShoolMCPServer {
  constructor() {
    this.server = new Server(
      {
        name: 'rougeshool-mcp-server',
        version: '2.1.0',
      },
      {
        capabilities: {
          tools: {},
        },
      }
    );

    // 파일 스캔 캐싱 (성능 개선)
    this.fileCache = null;
    this.cacheTimestamp = null;
    this.CACHE_TTL = 30000; // 30초 TTL

    // 에러 로깅 (디버깅 개선)
    this.errorLog = [];
    this.maxErrorLog = 50;
    this.debugMode = process.env.MCP_DEBUG === 'true';

    this.setupHandlers();
  }

  /**
   * MCP 서버 핸들러 설정
   */
  setupHandlers() {
    // 도구 목록 제공
    this.server.setRequestHandler(ListToolsRequestSchema, async () => {
      return {
        tools: [
          {
            name: 'get_codebase_state',
            description: '현재 코드베이스의 전체 상태를 분석하고 반환합니다.',
            inputSchema: {
              type: 'object',
              properties: {
                random_string: {
                  type: 'string',
                  description: 'Dummy parameter for no-parameter tools'
                }
              },
              required: ['random_string']
            }
          },
          {
            name: 'analyze_file',
            description: '특정 파일의 의존성, 패턴, 인터페이스를 분석합니다.',
            inputSchema: {
              type: 'object',
              properties: {
                filePath: {
                  type: 'string',
                  description: '분석할 파일의 경로'
                }
              },
              required: ['filePath']
            }
          },
          {
            name: 'suggest_code_reuse',
            description: '요구사항에 맞는 기존 코드 재사용 방안을 제안합니다.',
            inputSchema: {
              type: 'object',
              properties: {
                requirement: {
                  type: 'string',
                  description: '구현하려는 기능의 요구사항'
                },
                targetSystem: {
                  type: 'string',
                  enum: ['CoreSystem', 'CombatSystem', 'CharacterSystem', 'SkillCardSystem', 'SaveSystem', 'StageSystem', 'UISystem', 'UtilitySystem'],
                  description: '대상 시스템'
                }
              },
              required: ['requirement']
            }
          },
          {
            name: 'predict_conflicts',
            description: '새로운 코드가 기존 코드와 충돌할 가능성을 예측합니다.',
            inputSchema: {
              type: 'object',
              properties: {
                newCode: {
                  type: 'string',
                  description: '새로 추가할 코드'
                },
                targetFile: {
                  type: 'string',
                  description: '대상 파일 경로'
                }
              },
              required: ['newCode', 'targetFile']
            }
            },
            {
              name: 'check_forbidden_apis',
              description: '금지된 API 사용(FindObjectOfType, Resources.Load, DOTween.KillAll 등)을 스캔합니다.',
              inputSchema: {
                type: 'object',
                properties: {
                  pathPrefix: { type: 'string', description: '스캔 시작 경로(기본: Assets/Script)' },
                  maxResults: { type: 'number', description: '최대 결과 개수', default: 200 }
                }
              }
            },
            {
              name: 'detect_update_loops',
              description: 'Update/FixedUpdate/LateUpdate 사용을 감지하고 대체 패턴을 제안합니다.',
              inputSchema: {
                type: 'object',
                properties: {
                  pathPrefix: { type: 'string' },
                  maxResults: { type: 'number', default: 200 }
                }
              }
            },
            {
              name: 'zenject_binding_map',
              description: 'Zenject Installer를 스캔하여 인터페이스→구현/스코프/NonLazy 맵을 생성합니다.',
              inputSchema: { type: 'object', properties: {} }
            },
            {
              name: 'dotween_lifecycle_check',
              description: 'DOTween 사용 파일의 수명주기 정리(OnDisable/OnDestroy) 여부를 점검합니다.',
              inputSchema: { type: 'object', properties: { pathPrefix: { type: 'string' } } }
            },
            {
              name: 'addressables_audit',
              description: 'Resources.Load 사용 지점을 보고하고 Addressables 전환 체크리스트를 제공합니다.',
              inputSchema: { type: 'object', properties: { pathPrefix: { type: 'string' } } }
            },
            {
              name: 'save_schema_version_check',
              description: '세이브 스키마의 SaveVersion 및 마이그레이션 스텝 유무를 검사합니다.',
              inputSchema: { type: 'object', properties: { pathPrefix: { type: 'string' } } }
            },
            {
              name: 'inspector_korean_labels_check',
              description: 'Inspector 한글화 규칙(Header/Tooltip) 적용 누락을 점검합니다.',
              inputSchema: { type: 'object', properties: { pathPrefix: { type: 'string' }, maxResults: { type: 'number', default: 200 } } }
            },
            {
              name: 'quality_gate_report',
              description: '프로젝트 품질 게이트 요약 리포트를 생성합니다.',
              inputSchema: { type: 'object', properties: {} }
            },
            {
              name: 'detect_circular_dependencies',
              description: '순환 의존성을 감지하고 의존성 그래프를 생성합니다.',
              inputSchema: {
                type: 'object',
                properties: {
                  targetSystem: { type: 'string', description: '검사할 대상 시스템 (선택)' }
                }
              }
            },
            {
              name: 'get_error_log',
              description: 'MCP 서버의 에러 로그를 조회합니다.',
              inputSchema: { type: 'object', properties: {} }
            },
            {
              name: 'invalidate_cache',
              description: '파일 스캔 캐시를 무효화합니다.',
              inputSchema: { type: 'object', properties: {} }
            },
            {
              name: 'calculate_cyclomatic_complexity',
              description: '메서드의 순환 복잡도를 계산합니다.',
              inputSchema: {
                type: 'object',
                properties: {
                  pathPrefix: { type: 'string', description: '검사할 경로 (선택)' },
                  maxResults: { type: 'number', default: 100, description: '최대 결과 개수' },
                  threshold: { type: 'number', default: 10, description: '복잡도 임계값' }
                }
              }
            },
            {
              name: 'detect_code_duplication',
              description: '중복 코드 블록을 감지합니다.',
              inputSchema: {
                type: 'object',
                properties: {
                  pathPrefix: { type: 'string', description: '검사할 경로 (선택)' },
                  minLines: { type: 'number', default: 5, description: '최소 중복 라인 수' }
                }
              }
            },
            {
              name: 'check_xml_documentation',
              description: 'XML 문서화 완성도를 체크합니다.',
              inputSchema: {
                type: 'object',
                properties: {
                  pathPrefix: { type: 'string', description: '검사할 경로 (선택)' },
                  maxResults: { type: 'number', default: 200 }
                }
              }
            },
            {
              name: 'analyze_gc_allocations',
              description: 'GC 할당 패턴을 분석합니다.',
              inputSchema: {
                type: 'object',
                properties: {
                  pathPrefix: { type: 'string', description: '검사할 경로 (선택)' }
                }
              }
            },
            {
              name: 'detect_missing_tests',
              description: '단위 테스트가 누락된 public 메서드를 찾습니다.',
              inputSchema: {
                type: 'object',
                properties: {
                  pathPrefix: { type: 'string', description: '검사할 경로 (선택)' }
                }
              }
            }
        ]
      };
    });

    // 도구 실행 처리
    this.server.setRequestHandler(CallToolRequestSchema, async (request) => {
      const { name, arguments: args } = request.params;

      try {
        switch (name) {
          case 'get_codebase_state':
            return await this.getCodebaseState();
          
          case 'analyze_file':
            return await this.analyzeFile(args.filePath);
          
          case 'suggest_code_reuse':
            return await this.suggestCodeReuse(args.requirement, args.targetSystem);
          
          case 'predict_conflicts':
            return await this.predictConflicts(args.newCode, args.targetFile);
          
          case 'check_forbidden_apis':
            return await this.checkForbiddenApis(args?.pathPrefix, args?.maxResults);
          
          case 'detect_update_loops':
            return await this.detectUpdateLoops(args?.pathPrefix, args?.maxResults);
          
          case 'zenject_binding_map':
            return await this.zenjectBindingMap();
          
          case 'dotween_lifecycle_check':
            return await this.dotweenLifecycleCheck(args?.pathPrefix);
          
          case 'addressables_audit':
            return await this.addressablesAudit(args?.pathPrefix);
          
          case 'save_schema_version_check':
            return await this.saveSchemaVersionCheck(args?.pathPrefix);
          
          case 'inspector_korean_labels_check':
            return await this.inspectorKoreanLabelsCheck(args?.pathPrefix, args?.maxResults);
          
          case 'quality_gate_report':
            return await this.qualityGateReport();

          case 'detect_circular_dependencies':
            return await this.detectCircularDependencies(args?.targetSystem);

          case 'get_error_log':
            return await this.getErrorLog();

          case 'invalidate_cache':
            return await this.invalidateCache();

          case 'calculate_cyclomatic_complexity':
            return await this.calculateCyclomaticComplexity(args?.pathPrefix, args?.maxResults, args?.threshold);

          case 'detect_code_duplication':
            return await this.detectCodeDuplication(args?.pathPrefix, args?.minLines);

          case 'check_xml_documentation':
            return await this.checkXmlDocumentation(args?.pathPrefix, args?.maxResults);

          case 'analyze_gc_allocations':
            return await this.analyzeGCAllocations(args?.pathPrefix);

          case 'detect_missing_tests':
            return await this.detectMissingTests(args?.pathPrefix);

          default:
            throw new Error(`알 수 없는 도구: ${name}`);
        }
      } catch (error) {
        return {
          content: [
            {
              type: 'text',
              text: JSON.stringify({
                error: true,
                message: error.message,
                timestamp: new Date().toISOString()
              }, null, 2)
            }
          ],
          isError: true
        };
      }
    });
  }

  /**
   * 코드베이스 상태 분석
   */
  async getCodebaseState() {
    try {
      const state = {
        projectRoot: PROJECT_ROOT,
        scriptPath: SCRIPT_PATH,
        systems: await this.analyzeSystems(),
        files: await this.scanFiles(),
        patterns: await this.analyzePatterns(),
        lastUpdated: new Date().toISOString(),
      };

      return {
        content: [
          {
            type: 'text',
            text: JSON.stringify(state, null, 2)
          }
        ]
      };
    } catch (error) {
      throw new Error(`코드베이스 상태 분석 실패: ${error.message}`);
    }
  }

  /**
   * 시스템별 분석
   */
  async analyzeSystems() {
    const systems = [
      'CoreSystem', 'CombatSystem', 'CharacterSystem', 
      'SkillCardSystem', 'SaveSystem', 'StageSystem', 
      'UISystem', 'UtilitySystem', 'InventorySystem'
    ];

    const systemAnalysis = {};

    for (const system of systems) {
      const systemPath = path.join(SCRIPT_PATH, system);
      if (fs.existsSync(systemPath)) {
        systemAnalysis[system] = {
          path: systemPath,
          exists: true,
          files: await this.getFilesInDirectory(systemPath, '.cs'),
        };
      } else {
        systemAnalysis[system] = {
          path: systemPath,
          exists: false,
        };
      }
    }

    return systemAnalysis;
  }

  /**
   * 파일 스캔 (캐싱 지원)
   */
  async scanFiles() {
    const now = Date.now();

    // 캐시가 유효하면 재사용
    if (this.fileCache && this.cacheTimestamp && (now - this.cacheTimestamp < this.CACHE_TTL)) {
      if (this.debugMode) {
        console.error(`[Cache Hit] 파일 스캔 캐시 사용 (${this.fileCache.length}개 파일)`);
      }
      return this.fileCache;
    }

    const files = [];

    const scanDirectory = (dir) => {
      try {
        const items = fs.readdirSync(dir);

        for (const item of items) {
          const fullPath = path.join(dir, item);
          const stat = fs.statSync(fullPath);

          if (stat.isDirectory()) {
            scanDirectory(fullPath);
          } else if (item.endsWith('.cs')) {
            files.push({
              path: fullPath,
              relativePath: path.relative(SCRIPT_PATH, fullPath),
              size: stat.size,
              modified: stat.mtime.toISOString(),
            });
          }
        }
      } catch (error) {
        this.logError('scanDirectory', error, dir);
      }
    };

    scanDirectory(SCRIPT_PATH);

    // 캐시 저장
    this.fileCache = files;
    this.cacheTimestamp = now;

    if (this.debugMode) {
      console.error(`[Cache Miss] 파일 스캔 완료 (${files.length}개 파일)`);
    }

    return files;
  }

  /**
   * 캐시 무효화
   */
  async invalidateCache() {
    this.fileCache = null;
    this.cacheTimestamp = null;

    return {
      content: [{
        type: 'text',
        text: JSON.stringify({ message: '캐시가 무효화되었습니다.' }, null, 2)
      }]
    };
  }

  /**
   * 에러 로깅
   */
  logError(context, error, filePath = '') {
    if (this.debugMode) {
      console.error(`[${context}] ${filePath}: ${error.message}`);
    }

    this.errorLog.push({
      context,
      file: filePath,
      error: error.message,
      timestamp: new Date().toISOString()
    });

    // 로그 크기 제한
    if (this.errorLog.length > this.maxErrorLog) {
      this.errorLog.shift();
    }
  }

  /**
   * 에러 로그 조회
   */
  async getErrorLog() {
    return {
      content: [{
        type: 'text',
        text: JSON.stringify({
          errors: this.errorLog,
          totalErrors: this.errorLog.length,
          debugMode: this.debugMode
        }, null, 2)
      }]
    };
  }

  /**
   * 패턴 분석
   */
  async analyzePatterns() {
    const patterns = {
      managers: [],
      interfaces: [],
      dataClasses: [],
      installers: [],
    };

    const files = await this.scanFiles();

    for (const file of files) {
      try {
        const content = fs.readFileSync(file.path, 'utf8');
        
        // Manager 패턴
        if (content.includes('Manager') && content.includes('class')) {
          patterns.managers.push(file.relativePath);
        }
        
        // Interface 패턴
        if (content.includes('interface ')) {
          patterns.interfaces.push(file.relativePath);
        }
        
        // Data 클래스 패턴
        if (content.includes('ScriptableObject') || content.includes('[System.Serializable]')) {
          patterns.dataClasses.push(file.relativePath);
        }
        
        // Installer 패턴
        if (content.includes('Installer') && content.includes('MonoInstaller')) {
          patterns.installers.push(file.relativePath);
        }
      } catch (error) {
        // 파일 읽기 오류 무시
      }
    }

    return patterns;
  }

  /**
   * 금지된 API 검사
   */
  async checkForbiddenApis(pathPrefix, maxResults = 200) {
    const files = await this.scanFiles();
    const findings = [];
    const forbidden = [
      { api: 'FindObjectOfType', ruleId: 'FORBIDDEN_API_FIND_OBJECT' },
      { api: 'Resources.Load', ruleId: 'FORBIDDEN_API_RESOURCES_LOAD' },
      { api: 'DOTween.KillAll', ruleId: 'FORBIDDEN_API_DOTWEEN_KILLALL' }
    ];

    for (const file of files) {
      if (pathPrefix && !file.path.startsWith(pathPrefix)) continue;
      try {
        const content = fs.readFileSync(file.path, 'utf8');
        for (const f of forbidden) {
          if (content.includes(f.api)) {
            findings.push({ severity: 'error', ruleId: f.ruleId, file: file.relativePath, message: `${f.api} 사용 감지`, suggestion: this.getForbiddenApiSuggestion(f.api) });
          }
        }
        if (findings.length >= maxResults) break;
      } catch {}
    }

    return { content: [{ type: 'text', text: JSON.stringify({ findings }, null, 2) }] };
  }

  getForbiddenApiSuggestion(api) {
    if (api === 'FindObjectOfType') return 'Zenject 주입 또는 캐싱 사용으로 대체하세요.';
    if (api === 'Resources.Load') return 'Addressables 로 전환하세요 (그룹/라벨 설정).';
    if (api === 'DOTween.KillAll') return '개별 Tween 핸들 관리 및 OnDisable 정리로 대체하세요.';
    return '대체안을 적용하세요.';
  }

  /**
   * Update 루프 감지 (컨텍스트 기반 개선)
   */
  async detectUpdateLoops(pathPrefix, maxResults = 200) {
    const files = await this.scanFiles();
    const findings = [];
    const patterns = [
      { method: 'Update', severity: 'warn' },
      { method: 'FixedUpdate', severity: 'info' },
      { method: 'LateUpdate', severity: 'info' }
    ];

    for (const file of files) {
      if (pathPrefix && !file.path.startsWith(pathPrefix)) continue;

      try {
        const content = fs.readFileSync(file.path, 'utf8');

        // 주석 제거
        const cleanContent = content
          .replace(/\/\/.*$/gm, '')
          .replace(/\/\*[\s\S]*?\*\//g, '');

        for (const pattern of patterns) {
          const methodRegex = new RegExp(`void\\s+${pattern.method}\\s*\\(\\s*\\)\\s*{([^}]*)}`, 'gs');
          const matches = [...cleanContent.matchAll(methodRegex)];

          for (const match of matches) {
            const methodBody = match[1].trim();

            // 빈 메서드는 무시
            if (methodBody.length === 0) continue;

            // Input 처리만 있는 경우는 허용
            if (pattern.method === 'Update' &&
                /Input\.(GetKey|GetButton)/i.test(methodBody) &&
                methodBody.split('\n').length <= 5) {
              continue;
            }

            // 물리 계산만 있는 경우는 허용
            if (pattern.method === 'FixedUpdate' &&
                /Rigidbody|AddForce|velocity/i.test(methodBody)) {
              continue;
            }

            findings.push({
              severity: pattern.severity,
              ruleId: 'UPDATE_LOOP_USAGE',
              file: file.relativePath,
              method: pattern.method,
              bodyLength: methodBody.length,
              message: `${pattern.method} 메서드에 비즈니스 로직 감지`,
              suggestion: '이벤트/코루틴/타이머 기반으로 전환 검토',
              snippet: methodBody.substring(0, 100) + (methodBody.length > 100 ? '...' : '')
            });
          }

          if (findings.length >= maxResults) break;
        }
      } catch (error) {
        this.logError('detectUpdateLoops', error, file.path);
      }
    }

    return {
      content: [{
        type: 'text',
        text: JSON.stringify({
          findings,
          summary: {
            total: findings.length,
            byMethod: findings.reduce((acc, f) => {
              acc[f.method] = (acc[f.method] || 0) + 1;
              return acc;
            }, {})
          }
        }, null, 2)
      }]
    };
  }

  /**
   * Zenject 바인딩 맵 (개선된 파싱)
   */
  async zenjectBindingMap() {
    const files = await this.scanFiles();
    const installers = [];

    for (const file of files) {
      if (!file.relativePath.endsWith('Installer.cs')) continue;

      try {
        const content = fs.readFileSync(file.path, 'utf8');

        // 주석 제거
        const cleanContent = content
          .replace(/\/\/.*$/gm, '')
          .replace(/\/\*[\s\S]*?\*\//g, '');

        if (!cleanContent.includes('MonoInstaller')) continue;

        // 멀티라인 바인딩 지원
        const binds = [];
        const lines = cleanContent.split('\n');
        let currentBind = '';

        for (const line of lines) {
          if (line.includes('Container.Bind')) {
            currentBind = line.trim();
          } else if (currentBind && (line.includes('AsSingle') || line.includes('AsTransient') || line.includes('AsCached'))) {
            currentBind += ' ' + line.trim();
            binds.push(currentBind);
            currentBind = '';
          } else if (currentBind) {
            currentBind += ' ' + line.trim();
          }
        }

        installers.push({
          file: file.relativePath,
          binds,
          bindCount: binds.length
        });
      } catch (error) {
        this.logError('zenjectBindingMap', error, file.path);
      }
    }

    return {
      content: [{
        type: 'text',
        text: JSON.stringify({
          installers,
          totalBindings: installers.reduce((sum, i) => sum + i.bindCount, 0)
        }, null, 2)
      }]
    };
  }

  /**
   * DOTween 수명주기 점검
   */
  async dotweenLifecycleCheck(pathPrefix) {
    const files = await this.scanFiles();
    const findings = [];
    for (const file of files) {
      if (pathPrefix && !file.path.startsWith(pathPrefix)) continue;
      try {
        const content = fs.readFileSync(file.path, 'utf8');
        if (content.includes('DOTween.') || content.includes('DOFade(') || content.includes('DOMove(') || content.includes('DOScale(')) {
          const hasCleanup = content.includes('OnDisable()') || content.includes('OnDestroy()');
          if (!hasCleanup) {
            findings.push({ severity: 'warn', ruleId: 'DOTWEEN_LIFECYCLE_CLEANUP_MISSING', file: file.relativePath, message: 'DOTween 사용 파일에 수명주기 정리 코드 누락 가능', suggestion: 'OnDisable/OnDestroy에서 Tween 정리 로직 추가' });
          }
        }
      } catch {}
    }
    return { content: [{ type: 'text', text: JSON.stringify({ findings }, null, 2) }] };
  }

  /**
   * Addressables 점검
   */
  async addressablesAudit(pathPrefix) {
    const files = await this.scanFiles();
    const findings = [];
    for (const file of files) {
      if (pathPrefix && !file.path.startsWith(pathPrefix)) continue;
      try {
        const content = fs.readFileSync(file.path, 'utf8');
        if (content.includes('Resources.Load')) {
          findings.push({ severity: 'warn', ruleId: 'RESOURCES_LOAD_USAGE', file: file.relativePath, message: 'Resources.Load 사용', suggestion: 'Addressables 로 전환 및 그룹/라벨 설정' });
        }
      } catch {}
    }
    return { content: [{ type: 'text', text: JSON.stringify({ findings }, null, 2) }] };
  }

  /**
   * 세이브 스키마 버전 점검
   */
  async saveSchemaVersionCheck(pathPrefix) {
    const files = await this.scanFiles();
    const findings = [];
    for (const file of files) {
      if (pathPrefix && !file.path.startsWith(pathPrefix)) continue;
      try {
        const content = fs.readFileSync(file.path, 'utf8');
        if (content.includes('class') && (content.includes('Save') || content.includes('save'))) {
          const hasVersion = content.includes('SaveVersion');
          if (!hasVersion) {
            findings.push({ severity: 'warn', ruleId: 'SAVE_VERSION_MISSING', file: file.relativePath, message: 'SaveVersion 필드 누락 가능', suggestion: '세이브 데이터 루트에 SaveVersion 추가 및 마이그레이션 체인 정의' });
          }
        }
      } catch {}
    }
    return { content: [{ type: 'text', text: JSON.stringify({ findings }, null, 2) }] };
  }

  /**
   * Inspector 한글 라벨 점검 (개선된 검사)
   */
  async inspectorKoreanLabelsCheck(pathPrefix, maxResults = 200) {
    const files = await this.scanFiles();
    const findings = [];

    for (const file of files) {
      if (pathPrefix && !file.path.startsWith(pathPrefix)) continue;

      try {
        const content = fs.readFileSync(file.path, 'utf8');

        // MonoBehaviour 또는 ScriptableObject만 검사
        const isUnityComponent = content.includes(': MonoBehaviour') ||
                                 content.includes(': ScriptableObject');

        if (!isUnityComponent) continue;

        // SerializeField가 있는 라인 추출
        const lines = content.split('\n');
        const serializeFields = [];

        for (let i = 0; i < lines.length; i++) {
          if (lines[i].includes('[SerializeField')) {
            // 해당 필드의 전체 선언 찾기 (Header, Tooltip 포함)
            let fieldBlock = '';
            let j = i;

            // 역방향으로 Header, Tooltip 찾기
            while (j >= 0 && j >= i - 5) {
              if (lines[j].includes('[Header(') || lines[j].includes('[Tooltip(')) {
                fieldBlock = lines.slice(j, i + 2).join('\n');
                break;
              }
              j--;
            }

            // Header/Tooltip이 없으면 SerializeField부터 시작
            if (!fieldBlock) {
              fieldBlock = lines.slice(i, i + 2).join('\n');
            }

            serializeFields.push({
              lineNumber: i + 1,
              block: fieldBlock
            });
          }
        }

        // 각 필드의 한글화 검사
        for (const field of serializeFields) {
          const hasKoreanHeader = /\[Header\("[가-힣]+/u.test(field.block);
          const hasKoreanTooltip = /\[Tooltip\("[가-힣]+/u.test(field.block);

          // public 필드는 Header/Tooltip 모두 필수
          // private 필드는 Tooltip만 권장
          const isPublic = field.block.includes('public');

          if (isPublic && !hasKoreanHeader) {
            findings.push({
              severity: 'warn',
              ruleId: 'INSPECTOR_KOREAN_HEADER_MISSING',
              file: file.relativePath,
              line: field.lineNumber,
              message: 'public 필드에 한글 Header 누락',
              suggestion: '[Header("한글 제목")]을 추가하세요'
            });
          }

          if (!hasKoreanTooltip) {
            findings.push({
              severity: 'info',
              ruleId: 'INSPECTOR_KOREAN_TOOLTIP_MISSING',
              file: file.relativePath,
              line: field.lineNumber,
              message: 'Tooltip 한글화 권장',
              suggestion: '[Tooltip("한글 설명")]을 추가하세요'
            });
          }
        }

        if (findings.length >= maxResults) break;
      } catch (error) {
        this.logError('inspectorKoreanLabelsCheck', error, file.path);
      }
    }

    return { content: [{ type: 'text', text: JSON.stringify({ findings }, null, 2) }] };
  }

  /**
   * 품질 게이트 요약 리포트
   */
  async qualityGateReport() {
    const [apis, updates, addr, save] = await Promise.all([
      this.checkForbiddenApis(),
      this.detectUpdateLoops(),
      this.addressablesAudit(),
      this.saveSchemaVersionCheck()
    ]);
    const summary = {
      forbiddenApis: JSON.parse(apis.content[0].text).findings.length,
      updateLoops: JSON.parse(updates.content[0].text).findings.length,
      resourcesLoadUsages: JSON.parse(addr.content[0].text).findings.length,
      saveIssues: JSON.parse(save.content[0].text).findings.length
    };
    return { content: [{ type: 'text', text: JSON.stringify({ summary }, null, 2) }] };
  }

  /**
   * 특정 파일 분석
   */
  async analyzeFile(filePath) {
    try {
      const fullPath = path.isAbsolute(filePath) ? filePath : path.join(SCRIPT_PATH, filePath);
      
      if (!fs.existsSync(fullPath)) {
        throw new Error(`파일을 찾을 수 없습니다: ${fullPath}`);
      }

      const content = fs.readFileSync(fullPath, 'utf8');
      const relativePath = path.relative(SCRIPT_PATH, fullPath);

      const analysis = {
        filePath: relativePath,
        dependencies: this.extractDependencies(content),
        patterns: this.extractFilePatterns(content),
        classes: this.extractClasses(content),
        methods: this.extractMethods(content),
        size: fs.statSync(fullPath).size,
        lastModified: fs.statSync(fullPath).mtime.toISOString(),
      };

      return {
        content: [
          {
            type: 'text',
            text: JSON.stringify(analysis, null, 2)
          }
        ]
      };
    } catch (error) {
      throw new Error(`파일 분석 실패: ${error.message}`);
    }
  }

  /**
   * 파일에서 의존성 추출
   */
  extractDependencies(content) {
    const dependencies = [];
    
    // using 문에서 의존성 추출
    const usingMatches = content.match(/using\s+([^;]+);/g);
    if (usingMatches) {
      for (const match of usingMatches) {
        const namespace = match.replace(/using\s+/, '').replace(/;/, '');
        if (!namespace.startsWith('System') && !namespace.startsWith('Unity')) {
          dependencies.push(namespace);
        }
      }
    }

    return dependencies;
  }

  /**
   * 파일 패턴 추출
   */
  extractFilePatterns(content) {
    const patterns = [];
    
    if (content.includes('MonoBehaviour')) patterns.push('MonoBehaviour');
    if (content.includes('ScriptableObject')) patterns.push('ScriptableObject');
    if (content.includes('MonoInstaller')) patterns.push('MonoInstaller');
    if (content.includes('interface ')) patterns.push('Interface');
    if (content.includes('Manager')) patterns.push('Manager');
    if (content.includes('Factory')) patterns.push('Factory');
    
    return patterns;
  }

  /**
   * 클래스 추출
   */
  extractClasses(content) {
    const classes = [];
    const classMatches = content.match(/class\s+(\w+)/g);
    
    if (classMatches) {
      for (const match of classMatches) {
        classes.push(match.replace('class ', ''));
      }
    }
    
    return classes;
  }

  /**
   * 메서드 추출
   */
  extractMethods(content) {
    const methods = [];
    const methodMatches = content.match(/(public|private|protected)\s+\w+\s+(\w+)\s*\(/g);
    
    if (methodMatches) {
      for (const match of methodMatches) {
        const methodName = match.match(/(\w+)\s*\(/)[1];
        methods.push(methodName);
      }
    }
    
    return methods;
  }

  /**
   * 코드 재사용 제안
   */
  async suggestCodeReuse(requirement, targetSystem) {
    try {
      const suggestions = {
        requirement,
        targetSystem,
        existingClasses: await this.findReusableClasses(requirement, targetSystem),
        existingMethods: await this.findReusableMethods(requirement, targetSystem),
        existingPatterns: await this.findReusablePatterns(requirement, targetSystem),
        reuseStrategy: this.determineReuseStrategy(requirement, targetSystem),
      };

      return {
        content: [
          {
            type: 'text',
            text: JSON.stringify(suggestions, null, 2)
          }
        ]
      };
    } catch (error) {
      throw new Error(`코드 재사용 제안 실패: ${error.message}`);
    }
  }

  /**
   * 재사용 가능한 클래스 찾기 (스코어 기반)
   */
  async findReusableClasses(requirement, targetSystem) {
    const files = await this.scanFiles();
    const reusableClasses = [];

    for (const file of files) {
      if (targetSystem && !file.relativePath.includes(targetSystem)) {
        continue;
      }

      try {
        const content = fs.readFileSync(file.path, 'utf8');
        const classes = this.extractClasses(content);

        for (const className of classes) {
          const score = this.getReusabilityScore(className, requirement);
          if (score >= 40) {
            reusableClasses.push({
              className,
              file: file.relativePath,
              score,
              reason: this.getReusabilityReason(className, requirement, score),
            });
          }
        }
      } catch (error) {
        this.logError('findReusableClasses', error, file.path);
      }
    }

    // 스코어 기준 정렬
    reusableClasses.sort((a, b) => b.score - a.score);

    return reusableClasses;
  }

  /**
   * 재사용 가능한 메서드 찾기
   */
  async findReusableMethods(requirement, targetSystem) {
    const files = await this.scanFiles();
    const reusableMethods = [];

    for (const file of files) {
      if (targetSystem && !file.relativePath.includes(targetSystem)) {
        continue;
      }

      try {
        const content = fs.readFileSync(file.path, 'utf8');
        const methods = this.extractMethods(content);
        
        for (const methodName of methods) {
          if (this.isReusableMethod(methodName, requirement)) {
            reusableMethods.push({
              methodName,
              file: file.relativePath,
              reason: this.getReusabilityReason(methodName, requirement),
            });
          }
        }
      } catch (error) {
        // 파일 읽기 오류 무시
      }
    }

    return reusableMethods;
  }

  /**
   * 재사용 가능한 패턴 찾기
   */
  async findReusablePatterns(requirement, targetSystem) {
    const patterns = await this.analyzePatterns();
    const reusablePatterns = [];

    for (const [patternType, files] of Object.entries(patterns)) {
      for (const file of files) {
        if (targetSystem && !file.includes(targetSystem)) {
          continue;
        }

        if (this.isReusablePattern(patternType, requirement)) {
          reusablePatterns.push({
            patternType,
            file,
            reason: this.getReusabilityReason(patternType, requirement),
          });
        }
      }
    }

    return reusablePatterns;
  }

  /**
   * 재사용 전략 결정
   */
  determineReuseStrategy(requirement, targetSystem) {
    const requirementLower = requirement.toLowerCase();
    
    if (requirementLower.includes('manager') || requirementLower.includes('관리')) {
      return 'ExtendExistingManager';
    }
    
    if (requirementLower.includes('data') || requirementLower.includes('데이터')) {
      return 'CreateDataClass';
    }
    
    if (requirementLower.includes('ui') || requirementLower.includes('인터페이스')) {
      return 'ExtendExistingUI';
    }
    
    if (requirementLower.includes('system') || requirementLower.includes('시스템')) {
      return 'ExtendExistingSystem';
    }
    
    return 'FollowExistingPattern';
  }

  /**
   * 재사용 가능성 스코어 계산
   */
  getReusabilityScore(name, requirement) {
    const requirementLower = requirement.toLowerCase();
    const nameLower = name.toLowerCase();
    const requirementTokens = requirementLower.split(/\s+/);

    let score = 0;

    // 완전 일치: 최고 점수
    if (nameLower === requirementLower) {
      score = 100;
    }
    // 이름이 요구사항 포함
    else if (nameLower.includes(requirementLower)) {
      score = 80;
    }
    // 요구사항이 이름 포함
    else if (requirementLower.includes(nameLower)) {
      score = 60;
    }
    // 토큰 매칭
    else {
      for (const token of requirementTokens) {
        if (token.length < 3) continue; // 짧은 토큰 제외
        if (nameLower.includes(token)) {
          score += 20;
        }
      }
    }

    return score;
  }

  /**
   * 재사용 가능한 클래스인지 확인
   */
  isReusableClass(className, requirement) {
    return this.getReusabilityScore(className, requirement) >= 40;
  }

  /**
   * 재사용 가능한 메서드인지 확인
   */
  isReusableMethod(methodName, requirement) {
    const requirementLower = requirement.toLowerCase();
    const methodNameLower = methodName.toLowerCase();
    
    return requirementLower.includes(methodNameLower) || 
           methodNameLower.includes(requirementLower.split(' ')[0]);
  }

  /**
   * 재사용 가능한 패턴인지 확인
   */
  isReusablePattern(patternType, requirement) {
    const requirementLower = requirement.toLowerCase();
    
    switch (patternType) {
      case 'managers':
        return requirementLower.includes('manager') || requirementLower.includes('관리');
      case 'interfaces':
        return requirementLower.includes('interface') || requirementLower.includes('인터페이스');
      case 'dataClasses':
        return requirementLower.includes('data') || requirementLower.includes('데이터');
      case 'installers':
        return requirementLower.includes('installer') || requirementLower.includes('설치');
      default:
        return false;
    }
  }

  /**
   * 재사용 가능성 이유 반환 (스코어 기반)
   */
  getReusabilityReason(name, requirement, score = 0) {
    const requirementLower = requirement.toLowerCase();
    const nameLower = name.toLowerCase();

    if (score >= 100) {
      return `완전 일치: "${name}"은 요구사항과 정확히 일치합니다.`;
    }

    if (score >= 80) {
      return `높은 관련성: "${name}"은 요구사항 "${requirement}"를 포함합니다.`;
    }

    if (score >= 60) {
      return `관련성 있음: 요구사항이 "${name}"을 포함합니다.`;
    }

    if (score >= 40) {
      return `부분 일치: "${name}"이 요구사항의 일부 키워드와 일치합니다.`;
    }

    return `"${name}"이 요구사항과 관련이 있을 수 있습니다.`;
  }

  /**
   * 충돌 예측
   */
  async predictConflicts(newCode, targetFile) {
    try {
      const conflicts = {
        namingConflicts: await this.checkNamingConflicts(newCode, targetFile),
        interfaceConflicts: await this.checkInterfaceConflicts(newCode, targetFile),
        dependencyConflicts: await this.checkDependencyConflicts(newCode, targetFile),
        logicConflicts: await this.checkLogicConflicts(newCode, targetFile),
      };

      return {
        content: [
          {
            type: 'text',
            text: JSON.stringify(conflicts, null, 2)
          }
        ]
      };
    } catch (error) {
      throw new Error(`충돌 예측 실패: ${error.message}`);
    }
  }

  /**
   * 네이밍 충돌 검사
   */
  async checkNamingConflicts(newCode, targetFile) {
    const conflicts = [];
    const files = await this.scanFiles();
    
    // 새 코드에서 클래스명, 메서드명 추출
    const newClasses = this.extractClasses(newCode);
    const newMethods = this.extractMethods(newCode);
    
    for (const file of files) {
      if (file.relativePath === targetFile) continue;
      
      try {
        const content = fs.readFileSync(file.path, 'utf8');
        const existingClasses = this.extractClasses(content);
        const existingMethods = this.extractMethods(content);
        
        // 클래스명 충돌 검사
        for (const newClass of newClasses) {
          if (existingClasses.includes(newClass)) {
            conflicts.push({
              type: 'class',
              name: newClass,
              conflictFile: file.relativePath,
              reason: `클래스명 "${newClass}"이 이미 존재합니다.`,
            });
          }
        }
        
        // 메서드명 충돌 검사
        for (const newMethod of newMethods) {
          if (existingMethods.includes(newMethod)) {
            conflicts.push({
              type: 'method',
              name: newMethod,
              conflictFile: file.relativePath,
              reason: `메서드명 "${newMethod}"이 이미 존재합니다.`,
            });
          }
        }
      } catch (error) {
        // 파일 읽기 오류 무시
      }
    }
    
    return conflicts;
  }

  /**
   * 인터페이스 충돌 검사
   */
  async checkInterfaceConflicts(newCode, targetFile) {
    const conflicts = [];
    const newInterfaces = this.extractFilePatterns(newCode).filter(p => p === 'Interface');
    
    // 간단한 인터페이스 충돌 검사
    if (newInterfaces.length > 0) {
      conflicts.push({
        type: 'interface',
        name: 'Interface',
        reason: '새로운 인터페이스가 기존 인터페이스와 충돌할 수 있습니다.',
      });
    }
    
    return conflicts;
  }

  /**
   * 의존성 충돌 검사
   */
  async checkDependencyConflicts(newCode, targetFile) {
    const conflicts = [];
    const newDependencies = this.extractDependencies(newCode);
    
    // 간단한 의존성 충돌 검사
    for (const newDep of newDependencies) {
      if (newDep.includes('AnimationSystem')) {
        conflicts.push({
          type: 'dependency',
          name: newDep,
          reason: `의존성 "${newDep}"은 프로젝트에서 제거되었습니다.`,
        });
      }
    }
    
    return conflicts;
  }

  /**
   * 로직 충돌 검사
   */
  async checkLogicConflicts(newCode, targetFile) {
    const conflicts = [];
    
    // 간단한 로직 충돌 검사
    if (newCode.includes('DOTween.KillAll()')) {
      conflicts.push({
        type: 'logic',
        name: 'DOTween.KillAll',
        reason: 'DOTween.KillAll()은 다른 시스템의 애니메이션까지 종료시킬 수 있습니다.',
      });
    }
    
    if (newCode.includes('FindObjectOfType') && !newCode.includes('null')) {
      conflicts.push({
        type: 'logic',
        name: 'FindObjectOfType',
        reason: 'FindObjectOfType 결과에 null 체크가 필요합니다.',
      });
    }
    
    return conflicts;
  }

  /**
   * 디렉토리의 파일 목록 반환
   */
  async getFilesInDirectory(dirPath, extension) {
    try {
      const items = fs.readdirSync(dirPath);
      return items.filter(item => item.endsWith(extension));
    } catch (error) {
      return [];
    }
  }

  /**
   * 순환 복잡도 계산
   */
  async calculateCyclomaticComplexity(pathPrefix, maxResults = 100, threshold = 10) {
    const files = await this.scanFiles();
    const findings = [];

    for (const file of files) {
      if (pathPrefix && !file.path.startsWith(pathPrefix)) continue;

      try {
        const content = fs.readFileSync(file.path, 'utf8');

        // 주석 제거
        const cleanContent = content
          .replace(/\/\/.*$/gm, '')
          .replace(/\/\*[\s\S]*?\*\//g, '');

        // 메서드 추출 (public/private/protected)
        const methodRegex = /(public|private|protected)\s+(?:async\s+)?(?:static\s+)?(?:\w+\s+)?(\w+)\s*\([^)]*\)\s*{/g;
        let match;

        while ((match = methodRegex.exec(cleanContent)) !== null) {
          const methodName = match[2];
          const startIndex = match.index + match[0].length;

          // 메서드 본문 추출
          let braceCount = 1;
          let endIndex = startIndex;

          for (let i = startIndex; i < cleanContent.length && braceCount > 0; i++) {
            if (cleanContent[i] === '{') braceCount++;
            if (cleanContent[i] === '}') braceCount--;
            endIndex = i;
          }

          const methodBody = cleanContent.substring(startIndex, endIndex);

          // 순환 복잡도 계산
          const complexity = this.calculateComplexity(methodBody);

          if (complexity >= threshold) {
            findings.push({
              severity: complexity >= 15 ? 'error' : 'warn',
              ruleId: 'HIGH_CYCLOMATIC_COMPLEXITY',
              file: file.relativePath,
              method: methodName,
              complexity,
              threshold,
              message: `메서드 "${methodName}"의 순환 복잡도가 ${complexity}입니다 (임계값: ${threshold})`,
              suggestion: '메서드를 더 작은 단위로 분리하세요'
            });
          }

          if (findings.length >= maxResults) break;
        }
      } catch (error) {
        this.logError('calculateCyclomaticComplexity', error, file.path);
      }

      if (findings.length >= maxResults) break;
    }

    return {
      content: [{
        type: 'text',
        text: JSON.stringify({
          findings,
          summary: {
            total: findings.length,
            averageComplexity: findings.length > 0
              ? (findings.reduce((sum, f) => sum + f.complexity, 0) / findings.length).toFixed(2)
              : 0,
            highestComplexity: findings.length > 0
              ? Math.max(...findings.map(f => f.complexity))
              : 0
          }
        }, null, 2)
      }]
    };
  }

  /**
   * 복잡도 계산 헬퍼
   */
  calculateComplexity(methodBody) {
    let complexity = 1; // 기본 복잡도

    // 제어문 카운트
    const controlStructures = [
      /\bif\s*\(/g,
      /\belse\s+if\s*\(/g,
      /\bwhile\s*\(/g,
      /\bfor\s*\(/g,
      /\bforeach\s*\(/g,
      /\bcase\s+/g,
      /\bcatch\s*\(/g,
      /\b\?\s*.*\s*:/g, // 삼항 연산자
      /\&\&/g, // 논리 AND
      /\|\|/g  // 논리 OR
    ];

    for (const pattern of controlStructures) {
      const matches = methodBody.match(pattern);
      if (matches) {
        complexity += matches.length;
      }
    }

    return complexity;
  }

  /**
   * 코드 중복 감지
   */
  async detectCodeDuplication(pathPrefix, minLines = 5) {
    const files = await this.scanFiles();
    const codeBlocks = new Map();
    const duplicates = [];

    for (const file of files) {
      if (pathPrefix && !file.path.startsWith(pathPrefix)) continue;

      try {
        const content = fs.readFileSync(file.path, 'utf8');

        // 주석 제거
        const cleanContent = content
          .replace(/\/\/.*$/gm, '')
          .replace(/\/\*[\s\S]*?\*\//g, '');

        const lines = cleanContent.split('\n');

        // N줄씩 블록으로 나눠서 해시 생성
        for (let i = 0; i <= lines.length - minLines; i++) {
          const block = lines.slice(i, i + minLines)
            .map(l => l.trim())
            .filter(l => l.length > 0)
            .join('\n');

          if (block.length < 50) continue; // 너무 짧은 블록 제외

          const hash = this.simpleHash(block);

          if (!codeBlocks.has(hash)) {
            codeBlocks.set(hash, []);
          }

          codeBlocks.get(hash).push({
            file: file.relativePath,
            startLine: i + 1,
            block: block.substring(0, 200) + (block.length > 200 ? '...' : '')
          });
        }
      } catch (error) {
        this.logError('detectCodeDuplication', error, file.path);
      }
    }

    // 중복 찾기
    for (const [hash, occurrences] of codeBlocks.entries()) {
      if (occurrences.length > 1) {
        duplicates.push({
          severity: 'warn',
          ruleId: 'CODE_DUPLICATION',
          occurrences: occurrences.length,
          locations: occurrences,
          message: `${minLines}줄 이상의 중복 코드가 ${occurrences.length}곳에서 발견되었습니다`,
          suggestion: '중복 코드를 공통 메서드로 추출하세요'
        });
      }
    }

    // 발생 빈도순 정렬
    duplicates.sort((a, b) => b.occurrences - a.occurrences);

    return {
      content: [{
        type: 'text',
        text: JSON.stringify({
          duplicates: duplicates.slice(0, 50), // 최대 50개
          summary: {
            totalDuplicates: duplicates.length,
            totalOccurrences: duplicates.reduce((sum, d) => sum + d.occurrences, 0)
          }
        }, null, 2)
      }]
    };
  }

  /**
   * 간단한 해시 함수
   */
  simpleHash(str) {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash;
    }
    return hash.toString(36);
  }

  /**
   * XML 문서화 체크
   */
  async checkXmlDocumentation(pathPrefix, maxResults = 200) {
    const files = await this.scanFiles();
    const findings = [];

    for (const file of files) {
      if (pathPrefix && !file.path.startsWith(pathPrefix)) continue;

      try {
        const content = fs.readFileSync(file.path, 'utf8');
        const lines = content.split('\n');

        // public 메서드/프로퍼티 찾기
        for (let i = 0; i < lines.length; i++) {
          const line = lines[i].trim();

          // public 선언 확인
          if (line.startsWith('public ') && !line.includes('//')) {
            // 클래스/메서드/프로퍼티 구분
            const isClass = line.includes('class ');
            const isMethod = line.includes('(') && line.includes(')');
            const isProperty = !isClass && !isMethod && (line.includes('{ get') || line.includes('=> '));

            if (isClass || isMethod || isProperty) {
              // 이전 줄에 /// 주석이 있는지 확인
              let hasXmlDoc = false;
              let j = i - 1;

              while (j >= 0 && lines[j].trim().startsWith('///')) {
                hasXmlDoc = true;
                break;
              }

              if (!hasXmlDoc) {
                const memberType = isClass ? 'class' : isMethod ? 'method' : 'property';
                const memberName = this.extractMemberName(line);

                findings.push({
                  severity: 'warn',
                  ruleId: 'MISSING_XML_DOCUMENTATION',
                  file: file.relativePath,
                  line: i + 1,
                  memberType,
                  memberName,
                  message: `public ${memberType} "${memberName}"에 XML 문서화가 없습니다`,
                  suggestion: '/// <summary> 태그를 추가하세요'
                });
              }
            }
          }

          if (findings.length >= maxResults) break;
        }
      } catch (error) {
        this.logError('checkXmlDocumentation', error, file.path);
      }

      if (findings.length >= maxResults) break;
    }

    return {
      content: [{
        type: 'text',
        text: JSON.stringify({
          findings,
          summary: {
            total: findings.length,
            byType: findings.reduce((acc, f) => {
              acc[f.memberType] = (acc[f.memberType] || 0) + 1;
              return acc;
            }, {})
          }
        }, null, 2)
      }]
    };
  }

  /**
   * 멤버 이름 추출
   */
  extractMemberName(line) {
    // class/method/property 이름 추출
    const classMatch = line.match(/class\s+(\w+)/);
    if (classMatch) return classMatch[1];

    const methodMatch = line.match(/\s+(\w+)\s*\(/);
    if (methodMatch) return methodMatch[1];

    const propMatch = line.match(/\s+(\w+)\s+{/);
    if (propMatch) return propMatch[1];

    return 'Unknown';
  }

  /**
   * GC 할당 패턴 분석
   */
  async analyzeGCAllocations(pathPrefix) {
    const files = await this.scanFiles();
    const findings = [];

    const allocPatterns = [
      { pattern: /new\s+\w+\[/g, type: 'Array Allocation', severity: 'warn' },
      { pattern: /new\s+List</g, type: 'List Allocation', severity: 'info' },
      { pattern: /new\s+Dictionary</g, type: 'Dictionary Allocation', severity: 'info' },
      { pattern: /new\s+string\[/g, type: 'String Array Allocation', severity: 'warn' },
      { pattern: /string\.Concat\(/g, type: 'String Concatenation', severity: 'warn' },
      { pattern: /\+\s*"[^"]*"/g, type: 'String Concatenation (Operator)', severity: 'info' },
      { pattern: /\.ToList\(\)/g, type: 'LINQ ToList', severity: 'info' },
      { pattern: /\.ToArray\(\)/g, type: 'LINQ ToArray', severity: 'warn' },
      { pattern: /Instantiate\(/g, type: 'GameObject Instantiate', severity: 'info' },
      { pattern: /new\s+GameObject\(/g, type: 'GameObject Creation', severity: 'info' }
    ];

    for (const file of files) {
      if (pathPrefix && !file.path.startsWith(pathPrefix)) continue;

      try {
        const content = fs.readFileSync(file.path, 'utf8');

        // Update 메서드 내부인지 확인
        const updateRegex = /void\s+(Update|FixedUpdate|LateUpdate)\s*\([^)]*\)\s*{([^}]*)}/gs;
        const updateMatches = [...content.matchAll(updateRegex)];

        for (const updateMatch of updateMatches) {
          const methodName = updateMatch[1];
          const methodBody = updateMatch[2];

          for (const { pattern, type, severity } of allocPatterns) {
            const matches = methodBody.match(pattern);
            if (matches) {
              findings.push({
                severity,
                ruleId: 'GC_ALLOCATION_IN_UPDATE',
                file: file.relativePath,
                method: methodName,
                allocationType: type,
                count: matches.length,
                message: `${methodName}에서 ${type}이 ${matches.length}회 발생합니다`,
                suggestion: '오브젝트 풀링 또는 사전 할당을 고려하세요'
              });
            }
          }
        }
      } catch (error) {
        this.logError('analyzeGCAllocations', error, file.path);
      }
    }

    return {
      content: [{
        type: 'text',
        text: JSON.stringify({
          findings,
          summary: {
            total: findings.length,
            byType: findings.reduce((acc, f) => {
              acc[f.allocationType] = (acc[f.allocationType] || 0) + f.count;
              return acc;
            }, {}),
            totalAllocations: findings.reduce((sum, f) => sum + f.count, 0)
          }
        }, null, 2)
      }]
    };
  }

  /**
   * 누락된 테스트 감지
   */
  async detectMissingTests(pathPrefix) {
    const files = await this.scanFiles();
    const publicMethods = [];
    const testFiles = new Set();

    // 1단계: 모든 public 메서드 수집
    for (const file of files) {
      if (pathPrefix && !file.path.startsWith(pathPrefix)) continue;
      if (file.relativePath.includes('Test')) {
        testFiles.add(file.relativePath);
        continue;
      }

      try {
        const content = fs.readFileSync(file.path, 'utf8');
        const className = this.extractClasses(content)[0];

        if (!className) continue;

        // public 메서드 추출
        const methodRegex = /public\s+(?:async\s+)?(?:static\s+)?(?:\w+\s+)?(\w+)\s*\([^)]*\)/g;
        let match;

        while ((match = methodRegex.exec(content)) !== null) {
          const methodName = match[1];

          // 생성자, 프로퍼티 제외
          if (methodName === className) continue;
          if (content.includes(`${methodName} {`) || content.includes(`${methodName} =>`)) continue;

          publicMethods.push({
            file: file.relativePath,
            className,
            methodName
          });
        }
      } catch (error) {
        this.logError('detectMissingTests', error, file.path);
      }
    }

    // 2단계: 테스트 파일에서 테스트된 메서드 확인
    const testedMethods = new Set();

    for (const testFile of testFiles) {
      const fullPath = path.join(SCRIPT_PATH, testFile);
      try {
        const content = fs.readFileSync(fullPath, 'utf8');

        for (const method of publicMethods) {
          // 메서드 이름이 테스트 파일에 언급되는지 확인
          if (content.includes(method.methodName)) {
            testedMethods.add(`${method.className}.${method.methodName}`);
          }
        }
      } catch (error) {
        this.logError('detectMissingTests', error, fullPath);
      }
    }

    // 3단계: 테스트가 없는 메서드 찾기
    const missingTests = publicMethods
      .filter(m => !testedMethods.has(`${m.className}.${m.methodName}`))
      .map(m => ({
        severity: 'info',
        ruleId: 'MISSING_UNIT_TEST',
        file: m.file,
        className: m.className,
        methodName: m.methodName,
        message: `public 메서드 "${m.className}.${m.methodName}"에 대한 테스트가 없습니다`,
        suggestion: '단위 테스트를 작성하세요'
      }));

    return {
      content: [{
        type: 'text',
        text: JSON.stringify({
          missingTests: missingTests.slice(0, 100), // 최대 100개
          summary: {
            totalPublicMethods: publicMethods.length,
            testedMethods: testedMethods.size,
            missingTests: missingTests.length,
            coverage: publicMethods.length > 0
              ? ((testedMethods.size / publicMethods.length) * 100).toFixed(2) + '%'
              : '0%'
          }
        }, null, 2)
      }]
    };
  }

  /**
   * 순환 의존성 감지
   */
  async detectCircularDependencies(targetSystem) {
    const files = await this.scanFiles();
    const dependencyGraph = new Map();

    // 의존성 그래프 구축
    for (const file of files) {
      if (targetSystem && !file.relativePath.includes(targetSystem)) continue;

      try {
        const content = fs.readFileSync(file.path, 'utf8');
        const className = this.extractClasses(content)[0];
        if (!className) continue;

        const dependencies = [];

        // [Inject] 의존성 추출
        const injectMatches = content.match(/\[Inject\]\s+(?:private|public)\s+(\w+)/g);
        if (injectMatches) {
          for (const match of injectMatches) {
            const depType = match.match(/(\w+)$/)[0];
            dependencies.push(depType);
          }
        }

        // using 문에서 프로젝트 네임스페이스 추출
        const usingMatches = content.match(/using\s+Game\.([^;]+);/g);
        if (usingMatches) {
          for (const match of usingMatches) {
            const namespace = match.replace(/using\s+/, '').replace(/;/, '');
            const parts = namespace.split('.');
            if (parts.length > 2) {
              dependencies.push(parts[parts.length - 1]);
            }
          }
        }

        dependencyGraph.set(className, { file: file.relativePath, dependencies });
      } catch (error) {
        this.logError('detectCircularDependencies', error, file.path);
      }
    }

    // 순환 의존성 탐지 (DFS)
    const cycles = [];
    const visited = new Set();
    const recursionStack = new Set();

    const detectCycle = (node, path = []) => {
      if (recursionStack.has(node)) {
        const cycleStart = path.indexOf(node);
        if (cycleStart >= 0) {
          const cycle = path.slice(cycleStart).concat(node);
          // 중복 사이클 제거
          const cycleKey = cycle.sort().join('->');
          if (!cycles.find(c => c.key === cycleKey)) {
            cycles.push({ key: cycleKey, cycle });
          }
        }
        return;
      }

      if (visited.has(node)) return;

      visited.add(node);
      recursionStack.add(node);
      path.push(node);

      const nodeData = dependencyGraph.get(node);
      if (nodeData) {
        for (const dep of nodeData.dependencies) {
          detectCycle(dep, [...path]);
        }
      }

      recursionStack.delete(node);
    };

    for (const node of dependencyGraph.keys()) {
      if (!visited.has(node)) {
        detectCycle(node);
      }
    }

    return {
      content: [{
        type: 'text',
        text: JSON.stringify({
          dependencyGraph: Array.from(dependencyGraph.entries()).map(([name, data]) => ({
            class: name,
            file: data.file,
            dependencies: data.dependencies
          })),
          cycles: cycles.map(c => c.cycle),
          hasCycles: cycles.length > 0,
          summary: {
            totalClasses: dependencyGraph.size,
            totalCycles: cycles.length
          }
        }, null, 2)
      }]
    };
  }

  /**
   * 서버 시작
   */
  async start() {
    const transport = new StdioServerTransport();
    await this.server.connect(transport);
    console.error('RougeShool MCP Server v2.1.0이 시작되었습니다.');
    if (this.debugMode) {
      console.error('[DEBUG MODE] 디버그 모드가 활성화되었습니다.');
    }
  }
}

// 서버 시작
const server = new CleanRougeShoolMCPServer();
server.start().catch(console.error);
