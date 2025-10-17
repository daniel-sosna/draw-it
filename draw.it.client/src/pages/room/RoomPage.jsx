import "./RoomPage.css";
import {useEffect, useState} from "react";
import { useNavigate, useParams } from "react-router";
import api from "@/utils/api.js";
import Button from "@/components/button/button.jsx";
import * as signalR from "@microsoft/signalr";

const initialRoomState = {
    id: "",
    name: "Game Room",
    players: [],
    settings: { durationSec: 90, rounds: 3, category: "Loading..." },
};

export default function RoomPage() {
    const [isReady, setIsReady] = useState(false);
    const [lobbyConnection, setLobbyConnection] = useState(null);
    const [roomState, setRoomState] = useState(initialRoomState); // state for the room
    const { players, settings } = roomState;
    
    const navigate = useNavigate();
    
    const { roomId = "DEMO" } = useParams();   // paims id iš URL, pvz. /room/ABCD
    
    const formatDuration = (seconds) => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    };
    
    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7200/lobbyHub")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        setLobbyConnection(connection);
        
        async function start() {
            try {
                await connection.start();
                console.log("SignalR Connected.");
                console.log(`Room id: ${roomId}`);
                connection.invoke("JoinRoomGroup", roomId);
                await connection.invoke("RequestSettingsUpdate", roomId);
            } catch (err) {
                console.error("Initial connection failed:", err);
            }
        }
        
        connection.onreconnected(connectionId => {
            console.log("Reconnected successfully!");
            connection.invoke("JoinRoomGroup", roomId)
                .then(() => console.log(`Re-joined Room id: ${roomId}`))
                .catch(err => console.error("Failed to re-join group:", err));
        });

        connection.on("ReceiveUpdateSettings", (categoryId, drawingTime, numberOfRounds, roomName) => {
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

        connection.on("RequestCurrentSettings", () => {
            console.log("Ignoring request. Request for the host only");
        });
        
        
        start();
        
        return () => {
            connection.stop(); // fail-safe on unmount
        }
    }, [roomId, navigate]); // Ensures it runs once per room ID
    // If we don't add 'navigate' it will be flagged by the ESLint rule exhaustive-deps

    const leaveRoom = async () => {
        try {
            await lobbyConnection.invoke("LeaveRoom", roomId);
            console.log("Left room: " + roomId);
            navigate("/");
            
        } catch (err) {
          console.error("Error leaving room:", err);
          alert(err.response?.data?.error || "Could not leave room. Please try again.");
        }
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
