import { useParams } from "react-router";
import { useState, useEffect } from 'react';
import api from "@/utils/api.js";
import Button from "@/components/button/button.jsx";
import Input from "@/components/input/Input.jsx"
import "./RoomPage.css";
import { useNavigate } from 'react-router';

const initialRoomState = {
    id: "",
    players: [],
    settings: {
        drawingTime: 0,
        numberOfRounds: 0,
        categories: [],
        roomName: ""
    },
};

export default function RoomPage() {
    const { roomId = "" } = useParams();
    const navigate = useNavigate();
    const [room, setRoom] = useState(initialRoomState);
    const [loading, setLoading] = useState(true);

    const currentUserId = parseInt(localStorage.getItem("userId"));

    const playerSelf = room.players.find(p => p.id === currentUserId);
    
    const isReady = room.players.find(p => p.id === currentUserId)?.isReady || false;
    const isHost = playerSelf?.isHost || false;
    
    const formatDuration = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

    const fetchRoomStatus = async () => {
        if (!roomId) return;

        try {
            const response = await api.get(`api/v1/Room/${roomId}/status`);

            if (response.status === 200) {
                
                const roomData = response.data;
                const userIsHost = roomData.players.find(p => p.id === currentUserId)?.isHost;
                
                if (userIsHost) {
                    navigate(`/host/${roomId}`);
                    return;
                }

                if (roomData.status === 'IN_GAME') {
                    navigate(`/gameplay/${roomId}`);
                    return;
                }
                 
                
                setRoom(response.data);
            }
        } catch (error) {
            console.error("Kambario būsenos gavimo klaida:", error);
            if (error.response?.status === 404) {
                alert("Kambarys nerastas!");
                navigate('/');
            }
        } finally {
            setLoading(false);
        }
    };

    const toggleReadyStatus = async () => {
        if (isHost) return;
        
        const newReadyStatus = !isReady;
        
        if (!currentUserId || !roomId) return;

        try {
            await api.put(`api/v1/Room/${roomId}/player/${currentUserId}/ready`, {
                isReady: newReadyStatus
            });
            setRoom(prev => ({
                ...prev,
                players: prev.players.map(p =>
                    p.id === currentUserId ? { ...p, isReady: newReadyStatus } : p
                )
            }));
        } catch (error) {
            console.error("Ready būsenos nustatymo klaida:", error);
            alert("Nepavyko pakeisti Ready būsenos.");
        }
    };

    useEffect(() => {
        if (!currentUserId) {
            navigate('/');
            return;
        }

        fetchRoomStatus();
        const intervalId = setInterval(fetchRoomStatus, 3000);
        return () => clearInterval(intervalId);
    }, [roomId, navigate, currentUserId]);

    if (loading) {
        return <div className="game-room">Loading...</div>;
    }

    const allPlayersReady = room.players.length >= 2 &&
        room.players.every(p => p.isReady);

    const MAX_PLAYERS = 3;
    
    return (
        <div className="game-room">
            <div className="game-room-container">
                <h1 className="game-room-title">GAME ROOM: {room.settings.roomName || room.id}</h1>
                <div className="room-id">Room ID: {room.id}</div>

                <div className="game-room-content">
                    <div className="players-section">
                        <h2 className="section-title">PLAYERS</h2>

                        <div className="player-count">
                            {room.players.length} / {MAX_PLAYERS}
                        </div>

                        <ul className="players-list">
                            {room.players.map((p) => (
                                <li
                                    key={p.id}
                                    className={`player-item ${p.isReady ? 'player-ready' : ''}`}
                                >
                                    {p.name} {p.isHost ? "👑" : ""}
                                    {p.isReady && <span style={{marginLeft: '5px'}}>✅</span>}
                                </li>
                            ))}
                        </ul>

                        {!isHost && (
                            <button
                                onClick={toggleReadyStatus}
                                className={`start-button ${isReady ? 'button-ready' : ''}`}
                            >
                                {isReady ? 'READY! ✅' : 'Click to Ready'}
                            </button>
                        )}

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
                            <span className="setting-value category-value">
                                {Array.isArray(room.settings.categories)
                                    ? room.settings.categories.join(', ')
                                    : room.settings.categories || 'Null'}
                            </span>
                        </div>
                        <div className="game-setting">
                            <span className="setting-label">DURATION:</span>
                            <span className="setting-value">{formatDuration(room.settings.drawingTime)}</span>
                        </div>
                        <div className="game-setting">
                            <span className="setting-label">ROUNDS:</span>
                            <span className="setting-value">{room.settings.numberOfRounds}</span>
                        </div>

                        <a href="/" className="back-link">LEAVE ROOM</a>
                    </div>
                </div>
            </div>
        </div>
    );
}
