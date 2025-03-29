import { SecretVault, Secret } from '../../src/utils/vault/index';

jest.mock('../../src/utils/tee', () => ({
  encryptWithTEE: {
    generateConfig: async () => ({
      encryptionKeyId: 'test-key',
      attestationToken: 'test-token',
      mrEnclave: 'test-mrenclave',
    }),
    encrypt: async (value: string) => `encrypted_${value}`,
  },
  decryptWithTEE: {
    decrypt: async (value: string) => value.replace('encrypted_', ''),
  },
}));

describe('SecretVault', () => {
  let vault: SecretVault;
  const testNeoAddress = 'NeoAddress123';

  // Define the test secret as a constant to be used throughout the tests
  const testSecret = {
    id: 'test-secret-1',
    name: 'Test Secret',
    value: 'test-value',
    neoAddress: testNeoAddress,
    createdAt: new Date().toISOString(),
    lastAccessed: new Date().toISOString(),
    accessCount: 0,
    permissions: {
      functionIds: ['func1'],
      roles: ['role1']
    },
    teeConfig: {
      encryptionKeyId: 'key1',
      attestationToken: 'token1',
      mrEnclave: 'enclave1'
    },
    tags: ['test'],
    metadata: { purpose: 'testing' }
  } as Secret;

  beforeEach(() => {
    vault = new SecretVault({
      teeEnabled: true,
      backupEnabled: true
    });
  });

  describe('Secret Management', () => {
    it('should create and retrieve a secret', async () => {
      await vault.createSecret(testSecret);
      const retrieved = await vault.getSecret(testSecret.id);
      expect(retrieved).not.toBeNull();
      expect(retrieved?.id).toBe(testSecret.id);
      expect(retrieved?.name).toBe(testSecret.name);
      expect(retrieved?.value).toBe(testSecret.value);
    });

    it('should update a secret', async () => {
      await vault.createSecret(testSecret);
      
      const updatedSecret = {
        ...testSecret,
        name: 'Updated Secret',
        metadata: {
          purpose: 'testing',
          updated: true
        }
      } as Secret;
      
      await vault.updateSecret(testSecret.id, updatedSecret);
      
      const retrieved = await vault.getSecret(testSecret.id);
      expect(retrieved?.name).toBe('Updated Secret');
      expect(retrieved?.metadata?.updated).toBe(true);
    });

    it('should delete a secret', async () => {
      await vault.createSecret(testSecret);
      
      const beforeDelete = await vault.getSecret(testSecret.id);
      expect(beforeDelete).not.toBeNull();
      
      await vault.deleteSecret(testSecret.id);
      
      const afterDelete = await vault.getSecret(testSecret.id);
      expect(afterDelete).toBeNull();
    });

    it('should list all secrets', async () => {
      await vault.createSecret(testSecret);
      
      const secondSecret = {
        ...testSecret,
        id: 'test-secret-2',
        name: 'Second Secret'
      } as Secret;
      
      await vault.createSecret(secondSecret);
      
      const secrets = await vault.listSecrets();
      expect(secrets.length).toBe(2);
      expect(secrets.find(s => s.id === testSecret.id)).not.toBeUndefined();
      expect(secrets.find(s => s.id === secondSecret.id)).not.toBeUndefined();
    });
  });

  describe('Access Logging', () => {
    it('should track access logs', async () => {
      await vault.createSecret(testSecret);
      
      // Record the access
      await vault.recordAccess(testSecret.id, 'func1', testNeoAddress, true);
      
      // Check that the secret was updated
      const secret = await vault.getSecret(testSecret.id);
      expect(secret?.accessCount).toBe(1);
      expect(new Date(secret?.lastAccessed || 0).getTime()).toBeGreaterThan(
        new Date(Date.now() - 1000).getTime()
      );
      
      // Record another access
      await vault.recordAccess(testSecret.id, 'func1', testNeoAddress, true);
      
      // Check access logs
      const logs = await vault.getAccessLogs(testSecret.id);
      expect(logs.length).toBe(2);
    });
  });

  describe('Secret Rotation', () => {
    it('should rotate secrets', async () => {
      await vault.createSecret(testSecret);
      
      // Rotate the secret
      const newValue = 'new-secret-value';
      await vault.rotateSecret(testSecret.id, newValue, 'manual rotation');
      
      // Get the updated secret
      const updated = await vault.getSecret(testSecret.id);
      expect(updated?.value).toBe(newValue);
    });
    
    it('should maintain rotation history', async () => {
      await vault.createSecret(testSecret);
      
      // Rotate the secret
      await vault.rotateSecret(testSecret.id, 'new-secret-value', 'manual rotation');
      
      // Get rotation history
      const history = await vault.getRotationHistory(testSecret.id);
      expect(history.length).toBe(1);
      
      // Rotate again
      await vault.rotateSecret(testSecret.id, 'new-secret-value-2', 'manual rotation');
      
      // History should have two entries now
      const updatedHistory = await vault.getRotationHistory(testSecret.id);
      expect(updatedHistory.length).toBe(2);
      expect(updatedHistory[0].previousValue).toBe(testSecret.value);
      expect(updatedHistory[1].previousValue).toBe('new-secret-value');
    });
  });
});