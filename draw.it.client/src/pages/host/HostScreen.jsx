import './HostScreen.css';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import api from "@/utils/api.js";
import Button from "@/components/button/button.jsx";
import Input from "@/components/input/Input.jsx"

function HostScreen() {
    const { roomId } = useParams();
    const [roomName, setRoomName] = useState('');
    const [selectedCategories, setSelectedCategories] = useState([]);
    const [customWords, setCustomWords] = useState('');
    const [drawingTime, setDrawingTime] = useState(60);
    const [numberOfRounds, setNumberOfRounds] = useState(2);
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const [joinedPlayers] = useState([
        { id: 1, name: 'Player 1', isReady: true },
        { id: 2, name: 'Player 2', isReady: false },
        { id: 3, name: 'Player 3', isReady: true },
    ]);

    const handleCategoryChange = (event) => {
        const { value, checked } = event.target;
        if (value === 'Custom') {
            setSelectedCategories(checked ? ['Custom'] : []);
        } else {
            setSelectedCategories(prev => {
                const updated = prev.filter(cat => cat !== 'Custom');
                return checked ? [...updated, value] : updated.filter(cat => cat !== value);
            });
        }
    };

    const handleNumberInput = (event, setter, min) => {
        const value = parseInt(event.target.value, 10);
        if (value < min || isNaN(value)) {
            setter(min);
        } else {
            setter(value);
        }
    };

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
                            <h3>Choose Categories:</h3>
                            <div className="checkbox-group" style={{
                                display: 'flex',
                                flexDirection: 'column',
                                gap: '8px',
                                alignItems: 'flex-start'
                            }}>
                                {['Animals', 'Vehicle type', 'Games', 'Custom'].map(cat => (
                                    <label key={cat} style={{ display: 'flex',  alignItems: 'center', gap: '8px' }}>
                                        <Input
                                            type="checkbox"
                                            value={cat}
                                            checked={selectedCategories.includes(cat)}
                                            onChange={handleCategoryChange}
                                        />
                                        {cat}
                                    </label>
                                ))}
                            </div>
                            {selectedCategories.includes('Custom') && (
                                <div className="custom-input" style={{ marginTop: '10px' }}>
                                    <Input
                                        type="text"
                                        value={customWords}
                                        onChange={(e) => setCustomWords(e.target.value)}
                                        placeholder="Enter words separated by commas"
                                    />
                                </div>
                            )}
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

            <div className="button-container">
                <Button onClick={startGame} disabled={loading}>
                    {loading ? 'Starting...' : 'Start Game'}
                </Button>
            </div>
        </div>
    );
}

export default HostScreen;