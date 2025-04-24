using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Tests.Mocks;

namespace NeoServiceLayer.Tests.TestFixtures
{
    public class ServiceTestFixture
    {
        public IServiceProvider ServiceProvider { get; }

        public ServiceTestFixture()
        {
            var services = new ServiceCollection();

            // Add configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtSettings:Secret", "test-jwt-secret-key-that-is-long-enough-for-testing" },
                    { "JwtSettings:Issuer", "test-issuer" },
                    { "JwtSettings:Audience", "test-audience" },
                    { "JwtSettings:ExpiryMinutes", "60" }
                })
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            // Add logging
            services.AddLogging(builder => builder.AddConsole());

            // Add mock enclave service
            services.AddSingleton<IEnclaveService, MockEnclaveService>();

            // Build service provider
            ServiceProvider = services.BuildServiceProvider();
        }

        public T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        public Mock<T> GetMock<T>() where T : class
        {
            var mock = new Mock<T>();
            return mock;
        }
    }
}
