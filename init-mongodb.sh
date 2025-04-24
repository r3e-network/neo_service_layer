#!/bin/bash

echo "Initializing MongoDB..."

# Wait for MongoDB to be ready
until mongosh --host mongodb --eval "print(\"waited for connection\")"
do
    echo "Waiting for MongoDB to be ready..."
    sleep 2
done

# Create the database and collections
mongosh --host mongodb <<EOFMONGO
use neo_service_layer;

// Create collections
db.createCollection("accounts");
db.createCollection("wallets");
db.createCollection("secrets");
db.createCollection("functions");
db.createCollection("function_executions");
db.createCollection("function_logs");
db.createCollection("function_templates");
db.createCollection("function_tests");
db.createCollection("function_test_results");
db.createCollection("function_test_suites");
db.createCollection("function_permissions");
db.createCollection("function_access_policies");
db.createCollection("function_access_requests");
db.createCollection("function_marketplace_items");
db.createCollection("function_marketplace_reviews");
db.createCollection("function_marketplace_purchases");
db.createCollection("function_compositions");
db.createCollection("function_composition_executions");
db.createCollection("prices");
db.createCollection("price_sources");
db.createCollection("price_history");
db.createCollection("price_feeds");
db.createCollection("events");
db.createCollection("event_subscriptions");
db.createCollection("notifications");
db.createCollection("notification_templates");
db.createCollection("user_notification_preferences");
db.createCollection("metrics");
db.createCollection("analytics_events");
db.createCollection("dashboards");
db.createCollection("reports");
db.createCollection("alerts");
db.createCollection("migrations");

// Create indexes
db.accounts.createIndex({ "email": 1 }, { unique: true });
db.wallets.createIndex({ "accountId": 1 });
db.secrets.createIndex({ "accountId": 1 });
db.functions.createIndex({ "accountId": 1 });
db.function_executions.createIndex({ "functionId": 1 });
db.function_logs.createIndex({ "executionId": 1 });
db.function_templates.createIndex({ "name": 1 });
db.prices.createIndex({ "symbol": 1, "timestamp": -1 });
db.price_feeds.createIndex({ "symbol": 1 });
db.events.createIndex({ "contractHash": 1, "eventName": 1 });
db.notifications.createIndex({ "accountId": 1, "status": 1 });
db.migrations.createIndex({ "version": 1 }, { unique: true });

// Insert initial migration records
db.migrations.insertOne({ "version": 1, "name": "InitialSchema", "appliedAt": new Date() });
db.migrations.insertOne({ "version": 2, "name": "AddIndexes", "appliedAt": new Date() });

print("MongoDB initialization completed successfully!");
EOFMONGO

echo "MongoDB initialization completed!"
