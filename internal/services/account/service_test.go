package account

import (
	"context"
	"testing"
)

type mockGasBank struct{}

func (m *mockGasBank) EnsureGas(ctx context.Context, address string, amount int64) error {
	return nil
}

type mockSecrets struct{}

func (m *mockSecrets) GetSecret(ctx context.Context, address string, key string) ([]byte, error) {
	return []byte("test"), nil
}

func (m *mockSecrets) StoreSecret(ctx context.Context, address string, key string, value []byte) error {
	return nil
}

type mockTEE struct{}

func (m *mockTEE) GenerateAddress(ctx context.Context) (string, error) {
	return "NeoTestAddress123", nil
}

func (m *mockTEE) VerifySignature(ctx context.Context, address string, message, signature []byte) error {
	return nil
}

func TestNewService(t *testing.T) {
	config := DefaultConfig()
	gasBank := &mockGasBank{}
	secrets := &mockSecrets{}
	tee := &mockTEE{}

	service, err := NewService(config, gasBank, secrets, tee)
	if err != nil {
		t.Fatalf("Failed to create service: %v", err)
	}

	if service == nil {
		t.Fatal("Service should not be nil")
	}
}

func TestCreateAccount(t *testing.T) {
	config := DefaultConfig()
	gasBank := &mockGasBank{}
	secrets := &mockSecrets{}
	tee := &mockTEE{}

	service, _ := NewService(config, gasBank, secrets, tee)

	ctx := context.Background()
	account, err := service.CreateAccount(ctx, StandardAccount)
	if err != nil {
		t.Fatalf("Failed to create account: %v", err)
	}

	if account.Address != "NeoTestAddress123" {
		t.Errorf("Expected address NeoTestAddress123, got %s", account.Address)
	}

	if account.Type != StandardAccount {
		t.Errorf("Expected account type %s, got %s", StandardAccount, account.Type)
	}
}

func TestGetAccount(t *testing.T) {
	config := DefaultConfig()
	gasBank := &mockGasBank{}
	secrets := &mockSecrets{}
	tee := &mockTEE{}

	service, _ := NewService(config, gasBank, secrets, tee)

	ctx := context.Background()
	created, _ := service.CreateAccount(ctx, StandardAccount)

	account, err := service.GetAccount(ctx, created.Address)
	if err != nil {
		t.Fatalf("Failed to get account: %v", err)
	}

	if account.Address != created.Address {
		t.Errorf("Expected address %s, got %s", created.Address, account.Address)
	}
}

func TestSubmitTransaction(t *testing.T) {
	config := DefaultConfig()
	gasBank := &mockGasBank{}
	secrets := &mockSecrets{}
	tee := &mockTEE{}

	service, _ := NewService(config, gasBank, secrets, tee)

	ctx := context.Background()
	account, _ := service.CreateAccount(ctx, StandardAccount)

	tx := &Transaction{
		Hash:      []byte("testhash"),
		Signature: []byte("testsig"),
		GasLimit:  1000,
	}

	err := service.SubmitTransaction(ctx, account.Address, tx)
	if err != nil {
		t.Fatalf("Failed to submit transaction: %v", err)
	}
}
