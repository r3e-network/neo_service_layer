package neo

import (
	"context"
	"fmt"
	
	"github.com/Jim8y/neo-service-layer/internal/common/logger"
	"github.com/neo-project/neo-go/pkg/crypto/keys"
	"github.com/neo-project/neo-go/pkg/wallet"
)

// SendTransaction sends a transaction to the Neo N3 blockchain
func (c *Client) SendTransaction(txHex string) (string, error) {
	ctx, cancel := context.WithTimeout(context.Background(), c.timeout)
	defer cancel()
	
	txHash, err := c.rpcClient.SendRawTransaction(ctx, txHex)
	if err != nil {
		return "", fmt.Errorf("failed to send transaction: %w", err)
	}
	
	c.log.Info("Transaction sent", 
		logger.Field{Key: "tx_hash", Value: txHash},
	)
	
	return txHash, nil
}

// GenerateKeyPair generates a new key pair for Neo N3
func GenerateKeyPair() (*wallet.PrivateKey, error) {
	privateKey, err := wallet.NewPrivateKey()
	if err != nil {
		return nil, fmt.Errorf("failed to generate key pair: %w", err)
	}
	
	return privateKey, nil
}

// GetWalletFromWIF returns a wallet from a WIF string
func GetWalletFromWIF(wif string) (*wallet.Account, error) {
	privateKey, err := keys.NewPrivateKeyFromWIF(wif)
	if err != nil {
		return nil, fmt.Errorf("failed to parse WIF: %w", err)
	}
	
	account, err := wallet.NewAccountFromPrivateKey(privateKey)
	if err != nil {
		return nil, fmt.Errorf("failed to create account: %w", err)
	}
	
	return account, nil
}

// GetAddressFromPublicKey returns a Neo N3 address from a public key
func GetAddressFromPublicKey(pubKey string) (string, error) {
	key, err := keys.NewPublicKeyFromString(pubKey)
	if err != nil {
		return "", fmt.Errorf("failed to parse public key: %w", err)
	}
	
	address := key.Address()
	return address, nil
}