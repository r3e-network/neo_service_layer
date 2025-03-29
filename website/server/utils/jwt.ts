import jwt from 'jsonwebtoken';
import { Logger } from '@/utils/logger';

/**
 * JWT Service for handling authentication tokens
 * Provides methods for creating, verifying, and refreshing JWT tokens
 */
export class JWTService {
  private static instance: JWTService;
  private logger = Logger.getInstance().child({ service: 'jwt-service' });
  private secretKey: string;
  private tokenExpiration: string;
  private refreshTokenExpiration: string;

  constructor() {
    this.secretKey = process.env.JWT_SECRET_KEY || 'neo-service-layer-default-secret';
    this.tokenExpiration = process.env.JWT_EXPIRATION || '1h';
    this.refreshTokenExpiration = process.env.JWT_REFRESH_EXPIRATION || '7d';
    
    if (process.env.NODE_ENV === 'production' && this.secretKey === 'neo-service-layer-default-secret') {
      this.logger.warn('Using default JWT secret key in production environment');
    }
  }

  /**
   * Get the singleton instance of JWTService
   */
  public static getInstance(): JWTService {
    if (!JWTService.instance) {
      JWTService.instance = new JWTService();
    }
    return JWTService.instance;
  }

  /**
   * Create a new JWT token
   * @param payload Data to include in the token
   * @returns Generated token
   */
  public createToken(payload: Record<string, any>): string {
    // Using the sign method directly with the options
    return jwt.sign(
      payload, 
      this.secretKey, 
      { expiresIn: this.tokenExpiration as any }
    );
  }

  /**
   * Create a refresh token
   * @param payload Data to include in the token
   * @returns Generated refresh token
   */
  public createRefreshToken(payload: Record<string, any>): string {
    // Using the sign method directly with the options
    return jwt.sign(
      payload, 
      this.secretKey, 
      { expiresIn: this.refreshTokenExpiration as any }
    );
  }

  /**
   * Verify a JWT token
   * @param token Token to verify
   * @returns Decoded token payload or null if invalid
   */
  public verifyToken(token: string): Record<string, any> | null {
    try {
      return jwt.verify(token, this.secretKey) as Record<string, any>;
    } catch (error) {
      this.logger.error('Error verifying JWT token', { error });
      return null;
    }
  }

  /**
   * Decode a JWT token without verification
   * @param token Token to decode
   * @returns Decoded token payload or null if invalid
   */
  public decodeToken(token: string): Record<string, any> | null {
    try {
      return jwt.decode(token) as Record<string, any>;
    } catch (error) {
      this.logger.error('Error decoding JWT token', { error });
      return null;
    }
  }

  /**
   * Refresh an existing token
   * @param refreshToken Refresh token to use
   * @returns New access token or null if refresh token is invalid
   */
  public refreshToken(refreshToken: string): string | null {
    try {
      const decoded = jwt.verify(refreshToken, this.secretKey) as Record<string, any>;
      const { iat, exp, ...payload } = decoded;
      return this.createToken(payload);
    } catch (error) {
      this.logger.error('Error refreshing token', { error });
      return null;
    }
  }
}