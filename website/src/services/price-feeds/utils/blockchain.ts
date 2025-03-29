import { ethers } from 'ethers';
import { PRICE_FEED_CONSTANTS } from '../constants';

export async function signPriceData(
  signer: ethers.Signer,
  symbol: string,
  price: number,
  timestamp: number
): Promise<string> {
  const message = ethers.solidityPackedKeccak256(
    ['string', 'uint256', 'uint256'],
    [symbol, ethers.parseUnits(price.toString(), PRICE_FEED_CONSTANTS.PRICE_DECIMALS), timestamp]
  );

  return await signer.signMessage(ethers.getBytes(message));
}

export function verifyPriceData(
  signature: string,
  symbol: string,
  price: number,
  timestamp: number
): string {
  const message = ethers.solidityPackedKeccak256(
    ['string', 'uint256', 'uint256'],
    [symbol, ethers.parseUnits(price.toString(), PRICE_FEED_CONSTANTS.PRICE_DECIMALS), timestamp]
  );

  return ethers.verifyMessage(ethers.getBytes(message), signature);
}

export function formatPriceForChain(price: number): string {
  return ethers.parseUnits(price.toString(), PRICE_FEED_CONSTANTS.PRICE_DECIMALS).toString();
}

export function parsePriceFromChain(priceHex: string): number {
  return parseFloat(ethers.formatUnits(priceHex, PRICE_FEED_CONSTANTS.PRICE_DECIMALS));
}

export function calculateSourcesHash(sources: { id: string; price: number }[]): string {
  const sortedSources = [...sources].sort((a, b) => a.id.localeCompare(b.id));
  const concatenatedData = sortedSources.map(s => 
    `${s.id}:${ethers.parseUnits(s.price.toString(), PRICE_FEED_CONSTANTS.PRICE_DECIMALS)}`
  ).join('');
  
  return ethers.solidityPackedKeccak256(['string'], [concatenatedData]);
}

export async function estimateGasForPriceUpdate(
  contract: ethers.Contract,
  symbol: string,
  price: number,
  timestamp: number,
  sourcesHash: string,
  signature: string
): Promise<bigint> {
  return await contract.getFunction("updatePrice").estimateGas(
    symbol,
    ethers.parseUnits(price.toString(), PRICE_FEED_CONSTANTS.PRICE_DECIMALS),
    timestamp,
    sourcesHash,
    signature
  );
}

export function getNetworkConfig(chainId: number): {
  rpcUrl: string;
  contractAddress: string;
  explorerUrl: string;
} {
  // Add more networks as needed
  const networks: Record<number, { rpcUrl: string; contractAddress: string; explorerUrl: string }> = {
    1: {
      rpcUrl: process.env.MAINNET_RPC_URL!,
      contractAddress: process.env.MAINNET_CONTRACT_ADDRESS!,
      explorerUrl: 'https://explorer.neo.org'
    },
    2: {
      rpcUrl: process.env.TESTNET_RPC_URL!,
      contractAddress: process.env.TESTNET_CONTRACT_ADDRESS!,
      explorerUrl: 'https://testnet.explorer.neo.org'
    }
  };

  const network = networks[chainId];
  if (!network) {
    throw new Error(`Unsupported network: ${chainId}`);
  }

  return network;
}

export async function waitForPriceUpdate(
  contract: ethers.Contract,
  symbol: string,
  expectedPrice: number,
  timeout: number = 60000
): Promise<boolean> {
  const startTime = Date.now();
  
  while (Date.now() - startTime < timeout) {
    try {
      const currentPrice = await contract.getFunction("getPrice").staticCall(symbol);
      const parsedPrice = parsePriceFromChain(currentPrice.toString());
      
      if (Math.abs(parsedPrice - expectedPrice) / expectedPrice < PRICE_FEED_CONSTANTS.DEFAULT_DEVIATION_THRESHOLD) {
        return true;
      }
    } catch (error) {
      console.error('Error checking price update:', error);
    }

    await new Promise(resolve => setTimeout(resolve, 1000));
  }

  return false;
}