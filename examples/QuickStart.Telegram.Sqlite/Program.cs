using DioRed.Vermilion.Hosting;

using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .AddVermilion("Vermilion.Sqlite", v =>
    {
        v.ConfigureChatStorage(s => s.UseSqlite());
        v.ConfigureConnectors(c => c.AddTelegram());

        v.ConfigureCommandHandlers(h =>
        {
            h.Add("/ping", () => "pong");
            h.Add("/echo", tail => tail);
        });
    })
    .Build()
    .Run();
