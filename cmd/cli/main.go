package main

import (
	"fmt"
	"os"

	"github.com/r3e-network/neo_service_layer/cmd/cli/commands"
)

func main() {
	if err := commands.Execute(); err != nil {
		fmt.Fprintf(os.Stderr, "Error: %v\n", err)
		os.Exit(1)
	}
}
