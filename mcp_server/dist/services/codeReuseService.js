import { logger } from '../utils/logger.js';
import { cacheManager } from '../utils/cache.js';
import { AnalysisError } from '../utils/errors.js';
/**
 * 코드 재사용 서비스
 * 기존 코드 활용 방안 제안
 */
export class CodeReuseService {
    /**
     * 코드 재사용 제안
     */
    async suggestCodeReuse(requirement, targetSystem) {
        const cacheKey = `code_reuse_${requirement}_${targetSystem || 'all'}`;
        // 캐시 확인
        if (cacheManager.isEnabled()) {
            const cached = cacheManager.get(cacheKey);
            if (cached) {
                logger.debug('코드 재사용 제안 캐시 히트');
                return cached;
            }
        }
        try {
            logger.info(`코드 재사용 제안 시작: ${requirement}`);
            const suggestions = {
                requirement,
                targetSystem: targetSystem || 'all',
                existingCode: await this.findExistingCode(requirement, targetSystem),
                reuseStrategies: await this.generateReuseStrategies(requirement, targetSystem),
                improvementSuggestions: await this.generateImprovementSuggestions(requirement, targetSystem),
                warnings: await this.generateWarnings(requirement, targetSystem),
                timestamp: new Date().toISOString()
            };
            // 캐시 저장
            if (cacheManager.isEnabled()) {
                cacheManager.set(cacheKey, suggestions, 600000); // 10분 캐시
            }
            logger.info('코드 재사용 제안 완료');
            return suggestions;
        }
        catch (error) {
            logger.error('코드 재사용 제안 실패', error);
            throw new AnalysisError('suggestCodeReuse', error instanceof Error ? error.message : 'Unknown error');
        }
    }
    /**
     * 기존 코드 찾기
     */
    async findExistingCode(requirement, targetSystem) {
        // 실제 구현에서는 파일 시스템을 스캔하여 기존 코드를 찾음
        return {
            similarClasses: this.findSimilarClasses(requirement, targetSystem),
            similarMethods: this.findSimilarMethods(requirement, targetSystem),
            similarPatterns: this.findSimilarPatterns(requirement, targetSystem),
            reusableComponents: this.findReusableComponents(requirement, targetSystem)
        };
    }
    /**
     * 유사한 클래스 찾기
     */
    findSimilarClasses(requirement, targetSystem) {
        const keywords = this.extractKeywords(requirement);
        const classes = [];
        // 실제 구현에서는 파일 시스템을 스캔
        if (keywords.includes('manager') || keywords.includes('매니저')) {
            classes.push('AudioManager', 'SaveManager', 'EnemyManager');
        }
        if (keywords.includes('controller') || keywords.includes('컨트롤러')) {
            classes.push('PlayerCharacterUIController', 'CombatExecutionManager');
        }
        if (keywords.includes('factory') || keywords.includes('팩토리')) {
            classes.push('SkillCardFactory');
        }
        return classes;
    }
    /**
     * 유사한 메서드 찾기
     */
    findSimilarMethods(requirement, targetSystem) {
        const keywords = this.extractKeywords(requirement);
        const methods = [];
        if (keywords.includes('initialize') || keywords.includes('초기화')) {
            methods.push('Initialize', 'InitializeCharacter', 'InitializeAsync');
        }
        if (keywords.includes('execute') || keywords.includes('실행')) {
            methods.push('ExecuteCardInBattleSlot', 'ExecuteImmediately');
        }
        if (keywords.includes('update') || keywords.includes('업데이트')) {
            methods.push('UpdateHP', 'UpdateResource', 'UpdateUI');
        }
        return methods;
    }
    /**
     * 유사한 패턴 찾기
     */
    findSimilarPatterns(requirement, targetSystem) {
        const patterns = [];
        if (requirement.includes('싱글톤')) {
            patterns.push('Singleton Pattern (AudioManager, SaveManager)');
        }
        if (requirement.includes('팩토리')) {
            patterns.push('Factory Pattern (SkillCardFactory)');
        }
        if (requirement.includes('상태')) {
            patterns.push('State Pattern (CombatState)');
        }
        if (requirement.includes('관찰자')) {
            patterns.push('Observer Pattern (Events, Actions)');
        }
        return patterns;
    }
    /**
     * 재사용 가능한 컴포넌트 찾기
     */
    findReusableComponents(requirement, targetSystem) {
        const components = [];
        if (requirement.includes('UI') || requirement.includes('인터페이스')) {
            components.push('PlayerCharacterUIController', 'CanvasGroup 애니메이션');
        }
        if (requirement.includes('오디오') || requirement.includes('사운드')) {
            components.push('AudioManager', 'AudioPoolManager');
        }
        if (requirement.includes('저장') || requirement.includes('세이브')) {
            components.push('SaveManager', 'AutoSaveManager');
        }
        return components;
    }
    /**
     * 재사용 전략 생성
     */
    async generateReuseStrategies(requirement, targetSystem) {
        return {
            extendExisting: this.suggestExtension(requirement, targetSystem),
            modifyExisting: this.suggestModification(requirement, targetSystem),
            composeExisting: this.suggestComposition(requirement, targetSystem),
            refactorExisting: this.suggestRefactoring(requirement, targetSystem)
        };
    }
    /**
     * 확장 제안
     */
    suggestExtension(requirement, targetSystem) {
        const suggestions = [];
        if (requirement.includes('새로운 기능')) {
            suggestions.push('기존 Manager 클래스에 새로운 메서드 추가');
            suggestions.push('기존 Interface에 새로운 메서드 정의');
        }
        return suggestions;
    }
    /**
     * 수정 제안
     */
    suggestModification(requirement, targetSystem) {
        const suggestions = [];
        if (requirement.includes('개선') || requirement.includes('최적화')) {
            suggestions.push('기존 메서드의 성능 최적화');
            suggestions.push('기존 로직의 버그 수정');
        }
        return suggestions;
    }
    /**
     * 조합 제안
     */
    suggestComposition(requirement, targetSystem) {
        const suggestions = [];
        if (requirement.includes('통합') || requirement.includes('연결')) {
            suggestions.push('기존 컴포넌트들을 조합하여 새로운 기능 구현');
            suggestions.push('기존 서비스들을 연결하여 복합 기능 제공');
        }
        return suggestions;
    }
    /**
     * 리팩토링 제안
     */
    suggestRefactoring(requirement, targetSystem) {
        const suggestions = [];
        if (requirement.includes('구조') || requirement.includes('아키텍처')) {
            suggestions.push('기존 코드의 구조 개선');
            suggestions.push('기존 패턴의 일관성 향상');
        }
        return suggestions;
    }
    /**
     * 개선 제안 생성
     */
    async generateImprovementSuggestions(requirement, targetSystem) {
        return {
            codeQuality: [
                '기존 코드에 예외 처리 추가',
                '기존 메서드에 XML 문서화 추가',
                '기존 클래스에 한국어 로깅 추가'
            ],
            performance: [
                '기존 메서드의 성능 최적화',
                '기존 알고리즘의 복잡도 개선',
                '기존 메모리 사용량 최적화'
            ],
            maintainability: [
                '기존 코드의 가독성 향상',
                '기존 구조의 모듈화 개선',
                '기존 테스트 코드 추가'
            ]
        };
    }
    /**
     * 경고 생성
     */
    async generateWarnings(requirement, targetSystem) {
        const warnings = [];
        if (requirement.includes('새로운') && !requirement.includes('기존')) {
            warnings.push('기존 코드와 중복될 가능성이 있습니다. 기존 코드를 먼저 확인해주세요.');
        }
        if (requirement.includes('AnimationSystem')) {
            warnings.push('AnimationSystem은 제거되었습니다. DOTween Pro를 사용해주세요.');
        }
        return warnings;
    }
    /**
     * 키워드 추출
     */
    extractKeywords(requirement) {
        const keywords = requirement.toLowerCase()
            .split(/[\s,.\-!?]+/)
            .filter(word => word.length > 2);
        return keywords;
    }
}
//# sourceMappingURL=codeReuseService.js.map