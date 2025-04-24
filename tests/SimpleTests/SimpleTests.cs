using System;
using Xunit;

namespace SimpleTests
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

        [Fact]
        public void Test_StringComparison_Passes()
        {
            // Arrange
            var expected = "test";
            var actual = "test";

            // Act & Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_BooleanAssertion_Passes()
        {
            // Arrange
            var condition = true;

            // Act & Assert
            Assert.True(condition);
        }

        [Fact]
        public void Test_NullCheck_Passes()
        {
            // Arrange
            string? nullString = null;
            string nonNullString = "not null";

            // Act & Assert
            Assert.Null(nullString);
            Assert.NotNull(nonNullString);
        }

        [Fact]
        public void Test_CollectionAssertion_Passes()
        {
            // Arrange
            var collection = new[] { 1, 2, 3, 4, 5 };

            // Act & Assert
            Assert.Contains(3, collection);
            Assert.DoesNotContain(6, collection);
            Assert.Equal(5, collection.Length);
        }
    }
}
