/**
 * 파일 분석 서비스
 * 개별 파일의 의존성, 패턴, 인터페이스 분석
 */
export declare class FileAnalyzer {
    private projectConfig;
    /**
     * 파일 분석
     */
    analyzeFile(filePath: string): Promise<any>;
    /**
     * 파일 구조 분석
     */
    private analyzeStructure;
    /**
     * 의존성 분석
     */
    private analyzeDependencies;
    /**
     * 패턴 분석
     */
    private analyzePatterns;
    /**
     * 인터페이스 분석
     */
    private analyzeInterfaces;
    /**
     * 클래스 분석
     */
    private analyzeClasses;
    /**
     * 메서드 분석
     */
    private analyzeMethods;
    /**
     * 복잡도 분석
     */
    private analyzeComplexity;
    /**
     * 품질 분석
     */
    private analyzeQuality;
    /**
     * 들여쓰기 스타일 감지
     */
    private detectIndentationStyle;
    /**
     * 순환 복잡도 계산
     */
    private calculateCyclomaticComplexity;
    /**
     * 중첩 깊이 계산
     */
    private calculateNestingDepth;
    /**
     * 평균 메서드 길이 계산
     */
    private calculateAverageMethodLength;
    /**
     * 평균 매개변수 개수 계산
     */
    private calculateAverageParameterCount;
    /**
     * 주석 비율 계산
     */
    private calculateCommentRatio;
    /**
     * 한국어 내용 확인
     */
    private hasKoreanContent;
    /**
     * 코드 스타일 분석
     */
    private analyzeCodeStyle;
    /**
     * 중괄호 스타일 감지
     */
    private detectBraceStyle;
}
//# sourceMappingURL=fileAnalyzer.d.ts.map