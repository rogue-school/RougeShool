import fs from 'fs';
import path from 'path';
import { logger } from '../utils/logger.js';
import { config } from '../config/config.js';
import { cacheManager } from '../utils/cache.js';
import { FileNotFoundError, InvalidFileTypeError, FileSizeExceededError, AnalysisError } from '../utils/errors.js';
/**
 * 파일 분석 서비스
 * 개별 파일의 의존성, 패턴, 인터페이스 분석
 */
export class FileAnalyzer {
    projectConfig = config.get('project');
    /**
     * 파일 분석
     */
    async analyzeFile(filePath) {
        const cacheKey = `file_analysis_${filePath}`;
        // 캐시 확인
        if (cacheManager.isEnabled()) {
            const cached = cacheManager.get(cacheKey);
            if (cached) {
                logger.debug(`파일 분석 캐시 히트: ${filePath}`);
                return cached;
            }
        }
        try {
            logger.info(`파일 분석 시작: ${filePath}`);
            // 파일 존재 확인
            if (!fs.existsSync(filePath)) {
                throw new FileNotFoundError(filePath);
            }
            // 파일 타입 확인
            const ext = path.extname(filePath);
            if (!this.projectConfig.supportedExtensions.includes(ext)) {
                throw new InvalidFileTypeError(filePath, this.projectConfig.supportedExtensions);
            }
            // 파일 크기 확인
            const stats = fs.statSync(filePath);
            if (stats.size > this.projectConfig.maxFileSize) {
                throw new FileSizeExceededError(filePath, stats.size, this.projectConfig.maxFileSize);
            }
            const content = fs.readFileSync(filePath, 'utf8');
            const analysis = {
                fileInfo: {
                    path: filePath,
                    size: stats.size,
                    lastModified: stats.mtime.toISOString(),
                    extension: ext
                },
                content: {
                    lines: content.split('\n').length,
                    characters: content.length,
                    isEmpty: content.trim().length === 0
                },
                structure: this.analyzeStructure(content),
                dependencies: this.analyzeDependencies(content),
                patterns: this.analyzePatterns(content, filePath),
                interfaces: this.analyzeInterfaces(content),
                classes: this.analyzeClasses(content),
                methods: this.analyzeMethods(content),
                complexity: this.analyzeComplexity(content),
                quality: this.analyzeQuality(content),
                timestamp: new Date().toISOString()
            };
            // 캐시 저장
            if (cacheManager.isEnabled()) {
                cacheManager.set(cacheKey, analysis, 600000); // 10분 캐시
            }
            logger.info(`파일 분석 완료: ${filePath}`);
            return analysis;
        }
        catch (error) {
            logger.error(`파일 분석 실패: ${filePath}`, error);
            throw new AnalysisError('analyzeFile', error instanceof Error ? error.message : 'Unknown error');
        }
    }
    /**
     * 파일 구조 분석
     */
    analyzeStructure(content) {
        const lines = content.split('\n');
        return {
            hasNamespace: content.includes('namespace'),
            hasUsing: content.includes('using '),
            hasClass: content.includes('class '),
            hasInterface: content.includes('interface '),
            hasEnum: content.includes('enum '),
            hasStruct: content.includes('struct '),
            hasRegion: content.includes('#region'),
            hasComment: content.includes('//') || content.includes('/*'),
            hasXMLDoc: content.includes('///'),
            indentationStyle: this.detectIndentationStyle(lines)
        };
    }
    /**
     * 의존성 분석
     */
    analyzeDependencies(content) {
        const usingMatches = content.match(/using\s+([^;]+);/g);
        const dependencies = usingMatches ? usingMatches.map(match => match.replace('using ', '').replace(';', '').trim()) : [];
        return {
            external: dependencies.filter(dep => !dep.includes('System') || dep.includes('System.')),
            internal: dependencies.filter(dep => dep.includes('System') && !dep.includes('System.')),
            unity: dependencies.filter(dep => dep.includes('Unity') || dep.includes('Zenject') || dep.includes('DG.Tweening')),
            total: dependencies.length
        };
    }
    /**
     * 패턴 분석
     */
    analyzePatterns(content, filePath) {
        const patterns = {
            singleton: content.includes('Instance') && content.includes('static'),
            factory: content.includes('Factory') || content.includes('Create'),
            manager: filePath.includes('Manager'),
            installer: filePath.includes('Installer'),
            controller: filePath.includes('Controller'),
            state: content.includes('IState') || content.includes('State'),
            observer: content.includes('event ') || content.includes('Action'),
            monobehaviour: content.includes('MonoBehaviour'),
            scriptableObject: content.includes('ScriptableObject'),
            zenject: content.includes('Zenject') || content.includes('[Inject]'),
            dotween: content.includes('DOTween') || content.includes('DOTweenSequence')
        };
        return patterns;
    }
    /**
     * 인터페이스 분석
     */
    analyzeInterfaces(content) {
        const interfaceMatches = content.match(/interface\s+(\w+)/g);
        const interfaces = interfaceMatches ? interfaceMatches.map(match => match.replace('interface ', '').trim()) : [];
        return {
            count: interfaces.length,
            names: interfaces,
            hasPublic: content.includes('public interface'),
            hasInternal: content.includes('internal interface')
        };
    }
    /**
     * 클래스 분석
     */
    analyzeClasses(content) {
        const classMatches = content.match(/class\s+(\w+)/g);
        const classes = classMatches ? classMatches.map(match => match.replace('class ', '').trim()) : [];
        return {
            count: classes.length,
            names: classes,
            hasPublic: content.includes('public class'),
            hasInternal: content.includes('internal class'),
            hasAbstract: content.includes('abstract class'),
            hasStatic: content.includes('static class')
        };
    }
    /**
     * 메서드 분석
     */
    analyzeMethods(content) {
        const methodMatches = content.match(/(public|private|protected|internal)\s+\w+\s+\w+\s*\(/g);
        const methods = methodMatches ? methodMatches.length : 0;
        return {
            count: methods,
            hasPublic: content.includes('public '),
            hasPrivate: content.includes('private '),
            hasProtected: content.includes('protected '),
            hasAsync: content.includes('async '),
            hasStatic: content.includes('static '),
            hasVirtual: content.includes('virtual '),
            hasOverride: content.includes('override ')
        };
    }
    /**
     * 복잡도 분석
     */
    analyzeComplexity(content) {
        const lines = content.split('\n');
        return {
            cyclomaticComplexity: this.calculateCyclomaticComplexity(content),
            nestingDepth: this.calculateNestingDepth(content),
            methodLength: this.calculateAverageMethodLength(content),
            parameterCount: this.calculateAverageParameterCount(content),
            commentRatio: this.calculateCommentRatio(content)
        };
    }
    /**
     * 품질 분석
     */
    analyzeQuality(content) {
        return {
            hasXMLDocumentation: content.includes('///'),
            hasErrorHandling: content.includes('try') && content.includes('catch'),
            hasLogging: content.includes('GameLogger') || content.includes('Debug.Log'),
            hasValidation: content.includes('null') && content.includes('throw'),
            hasKoreanComments: this.hasKoreanContent(content),
            codeStyle: this.analyzeCodeStyle(content)
        };
    }
    /**
     * 들여쓰기 스타일 감지
     */
    detectIndentationStyle(lines) {
        let spaces = 0;
        let tabs = 0;
        for (const line of lines) {
            if (line.startsWith(' '))
                spaces++;
            if (line.startsWith('\t'))
                tabs++;
        }
        return spaces > tabs ? 'spaces' : 'tabs';
    }
    /**
     * 순환 복잡도 계산
     */
    calculateCyclomaticComplexity(content) {
        const complexityKeywords = [
            'if', 'else', 'while', 'for', 'foreach', 'switch', 'case',
            'catch', '&&', '||', '?', ':', 'return'
        ];
        let complexity = 1; // 기본 복잡도
        for (const keyword of complexityKeywords) {
            const matches = content.match(new RegExp(`\\b${keyword}\\b`, 'g'));
            if (matches) {
                complexity += matches.length;
            }
        }
        return complexity;
    }
    /**
     * 중첩 깊이 계산
     */
    calculateNestingDepth(content) {
        let maxDepth = 0;
        let currentDepth = 0;
        for (const char of content) {
            if (char === '{') {
                currentDepth++;
                maxDepth = Math.max(maxDepth, currentDepth);
            }
            else if (char === '}') {
                currentDepth--;
            }
        }
        return maxDepth;
    }
    /**
     * 평균 메서드 길이 계산
     */
    calculateAverageMethodLength(content) {
        // 간단한 구현 - 실제로는 더 정교한 파싱 필요
        const methodMatches = content.match(/\{[^}]*\}/g);
        if (!methodMatches)
            return 0;
        const totalLines = methodMatches.reduce((sum, match) => sum + match.split('\n').length, 0);
        return Math.round(totalLines / methodMatches.length);
    }
    /**
     * 평균 매개변수 개수 계산
     */
    calculateAverageParameterCount(content) {
        const methodMatches = content.match(/\([^)]*\)/g);
        if (!methodMatches)
            return 0;
        const totalParams = methodMatches.reduce((sum, match) => {
            const params = match.replace(/[()]/g, '').split(',');
            return sum + (params[0]?.trim() ? params.length : 0);
        }, 0);
        return Math.round(totalParams / methodMatches.length);
    }
    /**
     * 주석 비율 계산
     */
    calculateCommentRatio(content) {
        const lines = content.split('\n');
        const commentLines = lines.filter(line => line.trim().startsWith('//') ||
            line.trim().startsWith('/*') ||
            line.trim().startsWith('*')).length;
        return lines.length > 0 ? Math.round((commentLines / lines.length) * 100) : 0;
    }
    /**
     * 한국어 내용 확인
     */
    hasKoreanContent(content) {
        return /[가-힣]/.test(content);
    }
    /**
     * 코드 스타일 분석
     */
    analyzeCodeStyle(content) {
        return {
            usesCamelCase: /[a-z][A-Z]/.test(content),
            usesPascalCase: /[A-Z][a-z]/.test(content),
            hasHungarianNotation: /\b[a-z][A-Z]/.test(content),
            usesUnderscores: content.includes('_'),
            braceStyle: this.detectBraceStyle(content)
        };
    }
    /**
     * 중괄호 스타일 감지
     */
    detectBraceStyle(content) {
        if (content.includes('{\n'))
            return 'allman';
        if (content.includes('{ '))
            return 'k&r';
        return 'unknown';
    }
}
//# sourceMappingURL=fileAnalyzer.js.map