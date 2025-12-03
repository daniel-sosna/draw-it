import './HostScreen.css';
import { useContext, useEffect, useState, useMemo } from 'react';
import { useNavigate, useParams } from 'react-router';
import * as signalR from '@microsoft/signalr';
import Button from "@/components/button/button.jsx";
import TextInput from "@/components/input/TextInput.jsx";
import NumberInput from "@/components/input/NumberInput.jsx";
import RadioGroup from "@/components/input/RadioGroup.jsx";
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
    const [drawingTime, setDrawingTime] = useState(60);
    const [numberOfRounds, setNumberOfRounds] = useState(2);
    const [loading, setLoading] = useState(false);
    const [deleting, setDeleting] = useState(false);
    const [joinedPlayers, setJoinedPlayers] = useState([]);

    const navigate = useNavigate();
    
    useEffect(() => {
        if (!lobbyConnection) return;

        (async () => {
            // Ensure the connection is in Connected state before sending initial settings
            const start = Date.now();
            while (lobbyConnection.state !== signalR.HubConnectionState.Connected) {
                if (Date.now() - start > 10000) {
                    throw new Error('Timed out waiting for SignalR connection to become Connected');
                }
                // small pause
                await new Promise(r => setTimeout(r, 300));
            }
            await sendSettingsUpdate(categoryId, drawingTime, numberOfRounds, roomName);
        })();

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

    const sendSettingsUpdate = async (catId, drawingTime, numberOfRounds, roomName) => {
        if (!lobbyConnection) {
            console.error("SignalR connection not established.");
            return;
        }

        try {
            await lobbyConnection.invoke("UpdateRoomSettings", {
                roomName: roomName || `Room-${roomId}`,
                categoryId: Number(catId),
                drawingTime: Number(drawingTime),
                numberOfRounds: Number(numberOfRounds),
            });
        } catch (err) {
            console.error('Error sending real-time settings update:', err);
            alert("Number of rounds should be more than 1 and Drawing time must be between 20 and 300 seconds.");
        }
    };

    // Waits 500ms after the last change before sending the update
    const debouncedSend = useMemo(() => {
        return debounce((...args) => {
            sendSettingsUpdate(...args);
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

    const RULES = Object.freeze({
        drawingTime: { min: 20, max: 180, step: 1 },
        numberOfRounds: { min: 1, max: 10, step: 1 },
    });

    // Waits 500ms after the last input before clamping and snapping the value
    const debouncedClampAndSnap = useMemo(() => {
        return debounce((val, rules, setter, fieldName) => {
            const newValue = clampAndSnap(val, rules);
            setter(newValue);

            if (fieldName === 'drawingTime') {
                sendSettingsUpdate(categoryId, newValue, numberOfRounds, roomName);
            } else if (fieldName === 'numberOfRounds') {
                sendSettingsUpdate(categoryId, drawingTime, newValue, roomName);
            }
        }, 1000);
    }, [lobbyConnection, roomId]);

    const clampAndSnap = (val, { min, max, step }) => {
        let v = Number(val);
        if (Number.isNaN(v)) v = min;
        v = Math.min(max, Math.max(min, v));
        const base = min ?? 0;
        if (step && step > 0) {
            v = Math.round((v - base) / step) * step + base;
        }
        return v;
    };

    const handleNumberInput = (event, setter, fieldName) => {
        const rules = RULES[fieldName];

        setter(event.target.value);

        debouncedClampAndSnap(event.target.value, rules, setter, fieldName);
    };

    const startGame = async () => {
        setLoading(true);
        
        try {
            await lobbyConnection.invoke("StartGame");
        } catch (error) {
            console.error("Failed to invoke StartGame:", error);
            alert("An unexpected network error occurred.");
        }
        setLoading(false);
    };

    const deleteRoom = async () => {
        setDeleting(true);
        console.log("Deleting room: " + roomId);
        await lobbyConnection.invoke("LeaveRoom");
        navigate("/");
        setDeleting(false);
    };

    return (
        <div className="host-screen-container">
            <div className="top-info-bar">
                <div className="room-name-input">
                    <label htmlFor="roomName">Room Name:</label>
                    <TextInput
                        id="roomName"
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
                            <RadioGroup
                                name="categoryId"
                                options={CATEGORIES}
                                value={categoryId}
                                onChange={handleCategoryChange}
                                className="radio-group"
                            />
                        </div>

                        <div className="game-options-section">
                            <div className="setting-item">
                                <label htmlFor="drawingTime">Drawing Time (seconds):</label>
                                <NumberInput
                                    id="drawingTime"
                                    value={drawingTime}
                                    onChange={(e) => handleNumberInput(e, setDrawingTime, 'drawingTime')}
                                    min={RULES.drawingTime.min}
                                    max={RULES.drawingTime.max}
                                    step={RULES.drawingTime.step}
                                />
                            </div>
                            <div className="setting-item">
                                <label htmlFor="numberOfRounds">Number of Rounds:</label>
                                <NumberInput
                                    id="numberOfRounds"
                                    value={numberOfRounds}
                                    onChange={(e) => handleNumberInput(e, setNumberOfRounds, 'numberOfRounds')}
                                    min={RULES.numberOfRounds.min}
                                    max={RULES.numberOfRounds.max}
                                    step={RULES.numberOfRounds.step}
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div className="button-container action-buttons">
                <Button onClick={startGame} disabled={loading}>
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