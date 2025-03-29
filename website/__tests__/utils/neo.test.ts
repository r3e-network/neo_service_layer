import { formatNeoAddress, validateContractHash, getNeoScriptHash } from '../../src/utils/neo';

jest.mock('@cityofzion/neon-js', () => ({
  wallet: {
    isAddress: (address: string) => 
      address === 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd',
    getScriptHashFromAddress: (address: string) =>
      address === 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd'
        ? '0x1234567890abcdef1234567890abcdef12345678'
        : '',
  },
}));

describe('Neo Utilities', () => {
  describe('formatNeoAddress', () => {
    it('should format Neo address correctly', () => {
      const address = 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd';
      expect(formatNeoAddress(address)).toBe('NXv2...Hd');
    });

    it('should handle empty address', () => {
      expect(formatNeoAddress('')).toBe('');
    });
  });

  describe('validateContractHash', () => {
    it('should validate correct contract hash format', async () => {
      const validHash = '0x1234567890abcdef1234567890abcdef12345678';
      expect(await validateContractHash(validHash)).toBe(true);
    });

    it('should reject invalid contract hash format', async () => {
      const invalidHashes = [
        '1234567890abcdef1234567890abcdef12345678', // missing 0x
        '0xg234567890abcdef1234567890abcdef12345678', // invalid character
        '0x123456', // too short
        '0x1234567890abcdef1234567890abcdef123456789', // too long
      ];

      for (const hash of invalidHashes) {
        expect(await validateContractHash(hash)).toBe(false);
      }
    });
  });

  describe('getNeoScriptHash', () => {
    it('should convert valid Neo address to script hash', () => {
      const address = 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd';
      const hash = getNeoScriptHash(address);
      expect(hash).toBe('0x1234567890abcdef1234567890abcdef12345678');
    });

    it('should handle invalid Neo address', () => {
      const invalidAddress = 'invalid-address';
      const hash = getNeoScriptHash(invalidAddress);
      expect(hash).toBe('');
    });
  });
});