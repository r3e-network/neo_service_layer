import { NextApiRequest, NextApiResponse } from 'next';
import { JsonRpcProvider, Wallet, ethers } from 'ethers';
import {
  ApiResponse,
  GetPriceResponse,
  GetHistoricalPricesResponse,
  GetPriceSourcesResponse,
  GetPriceMetricsResponse,
  GetPriceConfigResponse,
  UpdatePriceConfigResponse,
  AuthenticatedRequest,
  PriceFeedError,
  PriceFeedErrorCode
} from './types';
import { PriceFeedServiceImpl } from './service';
import { PRICE_FEED_CONSTANTS } from '../../../services/price-feeds/constants';

// Initialize service (should be done in a proper DI container)
const provider = new JsonRpcProvider(process.env.NEO_RPC_URL);
const signer = new Wallet(process.env.PRIVATE_KEY!, provider);
const priceFeedService = new PriceFeedServiceImpl(
  provider,
  process.env.PRICE_FEED_CONTRACT_ADDRESS!,
  signer
);

// Authentication middleware
export const authenticate = async (req: NextApiRequest, res: NextApiResponse, next: () => void) => {
  try {
    const { authorization } = req.headers;
    if (!authorization) {
      throw new Error('Missing authorization header');
    }

    const [message, signature] = authorization.split(':');
    if (!message || !signature) {
      throw new Error('Missing authentication headers');
    }

    const address = ethers.verifyMessage(message, signature);
    (req as AuthenticatedRequest).auth = {
      isAuthenticated: true,
      walletAddress: address
    };
    next();
  } catch (error) {
    res.status(401).json({
      success: false,
      error: 'Authentication failed',
      details: error instanceof Error ? error.message : 'Unknown error'
    });
  }
};

// Error handling helper
function handleError(res: NextApiResponse, error: unknown): void {
  console.error('Price feed API error:', error);
  
  if (error instanceof Error) {
    // For general errors
    res.status(500).json({
      success: false,
      error: {
        code: 'INTERNAL_ERROR',
        message: error.message
      }
    });
  } else {
    // For unknown errors
    res.status(500).json({
      success: false,
      error: {
        code: 'UNKNOWN_ERROR',
        message: 'An unknown error occurred'
      }
    });
  }
}

// Convert hex to decimal with proper formatting
function formatHexValue(hexValue: string): string {
  // Use formatUnits from ethers for proper decimal conversion
  return ethers.formatUnits(hexValue, 18);
}

// Route handlers
export async function getCurrentPrice(
  req: NextApiRequest,
  res: NextApiResponse<GetPriceResponse>
) {
  try {
    const { symbol } = req.query;
    
    if (!symbol || typeof symbol !== 'string') {
      throw new Error('Invalid symbol parameter');
    }

    const price = await priceFeedService.getCurrentPrice(symbol);
    
    res.json({
      success: true,
      data: {
        currentPrice: price,
        lastUpdate: Date.now()
      }
    });
  } catch (error) {
    handleError(res, error);
  }
}

export async function getHistoricalPrices(
  req: NextApiRequest,
  res: NextApiResponse<GetHistoricalPricesResponse>
) {
  try {
    const { symbol, timeRange } = req.query;
    
    if (!symbol || typeof symbol !== 'string' || !timeRange || typeof timeRange !== 'string') {
      throw new Error('Invalid parameters');
    }

    const prices = await priceFeedService.getHistoricalPrices(symbol, timeRange);
    
    res.json({
      success: true,
      data: {
        prices,
        timeRange
      }
    });
  } catch (error) {
    handleError(res, error);
  }
}

export async function getPriceSources(
  req: NextApiRequest,
  res: NextApiResponse<GetPriceSourcesResponse>
) {
  try {
    const { symbol } = req.query;
    
    if (!symbol || typeof symbol !== 'string') {
      throw new Error('Invalid symbol parameter');
    }

    const { sources, stats } = await priceFeedService.getPriceSources(symbol);
    
    res.json({
      success: true,
      data: {
        sources,
        stats
      }
    });
  } catch (error) {
    handleError(res, error);
  }
}

export async function getPriceMetrics(
  req: NextApiRequest,
  res: NextApiResponse<GetPriceMetricsResponse>
) {
  try {
    const { symbol } = req.query;
    
    if (!symbol || typeof symbol !== 'string') {
      throw new Error('Invalid symbol parameter');
    }

    const metrics = await priceFeedService.getPriceMetrics(symbol);
    
    res.json({
      success: true,
      data: {
        metrics,
        symbol
      }
    });
  } catch (error) {
    handleError(res, error);
  }
}

export async function getConfig(
  req: NextApiRequest,
  res: NextApiResponse<GetPriceConfigResponse>
) {
  try {
    const { symbol } = req.query;
    
    if (!symbol || typeof symbol !== 'string') {
      throw new Error('Invalid symbol parameter');
    }

    const config = await priceFeedService.getConfig(symbol);
    
    res.json({
      success: true,
      data: {
        config,
        symbol
      }
    });
  } catch (error) {
    handleError(res, error);
  }
}

export async function updateConfig(
  req: AuthenticatedRequest,
  res: NextApiResponse<UpdatePriceConfigResponse>
) {
  try {
    const { symbol } = req.query;
    const config = req.body;
    
    if (!symbol || typeof symbol !== 'string' || !config) {
      throw new Error('Invalid parameters');
    }

    if (!req.auth?.isAuthenticated) {
      throw new Error('Authentication required');
    }

    const validationErrors = priceFeedService.validateConfig(config);
    if (validationErrors.length > 0) {
      res.status(400).json({
        success: false,
        error: 'Configuration validation failed',
        data: {
          config,
          validationErrors
        }
      });
      return;
    }

    const updatedConfig = await priceFeedService.updateConfig(symbol, config);
    
    res.json({
      success: true,
      data: {
        config: updatedConfig
      }
    });
  } catch (error) {
    handleError(res, error);
  }
}

export async function resetConfig(
  req: AuthenticatedRequest,
  res: NextApiResponse<GetPriceConfigResponse>
) {
  try {
    const { symbol } = req.query;
    
    if (!symbol || typeof symbol !== 'string') {
      throw new Error('Invalid symbol parameter');
    }

    if (!req.auth?.isAuthenticated) {
      throw new Error('Authentication required');
    }

    const config = await priceFeedService.resetConfig(symbol);
    
    res.json({
      success: true,
      data: {
        config,
        symbol
      }
    });
  } catch (error) {
    handleError(res, error);
  }
}