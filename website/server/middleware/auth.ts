import { NextApiRequest, NextApiResponse } from 'next';
import { JWTService } from '../utils/jwt';
import { Logger } from '@/utils/logger';

export interface AuthenticatedRequest extends NextApiRequest {
  user?: {
    address: string;
    networkMagic: number;
  };
}

export function withAuth(handler: (req: AuthenticatedRequest, res: NextApiResponse) => Promise<void>) {
  return async (req: AuthenticatedRequest, res: NextApiResponse) => {
    const logger = Logger.getInstance().child({ service: 'auth-middleware' });
    const jwtService = JWTService.getInstance();

    try {
      const authHeader = req.headers.authorization;
      
      if (!authHeader || !authHeader.startsWith('Bearer ')) {
        return res.status(401).json({ error: 'No token provided' });
      }

      const token = authHeader.split(' ')[1];
      const decoded = jwtService.verifyToken(token);
      
      if (!decoded) {
        return res.status(401).json({ error: 'Invalid token' });
      }

      // Attach user info to request
      req.user = {
        address: decoded.address,
        networkMagic: decoded.networkMagic
      };

      // Call the handler
      return handler(req, res);
    } catch (error) {
      logger.error('Authentication error', { error });
      return res.status(500).json({ error: 'Authentication error' });
    }
  };
}

export function withOptionalAuth(handler: (req: AuthenticatedRequest, res: NextApiResponse) => Promise<void>) {
  return async (req: AuthenticatedRequest, res: NextApiResponse) => {
    const logger = Logger.getInstance().child({ service: 'auth-middleware' });
    const jwtService = JWTService.getInstance();

    try {
      const authHeader = req.headers.authorization;
      
      if (authHeader && authHeader.startsWith('Bearer ')) {
        const token = authHeader.split(' ')[1];
        try {
          const decoded = jwtService.verifyToken(token);
          if (!decoded) {
            logger.warn('Invalid token in optional auth');
          } else {
            req.user = {
              address: decoded.address,
              networkMagic: decoded.networkMagic
            };
          }
        } catch (error) {
          logger.warn('Invalid token in optional auth', { error });
        }
      }

      // Call the handler regardless of auth status
      return handler(req, res);
    } catch (error) {
      logger.error('Optional authentication error', { error });
      return res.status(500).json({ error: 'Internal server error' });
    }
  };
}