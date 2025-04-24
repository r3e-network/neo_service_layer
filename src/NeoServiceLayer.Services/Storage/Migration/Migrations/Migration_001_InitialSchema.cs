using System.Threading.Tasks;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Storage.Migration;

namespace NeoServiceLayer.Services.Storage.Migration.Migrations
{
    /// <summary>
    /// Initial schema migration
    /// </summary>
    public class Migration_001_InitialSchema : BaseMigration
    {
        /// <inheritdoc/>
        public override string Name => "InitialSchema";

        /// <inheritdoc/>
        public override int Version => 1;

        /// <inheritdoc/>
        public override string Description => "Creates the initial database schema";

        /// <inheritdoc/>
        public override async Task UpAsync(IStorageProvider provider)
        {
            // Create collections
            await CreateCollectionIfNotExistsAsync(provider, "accounts");
            await CreateCollectionIfNotExistsAsync(provider, "account_settings");
            await CreateCollectionIfNotExistsAsync(provider, "account_roles");
            await CreateCollectionIfNotExistsAsync(provider, "account_permissions");
            
            await CreateCollectionIfNotExistsAsync(provider, "wallets");
            await CreateCollectionIfNotExistsAsync(provider, "wallet_transactions");
            
            await CreateCollectionIfNotExistsAsync(provider, "secrets");
            await CreateCollectionIfNotExistsAsync(provider, "secret_versions");
            
            await CreateCollectionIfNotExistsAsync(provider, "functions");
            await CreateCollectionIfNotExistsAsync(provider, "function_versions");
            await CreateCollectionIfNotExistsAsync(provider, "function_executions");
            await CreateCollectionIfNotExistsAsync(provider, "function_logs");
            
            await CreateCollectionIfNotExistsAsync(provider, "price_sources");
            await CreateCollectionIfNotExistsAsync(provider, "prices");
            await CreateCollectionIfNotExistsAsync(provider, "price_history");
            
            await CreateCollectionIfNotExistsAsync(provider, "gasbank_accounts");
            await CreateCollectionIfNotExistsAsync(provider, "gasbank_allocations");
            await CreateCollectionIfNotExistsAsync(provider, "gasbank_transactions");
            
            await CreateCollectionIfNotExistsAsync(provider, "event_subscriptions");
            await CreateCollectionIfNotExistsAsync(provider, "event_logs");
            
            await CreateCollectionIfNotExistsAsync(provider, "notifications");
            await CreateCollectionIfNotExistsAsync(provider, "notification_templates");
            await CreateCollectionIfNotExistsAsync(provider, "user_notification_preferences");
            
            await CreateCollectionIfNotExistsAsync(provider, "metrics");
            await CreateCollectionIfNotExistsAsync(provider, "events");
            await CreateCollectionIfNotExistsAsync(provider, "dashboards");
            await CreateCollectionIfNotExistsAsync(provider, "reports");
            await CreateCollectionIfNotExistsAsync(provider, "alerts");
        }

        /// <inheritdoc/>
        public override async Task DownAsync(IStorageProvider provider)
        {
            // Delete collections
            await DeleteCollectionIfExistsAsync(provider, "accounts");
            await DeleteCollectionIfExistsAsync(provider, "account_settings");
            await DeleteCollectionIfExistsAsync(provider, "account_roles");
            await DeleteCollectionIfExistsAsync(provider, "account_permissions");
            
            await DeleteCollectionIfExistsAsync(provider, "wallets");
            await DeleteCollectionIfExistsAsync(provider, "wallet_transactions");
            
            await DeleteCollectionIfExistsAsync(provider, "secrets");
            await DeleteCollectionIfExistsAsync(provider, "secret_versions");
            
            await DeleteCollectionIfExistsAsync(provider, "functions");
            await DeleteCollectionIfExistsAsync(provider, "function_versions");
            await DeleteCollectionIfExistsAsync(provider, "function_executions");
            await DeleteCollectionIfExistsAsync(provider, "function_logs");
            
            await DeleteCollectionIfExistsAsync(provider, "price_sources");
            await DeleteCollectionIfExistsAsync(provider, "prices");
            await DeleteCollectionIfExistsAsync(provider, "price_history");
            
            await DeleteCollectionIfExistsAsync(provider, "gasbank_accounts");
            await DeleteCollectionIfExistsAsync(provider, "gasbank_allocations");
            await DeleteCollectionIfExistsAsync(provider, "gasbank_transactions");
            
            await DeleteCollectionIfExistsAsync(provider, "event_subscriptions");
            await DeleteCollectionIfExistsAsync(provider, "event_logs");
            
            await DeleteCollectionIfExistsAsync(provider, "notifications");
            await DeleteCollectionIfExistsAsync(provider, "notification_templates");
            await DeleteCollectionIfExistsAsync(provider, "user_notification_preferences");
            
            await DeleteCollectionIfExistsAsync(provider, "metrics");
            await DeleteCollectionIfExistsAsync(provider, "events");
            await DeleteCollectionIfExistsAsync(provider, "dashboards");
            await DeleteCollectionIfExistsAsync(provider, "reports");
            await DeleteCollectionIfExistsAsync(provider, "alerts");
        }
    }
}
