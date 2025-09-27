/**
 * 충돌 예측 서비스
 * 새로운 코드가 기존 코드와 충돌할 가능성 예측
 */
export declare class ConflictPredictor {
    /**
     * 충돌 예측
     */
    predictConflicts(newCode: string, targetFile: string): Promise<any>;
    /**
     * 네이밍 충돌 예측
     */
    private predictNamingConflicts;
    /**
     * 클래스명 충돌 찾기
     */
    private findClassNameConflicts;
    /**
     * 메서드명 충돌 찾기
     */
    private findMethodNameConflicts;
    /**
     * 변수명 충돌 찾기
     */
    private findVariableNameConflicts;
    /**
     * 네임스페이스 충돌 찾기
     */
    private findNamespaceConflicts;
    /**
     * 인터페이스 충돌 예측
     */
    private predictInterfaceConflicts;
    /**
     * 인터페이스 충돌 찾기
     */
    private findInterfaceConflicts;
    /**
     * 메서드 시그니처 충돌 찾기
     */
    private findMethodSignatureConflicts;
    /**
     * 프로퍼티 충돌 찾기
     */
    private findPropertyConflicts;
    /**
     * 의존성 충돌 예측
     */
    private predictDependencyConflicts;
    /**
     * Using 문 충돌 찾기
     */
    private findUsingConflicts;
    /**
     * 어셈블리 충돌 찾기
     */
    private findAssemblyConflicts;
    /**
     * 버전 충돌 찾기
     */
    private findVersionConflicts;
    /**
     * 로직 충돌 예측
     */
    private predictLogicConflicts;
    /**
     * 상태 충돌 찾기
     */
    private findStateConflicts;
    /**
     * 플로우 충돌 찾기
     */
    private findFlowConflicts;
    /**
     * 데이터 충돌 찾기
     */
    private findDataConflicts;
    /**
     * 위험도 계산
     */
    private calculateRiskLevel;
    /**
     * 권장사항 생성
     */
    private generateRecommendations;
}
//# sourceMappingURL=conflictPredictor.d.ts.map