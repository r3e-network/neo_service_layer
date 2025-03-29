import { ethers } from 'ethers';
import {
  signPriceData,
  verifyPriceData,
  formatPriceForChain,
  parsePriceFromChain,
  calculateSourcesHash,
  estimateGasForPriceUpdate,
  getNetworkConfig,
  waitForPriceUpdate
} from '../../utils/blockchain';
import { PRICE_FEED_CONSTANTS } from '../../constants';

jest.mock('ethers');

describe('Blockchain Utils', () => {
  const mockSigner = {
    signMessage: jest.fn(),
  };

  const mockContract = {
    estimateGas: {
      updatePrice: jest.fn(),
    },
    getPrice: jest.fn(),
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('signPriceData', () => {
    it('should sign price data correctly', async () => {
      const expectedSignature = '0x123...';
      mockSigner.signMessage.mockResolvedValue(expectedSignature);

      const result = await signPriceData(
        mockSigner as unknown as ethers.Signer,
        'NEO/USD',
        100,
        Date.now()
      );

      expect(result).toBe(expectedSignature);
      expect(mockSigner.signMessage).toHaveBeenCalled();
    });
  });

  describe('verifyPriceData', () => {
    it('should verify signed price data correctly', () => {
      const mockAddress = '0x123...';
      (ethers.utils.verifyMessage as jest.Mock).mockReturnValue(mockAddress);

      const result = verifyPriceData(
        '0xsignature',
        'NEO/USD',
        100,
        Date.now()
      );

      expect(result).toBe(mockAddress);
    });
  });

  describe('formatPriceForChain', () => {
    it('should format price with correct decimals', () => {
      const price = 123.456789;
      const expectedResult = '12345678900000000';
      (ethers.utils.parseUnits as jest.Mock).mockReturnValue(expectedResult);

      const result = formatPriceForChain(price);

      expect(result).toBe(expectedResult);
      expect(ethers.utils.parseUnits).toHaveBeenCalledWith(
        price.toString(),
        PRICE_FEED_CONSTANTS.PRICE_DECIMALS
      );
    });
  });

  describe('parsePriceFromChain', () => {
    it('should parse on-chain price correctly', () => {
      const priceHex = '0x123';
      const expectedResult = 123.456789;
      (ethers.utils.formatUnits as jest.Mock).mockReturnValue(expectedResult.toString());

      const result = parsePriceFromChain(priceHex);

      expect(result).toBe(expectedResult);
      expect(ethers.utils.formatUnits).toHaveBeenCalledWith(
        priceHex,
        PRICE_FEED_CONSTANTS.PRICE_DECIMALS
      );
    });
  });

  describe('calculateSourcesHash', () => {
    it('should calculate sources hash correctly', () => {
      const sources = [
        { id: 'source2', price: 100 },
        { id: 'source1', price: 200 }
      ];
      const expectedHash = '0xhash';
      (ethers.utils.solidityKeccak256 as jest.Mock).mockReturnValue(expectedHash);

      const result = calculateSourcesHash(sources);

      expect(result).toBe(expectedHash);
      expect(ethers.utils.solidityKeccak256).toHaveBeenCalled();
    });

    it('should sort sources by ID before hashing', () => {
      const sources = [
        { id: 'source2', price: 100 },
        { id: 'source1', price: 200 }
      ];
      calculateSourcesHash(sources);

      const concatenatedData = (ethers.utils.solidityKeccak256 as jest.Mock).mock.calls[0][1][0];
      expect(concatenatedData.startsWith('source1:')).toBeTruthy();
    });
  });

  describe('estimateGasForPriceUpdate', () => {
    it('should estimate gas correctly', async () => {
      const expectedGas = ethers.BigNumber.from('100000');
      mockContract.estimateGas.updatePrice.mockResolvedValue(expectedGas);

      const result = await estimateGasForPriceUpdate(
        mockContract as unknown as ethers.Contract,
        'NEO/USD',
        100,
        Date.now(),
        '0xhash',
        '0xsignature'
      );

      expect(result).toBe(expectedGas);
      expect(mockContract.estimateGas.updatePrice).toHaveBeenCalled();
    });
  });

  describe('getNetworkConfig', () => {
    const originalEnv = process.env;

    beforeEach(() => {
      process.env = {
        ...originalEnv,
        MAINNET_RPC_URL: 'https://mainnet.neo.org',
        MAINNET_CONTRACT_ADDRESS: '0xcontract',
        TESTNET_RPC_URL: 'https://testnet.neo.org',
        TESTNET_CONTRACT_ADDRESS: '0xtestcontract'
      };
    });

    afterEach(() => {
      process.env = originalEnv;
    });

    it('should return mainnet config', () => {
      const config = getNetworkConfig(1);
      expect(config.rpcUrl).toBe(process.env.MAINNET_RPC_URL);
      expect(config.contractAddress).toBe(process.env.MAINNET_CONTRACT_ADDRESS);
    });

    it('should return testnet config', () => {
      const config = getNetworkConfig(2);
      expect(config.rpcUrl).toBe(process.env.TESTNET_RPC_URL);
      expect(config.contractAddress).toBe(process.env.TESTNET_CONTRACT_ADDRESS);
    });

    it('should throw error for unsupported network', () => {
      expect(() => getNetworkConfig(999)).toThrow('Unsupported network: 999');
    });
  });

  describe('waitForPriceUpdate', () => {
    beforeEach(() => {
      jest.useFakeTimers();
    });

    afterEach(() => {
      jest.useRealTimers();
    });

    it('should return true when price is updated within threshold', async () => {
      mockContract.getPrice.mockResolvedValue(ethers.utils.parseUnits('100', PRICE_FEED_CONSTANTS.PRICE_DECIMALS));

      const waitPromise = waitForPriceUpdate(
        mockContract as unknown as ethers.Contract,
        'NEO/USD',
        100,
        1000
      );

      jest.advanceTimersByTime(500);
      const result = await waitPromise;

      expect(result).toBe(true);
      expect(mockContract.getPrice).toHaveBeenCalled();
    });

    it('should return false on timeout', async () => {
      mockContract.getPrice.mockResolvedValue(ethers.utils.parseUnits('90', PRICE_FEED_CONSTANTS.PRICE_DECIMALS));

      const waitPromise = waitForPriceUpdate(
        mockContract as unknown as ethers.Contract,
        'NEO/USD',
        100,
        1000
      );

      jest.advanceTimersByTime(1500);
      const result = await waitPromise;

      expect(result).toBe(false);
    });

    it('should handle errors gracefully', async () => {
      mockContract.getPrice.mockRejectedValue(new Error('RPC Error'));

      const waitPromise = waitForPriceUpdate(
        mockContract as unknown as ethers.Contract,
        'NEO/USD',
        100,
        1000
      );

      jest.advanceTimersByTime(1500);
      const result = await waitPromise;

      expect(result).toBe(false);
    });
  });
});