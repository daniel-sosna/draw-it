import './HostScreen.css';
import {useEffect, useState, useMemo, useRef} from 'react';
import { useNavigate, useParams } from 'react-router';
import api from "@/utils/api.js";
import Button from "@/components/button/button.jsx";
import Input from "@/components/input/Input.jsx"
import * as signalR from "@microsoft/signalr";

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
    const [lobbyConnection, setLobbyConnection] = useState(null);
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

    const [joinedPlayers] = useState([
        { id: 1, name: 'Player 1', isReady: true },
        { id: 2, name: 'Player 2', isReady: false },
        { id: 3, name: 'Player 3', isReady: true },
    ]);

    // This is needed for sending users the current setting when the user joins for THE FIRST TIME
    // Store the LATEST settings state
    const settingsRef = useRef({ roomName, categoryId, drawingTime, numberOfRounds });

    // Update the ref on every state change
    useEffect(() => {
        settingsRef.current = { roomName, categoryId, drawingTime, numberOfRounds };
    }, [roomName, categoryId, drawingTime, numberOfRounds]);
    
    const sendSettingsUpdate = async (newCatId, newDrawingTime, newNumberOfRounds, newRoomName) => {
        if (!lobbyConnection) {
            console.error("SignalR connection not established.");
            return;
        }

        setSaving(true);
        try {
            await lobbyConnection.invoke("UpdateRoomSettings", roomId, {
                categoryId: Number(newCatId),
                drawingTime: Number(newDrawingTime),
                numberOfRounds: Number(newNumberOfRounds),
                roomName: newRoomName,
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
            sendSettingsUpdate(catId, drawTime, rounds, name);
        }, 500);
    }, [lobbyConnection, roomId]);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7200/lobbyHub")
            .configureLogging(signalR.LogLevel.Information)
            .withAutomaticReconnect()
            .build();

        setLobbyConnection(connection);

        const sendImmediateSettings = async (conn) => {
            if (conn.state !== signalR.HubConnectionState.Connected) return;

            const currentSettings = settingsRef.current;
            try {
                // Use the latest state values captured by the useEffect closure
                await conn.invoke("UpdateRoomSettings",
                    roomId,
                    currentSettings.categoryId,
                    currentSettings.drawingTime,
                    currentSettings.numberOfRounds,
                    currentSettings.roomName,
                );
                console.log("Initial settings sent successfully.");
            } catch (err) {
                console.error('Error sending initial settings:', err);
            }
        };

        async function start() {
            try {
                await connection.start();
                console.log("SignalR Connected.");
                console.log(`Room id: ${roomId}`);
                connection.invoke("JoinRoomGroup", roomId);
            } catch (err) {
                console.log(err);
            }
        };

        connection.onreconnected(connectionId => {
            console.log("Reconnected successfully!");
            connection.invoke("JoinRoomGroup", roomId)
                .then(() => console.log(`Re-joined Room id: ${roomId}`))
                .catch(err => console.error("Failed to re-join group:", err));
            });

        connection.on("ReceiveUpdateSettings", (newCategoryId, newDrawingTime, newNumberOfRounds) => {
            console.log("Host received settings update broadcast. Ignoring this");
        });

        connection.on("RequestCurrentSettings", () => {
            console.log("Server requested current settings. Sending state now.");
            sendImmediateSettings(connection);
        });

        start();
        
        return () => {
            connection.stop(); // fail-safe on unmount
        }
        
    }, [roomId]); // Ensures it runs once per room ID
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

    // The settings payload will be obsolete because settings are now automatically saved to backend
    const startGame = async () => {
        setLoading(true);

        const settingsPayload = {
            roomName: roomName || `Room-${roomId}`, 
            categories: selectedCategories,
            customWords: selectedCategories.includes('Custom')
                ? customWords.split(',').map(word => word.trim()).filter(w => w)
                : [],
            drawingTime: drawingTime,
            numberOfRounds: numberOfRounds,
        };

        try {
            await api.put(`room/${roomId}/settings`, settingsPayload); // Endpoint is not implemented yet
            const response = await api.post(`room/${roomId}/start`); // Endpoint is not implemented yet

            if (response.status === 204) {
                navigate(`/gameplay/${roomId}`);
            }
        } catch (err) {
            console.error('Error starting game:', err);
            alert(err.response?.data?.error || 'Could not start game. Please try again.');
        } finally {
            setLoading(false);
        }
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
                                <th>#</th>
                                <th>Name</th>
                            </tr>
                        </thead>
                        <tbody>
                            {joinedPlayers.map((player, index) => (
                                <tr key={player.id}>
                                    <td className={player.isReady ? 'ready' : ''}>{index + 1}</td>
                                    <td className={player.isReady ? 'ready' : ''}>{player.name}</td>
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
                <Button disabled={saving}>
                    {saving ? 'Saving...' : 'Save Settings'}
                </Button>
                <Button onClick={startGame} disabled={loading}>
                    {loading ? 'Starting...' : 'Start Game'}
                </Button>
                <Button disabled={deleting} className="delete-button">
                    {deleting ? 'Deleting...' : 'Delete Room'}
                </Button>
            </div>
        </div>
    );
}

export default HostScreen;