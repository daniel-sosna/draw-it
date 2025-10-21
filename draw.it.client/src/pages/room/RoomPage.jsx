import "./RoomPage.css";
import { useContext, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router";
import Button from "@/components/button/button.jsx";
import { LobbyHubContext } from "@/utils/LobbyHubProvider.jsx";

const initialRoomState = {
    id: "",
    name: "Game Room",
    players: [],
    settings: { durationSec: 90, rounds: 3, category: "Loading..." },
};

export default function RoomPage() {
    const lobbyConnection = useContext(LobbyHubContext);
    const { roomId } = useParams();
    const [isPlayerReady, setIsPlayerReady] = useState(false);
    const [roomState, setRoomState] = useState(initialRoomState); // state for the room
    const { players, settings } = roomState;

    const navigate = useNavigate();

    const formatDuration = (seconds) => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    };

    const handleReadyToggle = async () => {
        if (!lobbyConnection) return;
        const newState = !isPlayerReady;

        try {
            await lobbyConnection.invoke("SetPlayerReady", newState);
            setIsPlayerReady(newState);
        } catch (error) {
            console.error("Failed to toggle ready state:", error);
        }
    };

    useEffect(() => {
        if (!lobbyConnection) return;

        lobbyConnection.on("ReceiveUpdateSettings", (settings) => {
            console.log("Received new settings:", settings);

            setRoomState(prev => ({
                ...prev,
                name: settings.roomName,
                settings: {
                    category: settings.categoryName,
                    durationSec: settings.drawingTime,
                    rounds: settings.numberOfRounds,
                }
            }));
        });

        lobbyConnection.on("ReceiveRoomDeleted", () => {
            console.warn("Room was deleted by host. Navigating to index.");
            alert("The room was deleted by the host.");
            navigate("/");
        });

        lobbyConnection.on("ReceivePlayerList", (players) => {
            console.log("Received new player list:", players);
            setRoomState(prev => ({
                ...prev,
                players: players
            }));
        });

        lobbyConnection.on("ReceiveGameStart", () => {
            console.log("Host started the game, navigating to /gameplay");
            navigate(`/gameplay/${roomId}`);
        });

        return () => {
            lobbyConnection.off("ReceiveUpdateSettings");
            lobbyConnection.off("ReceiveRoomDeleted");
            lobbyConnection.off("ReceivePlayerList");
            lobbyConnection.off("ReceiveGameStart");
        }
    }, [lobbyConnection, roomId]);

    const leaveRoom = async () => {
        console.log("Leaving room: " + roomId);
        navigate("/");
    };

    return (
    <div className="game-room">
      <div className="game-room-container">
        <h1 className="game-room-title">{roomState.name}</h1>
        <div className="room-id">Room ID: {roomState.id}</div>

        <div className="game-room-content">
          {/* Left Column - Players */}
          <div className="players-section">
            <h2 className="section-title">PLAYERS</h2>
            <div className="player-count">
              {players.length} / 4
            </div>
            <ul className="players-list">
                {players.map((p) => (
                    <li key={p.name} className={`player-item ${p.isReady ? 'ready' : ''}`}>
                        {p.name} {p.isHost ? "👑" : ""} {p.isReady ? "✅" : "❌"}
                    </li>
                ))}
            </ul>

          <div className="action-buttons ready-action">
              <Button
                  onClick={handleReadyToggle} 
                  className={isPlayerReady ? 'ready-button is-ready' : 'ready-button'}
              >
                  {isPlayerReady ? 'UNREADY' : 'READY'}
              </Button>
          </div> 
          </div>

          {/* Divider */}
          <div className="divider"></div>

          {/* Right Column - Game Details */}
          <div className="game-details-section">
            <div className="game-setting">
              <span className="setting-label">COUNT:</span>
              <span className="setting-value">{players.length}</span>
            </div>
            <div className="game-setting">
              <span className="setting-label">CATEGORY:</span>
              <span className="setting-value category-value">{settings.category}</span>
            </div>
            <div className="game-setting">
              <span className="setting-label">DURATION:</span>
              <span className="setting-value">{formatDuration(settings.durationSec)}</span>
            </div>
            <div className="game-setting">
              <span className="setting-label">ROUNDS:</span>
              <span className="setting-value">{settings.rounds}</span>
            </div>
            <div className="leave-button-container">
              <Button onClick={leaveRoom}>
                LEAVE ROOM
              </Button>
            </div>
          </div>
        </div>
      </div>
    </div>
    );
}
