import React from 'react';

const PlayerStatusList = ({ players }) => {
    return (
        <div className="bg-gray-800 p-4 rounded-xl shadow-lg mb-4 text-white max-h-64 overflow-y-scroll flex-shrink-0">

            <h3 className="text-lg font-bold mb-2 border-b border-gray-600 pb-1 sticky top-0 bg-gray-800 z-10 text-white">
                Leaderboard
            </h3>

            {players.length === 0 && (
                <p className="text-gray-400">Waiting for players...</p>
            )}

            {players.map((player) => (
                <div
                    // PATAISYMAS: Naudojame player.name
                    key={player.name}
                    className={`flex justify-between items-center p-2 rounded-md transition-colors text-sm text-white
                        ${player.isDrawer // PATAISYMAS: player.isDrawer
                        ? 'bg-indigo-600 font-extrabold shadow-md'
                        : ''
                    }
                        ${player.hasGuessed && !player.isDrawer // PATAISYMAS: player.hasGuessed
                        ? 'text-lime-400'
                        : ''
                    }
                    `}
                >
                    <div className="flex items-center">
                        {/* Piešėjo simbolis */}
                        {player.isDrawer && <span className="mr-2 text-yellow-300">✏️</span>}

                        {/* Atspėjusio žaidėjo simbolis */}
                        {player.hasGuessed && !player.isDrawer && <span className="mr-2 text-lime-400">✔️</span>}

                        {/* Vardo atvaizdavimas */}
                        <span className="truncate">{player.name || "Unknown Player"}</span>
                    </div>

                    {/* Taškų atvaizdavimas */}
                    <span className="font-semibold text-white">{player.score} pts</span>
                </div>
            ))}
        </div>
    );
};

export default PlayerStatusList;