using InventoryService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<InventoryConsumerService>();

var host = builder.Build();
host.Run();