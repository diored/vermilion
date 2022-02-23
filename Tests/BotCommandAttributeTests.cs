using DioRed.Vermilion.Attributes;

using Xunit;

namespace DioRed.Vermilion.Tests
{
    public class BotCommandAttributeTests
    {
        [Fact]
        public void CheckRegularTextPattern()
        {
            string command = "test";

            var attribute = new BotCommandAttribute(command);

            Assert.Matches(attribute.Regex, "test");
            Assert.DoesNotMatch(attribute.Regex, "12test12");
            Assert.DoesNotMatch(attribute.Regex, "Test");
        }
    }
}