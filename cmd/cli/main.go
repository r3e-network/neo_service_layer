package main

import (
	"fmt"
	"os"

	"github.com/will/neo_service_layer/cmd/cli/commands"
)

func main() {
	if err := commands.Execute(); err != nil {
		fmt.Fprintf(os.Stderr, "Error: %v\n", err)
		os.Exit(1)
	}
}