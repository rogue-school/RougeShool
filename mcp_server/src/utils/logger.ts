import winston from 'winston';
import path from 'path';

/**
 * 전문적인 로깅 시스템
 * 엔터프라이즈급 로깅 기능 제공
 */
export class Logger {
  private static instance: Logger;
  private logger: winston.Logger;

  private constructor() {
    this.logger = winston.createLogger({
      level: process.env.LOG_LEVEL || 'info',
      format: winston.format.combine(
        winston.format.timestamp({
          format: 'YYYY-MM-DD HH:mm:ss'
        }),
        winston.format.errors({ stack: true }),
        winston.format.json()
      ),
      defaultMeta: { service: 'rougeshool-mcp-server' },
      transports: [
        // 콘솔 출력
        new winston.transports.Console({
          format: winston.format.combine(
            winston.format.colorize(),
            winston.format.simple()
          )
        }),
        // 파일 출력
        new winston.transports.File({
          filename: path.join(process.cwd(), 'logs', 'error.log'),
          level: 'error',
          maxsize: 5242880, // 5MB
          maxFiles: 5
        }),
        new winston.transports.File({
          filename: path.join(process.cwd(), 'logs', 'combined.log'),
          maxsize: 5242880, // 5MB
          maxFiles: 5
        })
      ]
    });
  }

  public static getInstance(): Logger {
    if (!Logger.instance) {
      Logger.instance = new Logger();
    }
    return Logger.instance;
  }

  public info(message: string, meta?: any): void {
    this.logger.info(message, meta);
  }

  public warn(message: string, meta?: any): void {
    this.logger.warn(message, meta);
  }

  public error(message: string, error?: Error, meta?: any): void {
    this.logger.error(message, { error: error?.stack, ...meta });
  }

  public debug(message: string, meta?: any): void {
    this.logger.debug(message, meta);
  }

  public performance(operation: string, duration: number, meta?: any): void {
    this.logger.info(`Performance: ${operation}`, {
      duration: `${duration}ms`,
      ...meta
    });
  }
}

export const logger = Logger.getInstance();
