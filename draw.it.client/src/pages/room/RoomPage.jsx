import "./RoomPage.css";
import {useEffect, useState} from "react";
import { useNavigate, useParams } from "react-router";
import api from "@/utils/api.js";
import Button from "@/components/button/button.jsx";
import * as signalR from "@microsoft/signalr";

const mockRoom = (id) => ({
    id,
    players: [
    { id: "1", name: "Petras", isHost: true },
    { id: "2", name: "Ona" },
    { id: "3", name: "Lukas" },
    ],
    settings: { durationSec: 90, rounds: 3, category: "Animals" },
    });

export default function RoomPage() {
    const [isReady, setIsReady] = useState(false);
    const [lobbyConnection, setLobbyConnection] = useState(null);

    const navigate = useNavigate();
    
    const { roomId = "DEMO" } = useParams();   // paims id iš URL, pvz. /room/ABCD
    const room = mockRoom(roomId);
    
    const formatDuration = (seconds) => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    };
    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/lobbyHub")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        setLobbyConnection(connection);

        const userId = "12345"
        
        connection.start()
            .then(() => {
                connection.invoke("JoinRoomGroup", userId, roomId) // Need to extract actual userId and use actual roomId
            })
            .catch(err => console.error("Connection failed:", err));

        return () => {
            connection.stop();
        };
    }, [roomId]); // Ensures it runs once per room ID

    const leaveRoom = async () => {
        try {
          const response = api.post(`room/${roomId}/leave`);
        
          if (response.status === 204) {
            navigate("/");
          }
        } catch (err) {
          console.error("Error leaving room:", err);
          alert(err.response?.data?.error || "Could not leave room. Please try again.");
        }
    };

    return (
    <div className="game-room">
      <div className="game-room-container">
        <h1 className="game-room-title">GAME ROOM</h1>
        <div className="room-id">Room ID: {room.id}</div>
        
        <div className="game-room-content">
          {/* Left Column - Players */}
          <div className="players-section">
            <h2 className="section-title">PLAYERS</h2>
            <div className="player-count">
              {room.players.length} / 4
            </div>
            <ul className="players-list">
              {room.players.map((p) => (
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
              <span className="setting-value">{room.players.length}</span>
            </div>
            <div className="game-setting">
              <span className="setting-label">CATEGORY:</span>
              <span className="setting-value category-value">{room.settings.category}</span>
            </div>
            <div className="game-setting">
              <span className="setting-label">DURATION:</span>
              <span className="setting-value">{formatDuration(room.settings.durationSec)}</span>
            </div>
            <div className="game-setting">
              <span className="setting-label">ROUNDS:</span>
              <span className="setting-value">{room.settings.rounds}</span>
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
