using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.ServiceTests.TestFixtures
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
                    { "Jwt:Secret", "test-jwt-secret-key-that-is-long-enough-for-testing" },
                    { "Jwt:Issuer", "test-issuer" },
                    { "Jwt:Audience", "test-audience" },
                    { "Jwt:ExpiryMinutes", "60" }
                })
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            // Add logging
            services.AddLogging();

            // Build service provider
            ServiceProvider = services.BuildServiceProvider();
        }

        public T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        public Mock<T> CreateMock<T>() where T : class
        {
            return new Mock<T>();
        }
    }
}
