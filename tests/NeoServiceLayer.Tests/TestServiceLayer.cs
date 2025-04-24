using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Storage;
using NeoServiceLayer.Services.Storage.Configuration;
using NeoServiceLayer.Services.Storage.Providers;
using Xunit;

namespace NeoServiceLayer.Tests
{
    public class TestServiceLayer
    {
        [Fact]
        public void TestServiceLayerWorks()
        {
            // This is a simple test to verify that the test project can reference the service layer
            Assert.True(true);
        }
    }
}
