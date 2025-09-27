import fs from 'fs';
import path from 'path';
import { logger } from '../utils/logger.js';
import { config } from '../config/config.js';
import { cacheManager } from '../utils/cache.js';
import { FileNotFoundError, AnalysisError } from '../utils/errors.js';
/**
 * 코드베이스 분석 서비스
 * 전체 프로젝트 구조와 의존성을 분석
 */
export class CodebaseAnalyzer {
    projectConfig = config.get('project');
    /**
     * 코드베이스 전체 상태 분석
     */
    async getCodebaseState() {
        const cacheKey = 'codebase_state';
        // 캐시 확인
        if (cacheManager.isEnabled()) {
            const cached = cacheManager.get(cacheKey);
            if (cached) {
                logger.debug('코드베이스 상태 캐시 히트');
                return cached;
            }
        }
        try {
            logger.info('코드베이스 상태 분석 시작');
            const analysis = {
                projectInfo: {
                    name: 'RougeShool',
                    type: 'Unity 2D Game',
                    rootPath: this.projectConfig.rootPath,
                    scriptPath: this.projectConfig.scriptPath
                },
                fileStructure: await this.analyzeFileStructure(),
                systemArchitecture: await this.analyzeSystemArchitecture(),
                dependencies: await this.analyzeDependencies(),
                patterns: await this.analyzePatterns(),
                statistics: await this.generateStatistics(),
                timestamp: new Date().toISOString()
            };
            // 캐시 저장
            if (cacheManager.isEnabled()) {
                cacheManager.set(cacheKey, analysis, 300000); // 5분 캐시
            }
            logger.info('코드베이스 상태 분석 완료');
            return analysis;
        }
        catch (error) {
            logger.error('코드베이스 상태 분석 실패', error);
            throw new AnalysisError('getCodebaseState', error instanceof Error ? error.message : 'Unknown error');
        }
    }
    /**
     * 파일 구조 분석
     */
    async analyzeFileStructure() {
        const scriptPath = this.projectConfig.scriptPath;
        if (!fs.existsSync(scriptPath)) {
            throw new FileNotFoundError(scriptPath);
        }
        const structure = {
            totalFiles: 0,
            fileTypes: {},
            directories: [],
            coreFiles: []
        };
        const scanDirectory = (dir, depth = 0) => {
            if (depth > 3)
                return; // 깊이 제한
            const items = fs.readdirSync(dir);
            for (const item of items) {
                const fullPath = path.join(dir, item);
                const stat = fs.statSync(fullPath);
                if (stat.isDirectory()) {
                    structure.directories.push(fullPath);
                    scanDirectory(fullPath, depth + 1);
                }
                else if (stat.isFile()) {
                    structure.totalFiles++;
                    const ext = path.extname(item);
                    structure.fileTypes[ext] = (structure.fileTypes[ext] || 0) + 1;
                    // 핵심 파일 식별
                    if (this.isCoreFile(item)) {
                        structure.coreFiles.push(fullPath);
                    }
                }
            }
        };
        scanDirectory(scriptPath);
        return structure;
    }
    /**
     * 시스템 아키텍처 분석
     */
    async analyzeSystemArchitecture() {
        const systems = [
            'CoreSystem', 'CombatSystem', 'CharacterSystem',
            'SkillCardSystem', 'SaveSystem', 'StageSystem',
            'UISystem', 'UtilitySystem'
        ];
        const architecture = {
            systems: {},
            relationships: [],
            patterns: []
        };
        for (const system of systems) {
            architecture.systems[system] = await this.analyzeSystem(system);
        }
        return architecture;
    }
    /**
     * 개별 시스템 분석
     */
    async analyzeSystem(systemName) {
        const systemPath = path.join(this.projectConfig.scriptPath, systemName);
        if (!fs.existsSync(systemPath)) {
            return { exists: false };
        }
        const files = fs.readdirSync(systemPath);
        const csFiles = files.filter(f => f.endsWith('.cs'));
        return {
            exists: true,
            fileCount: csFiles.length,
            files: csFiles,
            hasManager: csFiles.some(f => f.includes('Manager')),
            hasInstaller: csFiles.some(f => f.includes('Installer')),
            hasFactory: csFiles.some(f => f.includes('Factory'))
        };
    }
    /**
     * 의존성 분석
     */
    async analyzeDependencies() {
        return {
            external: [
                'Unity Engine',
                'Zenject DI',
                'DOTween Pro',
                'TextMeshPro'
            ],
            internal: await this.findInternalDependencies()
        };
    }
    /**
     * 내부 의존성 찾기
     */
    async findInternalDependencies() {
        const dependencies = new Set();
        const scriptPath = this.projectConfig.scriptPath;
        const scanForDependencies = (dir) => {
            const files = fs.readdirSync(dir);
            for (const file of files) {
                const fullPath = path.join(dir, file);
                const stat = fs.statSync(fullPath);
                if (stat.isDirectory()) {
                    scanForDependencies(fullPath);
                }
                else if (file.endsWith('.cs')) {
                    const content = fs.readFileSync(fullPath, 'utf8');
                    const matches = content.match(/using\s+(\w+);/g);
                    if (matches) {
                        matches.forEach(match => {
                            const system = match.replace('using ', '').replace(';', '');
                            if (system.includes('System') && !system.includes('System.')) {
                                dependencies.add(system);
                            }
                        });
                    }
                }
            }
        };
        scanForDependencies(scriptPath);
        return Array.from(dependencies);
    }
    /**
     * 패턴 분석
     */
    async analyzePatterns() {
        return {
            architectural: [
                'Singleton Pattern',
                'Factory Pattern',
                'State Pattern',
                'Observer Pattern'
            ],
            unity: [
                'MonoBehaviour',
                'ScriptableObject',
                'Zenject DI',
                'DOTween Pro'
            ],
            project: [
                'Manager Pattern',
                'Installer Pattern',
                'Core System Pattern'
            ]
        };
    }
    /**
     * 통계 생성
     */
    async generateStatistics() {
        const scriptPath = this.projectConfig.scriptPath;
        let totalLines = 0;
        let totalFiles = 0;
        const countLines = (dir) => {
            const files = fs.readdirSync(dir);
            for (const file of files) {
                const fullPath = path.join(dir, file);
                const stat = fs.statSync(fullPath);
                if (stat.isDirectory()) {
                    countLines(fullPath);
                }
                else if (file.endsWith('.cs')) {
                    const content = fs.readFileSync(fullPath, 'utf8');
                    const lines = content.split('\n').length;
                    totalLines += lines;
                    totalFiles++;
                }
            }
        };
        countLines(scriptPath);
        return {
            totalFiles,
            totalLines,
            averageLinesPerFile: totalFiles > 0 ? Math.round(totalLines / totalFiles) : 0,
            lastAnalyzed: new Date().toISOString()
        };
    }
    /**
     * 핵심 파일인지 확인
     */
    isCoreFile(filename) {
        const corePatterns = [
            'Manager', 'Installer', 'Factory', 'Controller',
            'Core', 'System', 'Initializer'
        ];
        return corePatterns.some(pattern => filename.includes(pattern));
    }
}
//# sourceMappingURL=codebaseAnalyzer.js.map