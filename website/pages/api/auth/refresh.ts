import { NextApiRequest, NextApiResponse } from 'next';
import { JWTService } from '@/server/utils/jwt';
import { Logger } from '@/utils/logger';
import { MetricsService } from '@/services/metrics';

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  const logger = Logger.getInstance().child({ service: 'auth-refresh' });
  const metrics = new MetricsService({
    namespace: 'neo_service_layer',
    subsystem: 'auth'
  });

  if (req.method !== 'POST') {
    return res.status(405).json({ error: 'Method not allowed' });
  }

  try {
    const authHeader = req.headers.authorization;
    
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return res.status(401).json({ error: 'No token provided' });
    }

    const token = authHeader.split(' ')[1];
    const jwtService = JWTService.getInstance();

    try {
      // Verify and refresh the token
      const newToken = jwtService.refreshToken(token);
      
      if (!newToken) {
        metrics.incrementCounter('auth_token_refresh_failed_total');
        return res.status(401).json({ error: 'Invalid token' });
      }
      
      const decoded = jwtService.decodeToken(newToken);

      metrics.incrementCounter('auth_token_refresh_success_total');

      return res.status(200).json({
        token: newToken,
        address: decoded?.address,
        networkMagic: decoded?.networkMagic
      });
    } catch (error) {
      metrics.incrementCounter('auth_token_refresh_failed_total');
      return res.status(401).json({ error: 'Invalid token' });
    }
  } catch (error) {
    logger.error('Error refreshing token', { error });
    metrics.incrementCounter('auth_token_refresh_error_total');
    return res.status(500).json({ error: 'Internal server error' });
  }
}