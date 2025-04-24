using Xunit;

namespace NeoServiceLayer.Tests
{
    public class BasicTests
    {
        [Fact]
        public void Test_SimpleAssertion_Passes()
        {
            // Arrange
            var expected = 1;
            var actual = 1;

            // Act & Assert
            Assert.Equal(expected, actual);
        }
    }
}
