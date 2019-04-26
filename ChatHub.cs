using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TodoApi 
{
    public class ChatHub : Hub    
    {        
        public async Task SendMessage(string user, string message, string connectionId)        
        {              
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", user, message);
        }
    }
}