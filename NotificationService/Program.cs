using NotificationService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<NotificationConsumerService>();

var host = builder.Build();
host.Run();