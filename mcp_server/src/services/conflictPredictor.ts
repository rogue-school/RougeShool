import { logger } from '../utils/logger.js';
import { cacheManager } from '../utils/cache.js';
import { AnalysisError } from '../utils/errors.js';

/**
 * 충돌 예측 서비스
 * 새로운 코드가 기존 코드와 충돌할 가능성 예측
 */
export class ConflictPredictor {
  /**
   * 충돌 예측
   */
  public async predictConflicts(newCode: string, targetFile: string): Promise<any> {
    const cacheKey = `conflict_prediction_${targetFile}_${newCode.length}`;
    
    // 캐시 확인
    if (cacheManager.isEnabled()) {
      const cached = cacheManager.get(cacheKey);
      if (cached) {
        logger.debug('충돌 예측 캐시 히트');
        return cached;
      }
    }

    try {
      logger.info(`충돌 예측 시작: ${targetFile}`);
      
      const predictions = {
        targetFile,
        newCodeLength: newCode.length,
        namingConflicts: await this.predictNamingConflicts(newCode, targetFile),
        interfaceConflicts: await this.predictInterfaceConflicts(newCode, targetFile),
        dependencyConflicts: await this.predictDependencyConflicts(newCode, targetFile),
        logicConflicts: await this.predictLogicConflicts(newCode, targetFile),
        riskLevel: this.calculateRiskLevel(newCode, targetFile),
        recommendations: await this.generateRecommendations(newCode, targetFile),
        timestamp: new Date().toISOString()
      };

      // 캐시 저장
      if (cacheManager.isEnabled()) {
        cacheManager.set(cacheKey, predictions, 300000); // 5분 캐시
      }

      logger.info('충돌 예측 완료');
      return predictions;
      
    } catch (error) {
      logger.error('충돌 예측 실패', error as Error);
      throw new AnalysisError('predictConflicts', error instanceof Error ? error.message : 'Unknown error');
    }
  }

  /**
   * 네이밍 충돌 예측
   */
  private async predictNamingConflicts(newCode: string, targetFile: string): Promise<any> {
    const conflicts = {
      classNames: this.findClassNameConflicts(newCode),
      methodNames: this.findMethodNameConflicts(newCode),
      variableNames: this.findVariableNameConflicts(newCode),
      namespaceConflicts: this.findNamespaceConflicts(newCode),
      severity: 'medium' as 'low' | 'medium' | 'high'
    };

    // 심각도 계산
    const totalConflicts = conflicts.classNames.length + 
                          conflicts.methodNames.length + 
                          conflicts.variableNames.length + 
                          conflicts.namespaceConflicts.length;
    
    if (totalConflicts === 0) {
      conflicts.severity = 'low';
    } else if (totalConflicts > 5) {
      conflicts.severity = 'high';
    }

    return conflicts;
  }

  /**
   * 클래스명 충돌 찾기
   */
  private findClassNameConflicts(newCode: string): string[] {
    const conflicts: string[] = [];
    const classMatches = newCode.match(/class\s+(\w+)/g);
    
    if (classMatches) {
      const classNames = classMatches.map(match => 
        match.replace('class ', '').trim()
      );

      // 실제 구현에서는 기존 코드베이스에서 클래스명을 검색
      const existingClasses = [
        'AudioManager', 'SaveManager', 'EnemyManager', 'TurnManager',
        'PlayerCharacter', 'EnemyCharacter', 'SkillCard', 'CombatState'
      ];

      for (const className of classNames) {
        if (existingClasses.includes(className)) {
          conflicts.push(className);
        }
      }
    }

    return conflicts;
  }

  /**
   * 메서드명 충돌 찾기
   */
  private findMethodNameConflicts(newCode: string): string[] {
    const conflicts: string[] = [];
    const methodMatches = newCode.match(/(public|private|protected|internal)\s+\w+\s+(\w+)\s*\(/g);
    
    if (methodMatches) {
      const methodNames = methodMatches.map(match => {
        const parts = match.split(/\s+/);
        return parts[parts.length - 1]?.replace('(', '').trim() || '';
      });

      // 실제 구현에서는 기존 코드베이스에서 메서드명을 검색
      const existingMethods = [
        'Initialize', 'Start', 'Update', 'Awake', 'OnEnable', 'OnDisable',
        'ExecuteCardInBattleSlot', 'MoveSlotsForward', 'TakeDamage', 'Heal'
      ];

      for (const methodName of methodNames) {
        if (existingMethods.includes(methodName)) {
          conflicts.push(methodName);
        }
      }
    }

    return conflicts;
  }

  /**
   * 변수명 충돌 찾기
   */
  private findVariableNameConflicts(newCode: string): string[] {
    const conflicts: string[] = [];
    const variableMatches = newCode.match(/(\w+)\s*=\s*[^;]+;/g);
    
    if (variableMatches) {
      const variableNames = variableMatches.map(match => 
        match.split('=')[0]?.trim() || ''
      );

      // 실제 구현에서는 기존 코드베이스에서 변수명을 검색
      const existingVariables = [
        'Instance', 'currentHP', 'maxHP', 'currentResource', 'maxResource',
        'isInitialized', 'isTransitioning', 'currentPhase'
      ];

      for (const variableName of variableNames) {
        if (existingVariables.includes(variableName)) {
          conflicts.push(variableName);
        }
      }
    }

    return conflicts;
  }

  /**
   * 네임스페이스 충돌 찾기
   */
  private findNamespaceConflicts(newCode: string): string[] {
    const conflicts: string[] = [];
    const namespaceMatches = newCode.match(/namespace\s+([^{]+)/g);
    
    if (namespaceMatches) {
      const namespaces = namespaceMatches.map(match => 
        match.replace('namespace ', '').trim()
      );

      // 실제 구현에서는 기존 코드베이스에서 네임스페이스를 검색
      const existingNamespaces = [
        'RougeShool.Core', 'RougeShool.Combat', 'RougeShool.Character',
        'RougeShool.SkillCard', 'RougeShool.Save', 'RougeShool.Stage'
      ];

      for (const namespace of namespaces) {
        if (existingNamespaces.includes(namespace)) {
          conflicts.push(namespace);
        }
      }
    }

    return conflicts;
  }

  /**
   * 인터페이스 충돌 예측
   */
  private async predictInterfaceConflicts(newCode: string, targetFile: string): Promise<any> {
    return {
      interfaceConflicts: this.findInterfaceConflicts(newCode),
      methodSignatureConflicts: this.findMethodSignatureConflicts(newCode),
      propertyConflicts: this.findPropertyConflicts(newCode),
      severity: 'medium' as 'low' | 'medium' | 'high'
    };
  }

  /**
   * 인터페이스 충돌 찾기
   */
  private findInterfaceConflicts(newCode: string): string[] {
    const conflicts: string[] = [];
    const interfaceMatches = newCode.match(/interface\s+(\w+)/g);
    
    if (interfaceMatches) {
      const interfaceNames = interfaceMatches.map(match => 
        match.replace('interface ', '').trim()
      );

      const existingInterfaces = [
        'ICoreSystemInitializable', 'ICombatTurnState', 'IEnemyCharacter',
        'ISkillCard', 'IAudioManager', 'ISaveManager'
      ];

      for (const interfaceName of interfaceNames) {
        if (existingInterfaces.includes(interfaceName)) {
          conflicts.push(interfaceName);
        }
      }
    }

    return conflicts;
  }

  /**
   * 메서드 시그니처 충돌 찾기
   */
  private findMethodSignatureConflicts(newCode: string): string[] {
    // 실제 구현에서는 더 정교한 시그니처 분석 필요
    return [];
  }

  /**
   * 프로퍼티 충돌 찾기
   */
  private findPropertyConflicts(newCode: string): string[] {
    const conflicts: string[] = [];
    const propertyMatches = newCode.match(/(public|private|protected|internal)\s+\w+\s+(\w+)\s*{\s*get;\s*set;\s*}/g);
    
    if (propertyMatches) {
      const propertyNames = propertyMatches.map(match => {
        const parts = match.split(/\s+/);
        return parts[parts.length - 2]?.trim() || '';
      });

      const existingProperties = [
        'IsInitialized', 'IsTransitioning', 'CurrentHP', 'MaxHP',
        'CurrentResource', 'MaxResource', 'CharacterName'
      ];

      for (const propertyName of propertyNames) {
        if (existingProperties.includes(propertyName)) {
          conflicts.push(propertyName);
        }
      }
    }

    return conflicts;
  }

  /**
   * 의존성 충돌 예측
   */
  private async predictDependencyConflicts(newCode: string, targetFile: string): Promise<any> {
    return {
      usingConflicts: this.findUsingConflicts(newCode),
      assemblyConflicts: this.findAssemblyConflicts(newCode),
      versionConflicts: this.findVersionConflicts(newCode),
      severity: 'low' as 'low' | 'medium' | 'high'
    };
  }

  /**
   * Using 문 충돌 찾기
   */
  private findUsingConflicts(newCode: string): string[] {
    const conflicts: string[] = [];
    const usingMatches = newCode.match(/using\s+([^;]+);/g);
    
    if (usingMatches) {
      const usings = usingMatches.map(match => 
        match.replace('using ', '').replace(';', '').trim()
      );

      // 실제 구현에서는 기존 코드베이스의 using 문과 비교
      const conflictingUsings = [
        'System.Collections.Generic',
        'UnityEngine',
        'Zenject',
        'DG.Tweening'
      ];

      for (const using of usings) {
        if (conflictingUsings.includes(using)) {
          conflicts.push(using);
        }
      }
    }

    return conflicts;
  }

  /**
   * 어셈블리 충돌 찾기
   */
  private findAssemblyConflicts(newCode: string): string[] {
    // 실제 구현에서는 어셈블리 참조 분석
    return [];
  }

  /**
   * 버전 충돌 찾기
   */
  private findVersionConflicts(newCode: string): string[] {
    // 실제 구현에서는 버전 정보 분석
    return [];
  }

  /**
   * 로직 충돌 예측
   */
  private async predictLogicConflicts(newCode: string, targetFile: string): Promise<any> {
    return {
      stateConflicts: this.findStateConflicts(newCode),
      flowConflicts: this.findFlowConflicts(newCode),
      dataConflicts: this.findDataConflicts(newCode),
      severity: 'low' as 'low' | 'medium' | 'high'
    };
  }

  /**
   * 상태 충돌 찾기
   */
  private findStateConflicts(newCode: string): string[] {
    const conflicts: string[] = [];

    if (newCode.includes('IsInitialized') && newCode.includes('true')) {
      conflicts.push('초기화 상태 충돌 가능성');
    }

    if (newCode.includes('IsTransitioning') && newCode.includes('true')) {
      conflicts.push('전환 상태 충돌 가능성');
    }

    return conflicts;
  }

  /**
   * 플로우 충돌 찾기
   */
  private findFlowConflicts(newCode: string): string[] {
    const conflicts: string[] = [];

    if (newCode.includes('Start()') && newCode.includes('Initialize()')) {
      conflicts.push('초기화 플로우 충돌 가능성');
    }

    if (newCode.includes('Update()') && newCode.includes('FixedUpdate()')) {
      conflicts.push('업데이트 플로우 충돌 가능성');
    }

    return conflicts;
  }

  /**
   * 데이터 충돌 찾기
   */
  private findDataConflicts(newCode: string): string[] {
    const conflicts: string[] = [];

    if (newCode.includes('currentHP') && newCode.includes('maxHP')) {
      conflicts.push('HP 데이터 충돌 가능성');
    }

    if (newCode.includes('currentResource') && newCode.includes('maxResource')) {
      conflicts.push('리소스 데이터 충돌 가능성');
    }

    return conflicts;
  }

  /**
   * 위험도 계산
   */
  private calculateRiskLevel(newCode: string, targetFile: string): 'low' | 'medium' | 'high' {
    let riskScore = 0;

    // 네이밍 충돌 점수
    const namingConflicts = this.findClassNameConflicts(newCode).length +
                           this.findMethodNameConflicts(newCode).length +
                           this.findVariableNameConflicts(newCode).length;
    riskScore += namingConflicts * 2;

    // 인터페이스 충돌 점수
    const interfaceConflicts = this.findInterfaceConflicts(newCode).length +
                               this.findPropertyConflicts(newCode).length;
    riskScore += interfaceConflicts * 3;

    // 로직 충돌 점수
    const logicConflicts = this.findStateConflicts(newCode).length +
                          this.findFlowConflicts(newCode).length +
                          this.findDataConflicts(newCode).length;
    riskScore += logicConflicts * 1;

    if (riskScore === 0) return 'low';
    if (riskScore <= 5) return 'medium';
    return 'high';
  }

  /**
   * 권장사항 생성
   */
  private async generateRecommendations(newCode: string, targetFile: string): Promise<string[]> {
    const recommendations: string[] = [];

    const namingConflicts = this.findClassNameConflicts(newCode);
    if (namingConflicts.length > 0) {
      recommendations.push(`클래스명 충돌 발견: ${namingConflicts.join(', ')}. 다른 이름을 사용하거나 기존 클래스를 확장하세요.`);
    }

    const methodConflicts = this.findMethodNameConflicts(newCode);
    if (methodConflicts.length > 0) {
      recommendations.push(`메서드명 충돌 발견: ${methodConflicts.join(', ')}. 다른 이름을 사용하거나 기존 메서드를 오버라이드하세요.`);
    }

    const interfaceConflicts = this.findInterfaceConflicts(newCode);
    if (interfaceConflicts.length > 0) {
      recommendations.push(`인터페이스 충돌 발견: ${interfaceConflicts.join(', ')}. 기존 인터페이스를 확장하거나 다른 이름을 사용하세요.`);
    }

    if (recommendations.length === 0) {
      recommendations.push('충돌이 발견되지 않았습니다. 안전하게 코드를 추가할 수 있습니다.');
    }

    return recommendations;
  }
}
