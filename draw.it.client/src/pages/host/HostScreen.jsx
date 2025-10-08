import './HostScreen.css';
import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router';
import api from "@/utils/api.js";
import Button from "@/components/button/button.jsx";
import Input from "@/components/input/Input.jsx"

const MOCK_CATEGORIES = [
    { id: 1, name: "Animals" },
    { id: 2, name: "Vehicle type" },
    { id: 3, name: "Games" },
];
function HostScreen() {
    const { roomId } = useParams();
    const [roomName, setRoomName] = useState('');
    const [categoryId, setCategoryId] = useState(MOCK_CATEGORIES[0].id);
    const [drawingTime, setDrawingTime] = useState(60);
    const [numberOfRounds, setNumberOfRounds] = useState(2);
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [deleting, setDeleting] = useState(false);
    const navigate = useNavigate();
    const [roomData, setRoomData] = useState(null);
    const [currentUserId, setCurrentUserId] = useState(null);

    useEffect(() => {
        const fetchHostData = async () => {
            try {
                const meResponse = await api.get("auth/me");
                if (meResponse.status === 200) {
                    setCurrentUserId(meResponse.data.id);
                } else {
                    navigate('/');
                }
            } catch (err) {
                console.warn("User not authenticated or error fetching host data.", err);
                navigate("/");
            }
        };
        fetchHostData();
    }, [roomId, navigate]);
    
    const [joinedPlayers] = useState([
        { id: 1, name: 'Player 1', isReady: true },
        { id: 2, name: 'Player 2', isReady: false },
        { id: 3, name: 'Player 3', isReady: true },
    ]);

    const handleCategoryChange = (event) => {
        const value = parseInt(event.target.value, 10);
        setCategoryId(value);
    };

    const handleNumberInput = (event, setter, min) => {
        const value = parseInt(event.target.value, 10);
        if (value < min || isNaN(value)) {
            setter(min);
        } else {
            setter(value);
        }
    };

    const saveSettings = async () => {
        setSaving(true);
        const settingsPayload = {
            roomName: roomName || `Room-${roomId}`,
            categoryId: categoryId,
            drawingTime: drawingTime,
            numberOfRounds: numberOfRounds,
        };

        try {
            await api.put(`room/${roomId}/settings`, settingsPayload);
            alert("Settings saved successfully!");
        } catch (err) {
            console.error('Error saving settings:', err);
            alert(err.response?.data?.error || 'Could not save settings. Please try again.');
        } finally {
            setSaving(false);
        }
    };

    const startGame = async () => {
        setLoading(true);

        const settingsPayload = {
            roomName: settings.roomName || `Room-${roomId}`,
            categoryId: settings.categoryId, 
            drawingTime: settings.drawingTime,
            numberOfRounds: settings.numberOfRounds,
        };

        try {
            await api.put(`room/${roomId}/settings`, settingsPayload);
            const response = await api.post(`room/${roomId}/start`);

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

    const deleteRoom = async () => {
        if (!window.confirm("Are you sure you want to delete this room? This action cannot be undone.")) {
            return;
        }
        setDeleting(true);
        try {
            await api.delete(`room/${roomId}`);
            alert("Room deleted successfully!");
            navigate("/"); 
        } catch (err) {
            console.error('Error deleting room:', err);
            alert(err.response?.data?.error || 'Could not delete room. Please try again.');
        } finally {
            setDeleting(false);
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
                        onChange={(e) => setRoomName(e.target.value)}
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
                                {MOCK_CATEGORIES.map(cat => (
                                    <label key={cat.id} className="radio-label">
                                        <input 
                                            type="radio"
                                            name="categoryId" 
                                            value={cat.id}
                                            checked={categoryId === cat.id}
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
                                    onChange={(e) => handleNumberInput(e, setDrawingTime, 20)}
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
                                    onChange={(e) => handleNumberInput(e, setNumberOfRounds, 1)}
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
                <Button onClick={saveSettings} disabled={saving}>
                    {saving ? 'Saving...' : 'Save Settings'}
                </Button>
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