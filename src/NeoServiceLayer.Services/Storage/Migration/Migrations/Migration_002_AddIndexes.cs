using System;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Storage.Migration;
using NeoServiceLayer.Services.Storage.Providers;

namespace NeoServiceLayer.Services.Storage.Migration.Migrations
{
    /// <summary>
    /// Migration to add indexes to collections
    /// </summary>
    public class Migration_002_AddIndexes : BaseMigration
    {
        /// <inheritdoc/>
        public override string Name => "AddIndexes";

        /// <inheritdoc/>
        public override int Version => 2;

        /// <inheritdoc/>
        public override string Description => "Adds indexes to collections for improved performance";

        /// <inheritdoc/>
        public override async Task UpAsync(IStorageProvider provider)
        {
            // Only MongoDB provider supports indexes
            if (provider is MongoDbStorageProvider mongoProvider)
            {
                // Add indexes to accounts collection
                await mongoProvider.CreateIndexAsync("accounts", "AccountId", true);
                await mongoProvider.CreateIndexAsync("accounts", "Email", true);
                await mongoProvider.CreateIndexAsync("accounts", "Username", true);

                // Add indexes to wallets collection
                await mongoProvider.CreateIndexAsync("wallets", "AccountId", false);
                await mongoProvider.CreateIndexAsync("wallets", "Address", true);

                // Add indexes to secrets collection
                await mongoProvider.CreateIndexAsync("secrets", "AccountId", false);
                await mongoProvider.CreateIndexAsync("secrets", "Name", false);

                // Add indexes to functions collection
                await mongoProvider.CreateIndexAsync("functions", "AccountId", false);
                await mongoProvider.CreateIndexAsync("functions", "Name", false);

                // Add indexes to prices collection
                await mongoProvider.CreateIndexAsync("prices", "Symbol", false);
                await mongoProvider.CreateIndexAsync("prices", "BaseCurrency", false);
                await mongoProvider.CreateIndexAsync("prices", "Timestamp", false);

                // Add indexes to gasbank_accounts collection
                await mongoProvider.CreateIndexAsync("gasbank_accounts", "AccountId", false);
                await mongoProvider.CreateIndexAsync("gasbank_accounts", "Name", false);

                // Add indexes to event_subscriptions collection
                await mongoProvider.CreateIndexAsync("event_subscriptions", "AccountId", false);
                await mongoProvider.CreateIndexAsync("event_subscriptions", "EventType", false);

                // Add indexes to notifications collection
                await mongoProvider.CreateIndexAsync("notifications", "AccountId", false);
                await mongoProvider.CreateIndexAsync("notifications", "CreatedAt", false);
            }
            else
            {
                // Skip for other providers
                Console.WriteLine("Skipping index creation for non-MongoDB provider");
            }
        }

        /// <inheritdoc/>
        public override async Task DownAsync(IStorageProvider provider)
        {
            // Only MongoDB provider supports indexes
            if (provider is MongoDbStorageProvider mongoProvider)
            {
                // Drop indexes from accounts collection
                await mongoProvider.DropIndexAsync("accounts", "AccountId");
                await mongoProvider.DropIndexAsync("accounts", "Email");
                await mongoProvider.DropIndexAsync("accounts", "Username");

                // Drop indexes from wallets collection
                await mongoProvider.DropIndexAsync("wallets", "AccountId");
                await mongoProvider.DropIndexAsync("wallets", "Address");

                // Drop indexes from secrets collection
                await mongoProvider.DropIndexAsync("secrets", "AccountId");
                await mongoProvider.DropIndexAsync("secrets", "Name");

                // Drop indexes from functions collection
                await mongoProvider.DropIndexAsync("functions", "AccountId");
                await mongoProvider.DropIndexAsync("functions", "Name");

                // Drop indexes from prices collection
                await mongoProvider.DropIndexAsync("prices", "Symbol");
                await mongoProvider.DropIndexAsync("prices", "BaseCurrency");
                await mongoProvider.DropIndexAsync("prices", "Timestamp");

                // Drop indexes from gasbank_accounts collection
                await mongoProvider.DropIndexAsync("gasbank_accounts", "AccountId");
                await mongoProvider.DropIndexAsync("gasbank_accounts", "Name");

                // Drop indexes from event_subscriptions collection
                await mongoProvider.DropIndexAsync("event_subscriptions", "AccountId");
                await mongoProvider.DropIndexAsync("event_subscriptions", "EventType");

                // Drop indexes from notifications collection
                await mongoProvider.DropIndexAsync("notifications", "AccountId");
                await mongoProvider.DropIndexAsync("notifications", "CreatedAt");
            }
            else
            {
                // Skip for other providers
                Console.WriteLine("Skipping index removal for non-MongoDB provider");
            }
        }
    }
}
