namespace DioRed.Vermilion.Tests.Integration;

public class IntegrationSmokeTests
{
    [Test]
    [Property("Category", "Integration")]
    public async Task IntegrationTestProject_IsDiscoverable()
    {
        await Assert.That(AppContext.BaseDirectory.Length).IsGreaterThan(0);
    }
}
