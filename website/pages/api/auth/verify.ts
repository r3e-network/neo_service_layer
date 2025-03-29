import { NextApiRequest, NextApiResponse } from 'next';
import { challenges } from './challenge';
import { JWTService } from '@/server/utils/jwt';
import { Logger } from '@/utils/logger';
import { MetricsService } from '@/services/metrics';
import { CryptoUtils } from '@/utils/crypto';

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  const logger = Logger.getInstance().child({ service: 'auth-verify' });
  const metrics = new MetricsService({
    namespace: 'neo_service_layer',
    subsystem: 'auth'
  });

  if (req.method !== 'POST') {
    return res.status(405).json({ error: 'Method not allowed' });
  }

  try {
    const { address, signature, networkMagic } = req.body;

    if (!address || !signature || !networkMagic) {
      return res.status(400).json({
        error: 'Address, signature, and networkMagic are required'
      });
    }

    // Get the stored challenge
    const storedData = challenges.get(address);
    if (!storedData) {
      return res.status(400).json({ error: 'No challenge found for address' });
    }

    const { challenge, timestamp } = storedData;

    // Check if challenge has expired
    if (Date.now() - timestamp > 5 * 60 * 1000) {
      challenges.delete(address);
      return res.status(400).json({ error: 'Challenge has expired' });
    }

    // Verify the signature
    const message = `Sign this message to authenticate: ${challenge}`;
    const isValid = CryptoUtils.getInstance().verifyMessageSignature(message, signature, address);

    if (!isValid) {
      metrics.incrementCounter('auth_verification_failed_total');
      return res.status(401).json({ error: 'Invalid signature' });
    }

    // Generate JWT token
    const jwtService = JWTService.getInstance();
    const token = jwtService.createToken({
      address,
      networkMagic
    });

    // Clean up the challenge
    challenges.delete(address);

    metrics.incrementCounter('auth_verification_success_total');

    return res.status(200).json({
      token,
      address,
      networkMagic
    });
  } catch (error) {
    logger.error('Error verifying signature', { error });
    metrics.incrementCounter('auth_verification_error_total');
    return res.status(500).json({ error: 'Internal server error' });
  }
}