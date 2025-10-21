import './HostScreen.css';
import { useContext, useEffect, useState, useMemo, useRef } from 'react';
import { useNavigate, useParams } from 'react-router';
import api from "@/utils/api.js";
import Button from "@/components/button/button.jsx";
import Input from "@/components/input/Input.jsx"
import { LobbyHubContext } from "@/utils/LobbyHubProvider.jsx";

// This debounce utility is for sending real time updates
// so there is a slight delay
// No need to send updates every milisecond
const debounce = (func, delay) => {
    let timeoutId;
    return (...args) => {
        if (timeoutId) {
            clearTimeout(timeoutId);
        }
        timeoutId = setTimeout(() => {
            func.apply(this, args);
        }, delay);
    };
};

const CATEGORIES = [
    { id: 1, name: 'Animals' },
    { id: 2, name: 'Vehicle type' },
    { id: 3, name: 'Games' },
    { id: 4, name: 'Custom' },
];

function HostScreen() {
    const lobbyConnection = useContext(LobbyHubContext);
    const { roomId } = useParams();
    const [roomName, setRoomName] = useState('');
    const [categoryId, setCategoryId] = useState(CATEGORIES[0].id.toString());
    const [selectedCategories, setSelectedCategories] = useState([]);
    const [customWords, setCustomWords] = useState('');
    const [drawingTime, setDrawingTime] = useState(60);
    const [numberOfRounds, setNumberOfRounds] = useState(2);
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [deleting, setDeleting] = useState(false);
    const navigate = useNavigate();
    const [joinedPlayers, setJoinedPlayers] = useState([]);
    
    useEffect(() => {
        if (!lobbyConnection) return;

        lobbyConnection.on("ReceiveUpdateSettings", (newCategoryId, newDrawingTime, newNumberOfRounds) => {
            console.log("Host received settings update broadcast. Ignoring this");
        });

        lobbyConnection.on("ReceivePlayerList", (newPlayers) => {
            console.log("Host received new player list:", newPlayers);
            setJoinedPlayers(newPlayers);
        });

        lobbyConnection.on("ReceiveErrorOnGameStart", (message) => {
            alert(`Failed to Start Game: ${message}`);
        });

        lobbyConnection.on("ReceiveGameStart", () => {
            console.log("Game is starting, navigating to /gameplay");
            navigate(`/gameplay/${roomId}`);
        });

        return () => {
            lobbyConnection.off("ReceiveUpdateSettings");
            lobbyConnection.off("ReceivePlayerList");
            lobbyConnection.off("ReceiveGameStart");
            lobbyConnection.off("ReceiveErrorOnGameStart");
        }
    }, [lobbyConnection, roomId]);

    const sendSettingsUpdate = async (roomName, catId, drawingTime, numberOfRounds) => {
        if (!lobbyConnection) {
            console.error("SignalR connection not established.");
            return;
        }

        setSaving(true);
        try {
            await lobbyConnection.invoke("UpdateRoomSettings", roomId, {
                roomName: roomName || `Room-${roomId}`,
                categoryId: Number(catId),
                drawingTime: Number(drawingTime),
                numberOfRounds: Number(numberOfRounds),
            });
        } catch (err) {
            console.error('Error sending real-time settings update:', err);
        } finally {
            setSaving(false);
        }
    };

    // Waits 500ms after the last change before sending the update
    const debouncedSend = useMemo(() => {
        return debounce((catId, drawTime, rounds, name) => {
            sendSettingsUpdate(name, catId, drawTime, rounds);
        }, 500);
    }, [lobbyConnection, roomId]);

    const handleRoomNameChange = (event) => {
        const newName = event.target.value || `Room-${roomId}`;
        setRoomName(newName);
        debouncedSend(categoryId, drawingTime, numberOfRounds, newName);
    };

    const handleCategoryChange = (event) => {
        const newCatId = event.target.value;
        setCategoryId(newCatId);
        debouncedSend(newCatId, drawingTime, numberOfRounds, roomName);
    };

    const handleNumberInput = (event, setter, fieldName) => {
        const value = parseInt(event.target.value);
        const newValue = isNaN(value) ? 0 : value;

        setter(newValue);

        if (fieldName === 'drawingTime') {
            debouncedSend(categoryId, newValue, numberOfRounds, roomName);
        } else if (fieldName === 'numberOfRounds') {
            debouncedSend(categoryId, drawingTime, newValue, roomName);
        }
    };

    const handleStartGame = async () => {
        if (!lobbyConnection) return;

        try {
            await lobbyConnection.invoke("StartGame");
        } catch (error) {
            console.error("Failed to invoke StartGame:", error);
            alert("An unexpected network error occurred.");
        }
    };

    const deleteRoom = async () => {
        setDeleting(true);
        console.log("Deleting room: " + roomId);
        navigate("/");
        setDeleting(false);
    };

    return (
        <div className="host-screen-container">
            <div className="top-info-bar">
                <div className="room-name-input">
                    <label htmlFor="roomName">Room Name:</label>
                    <Input
                        id="roomName"
                        type="text"
                        value={roomName}
                        onChange={handleRoomNameChange}
                        placeholder="e.g., Fun Room"
                    />
                </div>
                <div className="room-id">Room ID: <span>{roomId || 'Loading...'}</span></div>
            </div>

            <div className="main-content">
                <div className="players-container">
                    <h2>Players</h2>
                    <table className="players-table">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Ready?</th>
                            </tr>
                        </thead>
                        <tbody>                            
                            {joinedPlayers.map((player) => (
                                <tr key={player.name}>
                                    <td className={player.isReady ? 'ready' : ''}>
                                        {player.name}
                                    </td>
                                    <td>
                                        {player.isReady ? "👍" : "⌛"}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>

                <div className="settings-container">
                    <h2>Game Settings</h2>
                    <div className="settings-content">
                        <div className="categories-section">
                            <h3>Choose Category:</h3>
                            <div className="radio-group" style={{
                                display: 'flex',
                                flexDirection: 'column',
                                gap: '8px',
                                alignItems: 'flex-start'
                            }}>
                                {CATEGORIES.map(cat => (
                                    <label key={cat.id} className="radio-label">
                                        <input
                                            type="radio"
                                            name="categoryId"
                                            value={cat.id.toString()}
                                            checked={categoryId === cat.id.toString()}
                                            onChange={handleCategoryChange}
                                            className="category-radio"
                                        />
                                        {cat.name}
                                    </label>
                                ))}
                            </div>
                        </div>

                        <div className="game-options-section">
                            <div className="setting-item">
                                <label htmlFor="drawingTime">Drawing Time (seconds):</label>
                                <Input
                                    id="drawingTime"
                                    type="number"
                                    value={drawingTime}
                                    onChange={(e) => handleNumberInput(e, setDrawingTime, 'drawingTime')}
                                    min="20"
                                    max="180"
                                    step="1"
                                />
                            </div>
                            <div className="setting-item">
                                <label htmlFor="numberOfRounds">Number of Rounds:</label>
                                <Input
                                    id="numberOfRounds"
                                    type="number"
                                    value={numberOfRounds}
                                    onChange={(e) => handleNumberInput(e, setNumberOfRounds, 'numberOfRounds')}
                                    min="1"
                                    max="10"
                                    step="1"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div className="button-container action-buttons">
                <Button onClick={handleStartGame} disabled={loading}>
                    {loading ? 'Starting...' : 'Start Game'}
                </Button>
                <Button onClick={deleteRoom} disabled={deleting} className="delete-button">
                    {deleting ? 'Deleting...' : 'Delete Room'}
                </Button>
            </div>
        </div>
    );
}

export default HostScreen;