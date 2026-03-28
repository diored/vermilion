using DioRed.Vermilion.Hosting;

using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .AddVermilion("Vermilion.MongoDb", v =>
    {
        v.ConfigureChatStorage(s => s.UseMongoDb());
        v.ConfigureConnectors(c => c.AddTelegram());

        v.ConfigureCommandHandlers(h =>
        {
            h.Add("/ping", () => "pong");
            h.Add("/echo", tail => tail);
        });
    })
    .Build()
    .Run();
