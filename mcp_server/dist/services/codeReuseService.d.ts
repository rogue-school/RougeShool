/**
 * 코드 재사용 서비스
 * 기존 코드 활용 방안 제안
 */
export declare class CodeReuseService {
    /**
     * 코드 재사용 제안
     */
    suggestCodeReuse(requirement: string, targetSystem?: string): Promise<any>;
    /**
     * 기존 코드 찾기
     */
    private findExistingCode;
    /**
     * 유사한 클래스 찾기
     */
    private findSimilarClasses;
    /**
     * 유사한 메서드 찾기
     */
    private findSimilarMethods;
    /**
     * 유사한 패턴 찾기
     */
    private findSimilarPatterns;
    /**
     * 재사용 가능한 컴포넌트 찾기
     */
    private findReusableComponents;
    /**
     * 재사용 전략 생성
     */
    private generateReuseStrategies;
    /**
     * 확장 제안
     */
    private suggestExtension;
    /**
     * 수정 제안
     */
    private suggestModification;
    /**
     * 조합 제안
     */
    private suggestComposition;
    /**
     * 리팩토링 제안
     */
    private suggestRefactoring;
    /**
     * 개선 제안 생성
     */
    private generateImprovementSuggestions;
    /**
     * 경고 생성
     */
    private generateWarnings;
    /**
     * 키워드 추출
     */
    private extractKeywords;
}
//# sourceMappingURL=codeReuseService.d.ts.map