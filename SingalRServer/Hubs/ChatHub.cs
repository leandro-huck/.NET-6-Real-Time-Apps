using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace SignalRServer.Hubs
{
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"--> Connection established {Context.ConnectionId}");
            Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnectionID", Context.ConnectionId);
            return base.OnConnectedAsync();
        }
        public async Task SendMessageAsync(string message)
        {
            JsonElement routeObj = JsonSerializer.Deserialize<dynamic>(message);
            string toClient = routeObj.GetProperty("To").ToString();
            Console.WriteLine($"--> Message Received on {Context.ConnectionId}");

            if (String.IsNullOrEmpty(toClient))
            {
                await Clients.All.SendAsync("ReceiveMessage", message);
            }
            else
            {
                await Clients.Client(toClient).SendAsync("ReceiveMessage", message);
            }
        }
    }
}