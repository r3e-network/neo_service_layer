import { NextApiRequest, NextApiResponse } from 'next';
import { PriceFeedService } from '@/services/price-feeds';
import { NeoContractService } from '@/services/neo-contract';
import { SecretVault } from '@/utils/vault';

// Initialize services
const contractService = new NeoContractService(); // TODO: Add proper initialization
const vault = new SecretVault(); // TODO: Add proper initialization
const priceFeedService = new PriceFeedService({
  teeEnabled: true,
  contractService,
  vault
});

export default async function handler(
  req: NextApiRequest,
  res: NextApiResponse
) {
  const { symbol } = req.query;

  if (!symbol || typeof symbol !== 'string') {
    return res.status(400).json({ error: 'Symbol is required' });
  }

  try {
    switch (req.method) {
      case 'GET':
        const priceData = await priceFeedService.getAggregatedPrice(symbol);
        return res.status(200).json(priceData);

      case 'POST':
        // Handle price updates if needed
        return res.status(405).json({ error: 'Method not allowed' });

      default:
        return res.status(405).json({ error: 'Method not allowed' });
    }
  } catch (error) {
    console.error('Price feed API error:', error);
    return res.status(500).json({ error: 'Internal server error' });
  }
}