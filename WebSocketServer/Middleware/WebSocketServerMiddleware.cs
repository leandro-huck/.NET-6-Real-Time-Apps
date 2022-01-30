using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace WebSocketServer.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketConnectionManager _manager;

        public WebSocketServerMiddleware(RequestDelegate next, WebSocketConnectionManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // WriteRequestParam(context); // Write info about request to the console
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("--> WebSocket Connected");

                string ConnID = _manager.AddSocket(webSocket);
                await SendConnectionIDAsync(webSocket, ConnID);

                await ReceiveMessage(webSocket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine("--> Message received.");
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"    Message: {message}");
                        await RouteJSONMessageAsync(message);
                        return;
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        string id = _manager.GetAllSockets().FirstOrDefault(s => s.Value == webSocket).Key;
                        Console.WriteLine("--> Received Close message.");
                        _manager.GetAllSockets().TryRemove(id, out WebSocket? socket);
                        if (socket != null)
                        {
                            await socket.CloseAsync(result.CloseStatus.GetValueOrDefault(),
                                result.CloseStatusDescription,
                                CancellationToken.None);
                        }
                        return;
                    }
                });
            }
            else
            {
                Console.WriteLine("--> Hello from the 2nd request delegate");
                await _next(context);
            }
        }
        private void WriteRequestParam(HttpContext context)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"--> Request Method: {context.Request.Method}");
            Console.WriteLine($"    Request Protocol: {context.Request.Protocol}");

            if (context.Request.Headers != null)
            {
                Console.WriteLine("    Request Headers:");
                foreach (var h in context.Request.Headers)
                {
                    Console.WriteLine($"        {h.Key}: {h.Value}");
                }
            }
            Console.ResetColor();
        }

        private async Task SendConnectionIDAsync(WebSocket socket, string connectionID)
        {
            byte[] buffer = Encoding.UTF8.GetBytes($"ConnID: {connectionID}");

            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            byte[] buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                    cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }

        public async Task RouteJSONMessageAsync(string message)
        {
            JsonElement routeObj = JsonSerializer.Deserialize<dynamic>(message);
            if (Guid.TryParse(routeObj.GetProperty("To").ToString(), out Guid guidOutput))
            {
                Console.WriteLine("Targeted message");
                var socket = _manager.GetAllSockets().FirstOrDefault(s => s.Key == routeObj.GetProperty("To").ToString());
                if (socket.Value != null)
                {
                    if (socket.Value.State == WebSocketState.Open)
                    {
                        await socket.Value.SendAsync(Encoding.UTF8.GetBytes(routeObj.GetProperty("Message").ToString()),
                            WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else
                {
                    Console.WriteLine("--> Invalid recipient");
                }
            }
            else
            {
                Console.WriteLine("--> Broadcast Message");
                foreach (var socket in _manager.GetAllSockets())
                {
                    await socket.Value.SendAsync(Encoding.UTF8.GetBytes(routeObj.GetProperty("Message").ToString()),
                    WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}