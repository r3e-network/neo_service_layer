using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.ComponentModel;
using System.Numerics; // Required for BigInteger

namespace Neo.Contracts.Automation
{
    [DisplayName("UpkeepContract_TimeBased")] // Added suffix for clarity
    [ManifestExtra("Author", "Neo Service Layer Example")]
    [ManifestExtra("Description", "A time-based upkeep contract for Neo Service Layer Automation.")]
    // Add necessary permissions if performUpkeep calls other contracts
    // [ContractPermission("*", "someMethod")]
    public class UpkeepContract : SmartContract
    {
        // --- Storage Prefixes ---
        // Use unique prefixes for your contract to avoid collisions
        private static readonly StorageMap AuthorizedAddressMap = new(Storage.CurrentContext, 0x01);
        private static readonly StorageMap OwnerMap = new(Storage.CurrentContext, 0x02);
        private static readonly StorageMap LastRunTimeMap = new(Storage.CurrentContext, 0x10);
        private static readonly StorageMap RunIntervalMap = new(Storage.CurrentContext, 0x11);

        // --- Constants ---
        // Default run interval if not set (e.g., 1 hour in milliseconds)
        private const ulong DefaultRunInterval = 60 * 60 * 1000;

        // --- Events ---
        [DisplayName("UpkeepPerformed")]
        public static event Action<ByteString, ByteString, bool> OnUpkeepPerformed;

        [DisplayName("RunIntervalSet")]
        public static event Action<ulong> OnRunIntervalSet;

        [DisplayName("AuthorizedAddressUpdated")]
        public static event Action<UInt160> OnAuthorizedAddressUpdated;

        // --- Deployment ---
        public static void _deploy(object data, bool update)
        {
            if (update) return;

            var owner = (UInt160)data;
            if (owner is null || !owner.IsValid)
                throw new Exception("Invalid owner address during deployment.");

            OwnerMap.Put("owner", owner);
            AuthorizedAddressMap.Put("address", UInt160.Zero); // Must be set by owner later
            RunIntervalMap.Put("interval", DefaultRunInterval); // Set default interval
            LastRunTimeMap.Put("timestamp", 0); // Initialize last run time
        }

        // --- Owner Methods ---
        public static void UpdateAuthorizedAddress(UInt160 newAuthorizedAddress)
        {
            AssertOwnerOnly();
            if (newAuthorizedAddress is null) // Allow setting to Zero explicitly
                 throw new Exception("Invalid authorized address provided (null). Use UInt160.Zero to disable.");
            // No need to check IsValid for UInt160.Zero
            if (!newAuthorizedAddress.IsZero && !newAuthorizedAddress.IsValid)
                 throw new Exception("Invalid authorized address provided.");

            AuthorizedAddressMap.Put("address", newAuthorizedAddress);
            OnAuthorizedAddressUpdated(newAuthorizedAddress);
        }

        public static void SetRunInterval(ulong intervalMilliseconds)
        {
            AssertOwnerOnly();
            if (intervalMilliseconds == 0) // Interval must be positive
                throw new Exception("Run interval must be greater than zero milliseconds.");

            RunIntervalMap.Put("interval", intervalMilliseconds);
            OnRunIntervalSet(intervalMilliseconds);
        }

        public static void UpdateContract(ByteString nefFile, string manifest, object data)
        {
            AssertOwnerOnly();
            ContractManagement.Update(nefFile, manifest, data);
        }

        public static void DestroyContract()
        {
            AssertOwnerOnly();
            ContractManagement.Destroy();
        }

        // --- Public Getters (Safe) ---
        [Safe]
        public static UInt160 GetAuthorizedAddress()
        {
            var addr = AuthorizedAddressMap.Get("address");
            return addr is null ? UInt160.Zero : (UInt160)addr;
        }

        [Safe]
        public static UInt160 GetOwner()
        {
            var owner = OwnerMap.Get("owner");
            return owner is null ? null : (UInt160)owner;
        }

        [Safe]
        public static ulong GetRunInterval()
        {
            var interval = RunIntervalMap.Get("interval");
            // Return default if not set (though _deploy sets it)
            return interval is null ? DefaultRunInterval : (ulong)(BigInteger)interval;
        }

        [Safe]
        public static ulong GetLastRunTime()
        {
            var time = LastRunTimeMap.Get("timestamp");
            return time is null ? 0 : (ulong)(BigInteger)time;
        }

        // --- Automation Methods ---
        [Safe] // checkUpkeep MUST be Safe (read-only)
        public static object[] CheckUpkeep(ByteString checkData)
        {
            // checkData is ignored in this simple time-based example, but kept for interface compliance.
            bool upkeepNeeded = false;
            ByteString performData = ByteString.Empty;

            ulong lastTime = GetLastRunTime();
            ulong interval = GetRunInterval();
            ulong currentTime = Runtime.Time; // Current block timestamp in milliseconds

            if (currentTime >= lastTime + interval)
            {
                upkeepNeeded = true;
                // Pass the current time to performUpkeep to ensure the timestamp is accurate
                performData = (ByteString)currentTime;
            }

            return new object[] { upkeepNeeded, performData };
        }

        public static void PerformUpkeep(ByteString performData)
        {
            AssertAuthorizedOnly();

            bool success = false;
            ulong triggerTime = 0;

            try
            {
                 // PerformData contains the trigger time from CheckUpkeep
                if (performData is null || performData.Length == 0) {
                    // Fallback or error, depends on desired strictness
                    triggerTime = Runtime.Time; // Use current time as fallback
                    Runtime.Log("PerformUpkeep Warning: performData (trigger time) was empty. Using Runtime.Time.");
                } else {
                     triggerTime = (ulong)(BigInteger)performData;
                }


                // --- Main Upkeep Logic ---
                // In this example, we just log and update the timestamp.
                // Replace this with your actual production logic.
                Runtime.Log($"Performing time-based upkeep. Triggered at: {triggerTime}");

                // IMPORTANT: Update the last run time using the time provided in performData
                LastRunTimeMap.Put("timestamp", triggerTime);

                // Example: Call another contract
                // UInt160 targetContract = (UInt160)StdLib.Base58CheckDecode("...");
                // Contract.Call(targetContract, "someMethod", CallFlags.All, new object[] { "arg1", 123 });

                success = true; // Mark as success if logic completes without exception
            }
            catch (Exception e)
            {
                Runtime.Log($"PerformUpkeep failed: {e.Message}");
                success = false; // Logic failed
                // Decide whether to re-throw the exception.
                // If thrown, the transaction fails, and state changes (like timestamp update) are reverted.
                // If not thrown, the transaction succeeds, but the event marks internal failure.
                // throw; // Uncomment to make the transaction fail on internal error
            }
            finally
            {
                // Emit event regardless of success/failure
                // Passing null for checkData as it wasn't used/passed through in this example.
                OnUpkeepPerformed(null, performData, success);
            }
        }

        // --- Authorization Helpers ---
        private static void AssertOwnerOnly()
        {
            var owner = GetOwner();
            if (owner is null || !Runtime.CheckWitness(owner))
                throw new Exception("Owner authorization failed.");
        }

        private static void AssertAuthorizedOnly()
        {
            var authorized = GetAuthorizedAddress();
            if (authorized.IsZero || !Runtime.CheckWitness(authorized))
                throw new Exception("Automation Service authorization failed.");
        }
    }
}