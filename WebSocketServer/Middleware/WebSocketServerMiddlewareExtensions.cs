namespace WebSocketServer.Middleware
{
    public static class WebSocketServerMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketServerMiddleware>();
        }

        public static IServiceCollection AddWebSocketConnectionManager(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketConnectionManager>();
            return services;
        }
    }
}