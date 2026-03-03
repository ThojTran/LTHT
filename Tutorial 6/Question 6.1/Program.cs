using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Question_6._1;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();