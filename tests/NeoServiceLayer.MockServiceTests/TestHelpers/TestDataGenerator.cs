using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Models;
using ModelsPriceSourceType = NeoServiceLayer.Core.Models.PriceSourceType;

namespace NeoServiceLayer.MockServiceTests.TestHelpers
{
    /// <summary>
    /// Helper class for generating test data
    /// </summary>
    public static class TestDataGenerator
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Generates a random string of the specified length
        /// </summary>
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[_random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        /// <summary>
        /// Generates a random email address
        /// </summary>
        public static string GenerateRandomEmail()
        {
            return $"{GenerateRandomString(8)}@example.com";
        }

        /// <summary>
        /// Generates a random password that meets complexity requirements
        /// </summary>
        public static string GenerateRandomPassword()
        {
            return $"P@ssw0rd{_random.Next(1000, 9999)}";
        }

        /// <summary>
        /// Creates a test account
        /// </summary>
        public static Account CreateTestAccount(Guid? id = null, string username = null, string email = null)
        {
            return new Account
            {
                Id = id ?? Guid.NewGuid(),
                Username = username ?? $"user_{GenerateRandomString(8)}",
                Email = email ?? GenerateRandomEmail(),
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                IsActive = true,
                Credits = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a test wallet
        /// </summary>
        public static Wallet CreateTestWallet(Guid? id = null, string name = null, Guid? accountId = null)
        {
            return new Wallet
            {
                Id = id ?? Guid.NewGuid(),
                Name = name ?? $"wallet_{GenerateRandomString(8)}",
                Address = $"N{GenerateRandomString(33)}",
                ScriptHash = $"0x{GenerateRandomString(40)}",
                PublicKey = GenerateRandomString(66),
                EncryptedPrivateKey = GenerateRandomString(64),
                WIF = GenerateRandomString(52),
                AccountId = accountId ?? Guid.NewGuid(),
                IsServiceWallet = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a test secret
        /// </summary>
        public static Secret CreateTestSecret(Guid? id = null, string name = null, Guid? accountId = null, List<Guid> allowedFunctionIds = null)
        {
            return new Secret
            {
                Id = id ?? Guid.NewGuid(),
                Name = name ?? $"secret_{GenerateRandomString(8)}",
                Description = "Test secret",
                EncryptedValue = $"encrypted:{GenerateRandomString(32)}",
                Version = 1,
                AccountId = accountId ?? Guid.NewGuid(),
                AllowedFunctionIds = allowedFunctionIds ?? new List<Guid>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a test function
        /// </summary>
        public static Function CreateTestFunction(Guid? id = null, string name = null, Guid? accountId = null, string runtime = null)
        {
            return new Function
            {
                Id = id ?? Guid.NewGuid(),
                Name = name ?? $"function_{GenerateRandomString(8)}",
                Description = "Test function",
                Runtime = runtime ?? "dotnet",
                Handler = "TestFunction::Handler.Process",
                SourceCode = "public class Handler { public static object Process(object input) { return input; } }",
                EntryPoint = "Handler.Process",
                AccountId = accountId ?? Guid.NewGuid(),
                MaxExecutionTime = 30000,
                MaxMemory = 256,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "Active",
                SecretIds = new List<Guid>(),
                EnvironmentVariables = new Dictionary<string, string>()
            };
        }

        /// <summary>
        /// Creates a test price source
        /// </summary>
        public static PriceSource CreateTestPriceSource(Guid? id = null, string name = null, ModelsPriceSourceType? type = null)
        {
            return new PriceSource
            {
                Id = id ?? Guid.NewGuid(),
                Name = name ?? $"source_{GenerateRandomString(8)}",
                Type = type ?? ModelsPriceSourceType.Exchange,
                Url = $"https://api.{GenerateRandomString(8)}.com",
                ApiKey = GenerateRandomString(32),
                ApiSecret = GenerateRandomString(64),
                Weight = _random.Next(1, 100),
                Status = PriceSourceStatus.Active,
                UpdateIntervalSeconds = 60,
                TimeoutSeconds = 5,
                SupportedAssets = new List<string> { "BTC", "ETH", "NEO" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Config = new PriceSourceConfig
                {
                    PriceJsonPath = "$.price",
                    TimestampJsonPath = "$.timestamp",
                    TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ"
                }
            };
        }

        /// <summary>
        /// Creates a test price
        /// </summary>
        public static Price CreateTestPrice(Guid? id = null, string symbol = null, string baseCurrency = null, decimal? value = null)
        {
            var timestamp = DateTime.UtcNow;
            var sourceId = Guid.NewGuid();
            var sourceName = $"source_{GenerateRandomString(8)}";

            return new Price
            {
                Id = id ?? Guid.NewGuid(),
                Symbol = symbol ?? "BTC",
                BaseCurrency = baseCurrency ?? "USD",
                Value = value ?? _random.Next(10000, 60000) + _random.Next(0, 100) / 100.0m,
                Timestamp = timestamp,
                SourcePrices = new List<SourcePrice>
                {
                    new SourcePrice
                    {
                        Id = Guid.NewGuid(),
                        SourceId = sourceId,
                        SourceName = sourceName,
                        Value = value ?? _random.Next(10000, 60000) + _random.Next(0, 100) / 100.0m,
                        Timestamp = timestamp,
                        Weight = _random.Next(1, 100)
                    }
                },
                ConfidenceScore = _random.Next(80, 100),
                CreatedAt = DateTime.UtcNow,
                Signature = GenerateRandomString(64),
                Source = sourceName
            };
        }

        /// <summary>
        /// Creates a test price history
        /// </summary>
        public static PriceHistory CreateTestPriceHistory(Guid? id = null, string symbol = null, string baseCurrency = null, string interval = null)
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddDays(-1);

            return new PriceHistory
            {
                Id = id ?? Guid.NewGuid(),
                Symbol = symbol ?? "BTC",
                BaseCurrency = baseCurrency ?? "USD",
                Interval = interval ?? "1h",
                StartTime = startTime,
                EndTime = endTime,
                DataPoints = new List<PriceDataPoint>
                {
                    new PriceDataPoint
                    {
                        Timestamp = startTime.AddHours(1),
                        Open = 50000.0m,
                        High = 51000.0m,
                        Low = 49000.0m,
                        Close = 50500.0m,
                        Volume = 100000.0m
                    },
                    new PriceDataPoint
                    {
                        Timestamp = startTime.AddHours(2),
                        Open = 50500.0m,
                        High = 52000.0m,
                        Low = 50000.0m,
                        Close = 51500.0m,
                        Volume = 120000.0m
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
