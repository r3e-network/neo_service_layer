package commands

import (
	"fmt"

	"github.com/spf13/cobra"
)

func newGasBankCmd() *cobra.Command {
	cmd := &cobra.Command{
		Use:   "gasbank",
		Short: "Manage gas bank",
		Long:  `Monitor and manage gas bank allocations and usage`,
	}

	// Add subcommands
	cmd.AddCommand(
		newGasBankStatusCmd(),
		newGasBankDepositCmd(),
		newGasBankWithdrawCmd(),
		newGasBankAllocationsCmd(),
		newGasBankUsageCmd(),
	)

	return cmd
}

func newGasBankStatusCmd() *cobra.Command {
	return &cobra.Command{
		Use:   "status",
		Short: "Show gas bank status",
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := getConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := getLogger()
			log.Info("Getting gas bank status")

			// TODO: Implement gas bank status retrieval
			return fmt.Errorf("not implemented")
		},
	}
}

func newGasBankDepositCmd() *cobra.Command {
	var (
		amount float64
	)

	cmd := &cobra.Command{
		Use:   "deposit",
		Short: "Deposit GAS into the bank",
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := getConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := getLogger()
			log.Info("Depositing GAS", map[string]interface{}{
				"amount": amount,
			})

			// TODO: Implement gas deposit
			return fmt.Errorf("not implemented")
		},
	}

	cmd.Flags().Float64Var(&amount, "amount", 0, "Amount of GAS to deposit")
	cmd.MarkFlagRequired("amount")

	return cmd
}

func newGasBankWithdrawCmd() *cobra.Command {
	var (
		amount float64
		to     string
	)

	cmd := &cobra.Command{
		Use:   "withdraw",
		Short: "Withdraw GAS from the bank",
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := getConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := getLogger()
			log.Info("Withdrawing GAS", map[string]interface{}{
				"amount": amount,
				"to":     to,
			})

			// TODO: Implement gas withdrawal
			return fmt.Errorf("not implemented")
		},
	}

	cmd.Flags().Float64Var(&amount, "amount", 0, "Amount of GAS to withdraw")
	cmd.Flags().StringVar(&to, "to", "", "Address to withdraw GAS to")
	cmd.MarkFlagRequired("amount")
	cmd.MarkFlagRequired("to")

	return cmd
}

func newGasBankAllocationsCmd() *cobra.Command {
	return &cobra.Command{
		Use:   "allocations",
		Short: "List current gas allocations",
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := getConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := getLogger()
			log.Info("Listing gas allocations")

			// TODO: Implement allocation listing
			return fmt.Errorf("not implemented")
		},
	}
}

func newGasBankUsageCmd() *cobra.Command {
	var (
		days  int
		user  string
		limit int
	)

	cmd := &cobra.Command{
		Use:   "usage",
		Short: "Show gas usage history",
		RunE: func(cmd *cobra.Command, args []string) error {
			cfg, err := getConfig()
			if err != nil {
				return fmt.Errorf("failed to load config: %w", err)
			}

			log := getLogger()
			log.Info("Getting gas usage", map[string]interface{}{
				"days":  days,
				"user":  user,
				"limit": limit,
			})

			// TODO: Implement usage history retrieval
			return fmt.Errorf("not implemented")
		},
	}

	cmd.Flags().IntVar(&days, "days", 7, "Number of days of history to show")
	cmd.Flags().StringVar(&user, "user", "", "Filter usage by user address")
	cmd.Flags().IntVar(&limit, "limit", 100, "Maximum number of records to return")

	return cmd
}