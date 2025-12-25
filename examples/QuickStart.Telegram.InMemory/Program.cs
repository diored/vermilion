using DioRed.Vermilion.Hosting;

using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .AddVermilion("Vermilion.QuickStart", v =>
    {
        v.ConfigureChatStorage(s => s.UseInMemory());
        v.ConfigureConnectors(c => c.AddTelegram());

        // Minimal handler so the bot can start.
        v.ConfigureCommandHandlers(h =>
        {
            h.Add("/ping", () => "pong");
            h.Add("/echo", tail => tail);
        });
    })
    .Build()
    .Run();