package commands

import (
	"fmt"

	"github.com/spf13/cobra"
	"github.com/will/neo_service_layer/internal/common/logger"
)

var pricefeedsLogger = logger.NewLogger("info")

var pricefeedsCmd = &cobra.Command{
	Use:   "pricefeeds",
	Short: "Manage price feeds",
	Long:  `Configure and monitor price feed sources.`,
}

var addFeedCmd = &cobra.Command{
	Use:   "add [name] [url]",
	Short: "Add a new price feed",
	Long:  `Add a new price feed source with a name and URL.`,
	Args:  cobra.ExactArgs(2),
	Run: func(cmd *cobra.Command, args []string) {
		pricefeedsLogger.Info("Adding price feed...", map[string]interface{}{
			"name": args[0],
			"url":  args[1],
		})
		// Implementation goes here
		fmt.Println("Price feed added successfully")
	},
}

var removeFeedCmd = &cobra.Command{
	Use:   "remove [name]",
	Short: "Remove a price feed",
	Long:  `Remove an existing price feed source.`,
	Args:  cobra.ExactArgs(1),
	Run: func(cmd *cobra.Command, args []string) {
		pricefeedsLogger.Info("Removing price feed...", map[string]interface{}{
			"name": args[0],
		})
		// Implementation goes here
		fmt.Println("Price feed removed successfully")
	},
}

var listFeedsCmd = &cobra.Command{
	Use:   "list",
	Short: "List price feeds",
	Long:  `List all configured price feed sources.`,
	Run: func(cmd *cobra.Command, args []string) {
		pricefeedsLogger.Info("Listing price feeds...", nil)
		// Implementation goes here
		fmt.Println("Price feeds listed successfully")
	},
}

func init() {
	pricefeedsCmd.AddCommand(addFeedCmd)
	pricefeedsCmd.AddCommand(removeFeedCmd)
	pricefeedsCmd.AddCommand(listFeedsCmd)
}
