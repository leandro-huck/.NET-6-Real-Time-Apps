using WebSocketServer.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebSocketConnectionManager();

WebApplication app = builder.Build();

app.UseWebSockets();

app.UseWebSocketServer();

app.Run(async context =>
{
    Console.WriteLine("--> Hello from the 3rd request delegate");
    await context.Response.WriteAsync("Hello from the 3rd request delegate");
});

app.Run();