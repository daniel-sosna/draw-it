import { useParams } from "react-router-dom";

// Paprasti „mock“ duomenys – be backendo
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

  return (
    <div style={{ padding: 24 }}>
      <h2>Room: {room.id}</h2>
      <p>
        Players: <b>{room.players.length}</b> • Category: <b>{room.settings.category}</b> •{" "}
        Duration: <b>{room.settings.durationSec}s</b> • Rounds: <b>{room.settings.rounds}</b>
      </p>

      <h3>Players</h3>
      <ul>
        {room.players.map((p) => (
          <li key={p.id}>
            {p.name} {p.isHost ? "👑" : ""}
          </li>
        ))}
      </ul>

      <div style={{ marginTop: 12, display: "flex", gap: 8 }}>
        <button onClick={() => navigator.clipboard.writeText(window.location.href)}>
          Copy room link
        </button>
        <button disabled={!room.youAreHost} title={room.youAreHost ? "" : "Host only"}>
          Start
        </button>
        <a href="/">Back</a>
      </div>
    </div>
  );
}
