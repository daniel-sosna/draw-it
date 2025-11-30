import React from 'react';

const PlayerStatusList = ({ players }) => {
    
    const sortedPlayers = [...players].sort((a, b) => b.score - a.score);
    
    return (
        <div className="bg-gray-800 p-4 rounded-xl shadow-lg mb-4 text-white max-h-64 overflow-y-scroll flex-shrink-0">

            <h3 className="text-lg font-bold mb-2 border-b border-gray-600 pb-1 sticky top-0 bg-gray-800 z-10 text-white">
                Leaderboard
            </h3>

            {players.length === 0 && (
                <p className="text-gray-400">Waiting for players...</p>
            )}

            {sortedPlayers.map((player) => (
                <div
                    key={player.name}
                    className={`flex justify-between items-center p-2 rounded-md transition-colors text-sm text-white
                        ${player.isDrawer 
                        ? 'bg-indigo-600 font-extrabold shadow-md'
                        : ''
                    }
                        ${player.hasGuessed && !player.isDrawer
                        ? 'text-lime-400'
                        : ''
                    }
                    `}
                >
                    <div className="flex items-center flex-grow min-w-0">
                        {player.isDrawer && <span className="mr-2 text-yellow-300 flex-shrink-0">✏️</span>}
                        
                        {player.hasGuessed && !player.isDrawer && <span className="mr-2 text-lime-400 flex-shrink-0">✔️</span>}
                        
                        <span className="truncate" title={player.name}>{player.name || "Unknown Player"}</span>
                    </div>

                    <span className="font-semibold text-white flex-shrink-0">{player.score} pts</span>
                </div>
            ))}
        </div>
    );
};

export default PlayerStatusList;