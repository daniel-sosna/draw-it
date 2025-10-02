import { useParams } from "react-router";
import "./RoomPage.css";

const mockRoom = (id) => ({
  id,
  players: [
    { id: "1", name: "Petras", isHost: true },
    { id: "2", name: "Ona" },
    { id: "3", name: "Lukas" },
  ],
  settings: { durationSec: 90, rounds: 3, category: "Animals" },
  youAreHost: true,
});

export default function RoomPage() {
  const { roomId = "DEMO" } = useParams();   // paims id iš URL, pvz. /room/ABCD
  const room = mockRoom(roomId);

  const formatDuration = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
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
            <button 
              className="start-button"
              disabled={!room.youAreHost} 
              title={room.youAreHost ? "" : "Host only"}
            >
              START
            </button>
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
            <a href="/" className="back-link">LEAVE ROOM</a>
          </div>
        </div>
      </div>
    </div>
  );
}
