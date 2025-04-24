using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.MockServiceTests.TestFixtures
{
    public class MockServiceTestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; }
        public Mock<IAccountService> AccountServiceMock { get; }
        public Mock<ISecretsService> SecretsServiceMock { get; }
        public Mock<IFunctionService> FunctionServiceMock { get; }
        public Mock<IPriceFeedService> PriceFeedServiceMock { get; }
        public Mock<IWalletService> WalletServiceMock { get; }
        public Mock<IEnclaveService> EnclaveServiceMock { get; }

        public MockServiceTestFixture()
        {
            // Create mock services
            AccountServiceMock = new Mock<IAccountService>();
            SecretsServiceMock = new Mock<ISecretsService>();
            FunctionServiceMock = new Mock<IFunctionService>();
            PriceFeedServiceMock = new Mock<IPriceFeedService>();
            WalletServiceMock = new Mock<IWalletService>();
            EnclaveServiceMock = new Mock<IEnclaveService>();

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Jwt:Secret", "test-jwt-secret-key-that-is-long-enough-for-testing" },
                    { "Jwt:Issuer", "test-issuer" },
                    { "Jwt:Audience", "test-audience" },
                    { "Jwt:ExpiryMinutes", "60" }
                })
                .Build();

            // Setup DI
            var services = new ServiceCollection();

            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Add logging
            services.AddLogging(builder => builder.AddProvider(NullLoggerProvider.Instance));

            // Add mock services
            services.AddSingleton(AccountServiceMock.Object);
            services.AddSingleton(SecretsServiceMock.Object);
            services.AddSingleton(FunctionServiceMock.Object);
            services.AddSingleton(PriceFeedServiceMock.Object);
            services.AddSingleton(WalletServiceMock.Object);
            services.AddSingleton(EnclaveServiceMock.Object);

            // Build service provider
            ServiceProvider = services.BuildServiceProvider();
        }

        public void Dispose()
        {
            // Clean up resources if needed
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
