import React from "react";

export default function PlayerList({ players = [] }) {
    return (
        <div className="flex flex-col border border-gray-300 rounded bg-white p-3 h-full">
            <div className="flex items-center justify-between mb-2">
                <h3 className="font-semibold text-gray-800">Players</h3>
                <span className="text-xs text-gray-500">{players.length}</span>
            </div>

            <ol className="overflow-y-auto text-sm text-gray-800 space-y-1 text-left">
                {players.map((p, idx) => (
                    <li key={p.name} className="px-0 py-1 rounded hover:bg-gray-50">
                        <span className="font-medium">{idx + 1}. {p.name}</span>
                    </li>
                ))}
                {players.length === 0 && (
                    <li className="text-gray-500 text-sm list-none pl-0">No players yet</li>
                )}
            </ol>
        </div>
    );
}


