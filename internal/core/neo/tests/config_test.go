package neo_test

import (
	"os"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/core/neo"
)

// TestConfig provides test configuration
var TestConfig = &neo.Config{
	NodeURLs: []string{
		os.Getenv("NEO_TEST_NODE_URL"),
		"http://localhost:10332", // fallback
	},
	NetworkMagic: 769, // Neo N3 TestNet magic number
	MaxRetries:   3,
	RetryDelay:   time.Second,
}

// TestContractNEF is a simple test contract NEF file
var TestContractNEF = []byte(`
00000000    NEF3 neo-core-v3.5    0000
00000010    test-contract         0000
00000020    {
00000030        "methods": [
00000040            {
00000050                "name": "test",
00000060                "parameters": [
00000070                    {
00000080                        "name": "value",
00000090                        "type": "String"
00000100                    }
00000110                ],
00000120                "returntype": "String"
00000130            }
00000140        ]
00000150    }
`)

// TestContractManifest is a simple test contract manifest
var TestContractManifest = []byte(`{
    "name": "TestContract",
    "groups": [],
    "features": {},
    "supportedstandards": [],
    "abi": {
        "methods": [
            {
                "name": "test",
                "parameters": [
                    {
                        "name": "value",
                        "type": "String"
                    }
                ],
                "returntype": "String",
                "offset": 0
            }
        ],
        "events": []
    },
    "permissions": [
        {
            "contract": "*",
            "methods": "*"
        }
    ],
    "trusts": [],
    "extra": null
}`)
