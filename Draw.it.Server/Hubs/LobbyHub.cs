using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs
{
    public class LobbyHub : Hub
    {
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;

        public LobbyHub(IRoomService roomService, IUserService userService)
        {
            _roomService = roomService;
            _userService = userService;
        }

        public async Task JoinRoomGroup(string roomId, long userId, bool isHost)
        {
            var user = _userService.FindUserById(userId);
            
            var room = _roomService.AddPlayerToRoom(roomId, user, isHost); 

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            await Clients.Group(roomId).SendAsync("ReceiveRoomUpdate", room);
        }
        
        public async Task SetPlayerReady(string roomId, long userId, bool isReady)
        {
            try
            {
                var room = _roomService.SetPlayerReady(roomId, userId, isReady); 
                
                await Clients.Group(roomId).SendAsync("ReceiveRoomUpdate", room);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveError", ex.Message);
            }
        }
        
        public async Task StartGame(string roomId, long hostUserId)
        {
            try
            {
                _roomService.StartGame(roomId); 
                
                await Clients.Group(roomId).SendAsync("GameStarted", roomId); 
                
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveError", ex.Message);
            }
        }
    }
}