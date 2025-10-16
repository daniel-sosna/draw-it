using System.Net;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Extensions;
using Draw.it.Server.Models.User;
using Draw.it.Server.Services.Hub;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs;

public class LobbyHub : Hub
{
    private readonly IRoomService _roomService;
    private readonly IUserService _userService;
    private readonly ILogger<LobbyHub> _logger;

    public LobbyHub(IRoomService roomService, IUserService userService, ILogger<LobbyHub> logger)
    {
        _roomService = roomService;
        _userService = userService;
        _logger = logger;
    }

    public async Task joinRoomGroup(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        // Maybe tell the service that a user has joined (for state/database).
        // await _lobbyHubService.UserJoinedLobby(Context.UserIdentifier, roomId);

        // Maybe add a message that a user has joined
        // await Clients.Group(roomId).SendAsync("UserJoined", Context.UserIdentifier);
    }

    public async Task leaveRoom(string roomId)
    {
        if (!long.TryParse(Context.UserIdentifier, out long usrId))
        {
            throw new AppException("Invalid user identifier provided", HttpStatusCode.NotFound);
        }
        UserModel usr = _userService.GetUser(usrId);
        _roomService.LeaveRoom(roomId, usr);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        _logger.LogInformation("User {usrId} successfully left room {roomId}. The connection identifier={Context.UserIdentifier}", usrId, roomId, Context.UserIdentifier);
    }
    
}