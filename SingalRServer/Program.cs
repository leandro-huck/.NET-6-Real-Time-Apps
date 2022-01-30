using SignalRServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors(b => b
    .WithOrigins("null")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials());
app.MapHub<ChatHub>("/chatHub");


app.Run();
