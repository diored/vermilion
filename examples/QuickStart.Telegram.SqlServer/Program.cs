using DioRed.Vermilion.Hosting;

using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .AddVermilion("Vermilion.SqlServer", v =>
    {
        v.ConfigureChatStorage(s => s.UseSqlServer());
        v.ConfigureConnectors(c => c.AddTelegram());

        v.ConfigureCommandHandlers(h =>
        {
            h.Add("/ping", () => "pong");
            h.Add("/echo", tail => tail);
        });
    })
    .Build()
    .Run();