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
 * RougeShool 프로젝트를 위한 MCP 서버
 * 코드베이스 분석, 파일 분석, 코드 재사용 제안 기능을 제공
 */
class RougeShoolMCPServer {
  constructor() {
    this.server = new Server(
      {
        name: 'rougeshool-mcp-server',
        version: '1.0.0',
      },
      {
        capabilities: {
          tools: {},
        },
      }
    );

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
              properties: {},
            },
          },
          {
            name: 'analyze_file',
            description: '특정 파일의 의존성, 패턴, 인터페이스를 분석합니다.',
            inputSchema: {
              type: 'object',
              properties: {
                filePath: {
                  type: 'string',
                  description: '분석할 파일의 경로',
                },
              },
              required: ['filePath'],
            },
          },
          {
            name: 'suggest_code_reuse',
            description: '요구사항에 맞는 기존 코드 재사용 방안을 제안합니다.',
            inputSchema: {
              type: 'object',
              properties: {
                requirement: {
                  type: 'string',
                  description: '구현하려는 기능의 요구사항',
                },
                targetSystem: {
                  type: 'string',
                  description: '대상 시스템 (CoreSystem, CombatSystem, CharacterSystem, SkillCardSystem, SaveSystem, StageSystem, UISystem, UtilitySystem)',
                  enum: ['CoreSystem', 'CombatSystem', 'CharacterSystem', 'SkillCardSystem', 'SaveSystem', 'StageSystem', 'UISystem', 'UtilitySystem'],
                },
              },
              required: ['requirement'],
            },
          },
          {
            name: 'predict_conflicts',
            description: '새로운 코드가 기존 코드와 충돌할 가능성을 예측합니다.',
            inputSchema: {
              type: 'object',
              properties: {
                newCode: {
                  type: 'string',
                  description: '새로 추가할 코드',
                },
                targetFile: {
                  type: 'string',
                  description: '대상 파일 경로',
                },
              },
              required: ['newCode', 'targetFile'],
            },
          },
        ],
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
          
          default:
            throw new Error(`알 수 없는 도구: ${name}`);
        }
      } catch (error) {
        return {
          content: [
            {
              type: 'text',
              text: `오류 발생: ${error.message}`,
            },
          ],
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
        dependencies: await this.buildDependencyGraph(),
        patterns: await this.analyzePatterns(),
        interfaces: await this.analyzeInterfaces(),
        lastUpdated: new Date().toISOString(),
      };

      return {
        content: [
          {
            type: 'text',
            text: `# 코드베이스 상태 분석 완료\n\n${JSON.stringify(state, null, 2)}`,
          },
        ],
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
          subdirectories: await this.getSubdirectories(systemPath),
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
   * 파일 스캔
   */
  async scanFiles() {
    const files = [];
    
    const scanDirectory = (dir) => {
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
    };

    scanDirectory(SCRIPT_PATH);
    return files;
  }

  /**
   * 의존성 그래프 구축
   */
  async buildDependencyGraph() {
    const dependencies = {};
    const files = await this.scanFiles();

    for (const file of files) {
      try {
        const content = fs.readFileSync(file.path, 'utf8');
        const fileDependencies = this.extractDependencies(content);
        
        dependencies[file.relativePath] = fileDependencies;
      } catch (error) {
        console.warn(`파일 읽기 실패: ${file.path}`, error.message);
        dependencies[file.relativePath] = [];
      }
    }

    return dependencies;
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

    // 클래스 상속에서 의존성 추출
    const classMatches = content.match(/class\s+\w+\s*:\s*([^{]+)/g);
    if (classMatches) {
      for (const match of classMatches) {
        const inheritance = match.replace(/class\s+\w+\s*:\s*/, '');
        dependencies.push(inheritance.trim());
      }
    }

    return dependencies;
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
        console.warn(`패턴 분석 실패: ${file.path}`, error.message);
      }
    }

    return patterns;
  }

  /**
   * 인터페이스 분석
   */
  async analyzeInterfaces() {
    const interfaces = {};
    const files = await this.scanFiles();

    for (const file of files) {
      try {
        const content = fs.readFileSync(file.path, 'utf8');
        
        if (content.includes('interface ')) {
          const interfaceMatches = content.match(/interface\s+(\w+)/g);
          if (interfaceMatches) {
            for (const match of interfaceMatches) {
              const interfaceName = match.replace('interface ', '');
              interfaces[interfaceName] = {
                file: file.relativePath,
                methods: this.extractInterfaceMethods(content, interfaceName),
              };
            }
          }
        }
      } catch (error) {
        console.warn(`인터페이스 분석 실패: ${file.path}`, error.message);
      }
    }

    return interfaces;
  }

  /**
   * 인터페이스 메서드 추출
   */
  extractInterfaceMethods(content, interfaceName) {
    const methods = [];
    const lines = content.split('\n');
    let inInterface = false;

    for (const line of lines) {
      if (line.includes(`interface ${interfaceName}`)) {
        inInterface = true;
        continue;
      }
      
      if (inInterface) {
        if (line.includes('}')) {
          break;
        }
        
        if (line.includes('(') && line.includes(')')) {
          methods.push(line.trim());
        }
      }
    }

    return methods;
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
        interfaces: this.extractFileInterfaces(content),
        classes: this.extractClasses(content),
        methods: this.extractMethods(content),
        size: fs.statSync(fullPath).size,
        lastModified: fs.statSync(fullPath).mtime.toISOString(),
      };

      return {
        content: [
          {
            type: 'text',
            text: `# 파일 분석 결과: ${relativePath}\n\n${JSON.stringify(analysis, null, 2)}`,
          },
        ],
      };
    } catch (error) {
      throw new Error(`파일 분석 실패: ${error.message}`);
    }
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
   * 파일 인터페이스 추출
   */
  extractFileInterfaces(content) {
    const interfaces = [];
    const interfaceMatches = content.match(/interface\s+(\w+)/g);
    
    if (interfaceMatches) {
      for (const match of interfaceMatches) {
        interfaces.push(match.replace('interface ', ''));
      }
    }
    
    return interfaces;
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
      const codebaseState = await this.getCodebaseState();
      const suggestions = {
        requirement,
        targetSystem,
        existingClasses: await this.findReusableClasses(requirement, targetSystem),
        existingMethods: await this.findReusableMethods(requirement, targetSystem),
        existingPatterns: await this.findReusablePatterns(requirement, targetSystem),
        existingInterfaces: await this.findReusableInterfaces(requirement, targetSystem),
        reuseStrategy: this.determineReuseStrategy(requirement, targetSystem),
      };

      return {
        content: [
          {
            type: 'text',
            text: `# 코드 재사용 제안\n\n${JSON.stringify(suggestions, null, 2)}`,
          },
        ],
      };
    } catch (error) {
      throw new Error(`코드 재사용 제안 실패: ${error.message}`);
    }
  }

  /**
   * 재사용 가능한 클래스 찾기
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
          if (this.isReusableClass(className, requirement)) {
            reusableClasses.push({
              className,
              file: file.relativePath,
              reason: this.getReusabilityReason(className, requirement),
            });
          }
        }
      } catch (error) {
        console.warn(`클래스 분석 실패: ${file.path}`, error.message);
      }
    }

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
        console.warn(`메서드 분석 실패: ${file.path}`, error.message);
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
   * 재사용 가능한 인터페이스 찾기
   */
  async findReusableInterfaces(requirement, targetSystem) {
    const interfaces = await this.analyzeInterfaces();
    const reusableInterfaces = [];

    for (const [interfaceName, interfaceInfo] of Object.entries(interfaces)) {
      if (targetSystem && !interfaceInfo.file.includes(targetSystem)) {
        continue;
      }

      if (this.isReusableInterface(interfaceName, requirement)) {
        reusableInterfaces.push({
          interfaceName,
          file: interfaceInfo.file,
          methods: interfaceInfo.methods,
          reason: this.getReusabilityReason(interfaceName, requirement),
        });
      }
    }

    return reusableInterfaces;
  }

  /**
   * 재사용 전략 결정
   */
  determineReuseStrategy(requirement, targetSystem) {
    // 간단한 키워드 기반 전략 결정
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
   * 재사용 가능한 클래스인지 확인
   */
  isReusableClass(className, requirement) {
    const requirementLower = requirement.toLowerCase();
    const classNameLower = className.toLowerCase();
    
    return requirementLower.includes(classNameLower) || 
           classNameLower.includes(requirementLower.split(' ')[0]);
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
   * 재사용 가능한 인터페이스인지 확인
   */
  isReusableInterface(interfaceName, requirement) {
    const requirementLower = requirement.toLowerCase();
    const interfaceNameLower = interfaceName.toLowerCase();
    
    return requirementLower.includes(interfaceNameLower) || 
           interfaceNameLower.includes(requirementLower.split(' ')[0]);
  }

  /**
   * 재사용 가능성 이유 반환
   */
  getReusabilityReason(name, requirement) {
    const requirementLower = requirement.toLowerCase();
    const nameLower = name.toLowerCase();
    
    if (requirementLower.includes(nameLower)) {
      return `요구사항에 "${name}"이 포함되어 있습니다.`;
    }
    
    if (nameLower.includes(requirementLower.split(' ')[0])) {
      return `"${name}"이 요구사항의 핵심 키워드를 포함하고 있습니다.`;
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
            text: `# 충돌 예측 결과\n\n${JSON.stringify(conflicts, null, 2)}`,
          },
        ],
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
        console.warn(`충돌 검사 실패: ${file.path}`, error.message);
      }
    }
    
    return conflicts;
  }

  /**
   * 인터페이스 충돌 검사
   */
  async checkInterfaceConflicts(newCode, targetFile) {
    const conflicts = [];
    const newInterfaces = this.extractFileInterfaces(newCode);
    const interfaces = await this.analyzeInterfaces();
    
    for (const newInterface of newInterfaces) {
      if (interfaces[newInterface]) {
        conflicts.push({
          type: 'interface',
          name: newInterface,
          conflictFile: interfaces[newInterface].file,
          reason: `인터페이스 "${newInterface}"이 이미 존재합니다.`,
        });
      }
    }
    
    return conflicts;
  }

  /**
   * 의존성 충돌 검사
   */
  async checkDependencyConflicts(newCode, targetFile) {
    const conflicts = [];
    const newDependencies = this.extractDependencies(newCode);
    const dependencies = await this.buildDependencyGraph();
    
    for (const newDep of newDependencies) {
      // 순환 의존성 검사
      if (this.wouldCreateCircularDependency(newDep, targetFile, dependencies)) {
        conflicts.push({
          type: 'dependency',
          name: newDep,
          reason: `의존성 "${newDep}"이 순환 의존성을 생성할 수 있습니다.`,
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
   * 순환 의존성 생성 여부 확인
   */
  wouldCreateCircularDependency(newDep, targetFile, dependencies) {
    // 간단한 순환 의존성 검사
    const visited = new Set();
    const stack = [newDep];
    
    while (stack.length > 0) {
      const current = stack.pop();
      
      if (visited.has(current)) {
        return true; // 순환 의존성 발견
      }
      
      visited.add(current);
      
      if (dependencies[current]) {
        stack.push(...dependencies[current]);
      }
    }
    
    return false;
  }

  /**
   * 디렉토리의 하위 디렉토리 목록 반환
   */
  async getSubdirectories(dirPath) {
    try {
      const items = fs.readdirSync(dirPath);
      return items.filter(item => {
        const fullPath = path.join(dirPath, item);
        return fs.statSync(fullPath).isDirectory();
      });
    } catch (error) {
      return [];
    }
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
   * 서버 시작
   */
  async start() {
    const transport = new StdioServerTransport();
    await this.server.connect(transport);
    console.error('RougeShool MCP Server가 시작되었습니다.');
  }
}

// 서버 시작
const server = new RougeShoolMCPServer();
server.start().catch(console.error);
