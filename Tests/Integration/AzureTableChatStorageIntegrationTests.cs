using Azure;
using Azure.Data.Tables;

using DioRed.Common.AzureStorage;
using DioRed.Vermilion.ChatStorage;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace DioRed.Vermilion.Tests.Integration;

[RequiresDocker]
[NotInParallel("Docker")]
[Property("Category", "Integration")]
public class AzureTableChatStorageIntegrationTests
{
    private const ushort AzuriteTablePort = 10002;
    private const string AzuriteAccountName = "devstoreaccount1";
    private const string AzuriteAccountKey =
        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

    private static IContainer? _container;

    [Before(Class)]
    public static async Task StartContainer()
    {
        _container =
            new ContainerBuilder("mcr.microsoft.com/azure-storage/azurite:3.35.0")
                .WithImage("mcr.microsoft.com/azure-storage/azurite:3.35.0")
                .WithCommand("azurite-table", "--tableHost", "0.0.0.0", "--tablePort", AzuriteTablePort.ToString(), "--loose")
                .WithPortBinding(AzuriteTablePort, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(AzuriteTablePort))
                .Build();

        await Container.StartAsync().ConfigureAwait(false);
    }

    [After(Class)]
    public static async Task StopContainer()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync().ConfigureAwait(false);
            _container = null;
        }
    }

    [Test]
    public async Task SupportsDistinctChatTypesForSameNumericId()
    {
        string tableName = UniqueTableName();
        AzureTableChatStorage storage = CreateStorage(tableName);

        ChatId privateChatId = new("telegram", "Private", 42);
        ChatId groupChatId = new("telegram", "Group", 42);

        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = privateChatId,
            Tags = ["private"]
        }).ConfigureAwait(false);

        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = groupChatId,
            Tags = ["group"]
        }).ConfigureAwait(false);

        ChatMetadata privateChat = await storage.GetChatAsync(privateChatId).ConfigureAwait(false);
        ChatMetadata groupChat = await storage.GetChatAsync(groupChatId).ConfigureAwait(false);

        using var _ = Assert.Multiple();
        await Assert.That(privateChat.Tags).Contains("private");
        await Assert.That(groupChat.Tags).Contains("group");
    }

    [Test]
    public async Task MigratesLegacyRowsToTypeAwareRowKeys()
    {
        string tableName = UniqueTableName();
        TableClient legacyClient = CreateTableClient(tableName);
        await legacyClient.CreateIfNotExistsAsync().ConfigureAwait(false);

        await legacyClient.AddEntityAsync(new TableEntity("telegram", "42")
        {
            { "Type", "Private" },
            { "Title", "" },
            { "Tags", "[\"legacy\"]" }
        }).ConfigureAwait(false);

        AzureTableChatStorage storage = CreateStorage(tableName);

        ChatMetadata chat = await storage.GetChatAsync(new ChatId("telegram", "Private", 42)).ConfigureAwait(false);
        var legacyRow = await legacyClient.GetEntityIfExistsAsync<TableEntity>("telegram", "42").ConfigureAwait(false);
        var migratedRow = await legacyClient.GetEntityIfExistsAsync<TableEntity>("telegram", "Private|42").ConfigureAwait(false);

        using var _ = Assert.Multiple();
        await Assert.That(chat.Tags).Contains("legacy");
        await Assert.That(legacyRow.HasValue).IsFalse();
        await Assert.That(migratedRow.HasValue).IsTrue();
    }

    private static AzureTableChatStorage CreateStorage(string tableName)
    {
        return new AzureTableChatStorage(
            new AzureStorageSettings
            {
                ConnectionString = BuildConnectionString()
            },
            tableName
        );
    }

    private static TableClient CreateTableClient(string tableName)
    {
        return new TableClient(BuildConnectionString(), tableName);
    }

    private static string BuildConnectionString()
    {
        int port = Container.GetMappedPublicPort(AzuriteTablePort);
        return $"DefaultEndpointsProtocol=http;AccountName={AzuriteAccountName};AccountKey={AzuriteAccountKey};TableEndpoint=http://{Container.Hostname}:{port}/{AzuriteAccountName};";
    }

    private static string UniqueTableName() => $"chats{Guid.NewGuid():N}";

    private static IContainer Container =>
        _container ?? throw new InvalidOperationException("Azurite container has not been started.");
}
