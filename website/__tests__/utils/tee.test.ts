import { encryptWithTEE, decryptWithTEE, getTEEAttestation, verifyTEEAttestation, TEEConfig } from '../../src/utils/tee';

describe('TEE Service', () => {
  const testValue = 'test-secret-value';
  let teeConfig: TEEConfig;

  beforeEach(async () => {
    // Get a TEE configuration for testing
    teeConfig = await getTEEAttestation('test-mrenclave');
  });

  it('should encrypt and decrypt values', async () => {
    // Encrypt the test value
    const encrypted = await encryptWithTEE(testValue, teeConfig);
    expect(encrypted).toBeTruthy();
    expect(typeof encrypted).toBe('string');
    
    // Decrypt the encrypted value
    const decrypted = await decryptWithTEE(encrypted, teeConfig);
    expect(decrypted).toBe(testValue);
  });

  it('should handle different values', async () => {
    const values = ['string value', '123456', JSON.stringify({ key: 'value' })];
    
    for (const value of values) {
      const encrypted = await encryptWithTEE(value, teeConfig);
      const decrypted = await decryptWithTEE(encrypted, teeConfig);
      expect(decrypted).toBe(value);
    }
  });

  it('should verify attestation', async () => {
    const verification = await verifyTEEAttestation(teeConfig.attestationToken, teeConfig);
    
    expect(verification).toHaveProperty('attestationValid');
    expect(verification).toHaveProperty('mrEnclaveMatch');
  });

  it('should generate unique TEE configurations', async () => {
    const config1 = await getTEEAttestation('test-mrenclave');
    const config2 = await getTEEAttestation('test-mrenclave');
    
    expect(config1.encryptionKeyId).not.toBe(config2.encryptionKeyId);
    expect(config1.attestationToken).not.toBe(config2.attestationToken);
    expect(config1.mrEnclave).toBe(config2.mrEnclave);
  });
});