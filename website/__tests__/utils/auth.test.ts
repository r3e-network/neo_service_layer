import { verifySignature, verifyNeoAddress } from '../../src/utils/auth';

jest.mock('@cityofzion/neon-js', () => ({
  wallet: {
    isAddress: (address: string) => 
      address === 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd',
  },
}));

describe('Authentication Utilities', () => {
  describe('verifySignature', () => {
    it('should verify valid signature', async () => {
      const validSignature = '1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef';
      expect(await verifySignature(validSignature)).toBe(true);
    });

    it('should reject empty signature', async () => {
      expect(await verifySignature('')).toBe(false);
    });

    it('should handle verification errors gracefully', async () => {
      // Mock a scenario where verification throws an error
      const errorSignature = 'error-signature';
      expect(await verifySignature(errorSignature)).toBe(false);
    });
  });

  describe('verifyNeoAddress', () => {
    it('should verify valid Neo address', () => {
      const validAddress = 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd';
      expect(verifyNeoAddress(validAddress)).toBe(true);
    });

    it('should reject invalid Neo address', () => {
      const invalidAddresses = [
        '',
        'invalid-address',
        'NXv29LiogxZk6KvXnThwMgJxKanYAKMzH', // too short
        'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHdd', // too long
      ];

      invalidAddresses.forEach(address => {
        expect(verifyNeoAddress(address)).toBe(false);
      });
    });

    it('should handle verification errors gracefully', () => {
      // Mock a scenario where verification throws an error
      const errorAddress = 'error-address';
      expect(verifyNeoAddress(errorAddress)).toBe(false);
    });
  });
});