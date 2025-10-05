import React, { useState, useEffect, useCallback, useRef } from 'react';
import Button from "@/components/button/button.jsx";
import Input from "@/components/input/Input.jsx"
import './HostScreen.css';
import api from "@/utils/api.js";
import { useParams, useNavigate } from 'react-router';

const UI_CATEGORIES = ['Animals', 'Vehicle type', 'Games', 'Custom'];

const initialRoomState = {
    settings: { drawingTime: 60, numberOfRounds: 2, categories: [], roomName: "" },
    players: [],
    status: 'LOBBY'
};

function HostScreen() {
    const { roomId } = useParams();
    const navigate = useNavigate();

    const [room, setRoom] = useState(initialRoomState);
    const [localSettings, setLocalSettings] = useState(initialRoomState.settings);
    const [customWords, setCustomWords] = useState('');
    const [loading, setLoading] = useState(false);

    const intervalRef = useRef(null);

    const joinedPlayers = room.players;

    const fetchRoomStatus = useCallback(async () => {
        if (!roomId) return;

        try {
            const response = await api.get(`api/v1/Room/${roomId}/status`);

            if (response.status === 200) {
                const roomData = response.data;
                setRoom(roomData);

                setLocalSettings(roomData.settings);
                setCustomWords((roomData.settings.customWords || []).join(', '));
            }
        } catch (error) {
            console.warn('Failed to fetch room status:', error.response?.status || error.message);
        }
    }, [roomId]);

    const stopPolling = useCallback(() => {
        if (intervalRef.current) {
            clearInterval(intervalRef.current);
            intervalRef.current = null;
        }
    }, []);

    const startPolling = useCallback(async () => {
        if (intervalRef.current) return;

        await fetchRoomStatus();

        intervalRef.current = setInterval(fetchRoomStatus, 3000);
    }, [fetchRoomStatus]);


    const updateSettingsOnServer = useCallback(async (updatedSettings) => {
        if (!roomId) return;
        try {
            const payload = {
                RoomName: updatedSettings.roomName,
                Categories: updatedSettings.categories.filter(c => c !== 'Custom'),
                DrawingTime: parseInt(updatedSettings.drawingTime),
                NumberOfRounds: parseInt(updatedSettings.numberOfRounds),
                CustomWords: customWords.split(',').map(w => w.trim()).filter(w => w.length > 0)
            };

            await api.patch(`api/v1/Room/${roomId}/settings`, payload);
        } catch (error) {
            console.error("Klaida išsaugant nustatymus:", error.response?.data?.error || error);
        }
    }, [roomId, customWords]);

    useEffect(() => {
        startPolling();
        return () => stopPolling();
    }, [startPolling, stopPolling]);

    const handleNumberSettingChange = (event, settingKey, min, max) => {
        const value = parseInt(event.target.value, 10);
        let validatedValue = isNaN(value) ? min : value;
        validatedValue = Math.min(Math.max(validatedValue, min), max);

        setLocalSettings(prevSettings => {
            const updatedSettings = {
                ...prevSettings,
                [settingKey]: validatedValue
            };

            updateSettingsOnServer(updatedSettings);
            return updatedSettings;
        });
    };

    const handleCategoryChange = (event) => {
        const { value, checked } = event.target;

        setLocalSettings(prevSettings => {
            const currentCategories = prevSettings.categories || [];
            let newCategories;

            if (checked) {
                newCategories = [...currentCategories, value];
            } else {
                newCategories = currentCategories.filter(cat => cat !== value);
            }

            const updatedSettings = { ...prevSettings, categories: newCategories };
            updateSettingsOnServer(updatedSettings);

            return updatedSettings;
        });
    };

    const handleStartGame = async () => {
        setLoading(true);
        const hostUserId = localStorage.getItem("userId");

        if (!roomId || !hostUserId) {
            alert("Kambario ID arba Vartotojo ID nerastas.");
            setLoading(false);
            return;
        }

        try {
            stopPolling();
            await updateSettingsOnServer(localSettings);

            const startGameResponse = await api.put(`api/v1/Room/${roomId}/start`);

            if (startGameResponse.status === 200) {
                navigate(`/gameplay/${roomId}`);
            } else {
                alert(`Klaida paleidžiant žaidimą: ${startGameResponse.data?.error || 'Nežinoma klaida'}`);
            }
        } catch (error) {
            console.error('Klaida paleidžiant žaidimą:', error.response?.data?.error || error);
            alert(`Tinklo klaida paleidžiant žaidimą: ${error.response?.data?.error || 'Nežinoma klaida'}`);
        } finally {
            setLoading(false);
            startPolling();
        }
    };

    const nonHostPlayers = joinedPlayers.filter(p => !p.isHost);
    const allNonHostsReady = nonHostPlayers.length > 0 && nonHostPlayers.every(p => p.isReady);
    const canStartGame = joinedPlayers.length > 1 && allNonHostsReady;

    if (room.status === 'IN_GAME') {
        navigate(`/gameplay/${roomId}`);
        return null;
    }

    if (!roomId) {
        return <div className="host-screen-container">Invalid Room ID.</div>;
    }

    return (
        <div className="host-screen-container">
            <div className="top-info-bar">
                <div className="room-name-input">
                    <label htmlFor="roomName">Room Name:</label>
                    <Input
                        id="roomName"
                        type="text"
                        value={localSettings.roomName || ''}
                        onChange={(e) => {
                            setLocalSettings(prev => ({
                                ...prev,
                                roomName: e.target.value
                            }));
                        }}
                        onFocus={stopPolling}
                        onBlur={async () => {
                            await updateSettingsOnServer(localSettings);
                            startPolling();
                        }}
                        placeholder="e.g., Fun Room"
                    />
                </div>
                <div className="room-id">Room ID: <span>{roomId || 'Loading...'}</span></div>
            </div>

            <div className="main-content">
                <div className="players-container">
                    <h2>Players ({joinedPlayers.length})</h2>
                    <table className="players-table">
                        <thead>
                        <tr>
                            <th>#</th>
                            <th>Name</th>
                            <th>Ready</th>
                        </tr>
                        </thead>
                        <tbody>
                        {joinedPlayers.map((player, index) => (
                            <tr key={player.id}>
                                <td>{index + 1}</td>
                                <td className={player.isReady ? 'ready' : ''}>
                                    {player.name} {player.isHost ? '👑 (Host)' : ''}
                                </td>
                                <td className={player.isReady ? 'ready' : ''}>
                                    {player.isHost ? '-' : (player.isReady ? '✅' : '❌')}
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </table>

                    {joinedPlayers.length < 2 && (
                        <p style={{marginTop: '10px', color: '#ff6b35', fontWeight: 'bold'}}>
                            Reikia bent 2 žaidėjų žaidimui.
                        </p>
                    )}
                    {joinedPlayers.length >= 2 && !canStartGame && (
                        <p style={{ marginTop: '10px', color: '#ff6b35', fontWeight: 'bold' }}>
                            Laukiama, kol visi prisijungę žaidėjai bus Pasiruošę (Ready).
                        </p>
                    )}
                    {canStartGame && (
                        <p style={{ marginTop: '10px', color: '#4ade80', fontWeight: 'bold' }}>
                            Visi žaidėjai pasiruošę! Galima pradėti.
                        </p>
                    )}
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
                                {UI_CATEGORIES.map(cat => (
                                    <label key={cat} style={{ display: 'flex',  alignItems: 'center', gap: '8px' }}>
                                        <Input
                                            type="checkbox"
                                            value={cat}
                                            checked={localSettings.categories.includes(cat)}
                                            onChange={handleCategoryChange}
                                        />
                                        {cat}
                                    </label>
                                ))}
                            </div>
                            {localSettings.categories.includes('Custom') && (
                                <div className="custom-input" style={{ marginTop: '10px' }}>
                                    <Input
                                        type="text"
                                        value={customWords}
                                        onChange={(e) => setCustomWords(e.target.value)}
                                        onFocus={stopPolling}
                                        onBlur={async () => {
                                            await updateSettingsOnServer(localSettings);
                                            startPolling();
                                        }}
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
                                    value={localSettings.drawingTime}
                                    onChange={(e) => handleNumberSettingChange(e, 'drawingTime', 20, 180)}
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
                                    value={localSettings.numberOfRounds}
                                    onChange={(e) => handleNumberSettingChange(e, 'numberOfRounds', 1, 10)}
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
                <Button onClick={handleStartGame} disabled={loading || !canStartGame}>
                    {loading ? 'Kuriama...' : 'Start Game'}
                </Button>
            </div>
        </div>
    );
}

export default HostScreen;