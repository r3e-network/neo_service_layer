package commands

import (
	"encoding/hex"
	"encoding/json"
	"fmt"
	"strings"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/common/retry"
	"github.com/r3e-network/neo_service_layer/internal/common/types"
	"github.com/spf13/cobra"
)

// validateContractHash validates a contract hash string
func validateContractHash(hash string) error {
	if len(hash) != 40 {
		return fmt.Errorf("invalid contract hash length: expected 40 characters, got %d", len(hash))
	}
	if _, err := hex.DecodeString(hash); err != nil {
		return fmt.Errorf("invalid contract hash format: %w", err)
	}
	return nil
}

func newTriggersCmd() *cobra.Command {
	cmd := &cobra.Command{
		Use:   "triggers",
		Short: "Manage function triggers",
		Long:  `Create and manage triggers that invoke functions based on events, schedules, or contract conditions`,
	}

	// Add subcommands
	cmd.AddCommand(
		newTriggerCreateCmd(),
		newTriggerListCmd(),
		newTriggerDeleteCmd(),
		newTriggerEnableCmd(),
		newTriggerDisableCmd(),
		newTriggerHistoryCmd(),
		newTriggerStatusCmd(),
	)

	return cmd
}

func newTriggerCreateCmd() *cobra.Command {
	var (
		name         string
		functionName string
		schedule     string
		contract     string
		event        string
		method       string
		condition    string
		retryPolicy  string
		enabled      bool
		maxGas       float64
		params       string
	)

	cmd := &cobra.Command{
		Use:   "create",
		Short: "Create a new trigger",
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := GetConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := GetLogger()

			// Validate contract hash if provided
			if contract != "" {
				if err := validateContractHash(contract); err != nil {
					return fmt.Errorf("invalid contract hash: %w", err)
				}
			}

			// Parse parameters if provided
			var parameters map[string]interface{}
			if params != "" {
				if err := json.Unmarshal([]byte(params), &parameters); err != nil {
					return fmt.Errorf("invalid parameters JSON: %w", err)
				}
			}

			// Create trigger request
			req := &types.CreateTriggerRequest{
				Name:         name,
				FunctionName: functionName,
				Schedule:     schedule,
				Contract:     contract,
				Event:        event,
				Method:       method,
				Condition:    condition,
				RetryPolicy:  retry.Policy(retryPolicy),
				Enabled:      enabled,
				MaxGas:       maxGas,
				Parameters:   parameters,
			}

			// Validate request
			if err := req.Validate(); err != nil {
				return fmt.Errorf("invalid trigger configuration: %w", err)
			}

			log.Info("Creating trigger", map[string]interface{}{
				"name":         name,
				"functionName": functionName,
				"schedule":     schedule,
				"contract":     contract,
				"event":        event,
				"method":       method,
				"condition":    condition,
				"retryPolicy":  retryPolicy,
				"enabled":      enabled,
				"maxGas":       maxGas,
			})

			// Create trigger using triggers service
			client := triggers.NewClient(cfg.API.Endpoint)
			trigger, err := client.CreateTrigger(cmd.Context(), req)
			if err != nil {
				return fmt.Errorf("failed to create trigger: %w", err)
			}

			// Print created trigger details
			fmt.Printf("Successfully created trigger:\n")
			fmt.Printf("  ID: %s\n", trigger.ID)
			fmt.Printf("  Name: %s\n", trigger.Name)
			fmt.Printf("  Function: %s\n", trigger.FunctionName)
			if trigger.Schedule != "" {
				fmt.Printf("  Schedule: %s\n", trigger.Schedule)
			}
			if trigger.Contract != "" {
				fmt.Printf("  Contract: %s\n", trigger.Contract)
				if trigger.Event != "" {
					fmt.Printf("  Event: %s\n", trigger.Event)
				}
				if trigger.Method != "" {
					fmt.Printf("  Method: %s\n", trigger.Method)
				}
				if trigger.Condition != "" {
					fmt.Printf("  Condition: %s\n", trigger.Condition)
				}
			}
			fmt.Printf("  Status: %s\n", trigger.Status)

			return nil
		},
	}

	cmd.Flags().StringVar(&name, "name", "", "Trigger name")
	cmd.Flags().StringVar(&functionName, "function", "", "Name of function to trigger")
	cmd.Flags().StringVar(&schedule, "schedule", "", "Cron schedule expression")
	cmd.Flags().StringVar(&contract, "contract", "", "Contract hash to watch (40 characters hex)")
	cmd.Flags().StringVar(&event, "event", "", "Contract event name to watch for")
	cmd.Flags().StringVar(&method, "method", "", "Contract method to check")
	cmd.Flags().StringVar(&condition, "condition", "", "Condition expression for method check")
	cmd.Flags().StringVar(&retryPolicy, "retry", string(retry.PolicyExponential),
		fmt.Sprintf("Retry policy (%s)", strings.Join(retry.ValidPolicies(), ", ")))
	cmd.Flags().BoolVar(&enabled, "enabled", true, "Whether trigger should be enabled immediately")
	cmd.Flags().Float64Var(&maxGas, "max-gas", 10, "Maximum GAS to use per execution")
	cmd.Flags().StringVar(&params, "params", "", "Function parameters as JSON")

	cmd.MarkFlagRequired("name")
	cmd.MarkFlagRequired("function")

	return cmd
}

func newTriggerListCmd() *cobra.Command {
	var (
		status string
		limit  int
		format string
	)

	cmd := &cobra.Command{
		Use:   "list",
		Short: "List all triggers",
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := GetConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := GetLogger()
			log.Info("Listing triggers", map[string]interface{}{
				"status": status,
				"limit":  limit,
			})

			// Get triggers using triggers service
			client := triggers.NewClient(cfg.API.Endpoint)
			triggers, err := client.ListTriggers(cmd.Context(), &types.ListTriggersRequest{
				Status: status,
				Limit:  limit,
			})
			if err != nil {
				return fmt.Errorf("failed to list triggers: %w", err)
			}

			// Format output
			switch format {
			case "json":
				out, err := json.MarshalIndent(triggers, "", "  ")
				if err != nil {
					return fmt.Errorf("failed to format output: %w", err)
				}
				fmt.Println(string(out))
			default:
				if len(triggers) == 0 {
					fmt.Println("No triggers found")
					return nil
				}

				fmt.Printf("%-36s %-20s %-15s %-20s %-10s\n", "ID", "NAME", "FUNCTION", "NEXT EXECUTION", "STATUS")
				for _, t := range triggers {
					nextExec := "N/A"
					if !t.NextExecution.IsZero() {
						nextExec = t.NextExecution.Format(time.RFC3339)
					}
					fmt.Printf("%-36s %-20s %-15s %-20s %-10s\n",
						t.ID, t.Name, t.FunctionName, nextExec, t.Status)
				}
			}

			return nil
		},
	}

	cmd.Flags().StringVar(&status, "status", "", "Filter by trigger status (active, paused)")
	cmd.Flags().IntVar(&limit, "limit", 100, "Maximum number of triggers to list")
	cmd.Flags().StringVar(&format, "format", "table", "Output format (table, json)")

	return cmd
}

func newTriggerDeleteCmd() *cobra.Command {
	var force bool

	cmd := &cobra.Command{
		Use:   "delete [name]",
		Short: "Delete a trigger",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := GetConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := GetLogger()
			log.Info("Deleting trigger", map[string]interface{}{
				"name":  args[0],
				"force": force,
			})

			// Delete trigger using triggers service
			client := triggers.NewClient(cfg.API.Endpoint)
			if err := client.DeleteTrigger(cmd.Context(), args[0], force); err != nil {
				return fmt.Errorf("failed to delete trigger: %w", err)
			}

			fmt.Printf("Successfully deleted trigger '%s'\n", args[0])
			return nil
		},
	}

	cmd.Flags().BoolVar(&force, "force", false, "Force deletion even if trigger is active")

	return cmd
}

func newTriggerEnableCmd() *cobra.Command {
	return &cobra.Command{
		Use:   "enable [name]",
		Short: "Enable a trigger",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := GetConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := GetLogger()
			log.Info("Enabling trigger", map[string]interface{}{
				"name": args[0],
			})

			// Enable trigger using triggers service
			client := triggers.NewClient(cfg.API.Endpoint)
			if err := client.UpdateTriggerStatus(cmd.Context(), args[0], true); err != nil {
				return fmt.Errorf("failed to enable trigger: %w", err)
			}

			fmt.Printf("Successfully enabled trigger '%s'\n", args[0])
			return nil
		},
	}
}

func newTriggerDisableCmd() *cobra.Command {
	return &cobra.Command{
		Use:   "disable [name]",
		Short: "Disable a trigger",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := GetConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := GetLogger()
			log.Info("Disabling trigger", map[string]interface{}{
				"name": args[0],
			})

			// Disable trigger using triggers service
			client := triggers.NewClient(cfg.API.Endpoint)
			if err := client.UpdateTriggerStatus(cmd.Context(), args[0], false); err != nil {
				return fmt.Errorf("failed to disable trigger: %w", err)
			}

			fmt.Printf("Successfully disabled trigger '%s'\n", args[0])
			return nil
		},
	}
}

func newTriggerHistoryCmd() *cobra.Command {
	var (
		limit  int
		format string
		since  string
	)

	cmd := &cobra.Command{
		Use:   "history [name]",
		Short: "Show trigger execution history",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := GetConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := GetLogger()

			// Parse since duration
			var sinceDuration time.Duration
			if since != "" {
				sinceDuration, err = time.ParseDuration(since)
				if err != nil {
					return fmt.Errorf("invalid since duration: %w", err)
				}
			}

			log.Info("Getting trigger history", map[string]interface{}{
				"name":   args[0],
				"limit":  limit,
				"since":  since,
				"format": format,
			})

			// Get trigger history using triggers service
			client := triggers.NewClient(cfg.API.Endpoint)
			history, err := client.GetTriggerHistory(cmd.Context(), args[0], &types.GetTriggerHistoryRequest{
				Limit: limit,
				Since: sinceDuration,
			})
			if err != nil {
				return fmt.Errorf("failed to get trigger history: %w", err)
			}

			// Format output
			switch format {
			case "json":
				out, err := json.MarshalIndent(history, "", "  ")
				if err != nil {
					return fmt.Errorf("failed to format output: %w", err)
				}
				fmt.Println(string(out))
			default:
				if len(history) == 0 {
					fmt.Println("No execution history found")
					return nil
				}

				fmt.Printf("%-24s %-10s %-15s %-10s\n", "TIMESTAMP", "STATUS", "GAS USED", "TX HASH")
				for _, h := range history {
					fmt.Printf("%-24s %-10s %-15.8f %-10s\n",
						h.Timestamp.Format(time.RFC3339),
						h.Status,
						h.GasUsed,
						h.TxHash)
				}
			}

			return nil
		},
	}

	cmd.Flags().IntVar(&limit, "limit", 100, "Maximum number of records to show")
	cmd.Flags().StringVar(&format, "format", "table", "Output format (table, json)")
	cmd.Flags().StringVar(&since, "since", "24h", "Show history since duration ago")

	return cmd
}

func newTriggerStatusCmd() *cobra.Command {
	var format string

	cmd := &cobra.Command{
		Use:   "status [name]",
		Short: "Show trigger status",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := GetConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := GetLogger()
			log.Info("Getting trigger status", map[string]interface{}{
				"name": args[0],
			})

			// Get trigger status using triggers service
			client := triggers.NewClient(cfg.API.Endpoint)
			status, err := client.GetTriggerStatus(cmd.Context(), args[0])
			if err != nil {
				return fmt.Errorf("failed to get trigger status: %w", err)
			}

			// Format output
			switch format {
			case "json":
				out, err := json.MarshalIndent(status, "", "  ")
				if err != nil {
					return fmt.Errorf("failed to format output: %w", err)
				}
				fmt.Println(string(out))
			default:
				fmt.Printf("Name: %s\n", status.Name)
				fmt.Printf("Status: %s\n", status.Status)
				fmt.Printf("Last Execution: %s\n", status.LastExecution.Format(time.RFC3339))
				fmt.Printf("Next Execution: %s\n", status.NextExecution.Format(time.RFC3339))
				fmt.Printf("Success Count: %d\n", status.SuccessCount)
				fmt.Printf("Error Count: %d\n", status.ErrorCount)
				fmt.Printf("Average Gas Used: %.8f\n", status.AverageGasUsed)
				if status.LastError != "" {
					fmt.Printf("Last Error: %s\n", status.LastError)
				}
			}

			return nil
		},
	}

	cmd.Flags().StringVar(&format, "format", "text", "Output format (text, json)")

	return cmd
}
