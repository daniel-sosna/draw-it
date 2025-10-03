using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Draw.it.Server.Hubs 
{

    public class GameHub : Hub
    {
        public async Task SendMessage(string message, string room, string user) // room might not be a string later, need a separator to update the chats
        {
            await Clients.Group(room).SendAsync("ReceiveMessage", user, message);
        }
        
        public async Task JoinRoom(string room, string user)
        {
            // Need to add some auth
            /*
            if (!UserIsInRoom(Context.UserIdentifier, room)) 
                throw new HubException("Unauthorized");
            */
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("UserJoined", room, Context.ConnectionId, user);
        }
        
        public async Task LeaveRoom(string room, string user)
        {
            await  Groups.RemoveFromGroupAsync( Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("UserLeft", room, Context.ConnectionId, user);
        }
    }
}
