package neo

import (
	"context"
	"fmt"

	"github.com/nspcc-dev/neo-go/pkg/crypto/hash"
	"github.com/nspcc-dev/neo-go/pkg/smartcontract"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/vm/opcode"
)

// ContractManager handles smart contract interactions
type ContractManager struct {
	client    *Client
	txManager *TransactionManager
}

// NewContractManager creates a new contract manager
func NewContractManager(client *Client, txManager *TransactionManager) *ContractManager {
	return &ContractManager{
		client:    client,
		txManager: txManager,
	}
}

// DeployContract deploys a new contract
func (cm *ContractManager) DeployContract(
	ctx context.Context,
	nefFile []byte,
	manifest []byte,
	signer *Signer,
) (util.Uint160, error) {
	if len(nefFile) == 0 || len(manifest) == 0 {
		return util.Uint160{}, fmt.Errorf("empty contract data")
	}

	// Create deployment script
	script := []byte{byte(opcode.PUSH0), byte(opcode.NEWARRAY), byte(opcode.PUSH0)}
	script = append(script, nefFile...)
	script = append(script, manifest...)

	// Create and send transaction
	tx, err := cm.txManager.CreateTransaction(script, []Signer{*signer})
	if err != nil {
		return util.Uint160{}, fmt.Errorf("failed to create transaction: %w", err)
	}

	result, err := cm.txManager.SendTransaction(ctx, tx)
	if err != nil {
		return util.Uint160{}, fmt.Errorf("failed to send transaction: %w", err)
	}

	if !result.Success {
		return util.Uint160{}, fmt.Errorf("contract deployment failed")
	}

	// Calculate contract hash
	scriptHash := hash.Hash160(script)
	return scriptHash, nil
}

// InvokeContract invokes a contract method
func (cm *ContractManager) InvokeContract(
	ctx context.Context,
	hash util.Uint160,
	method string,
	params []ContractParameter,
	signers []Signer,
) (*TransactionResult, error) {
	// Build invocation script
	script := []byte{}

	// Add parameters in reverse order
	for i := len(params) - 1; i >= 0; i-- {
		param := params[i]
		paramScript, err := createParameterScript(param)
		if err != nil {
			return nil, fmt.Errorf("failed to create parameter script: %w", err)
		}
		script = append(script, paramScript...)
	}

	// Add method name and syscall
	script = append(script, []byte(method)...)
	script = append(script, byte(opcode.SYSCALL))
	callScript, err := smartcontract.CreateCallScript(hash, method, params)
	if err != nil {
		return nil, fmt.Errorf("failed to create call script: %w", err)
	}
	script = append(script, callScript...)

	// Create and send transaction
	tx, err := cm.txManager.CreateTransaction(script, signers)
	if err != nil {
		return nil, fmt.Errorf("failed to create transaction: %w", err)
	}

	result, err := cm.txManager.SendTransaction(ctx, tx)
	if err != nil {
		return nil, fmt.Errorf("failed to send transaction: %w", err)
	}

	return result, nil
}

// createParameterScript creates a script for a contract parameter
func createParameterScript(param ContractParameter) ([]byte, error) {
	script := []byte{}
	switch param.Type {
	case "String":
		script = append(script, []byte(param.Value.(string))...)
		script = append(script, byte(opcode.PUSHDATA1))
		script = append(script, byte(len(param.Value.(string))))
	case "Integer":
		val := param.Value.(int64)
		script = append(script, byte(opcode.PUSH1))
		script = append(script, byte(val))
	case "Boolean":
		if param.Value.(bool) {
			script = append(script, byte(opcode.PUSHT))
		} else {
			script = append(script, byte(opcode.PUSHF))
		}
	case "ByteArray":
		data := param.Value.([]byte)
		script = append(script, byte(opcode.PUSHDATA1))
		script = append(script, byte(len(data)))
		script = append(script, data...)
	case "Array":
		array := param.Value.([]ContractParameter)
		for i := len(array) - 1; i >= 0; i-- {
			paramScript, err := createParameterScript(array[i])
			if err != nil {
				return nil, err
			}
			script = append(script, paramScript...)
		}
		script = append(script, byte(opcode.PACK))
		script = append(script, byte(len(array)))
	default:
		return nil, fmt.Errorf("unsupported parameter type: %s", param.Type)
	}
	return script, nil
}
