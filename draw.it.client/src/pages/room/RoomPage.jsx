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
    const [isReady, setIsReady] = useState(false);
    const [roomState, setRoomState] = useState(initialRoomState); // state for the room
    const { players, settings } = roomState;

    const navigate = useNavigate();

    const formatDuration = (seconds) => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    };

    useEffect(() => {
        if (!lobbyConnection) return;

        lobbyConnection.on("ReceiveUpdateSettings", (categoryId, drawingTime, numberOfRounds, roomName) => {
            console.log("Received new settings:", categoryId, drawingTime, numberOfRounds, roomName);

            setRoomState(prev => ({
                ...prev,
                name: roomName,
                settings: {
                    ...prev.settings,
                    category: categoryId,
                    durationSec: drawingTime,
                    rounds: numberOfRounds,
                }
            }));
        });

        return () => {
            lobbyConnection.off("ReceiveUpdateSettings");
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
                <li key={p.id} className="player-item">
                  {p.name} {p.isHost ? "👑" : ""}
                </li>
              ))}
            </ul>
            <Button onClick={() => {setIsReady((prev) => !prev);}}>
              READY
            </Button>
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
