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

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Get the user id
        string? hubUserIdString = Context.UserIdentifier;
        
        _logger.LogInformation("User {UserId} disconnected. Exception: {Ex}", hubUserIdString, exception?.Message);

        // Safely parse and validate the user ID
        if (long.TryParse(hubUserIdString, out long usrId))
        {
            try
            {
                UserModel usr = _userService.GetUser(usrId);
                string? currentRoomId = usr.RoomId;

                if (!string.IsNullOrEmpty(currentRoomId))
                {
                    // Add this flag because the user left abruptly
                    _roomService.LeaveRoom(currentRoomId, usr, unexpectedLeave:true);
                    
                    // Broadcast the change to the remaining users in the room
                    // await Clients.Group(currentRoomId).SendAsync("ReceivePlayerLeft", usrId.ToString(), usr.Name);
                    
                    _logger.LogInformation("User {UserId} cleaned up from room {RoomId}.", usrId, currentRoomId);
                }
                
                // Delete the user
                _userService.DeleteUser(usrId); 
            }
            catch (Exception ex)
            {
                // Log any errors
                _logger.LogError(ex, "Error during OnDisconnectedAsync cleanup for user {UserId}.", usrId);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoomGroup(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        // Maybe tell the service that a user has joined (for state/database).
        // await _lobbyHubService.UserJoinedLobby(Context.UserIdentifier, roomId);

        // Maybe add a message that a user has joined
        // await Clients.Group(roomId).SendAsync("UserJoined", Context.UserIdentifier);
    }

    public async Task LeaveRoom(string roomId)
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

    public async Task UpdateRoomSettings(string roomId, string categoryId, int drawingTime, int numberOfRounds, string roomName)
    {
        await _roomService.SetSettingsAsync(Context.UserIdentifier, roomId, categoryId, drawingTime, numberOfRounds, roomName);
        await Clients.Group(roomId).SendAsync("RecieveUpdateSettings", categoryId, drawingTime, numberOfRounds, roomName);
    }
    
    public async Task RequestSettingsUpdate(string roomId)
    {
       await Clients.Group(roomId).SendAsync("RequestCurrentSettings");
    }
}