import { NextApiRequest, NextApiResponse } from 'next';
import { randomBytes } from 'crypto';
import { Logger } from '@/utils/logger';
import { MetricsService } from '@/services/metrics';

const CHALLENGE_EXPIRY = 5 * 60 * 1000; // 5 minutes
const challenges = new Map<string, { challenge: string; timestamp: number }>();

// Clean up expired challenges periodically
setInterval(() => {
  const now = Date.now();
  for (const [address, data] of challenges.entries()) {
    if (now - data.timestamp > CHALLENGE_EXPIRY) {
      challenges.delete(address);
    }
  }
}, 60 * 1000); // Clean up every minute

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  const logger = Logger.getInstance().child({ service: 'auth-challenge' });
  const metrics = new MetricsService({
    namespace: 'neo_service_layer',
    subsystem: 'auth'
  });

  if (req.method !== 'POST') {
    return res.status(405).json({ error: 'Method not allowed' });
  }

  try {
    const { address } = req.body;

    if (!address) {
      return res.status(400).json({ error: 'Address is required' });
    }

    // Generate a random challenge
    const challenge = randomBytes(32).toString('hex');
    const timestamp = Date.now();

    // Store the challenge
    challenges.set(address, { challenge, timestamp });

    metrics.incrementCounter('auth_challenges_generated_total');

    return res.status(200).json({
      challenge,
      message: `Sign this message to authenticate: ${challenge}`
    });
  } catch (error) {
    logger.error('Error generating challenge', { error });
    metrics.incrementCounter('auth_challenges_error_total');
    return res.status(500).json({ error: 'Internal server error' });
  }
}

// Export for use in other files
export { challenges };