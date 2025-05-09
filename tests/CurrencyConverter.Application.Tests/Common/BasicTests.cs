using Xunit;

namespace CurrencyConverter.Application.Tests.Common
{
    public class BasicTests
    {
        [Fact]
        public void True_IsTrue()
        {
            Assert.True(true);
        }

        [Fact]
        public void False_IsFalse()
        {
            Assert.False(false);
        }

        [Fact]
        public void String_Equals_Works()
        {
            Assert.Equal("test", "test");
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(2, 2, 4)]
        [InlineData(5, 3, 8)]
        public void Addition_Works(int a, int b, int expected)
        {
            Assert.Equal(expected, a + b);
        }
    }
}
